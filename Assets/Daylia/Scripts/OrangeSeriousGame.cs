using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class OrangeSeriousGame : MonoBehaviour 
{
    [Header("UI")]
    public TMP_InputField answerInput;
    public Button submitButton;
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI scoreText;
    public TMP_Dropdown jobDropdown;
    
    [Header("Game State")]
    public int currentQuestion = 0;
    public int dailyScore = 0;
    private List<QuestionData> currentQuestions;
    
    private string apiURL = "http://localhost:5001";
    
    [System.Serializable]
    public class QuestionData 
    {
        public string q;
        public string reponse;
    }
    
    [System.Serializable]
    public class GameRequest 
    {
        public string metier;
    }
    
    [System.Serializable]
    public class GameResponse 
    {
        public GameData game;
        public string status;
    }
    
    [System.Serializable]
    public class GameData 
    {
        public string type;
        public List<QuestionData> questions;
        public int progress;
    }
    
    void Start() 
    {
        submitButton.onClick.AddListener(SubmitAnswer);
        StartNewGame();
    }
    
    void StartNewGame() 
    {
        string job = jobDropdown.options[jobDropdown.value].text;
        StartCoroutine(LoadDailyGame(job));
    }
    
    IEnumerator LoadDailyGame(string job) 
    {
        GameRequest request = new GameRequest { metier = job };
        string json = JsonUtility.ToJson(request);
        
        using (UnityWebRequest www = UnityWebRequest.Post(apiURL + "/api/game/generate", json, "application/json"))
        {
            yield return www.SendWebRequest();
            
            if (www.result == UnityWebRequest.Result.Success)
            {
                GameResponse gameResponse = JsonUtility.FromJson<GameResponse>(www.downloadHandler.text);
                currentQuestions = gameResponse.game.questions;
                currentQuestion = 0;
                ShowCurrentQuestion();
            }
        }
    }
    
    void ShowCurrentQuestion() 
    {
        if (currentQuestion < currentQuestions.Count)
        {
            questionText.text = currentQuestions[currentQuestion].q;
            answerInput.text = "";
            feedbackText.text = "";
            submitButton.interactable = true;
        }
        else
        {
            questionText.text = "Jeu terminé ! Score: " + dailyScore;
        }
    }
    
    public void SubmitAnswer() 
    {
        if (currentQuestion < currentQuestions.Count)
        {
            StartCoroutine(CheckAnswer(currentQuestions[currentQuestion]));
        }
    }
    
    IEnumerator CheckAnswer(QuestionData question) 
    {
        submitButton.interactable = false;
        
        // Simule correction (remplace par API plus tard)
        bool isCorrect = answerInput.text.ToUpper().Contains(question.reponse[0]);
        string feedback = isCorrect ? "Bravo ! L'IA booste ta productivité !" : "Prochaine fois: " + question.reponse;
        dailyScore += isCorrect ? 10 : 0;
        
        feedbackText.text = feedback;
        scoreText.text = "Score: " + dailyScore;
        
        yield return new WaitForSeconds(2f);
        
        currentQuestion++;
        ShowCurrentQuestion();
    }
}
