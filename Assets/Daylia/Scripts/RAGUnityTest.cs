using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class OrangeIAGame : MonoBehaviour 
{
    [Header("UI")]
    public TMP_InputField questionField;
    public Button sendButton;
    public TextMeshProUGUI answerText;
    public TMP_Dropdown jobDropdown;
    
    private string apiURL = "http://localhost:5001/api/test/ask";
    
    void Start() 
{
        sendButton.onClick.AddListener(OnSendButton);
        
        jobDropdown.ClearOptions();
        jobDropdown.AddOptions(new List<string> { 
            "Vendeur", "Technicien", "Manager" 
        });
        jobDropdown.value = 0;
        jobDropdown.captionText.text = "Vendeur";
        jobDropdown.RefreshShownValue();
    }
    
    void OnSendButton() 
    {
        string job = jobDropdown.options[jobDropdown.value].text;
        StartCoroutine(AskOrangeIA(questionField.text, job));
    }
    
    IEnumerator AskOrangeIA(string question, string job) 
    {
        answerText.text = $"IA réfléchit pour {job}...";
        
        string jsonData = $"{{\"query\":\"{question}\", \"metier\":\"{job}\"}}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        using (UnityWebRequest request = new UnityWebRequest(apiURL, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                
                // EXTRAIT SEULEMENT "answer" du JSON
                Match match = Regex.Match(responseText, @"""answer""\s*:\s*""([^""]+)""");
                
                string cleanAnswer = "Réponse IA";
                if (match.Success)
                {
                    cleanAnswer = match.Groups[1].Value;
                }
                
                answerText.text = cleanAnswer;
            }
            else
            {
                answerText.text = "Erreur réseau";
            }
        }
    }
}
