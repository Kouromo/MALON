using UnityEngine;
using UnityEngine.UI; // Pour utiliser les composants UI comme InputField/TMP_InputField
using TMPro; // Si vous utilisez TextMeshPro pour les champs de saisie
using UnityEngine.SceneManagement;

public class AuthManager : MonoBehaviour
{
    // --- Champs assignables dans l'Inspecteur (Drag & Drop) ---
    // Changez à InputField si vous n'utilisez pas TextMeshPro
    [Header("UI Fields")]
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private TMP_InputField passwordInputField; 

    [Header("UI Fields - Registration Only")]
    [SerializeField] private TMP_InputField nameInputField;   
    [SerializeField] private TMP_InputField firstNameInputField;
    [SerializeField] private TMP_InputField jobTitleInputField; 
    [Header("Game State")]
    public static bool isLoggedIn = false; // Le bool global accessible partout

    // --- Clés PlayerPrefs ---
    private const string EmailKey = "UserEmail";
    private const string PasswordKey = "UserPassword";
    private const string NameKey = "UserName";          
    private const string FirstNameKey = "UserFirstName";  
    private const string JobTitleKey = "UserJobTitle";    

    void Start()
    {
        // Initialiser l'état de connexion au démarrage
        if (PlayerPrefs.HasKey(EmailKey) && PlayerPrefs.HasKey(PasswordKey))
        {
            // S'il y a déjà des identifiants enregistrés, on est techniquement "enregistré"
            Debug.Log("Identifiants locaux trouvés. Prêt pour la connexion.");
        }
    }

    // Fonction appelée par le bouton d'INSCRIPTION (ou Enregistrer)
    public void RegisterUser()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;

        string name = nameInputField.text;
        string firstName = firstNameInputField.text;
        string jobTitle = jobTitleInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(name))
        {
            // Nous vérifions au moins l'email, le mot de passe et le nom.
            Debug.LogError("Veuillez remplir au moins l'email, le mot de passe et le nom.");
            return;
        }

        // 1. Sauvegarde des identifiants dans PlayerPrefs
        PlayerPrefs.SetString(EmailKey, email);
        PlayerPrefs.SetString(PasswordKey, password);
        PlayerPrefs.SetString(NameKey, name);
        PlayerPrefs.SetString(FirstNameKey, firstName);
        PlayerPrefs.SetString(JobTitleKey, jobTitle);
        PlayerPrefs.Save(); // Assure que les données sont écrites sur le disque

        Debug.Log("Inscription (simulation) réussie et données enregistrées localement!");
        LoginUser(); 
    }

    // Fonction appelée par le bouton de CONNEXION
    public void LoginUser()
    {
        string enteredEmail = emailInputField.text;
        string enteredPassword = passwordInputField.text;

        // 1. Vérification si des données existent
        if (!PlayerPrefs.HasKey(EmailKey) || !PlayerPrefs.HasKey(PasswordKey))
        {
            Debug.LogWarning("Aucun compte n'est enregistré localement. Veuillez vous inscrire d'abord.");
            isLoggedIn = false;
            return;
        }

        // 2. Récupération des données enregistrées
        string savedEmail = PlayerPrefs.GetString(EmailKey);
        string savedPassword = PlayerPrefs.GetString(PasswordKey);

        // 3. Comparaison
        if (enteredEmail == savedEmail && enteredPassword == savedPassword)
        {
            // Connexion réussie !
            isLoggedIn = true;
            Debug.Log("Connexion réussie! Bienvenue.");
            SceneManager.LoadScene("Menu");

            // Chargez les données de l'utilisateur pour une utilisation dans le jeu avec cette commande :
            // currentUserName = PlayerPrefs.GetString(FirstNameKey) + " " + PlayerPrefs.GetString(NameKey);

            // ICI vous pouvez appeler votre fonction de chargement de scène !
            // SceneManager.LoadScene("MainGameScene");

        }
        else
        {
            // Échec
            isLoggedIn = false;
            Debug.LogError("Erreur de connexion. Email ou mot de passe incorrect.");
        }
    }
}