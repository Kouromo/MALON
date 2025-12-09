using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Text;

[System.Serializable]
public class CreerPromptUISet
{
    public TextMeshProUGUI sujetText;
    public TMP_InputField userPromptInput;
    public Button submitButton;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI scoreText;
}

public class CreerPromptGame : MonoBehaviour 
{
    [Header("UI Light")]
    public CreerPromptUISet lightUI;

    [Header("UI Dark")]
    public CreerPromptUISet darkUI;

    [Header("UI Blue")]
    public CreerPromptUISet blueUI;

    private CreerPromptUISet ui;

    private string apiURL = "http://localhost:5001/api/rag/chat";

    string CurrentJob => PlayerPrefs.GetString("User_Position", "Vendeur");

    private int exerciseCount = 0;
    private const int MAX_EXERCISES = 1;
    private const string CPCountKey = "CP_ExercisesDone";
    private const string ThemeKey = "CurrentThemeMode"; // 0=Dark,1=Blue,2=Light
    
    void Awake()
    {
        int theme = PlayerPrefs.GetInt(ThemeKey, 0);
        switch (theme)
        {
            case 2: ui = lightUI; break;
            case 1: ui = blueUI;  break;
            case 0:
            default: ui = darkUI; break;
        }
    }
    
    void Start() 
    {
        ui.submitButton.onClick.AddListener(SubmitAnswer);

        exerciseCount = PlayerPrefs.GetInt(CPCountKey, 0);

        if (exerciseCount >= MAX_EXERCISES)
            EndGame();
        else
            GenerateSujet();
    }
    
    void GenerateSujet()
    {
        if (exerciseCount >= MAX_EXERCISES)
        {
            EndGame();
            return;
        }

        string job = CurrentJob;
        string prompt = $@"
Génère 1 SUJET concret pour un collaborateur {job} chez Orange.

Objectif:
- Le sujet doit décrire une situation métier précise où l'IA pourrait aider (relation client, dépannage, analyse, management d'équipe, etc.).
- Une seule phrase, claire et spécifique.

Format EXACT (rien d'autre):
SUJET: [1 phrase précise]
";

        ui.submitButton.interactable = false;
        ui.feedbackText.text = "IA génère un sujet...";

        StartCoroutine(CallIA(prompt, response => {
            string line = response.Trim();
            if (line.StartsWith("SUJET:"))
                line = line.Replace("SUJET:", "").Trim();

            ui.sujetText.text = line;
            ui.userPromptInput.text = "";
            ui.feedbackText.text = "Écris le MEILLEUR prompt possible pour ce sujet :";
            ui.scoreText.text = "";
            ui.submitButton.interactable = true;
        }));
    }
    
    void SubmitAnswer()
    {
        string userPrompt = ui.userPromptInput.text.Trim();
        string sujet = ui.sujetText.text;

        if (string.IsNullOrEmpty(userPrompt))
        {
            ui.feedbackText.text = "Écris ton prompt !";
            return;
        }

        if (userPrompt.Length < 5)
        {
            ui.feedbackText.text = "Ton prompt est trop court pour être utile. Essaie de décrire précisément ce que tu veux que l'IA fasse.";
            ui.scoreText.text = "Note: 0/10";
            return;
        }

        string lower = userPrompt.ToLower();
        if (lower == "a" || lower == "test" || lower == "ok")
        {
            ui.feedbackText.text = "Ton prompt est trop vague. Décris la situation, le contexte et ce que tu attends de l'IA.";
            ui.scoreText.text = "Note: 0/10";
            return;
        }

        string job = CurrentJob;
        string prompt = $@"
Tu dois ÉVALUER la qualité d'un prompt écrit par un collaborateur {job} chez Orange.

SUJET: {sujet}
PROMPT UTILISATEUR: {userPrompt}

Barème (très strict, à respecter absolument):
- 0/10 : prompt très court, vide, ou complètement hors sujet.
- 1-3/10 : très faible, presque inutilisable.
- 4-5/10 : moyen, des idées mais trop vague ou incomplet.
- 6-8/10 : bon, utilisable mais améliorable.
- 9-10/10 : excellent, très clair, très précis, directement exploitable.

IMPORTANT:
- Si le prompt est très court ou sans sens, tu dois OBLIGATOIREMENT mettre 0/10.
- Tu dois respecter strictement ce barème.

Format EXACT (rien d'autre):
NOTE: [0-10]/10
FEEDBACK: [2 à 3 phrases avec ce qui va / ne va pas]
SUCCÈS: [OUI/NON] (OUI uniquement si NOTE >= 5)
";

        ui.submitButton.interactable = false;
        ui.feedbackText.text = "IA évalue ton prompt...";

        StartCoroutine(CallIA(prompt, response => {
            ui.feedbackText.text = response;

            int note = ExtractNote(response);
            ui.scoreText.text = $"Note: {note}/10";
            bool success = note >= 5;

            exerciseCount++;
            PlayerPrefs.SetInt(CPCountKey, exerciseCount);
            PlayerPrefs.Save();

            EndGame();
        }));
    }
    
    void EndGame()
    {
        ui.sujetText.text = "C'est tout pour aujourd'hui !";
        if (exerciseCount >= MAX_EXERCISES)
        {
            ui.feedbackText.text += "\n\nTu as terminé l'exercice du jour sur la création de prompt.\nReviens demain pour un nouveau sujet.";
        }
        ui.submitButton.interactable = false;
        ui.userPromptInput.interactable = false;
    }

    int ExtractNote(string feedback)
    {
        string pattern = @"NOTE:\s*(\d+)/10";
        System.Text.RegularExpressions.Match match = 
            System.Text.RegularExpressions.Regex.Match(feedback, pattern);
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }
    
    IEnumerator CallIA(string prompt, System.Action<string> callback)
    {
        ChatRequest request = new ChatRequest { 
            message = prompt,
            metier = CurrentJob 
        };
        
        string json = JsonUtility.ToJson(request);
        Debug.Log("ENVOYÉ: " + json);
        
        byte[] body = Encoding.UTF8.GetBytes(json);
        using (UnityWebRequest req = new UnityWebRequest(apiURL, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            
            yield return req.SendWebRequest();
            
            Debug.Log("STATUS: " + req.responseCode);
            if (req.result == UnityWebRequest.Result.Success)
            {
                ChatResponse response = JsonUtility.FromJson<ChatResponse>(req.downloadHandler.text);
                Debug.Log("GPT: " + response.content);
                callback(response.content);
            }
            else
            {
                ui.feedbackText.text = $"Erreur {req.responseCode}";
            }
        }
    }
}
