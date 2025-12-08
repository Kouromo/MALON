using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Text;

public class CreerPromptGame : MonoBehaviour 
{
    [Header("UI")]
    public TextMeshProUGUI sujetText;
    public TMP_InputField userPromptInput;
    public Button submitButton;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI scoreText;
    public Button backToMenuButton;
    
    private string apiURL = "http://localhost:5001/api/rag/chat";
    
    void Start() 
    {
        submitButton.onClick.AddListener(SubmitAnswer);
        backToMenuButton.onClick.AddListener(() => SceneManager.LoadScene("Scene_Menu"));
        GenerateSujet();
    }
    
    void GenerateSujet()
    {
        string job = GlobalGameState.Job;
        string prompt = $@"
    Génère 1 SUJET concret pour un collaborateur {job} chez Orange.

    Objectif:
    - Le sujet doit décrire une situation métier précise où l'IA pourrait aider (relation client, dépannage, analyse, management d'équipe, etc.).
    - Une seule phrase, claire et spécifique.

    Format EXACT (rien d'autre):
    SUJET: [1 phrase précise]
    ";

        StartCoroutine(CallIA(prompt, response => {
            string line = response.Trim();
            if (line.StartsWith("SUJET:"))
                line = line.Replace("SUJET:", "").Trim();

            sujetText.text = line;
            userPromptInput.text = "";
            feedbackText.text = "Écris le MEILLEUR prompt possible pour ce sujet :";
            scoreText.text = "";
            submitButton.interactable = true;
        }));
    }

    
    void SubmitAnswer()
    {
        string userPrompt = userPromptInput.text.Trim();
        string sujet = sujetText.text;

        // Vérification locale stricte AVANT IA
        if (string.IsNullOrEmpty(userPrompt))
        {
            feedbackText.text = "Écris ton prompt !";
            return;
        }

        // Trop court ou vide de sens
        if (userPrompt.Length < 5)
        {
            feedbackText.text = "Ton prompt est trop court pour être utile. Essaie de décrire précisément ce que tu veux que l'IA fasse.";
            scoreText.text = "Note: 0/10";
            return;
        }

        // Cas simples à bannir
        string lower = userPrompt.ToLower();
        if (lower == "a" || lower == "test" || lower == "ok")
        {
            feedbackText.text = "Ton prompt est trop vague. Décris la situation, le contexte et ce que tu attends de l'IA.";
            scoreText.text = "Note: 0/10";
            return;
        }

        string job = GlobalGameState.Job;
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

        submitButton.interactable = false;
        StartCoroutine(CallIA(prompt, response => {
            feedbackText.text = response;

            int note = ExtractNote(response);
            scoreText.text = $"Note: {note}/10";
            bool success = note >= 5;

            Invoke(nameof(GenerateSujet), 6f);
        }));
    }
    
    int ExtractNote(string feedback)
    {
        // Cherche "NOTE: 8/10" dans la réponse
        string pattern = @"NOTE:\s*(\d+)/10";
        System.Text.RegularExpressions.Match match = 
            System.Text.RegularExpressions.Regex.Match(feedback, pattern);
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }
    
    IEnumerator CallIA(string prompt, System.Action<string> callback)
    {
        feedbackText.text = "IA réfléchit...";
        
        ChatRequest request = new ChatRequest { 
            message = prompt,
            metier = GlobalGameState.Job 
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
                feedbackText.text = $"Erreur {req.responseCode}";
            }
        }
    }
}
