using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Text;

[System.Serializable]
public class ChatRequest 
{
    public string message;
    public string metier;
}

[System.Serializable]
public class ChatResponse 
{
    public string content;
    public string status;
}

[System.Serializable]
public class VraiFauxUISet
{
    public TextMeshProUGUI questionText;
    public Button vraiButton;
    public Button fauxButton;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI scoreText;
    public Button nextButton;
}

public class VraiFauxGame : MonoBehaviour
{
    [Header("UI Light")]
    public VraiFauxUISet lightUI;

    [Header("UI Dark")]
    public VraiFauxUISet darkUI;

    [Header("UI Blue")]
    public VraiFauxUISet blueUI;

    private VraiFauxUISet ui;  // le set utilisé selon le thème

    private string apiURL = "http://localhost:5001/api/rag/chat";
    private int score = 0;
    private int questionCount = 0;
    private const int MAX_QUESTIONS = 4;

    private const string VFCountKey = "VF_QuestionsDone";
    private const string ThemeKey = "CurrentThemeMode"; // 0=Dark,1=Blue,2=Light (comme dans UISyncManager)

    string CurrentJob => PlayerPrefs.GetString("User_Position", "Vendeur");
    
    void Awake()
    {
        int theme = PlayerPrefs.GetInt(ThemeKey, 0);
        switch (theme)
        {
            case 2: ui = lightUI; break;   // 2 = Light
            case 1: ui = blueUI;  break;   // 1 = Blue
            case 0:
            default: ui = darkUI; break;   // 0 = Dark par défaut
        }
    }

    void Start() 
    {
        string firstName = PlayerPrefs.GetString("User_FirstName", "");
        string lastName  = PlayerPrefs.GetString("User_LastName", "");
        Debug.Log($"Vrai/Faux: {firstName} {lastName} ({CurrentJob})");

        questionCount = PlayerPrefs.GetInt(VFCountKey, 0);

        ui.vraiButton.onClick.AddListener(() => SubmitAnswer("VRAI"));
        ui.fauxButton.onClick.AddListener(() => SubmitAnswer("FAUX"));
        ui.nextButton.onClick.AddListener(OnNextButtonClicked);
        
        ui.scoreText.text = "Score: 0";

        if (questionCount >= MAX_QUESTIONS)
            EndGame();
        else
            GenerateQuestion();
    }
    
    void GenerateQuestion()
    {
        if (questionCount >= MAX_QUESTIONS)
        {
            EndGame();
            return;
        }
        
        string job = CurrentJob;
        string prompt = $@"
Tu génères une question VRAI/FAUX pédagogique pour un collaborateur Orange au métier: {job}.
Objectif: lui montrer comment l'IA peut l'aider dans son travail au quotidien, sans le remplacer.

Contraintes:
- Ne PAS utiliser les mots 'serious game', 'Orange Manager', 'Manager', 'Vendeur', 'Technicien' ni parler de 'jeu'.
- La question doit parler d'une situation métier concrète (client, intervention, réunion, etc.).
- Une seule question, claire, en français.

Format EXACT de ta réponse (rien d'autre):
QUESTION: [ta question ici]?
";

        ui.vraiButton.interactable = false;
        ui.fauxButton.interactable = false;
        ui.nextButton.interactable = false;
        ui.feedbackText.text = "IA prépare une nouvelle question...";

        StartCoroutine(CallIA(prompt, response => {
            ui.questionText.text = response;
            ui.feedbackText.text = $"Question {questionCount + 1}/{MAX_QUESTIONS}\nRéponds par Vrai ou Faux.";
            ui.vraiButton.interactable = true;
            ui.fauxButton.interactable = true;
            ui.nextButton.interactable = false;
        }));
    }
    
    void SubmitAnswer(string userAnswer)
    {
        ui.vraiButton.interactable = false;
        ui.fauxButton.interactable = false;
        ui.nextButton.interactable = false;
        
        string question = ui.questionText.text;
        string job = CurrentJob;
        
        string prompt = $@"
Serious game Orange {job}.
QUESTION: {question}
RÉPONSE: {userAnswer}

FEEDBACK PÉDAGOGIQUE (2 phrases):
1. VRAI ou FAUX ? 
2. Explication simple + utilité IA pour {job}.
Ton: positif, rassurant.
";

        StartCoroutine(CallIA(prompt, response => {
            ui.feedbackText.text = response;
            score += 10;
            ui.scoreText.text = $"Score: {score}";
            
            questionCount++;
            PlayerPrefs.SetInt(VFCountKey, questionCount);
            PlayerPrefs.Save();

            if (questionCount >= MAX_QUESTIONS)
            {
                EndGame();
            }
            else
            {
                ui.nextButton.interactable = true;
                ui.feedbackText.text += "\n\nClique sur 'Suivant' pour passer à la prochaine question.";
            }
        }));
    }

    void OnNextButtonClicked()
    {
        if (questionCount < MAX_QUESTIONS)
            GenerateQuestion();
        else
            EndGame();
    }
    
    void EndGame()
    {
        ui.questionText.text = "C'est tout pour aujourd'hui !";
        ui.feedbackText.text = $"Score final: {score}/{MAX_QUESTIONS * 10}\n" +
                               $"Reviens demain pour de nouveaux défis !";

        ui.vraiButton.interactable = false;
        ui.fauxButton.interactable = false;
        ui.nextButton.interactable = false;
    }
    
    IEnumerator CallIA(string prompt, System.Action<string> callback)
    {
        ui.feedbackText.text = "IA réfléchit...";
        
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
