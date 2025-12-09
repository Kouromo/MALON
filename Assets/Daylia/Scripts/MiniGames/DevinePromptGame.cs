using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Text;

[System.Serializable]
public class DevinePromptUISet
{
    public TextMeshProUGUI generatedText;   // Texte généré par IA
    public TMP_InputField userPromptInput;  // Prompt deviné par utilisateur
    public Button submitButton;
    public TextMeshProUGUI feedbackText;
    public Button nextButton;               // Bouton "Suivant"
}

public class DevinePromptGame : MonoBehaviour 
{
    [Header("UI Light")]
    public DevinePromptUISet lightUI;

    [Header("UI Dark")]
    public DevinePromptUISet darkUI;

    [Header("UI Blue")]
    public DevinePromptUISet blueUI;

    private DevinePromptUISet ui;

    private string apiURL = "http://localhost:5001/api/rag/chat";

    string CurrentJob => PlayerPrefs.GetString("User_Position", "Vendeur");

    private int exerciseCount = 0;
    private const int MAX_EXERCISES = 2;
    private const string DPCountKey = "DP_ExercisesDone";
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
        ui.nextButton.onClick.AddListener(OnNextClicked);

        exerciseCount = PlayerPrefs.GetInt(DPCountKey, 0);

        if (exerciseCount >= MAX_EXERCISES)
            EndGame();
        else
            GenerateTextAndRealPrompt();
    }
    
    void GenerateTextAndRealPrompt()
    {
        if (exerciseCount >= MAX_EXERCISES)
        {
            EndGame();
            return;
        }

        string job = CurrentJob;
        string prompt = $@"
Tu joues au mini-jeu 'Devine le prompt' pour un collaborateur {job} chez Orange.

RÔLE:
- Tu dois d'abord INVENTER un PROMPT RÉEL qui pourrait être utilisé par ce collaborateur dans son métier.
- Ensuite, tu génères un TEXTE GÉNÉRÉ (2 à 3 phrases) qui est la réponse de l'IA à ce prompt.
- Le texte doit être réaliste, concret, lié au métier {job}. 

Contraintes:
- Ne parle pas de 'serious game' ni de 'jeu'.
- Situe toujours le contexte dans la vie professionnelle (client, réunion, intervention, management, etc.).

Format EXACT de ta réponse (rien d'autre):
TEXTE: [le texte généré]
PROMPT: [le prompt qui a créé ce texte]
";

        ui.submitButton.interactable = false;
        ui.nextButton.interactable = false;
        ui.feedbackText.text = "IA génère un nouveau texte...";

        StartCoroutine(CallIA(prompt, response => {
            ParseTextAndPrompt(response);
        }));
    }

    void ParseTextAndPrompt(string response)
    {
        string[] lines = response.Split('\n');
        string text = "";
        string realPrompt = "";
        
        foreach (string line in lines)
        {
            if (line.StartsWith("TEXTE:"))
                text = line.Replace("TEXTE:", "").Trim();
            else if (line.StartsWith("PROMPT:"))
                realPrompt = line.Replace("PROMPT:", "").Trim();
        }
        
        ui.generatedText.text = text;
        ui.userPromptInput.text = "";
        ui.feedbackText.text = $"Exercice {exerciseCount + 1}/{MAX_EXERCISES}\nQuel prompt penses-tu avoir utilisé pour générer ce texte ?";
        ui.submitButton.interactable = true;
        ui.nextButton.interactable = false;
        
        PlayerPrefs.SetString("RealPrompt", realPrompt);
        PlayerPrefs.Save();
    }
    
    void SubmitAnswer()
    {
        string userPrompt = ui.userPromptInput.text.Trim();
        string realPrompt = PlayerPrefs.GetString("RealPrompt", "");
        
        if (string.IsNullOrEmpty(userPrompt))
        {
            ui.feedbackText.text = "Tape ton idée de prompt !";
            return;
        }
        
        string job = CurrentJob;
        string prompt = $@"
Mini-jeu 'Devine le prompt' pour un collaborateur {job} chez Orange.

OBJECTIF:
Tu dois comparer deux prompts:
- PROMPT RÉEL: le prompt exact qui a servi à générer le texte.
- PROMPT UTILISATEUR: ce que l'utilisateur pense être le prompt.

TEXTE GÉNÉRÉ: {ui.generatedText.text}
PROMPT RÉEL: {realPrompt}
PROMPT UTILISATEUR: {userPrompt}

RÈGLES IMPORTANTES:
- Si le PROMPT UTILISATEUR est très court (moins de 5 caractères) ou visiblement sans rapport (ex: 'a', 'test', 'ok'), la proximité doit être 0%.
- Ne sois PAS gentil: si le sens est loin du prompt réel, mets une faible proximité.
- Ne juge que sur la similarité de sens entre les deux prompts.

Format EXACT de ta réponse:
1. Proximité: [un nombre entre 0 et 100]%
2. Explication: [1 à 2 phrases claires expliquant la similarité ou non]
3. Conseil: [1 phrase pour mieux formuler un prompt la prochaine fois]
";

        ui.submitButton.interactable = false;
        ui.nextButton.interactable = false;
        ui.feedbackText.text = "IA analyse ta proposition...";

        StartCoroutine(CallIA(prompt, response => {
            ui.feedbackText.text = response;

            exerciseCount++;
            PlayerPrefs.SetInt(DPCountKey, exerciseCount);
            PlayerPrefs.Save();

            if (exerciseCount >= MAX_EXERCISES)
            {
                EndGame();
            }
            else
            {
                ui.nextButton.interactable = true;
                ui.feedbackText.text += "\n\nClique sur 'Suivant' pour un nouvel exercice.";
            }
        }));
    }

    void OnNextClicked()
    {
        if (exerciseCount < MAX_EXERCISES)
            GenerateTextAndRealPrompt();
        else
            EndGame();
    }

    void EndGame()
    {
        ui.generatedText.text = "C'est tout pour aujourd'hui !";
        ui.feedbackText.text = "Tu as terminé tous les exercices de ce mini-jeu.\nReviens demain pour continuer à t'entraîner.";
        ui.submitButton.interactable = false;
        ui.nextButton.interactable = false;
        ui.userPromptInput.interactable = false;
    }

    IEnumerator CallIA(string prompt, System.Action<string> callback)
    {
        ui.feedbackText.text = "IA réfléchit...";
        
        ChatRequest request = new ChatRequest { 
            message = prompt,
            metier = CurrentJob 
        };
        
        string json = JsonUtility.ToJson(request);
        byte[] body = Encoding.UTF8.GetBytes(json);
        using (UnityWebRequest req = new UnityWebRequest(apiURL, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            
            yield return req.SendWebRequest();
            
            if (req.result == UnityWebRequest.Result.Success)
            {
                ChatResponse response = JsonUtility.FromJson<ChatResponse>(req.downloadHandler.text);
                callback(response.content);
            }
            else
            {
                ui.feedbackText.text = $"Erreur {req.responseCode}";
            }
        }
    }
}
