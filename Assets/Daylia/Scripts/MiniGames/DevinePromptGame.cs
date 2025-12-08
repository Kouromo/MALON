using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Text;

public class DevinePromptGame : MonoBehaviour 
{
    [Header("UI")]
    public TextMeshProUGUI generatedText;     // Texte généré par IA
    public TMP_InputField userPromptInput;    // Prompt deviné par utilisateur
    public Button submitButton;
    public TextMeshProUGUI feedbackText;
    public Button backToMenuButton;
    
    private string apiURL = "http://localhost:5001/api/rag/chat";
    
    void Start() 
    {
        submitButton.onClick.AddListener(SubmitAnswer);
        backToMenuButton.onClick.AddListener(() => SceneManager.LoadScene("Scene_Menu"));
        GenerateTextAndRealPrompt();
    }
    
    void GenerateTextAndRealPrompt()
    {
        string job = GlobalGameState.Job;
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
        
        generatedText.text = text;
        userPromptInput.text = "";
        feedbackText.text = "Quel prompt penses-tu avoir utilisé pour générer ce texte ?";
        submitButton.interactable = true;
        
        // Stocke le vrai prompt (invisible)
        PlayerPrefs.SetString("RealPrompt", realPrompt);
        PlayerPrefs.Save();
    }
    
    void SubmitAnswer()
    {
        string userPrompt = userPromptInput.text.Trim();
        string realPrompt = PlayerPrefs.GetString("RealPrompt", "");
        
        if (string.IsNullOrEmpty(userPrompt))
        {
            feedbackText.text = "Tape ton idée de prompt !";
            return;
        }
        
        string job = GlobalGameState.Job;
        string prompt = $@"
    Mini-jeu 'Devine le prompt' pour un collaborateur {job} chez Orange.

    OBJECTIF:
    Tu dois comparer deux prompts:
    - PROMPT RÉEL: le prompt exact qui a servi à générer le texte.
    - PROMPT UTILISATEUR: ce que l'utilisateur pense être le prompt.

    TEXTE GÉNÉRÉ: {generatedText.text}
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

        submitButton.interactable = false;
        StartCoroutine(CallIA(prompt, response => {
            feedbackText.text = response;
            Invoke(nameof(GenerateTextAndRealPrompt), 5f); // Nouvelle dans 5s
        }));
    }

    
    IEnumerator CallIA(string prompt, System.Action<string> callback)
    {
        feedbackText.text = "🤖 IA analyse...";
        
        // Même CallIA que VraiFaux (copie-colle)
        ChatRequest request = new ChatRequest { 
            message = prompt,
            metier = GlobalGameState.Job 
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
                feedbackText.text = $"❌ Erreur {req.responseCode}";
            }
        }
    }
}
