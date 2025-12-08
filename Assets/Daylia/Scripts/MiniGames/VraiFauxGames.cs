using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;
using TMPro;
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

public class VraiFauxGames : MonoBehaviour 
{
    [Header("UI")]
    public TextMeshProUGUI questionText;
    public TMP_InputField answerInput;
    public Button submitButton;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI scoreText;
    
    private string apiURL = "http://localhost:5001/api/rag/chat";
    private int score = 0;
    
    void Start() 
    {
        submitButton.onClick.AddListener(SubmitAnswer);
        GenerateQuestion();
    }
    
    void GenerateQuestion()
    {
        string prompt = "Génère 1 question VRAI/FAUX pour serious game Orange vendeur.\nFormat EXACT:\nQUESTION: [question]?\nRien d'autre.";
        
        StartCoroutine(CallIA(prompt, response => {
            questionText.text = response;
            answerInput.text = "";
            feedbackText.text = "";
            submitButton.interactable = true;
        }));
    }
    
    void SubmitAnswer()
    {
        string question = questionText.text;
        string userAnswer = answerInput.text.ToUpper();
        
        string prompt = $"QUESTION: {question}\nRÉPONSE: {userAnswer}\n\nFEEDBACK (2 phrases):\nVRAI/FAUX + explication vendeur Orange.\nTon positif.";
        
        StartCoroutine(CallIA(prompt, response => {
            feedbackText.text = response;
            score += 10;
            scoreText.text = "Score: " + score;
            Invoke(nameof(GenerateQuestion), 3f);
        }));
    }
    
    IEnumerator CallIA(string prompt, System.Action<string> callback)
    {
        feedbackText.text = "IA réfléchit...";
        submitButton.interactable = false;
        
        ChatRequest request = new ChatRequest { 
            message = prompt,
            metier = "Vendeur" 
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
            Debug.Log("RAW: " + req.downloadHandler.text);
            
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
