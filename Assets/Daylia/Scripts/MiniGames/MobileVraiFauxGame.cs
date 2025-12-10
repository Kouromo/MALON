using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Text;

[System.Serializable]
public class MobileVraiFauxUISet
{
    public TextMeshProUGUI questionText;
    public Button vraiButton;
    public Button fauxButton;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI scoreText;
    public Button nextButton;

    [Header("Mobile")]
    public GameObject feedbackPanel;
}

public class MobileVraiFauxGame : MonoBehaviour
{
    [Header("UI Light")]
    public MobileVraiFauxUISet lightUI;

    [Header("UI Dark")]
    public MobileVraiFauxUISet darkUI;

    [Header("UI Blue")]
    public MobileVraiFauxUISet blueUI;

    private MobileVraiFauxUISet ui;

    private string apiURL = "http://localhost:5001/api/rag/chat";
    private int questionCount = 0;
    private const int MAX_QUESTIONS = 4;

    // Version simple actuelle (globale machine)
    private const string VFCountKey = "VF_QuestionsDone";

    // Version future (par utilisateur + par jour) – À ACTIVER PLUS TARD
    // private string vfCountKeyPerUser;
    // private string vfDateKeyPerUser;

    private const string ThemeKey = "CurrentThemeMode"; // 0=Dark,1=Blue,2=Light

    string CurrentJob => PlayerPrefs.GetString("User_Position", "Vendeur");
    
    void Awake()
    {
        int theme = PlayerPrefs.GetInt(ThemeKey, 0);
        switch (theme)
        {
            case 2: ui = lightUI; break;   // Light
            case 1: ui = blueUI;  break;   // Blue
            case 0:
            default: ui = darkUI; break;   // Dark par défaut
        }
    }

