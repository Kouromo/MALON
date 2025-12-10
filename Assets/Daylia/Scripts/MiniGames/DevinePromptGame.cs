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

    private const string DPCompletedDaysKey = "DP_DaysCompleted";

    private string apiURL = "http://localhost:5001/api/rag/chat";

    string CurrentJob => PlayerPrefs.GetString("User_Position", "Vendeur");

    private int exerciseCount = 0;
    private const int MAX_EXERCISES = 2;

    // Version simple actuelle (globale machine)
    private const string DPCountKey = "DP_ExercisesDone";

    // Version future (par utilisateur + par jour) – À ACTIVER PLUS TARD
    // private string dpCountKeyPerUser;
    // private string dpDateKeyPerUser;

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

        // ---------- VERSION TEST (globale) ----------
        // Reset global pour tes tests (à retirer plus tard)
        PlayerPrefs.DeleteKey(DPCountKey);
        PlayerPrefs.Save();
        exerciseCount = PlayerPrefs.GetInt(DPCountKey, 0);

        // ---------- VERSION FUTURE (par user + date) ----------
        // string email = PlayerPrefs.GetString("User_Email", "unknown");
        // string today = System.DateTime.UtcNow.ToString("yyyyMMdd");
        //
        // dpCountKeyPerUser = $"DP_ExercisesDone_{email}";
        // dpDateKeyPerUser  = $"DP_LastDate_{email}";
        //
        // string lastDate = PlayerPrefs.GetString(dpDateKeyPerUser, "");
        // if (lastDate != today)
        // {
        //     exerciseCount = 0;
        //     PlayerPrefs.SetInt(dpCountKeyPerUser, 0);
        //     PlayerPrefs.SetString(dpDateKeyPerUser, today);
        //     PlayerPrefs.Save();
        // }
        // else
        // {
        //     exerciseCount = PlayerPrefs.GetInt(dpCountKeyPerUser, 0);
        // }

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

        RÔLE :
        - Tu dois d'abord INVENTER un PROMPT RÉEL qu'un collaborateur {job} pourrait effectivement taper pour être aidé par l'IA dans son travail.
        - Ensuite, tu génères un TEXTE GÉNÉRÉ (2 à 4 phrases) qui est la réponse de l'IA à ce prompt.
        - Le texte doit être réaliste, concret, crédible, et clairement exploitable dans une situation métier {job}.

        CONTRAINTES :
        - Ne parle pas de 'serious game', de 'jeu', ni des coulisses du jeu (ne dis pas que c'est un exercice ou un mini-jeu).
        - Situe toujours le contexte dans la vie professionnelle (client, réunion, intervention, management, analyse, etc.).
        - Varier les types de situations : relation client, suivi de dossier, préparation de rendez-vous, gestion d'équipe, analyse de données, rédaction d'email, etc.
        - Le PROMPT RÉEL doit être formulé comme un vrai prompt complet, pas seulement quelques mots-clés.

        FORMAT EXACT DE TA RÉPONSE (rien d'autre, pas de texte avant ou après) :
        TEXTE: [le texte généré par l'IA pour le collaborateur]
        PROMPT: [le prompt réel écrit par le collaborateur pour obtenir ce texte]
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

        TON RÔLE :
        Tu dois comparer deux prompts :
        - PROMPT RÉEL : le prompt exact qui a servi à générer le texte.
        - PROMPT UTILISATEUR : ce que l'utilisateur pense être le prompt.

        TEXTE GÉNÉRÉ (réponse de l'IA au prompt réel) :
        {ui.generatedText.text}

        PROMPT RÉEL :
        {realPrompt}

        PROMPT UTILISATEUR :
        {userPrompt}

        RÈGLES IMPORTANTES :
        - Si le PROMPT UTILISATEUR est très court (moins de 5 caractères) ou clairement hors sujet (ex : 'a', 'test', 'ok'), la proximité doit être 0%.
        - Ne sois PAS gentil : si le sens est loin du prompt réel, la proximité doit être faible.
        - La proximité doit refléter uniquement la similarité de sens entre les deux prompts (pas la qualité d'écriture).
        - Un score élevé (80% et plus) doit être réservé aux prompts vraiment très proches dans l’intention et les détails.

        FORMAT EXACT DE TA RÉPONSE (rien d'autre, pas de texte avant ou après) :
        1. Proximité: [un nombre ENTIER entre 0 et 100]%
        2. Explication: [1 à 2 phrases claires expliquant en quoi les deux prompts se ressemblent ou diffèrent]
        3. Conseil: [1 phrase pour aider l'utilisateur à écrire un prompt plus précis ou plus proche du prompt réel la prochaine fois]
        ";

        ui.submitButton.interactable = false;
        ui.nextButton.interactable = false;
        ui.feedbackText.text = "IA analyse ta proposition...";

        StartCoroutine(CallIA(prompt, response => {
            ui.feedbackText.text = response;

            exerciseCount++;

            // ---------- SAUVEGARDE ACTUELLE (globale) ----------
            PlayerPrefs.SetInt(DPCountKey, exerciseCount);
            PlayerPrefs.Save();

            // ---------- SAUVEGARDE FUTURE (par user + date) ----------
            // string email = PlayerPrefs.GetString("User_Email", "unknown");
            // string today = System.DateTime.UtcNow.ToString("yyyyMMdd");
            // dpCountKeyPerUser = $"DP_ExercisesDone_{email}";
            // dpDateKeyPerUser  = $"DP_LastDate_{email}";
            //
            // PlayerPrefs.SetInt(dpCountKeyPerUser, exerciseCount);
            // PlayerPrefs.SetString(dpDateKeyPerUser, today);
            // PlayerPrefs.Save();

            if (exerciseCount >= MAX_EXERCISES)
            {
                int completed = PlayerPrefs.GetInt(DPCompletedDaysKey, 0);
                completed++;
                PlayerPrefs.SetInt(DPCompletedDaysKey, completed);
                PlayerPrefs.Save();
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
