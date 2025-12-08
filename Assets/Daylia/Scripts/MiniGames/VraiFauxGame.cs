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

public class VraiFauxGame : MonoBehaviour  // ← Renommé
{
    [Header("UI")]
    public TextMeshProUGUI questionText;
    public Button vraiButton;      // ← NOUVEAU
    public Button fauxButton;      // ← NOUVEAU
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI scoreText;
    public Button backToMenuButton;
    
    private string apiURL = "http://localhost:5001/api/rag/chat";
    private int score = 0;
    private int questionCount = 0;
    private const int MAX_QUESTIONS = 4;
    
    void Start() 
    {
        // Récupère infos globales
        Debug.Log($"Vrai/Faux: {GlobalGameState.FirstName} ({GlobalGameState.Job})");
        
        vraiButton.onClick.AddListener(() => SubmitAnswer("VRAI"));
        fauxButton.onClick.AddListener(() => SubmitAnswer("FAUX"));
        backToMenuButton.onClick.AddListener(() => SceneManager.LoadScene("Scene_Menu"));
        
        scoreText.text = "Score: 0";
        GenerateQuestion();
    }
    
    void GenerateQuestion()
    {
        if (questionCount >= MAX_QUESTIONS)
        {
            EndGame();
            return;
        }
        
        string job = GlobalGameState.Job;
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


        StartCoroutine(CallIA(prompt, response => {
            questionText.text = response;
            feedbackText.text = $"Question {questionCount + 1}/{MAX_QUESTIONS}";
            vraiButton.interactable = true;
            fauxButton.interactable = true;
        }));
    }
    
    void SubmitAnswer(string userAnswer)
    {
        vraiButton.interactable = false;
        fauxButton.interactable = false;
        
        string question = questionText.text;
        string job = GlobalGameState.Job;
        
        string prompt = $@"Serious game Orange {job}.
QUESTION: {question}
RÉPONSE: {userAnswer}

FEEDBACK PÉDAGOGIQUE (2 phrases):
1. VRAI ou FAUX ? 
2. Explication simple + utilité IA pour {job}.
Ton: positif, rassurant.";

        StartCoroutine(CallIA(prompt, response => {
            feedbackText.text = response;
            score += 10;
            scoreText.text = $"Score: {score}";
            questionCount++;
            
            Invoke(nameof(GenerateQuestion), 3f);
        }));
    }
    
    void EndGame()
    {
        questionText.text = "Session terminée !";
        feedbackText.text = $"Score final: {score}/{MAX_QUESTIONS * 10}\n" +
                           $"Reviens demain pour de nouveaux défis !";
        vraiButton.gameObject.SetActive(false);
        fauxButton.gameObject.SetActive(false);
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