    void Start() 
    {
        PlayerPrefs.DeleteKey(VFCountKey);
        PlayerPrefs.Save();
        questionCount = PlayerPrefs.GetInt(VFCountKey, 0);

        string firstName = PlayerPrefs.GetString("User_FirstName", "");
        string lastName  = PlayerPrefs.GetString("User_LastName", "");
        Debug.Log($"Vrai/Faux: {firstName} {lastName} ({CurrentJob})");

        ui.vraiButton.onClick.AddListener(() => SubmitAnswer("VRAI"));
        ui.fauxButton.onClick.AddListener(() => SubmitAnswer("FAUX"));
        ui.nextButton.onClick.AddListener(OnNextButtonClicked);
            
        ui.scoreText.text = $"Progression : {questionCount}/{MAX_QUESTIONS}";

        // Au début aucun feedback affiché
        if (ui.feedbackPanel != null) ui.feedbackPanel.SetActive(false);

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
        Tu conçois une affirmation pour un quiz VRAI/FAUX destiné à un collaborateur Orange dont le métier est : {job}.

        OBJECTIF PÉDAGOGIQUE :
        - Illustrer de manière concrète comment l'IA peut aider dans son travail quotidien (sans le remplacer).
        - Tester sa compréhension d'un bon usage de l'IA dans des situations métier variées.

        CONTRAINTES :
        - Ne PAS utiliser les mots 'serious game', 'jeu', 'Orange Manager', 'Manager', 'Vendeur', 'Technicien'.
        - La situation doit être réaliste et liée au métier {job} : client, intervention technique, réclamation, vente, analyse de données, préparation de réunion, management d'équipe, etc.
        - Varier les thématiques d'une question à l'autre (ne pas toujours parler du même type de situation).
        - L'affirmation doit être clairement VRAIE ou clairement FAUSSE : il ne doit pas y avoir de réponse ambiguë.
        - Ne pas donner la réponse, uniquement l'affirmation.

        FORMAT EXACT DE TA RÉPONSE (rien d'autre, pas de texte avant ou après) :
        QUESTION: [une seule affirmation, claire, en français]
        ";

        ui.vraiButton.interactable = false;
        ui.fauxButton.interactable = false;
        ui.nextButton.interactable = false;
        ui.feedbackText.text = "IA prépare une nouvelle question...";

        // Masquer le panneau de feedback sur mobile pendant la génération
        if (ui.feedbackPanel != null) ui.feedbackPanel.SetActive(false);

        StartCoroutine(CallIA(prompt, response => {
            ui.questionText.text = response;
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
        Tu joues le rôle d'un formateur qui corrige une réponse à une question de type VRAI/FAUX pour un collaborateur Orange dont le métier est : {job}.

        QUESTION POSÉE AU COLLABORATEUR :
        {question}

        RÉPONSE DONNÉE PAR LE COLLABORATEUR :
        {userAnswer}   (il n'a pu répondre que 'VRAI' ou 'FAUX')

        TON RÔLE :
        1. Déterminer objectivement si l'affirmation de la QUESTION est VRAIE ou FAUSSE dans un contexte métier réaliste pour {job}.
        2. Comparer ta conclusion à la réponse du collaborateur.
        3. Dire clairement s'il a BON ou FAUX, sans essayer de lui donner raison quand il a tort.
        4. Donner un feedback pédagogique court sur le bon usage de l'IA dans cette situation.

        CONTRAINTES IMPORTANTES :
        - Tu dois choisir UNE seule vérité pour la question : l'affirmation est soit VRAIE, soit FAUSSE (pas de 'ça dépend').
        - Si la réponse du collaborateur ne correspond pas à la vérité que tu as déterminée, le résultat doit être FAUX.
        - Ne pas reformuler la question, ne pas inventer un troisième choix.
        - Ne pas être complaisant : si sa réponse est incorrecte, tu dois dire clairement qu'il s'est trompé.

        FORMAT EXACT DE TA RÉPONSE (rien d'autre, sans texte avant ou après) :
        1. Vérité: [VRAI ou FAUX]  // ta propre évaluation de l'affirmation de la question
        2. Résultat: [BON ou FAUX] // BON seulement si la réponse du collaborateur correspond à la Vérité
        3. Explication: [2 à 3 phrases simples expliquant pourquoi l'affirmation est VRAIE ou FAUSSE]
        4. Conseil: [1 phrase expliquant comment l'IA peut bien l'aider dans cette situation métier]
        ";

        StartCoroutine(CallIA(prompt, response => {
            ui.feedbackText.text = response;
            questionCount++;
            ui.scoreText.text = $"Progression : {questionCount}/{MAX_QUESTIONS}";

            
            PlayerPrefs.SetInt(VFCountKey, questionCount);
            PlayerPrefs.Save();

            // Afficher le panneau de feedback sur mobile
            if (ui.feedbackPanel != null) ui.feedbackPanel.SetActive(true);

            if (questionCount >= MAX_QUESTIONS)
            {
                ui.feedbackText.text += $"\n\nTu as terminé les {MAX_QUESTIONS} questions du jour.\nReviens demain pour continuer !";
                ui.vraiButton.interactable = false;
                ui.fauxButton.interactable = false;
                ui.nextButton.interactable = false;
                ui.questionText.text = "C'est tout pour aujourd'hui !";
            }
            else
            {
                ui.nextButton.interactable = true;
                ui.feedbackText.text += "\n\nClique sur la flêche pour passer à la prochaine question.";
            }
        }));
    }

    void OnNextButtonClicked()
    {
        // Cacher le panneau de feedback et passer à la question suivante
        if (ui.feedbackPanel != null) ui.feedbackPanel.SetActive(false);

        if (questionCount < MAX_QUESTIONS)
            GenerateQuestion();
        else
            EndGame();
    }

    void EndGame()
    {
        ui.questionText.text = "C'est tout pour aujourd'hui !";
        ui.feedbackText.text = $"Tu as répondu aux {MAX_QUESTIONS} questions du jour.\nReviens demain pour de nouveaux défis !";

        ui.vraiButton.interactable = false;
        ui.fauxButton.interactable = false;
        ui.nextButton.interactable = false;
        ui.scoreText.text = $"Progression : {MAX_QUESTIONS}/{MAX_QUESTIONS}";
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
