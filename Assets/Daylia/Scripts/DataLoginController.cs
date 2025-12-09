using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class DataLoginController : MonoBehaviour
{

    [Header ("Mobile ?")]
    public bool isMobile = false;

    public TMP_InputField inputLoginEmail;
    public TMP_InputField inputLoginPassword;
    public TextMeshProUGUI errorMessageText;


    public void OnLoginButtonClicked()
    {
        // Réinitialiser le message d'erreur à chaque clic
        if (errorMessageText != null) 
        {
            errorMessageText.text = "";
        }

        string email = inputLoginEmail.text;
        string password = inputLoginPassword.text;

        // Le DataManager se charge de tout : charger le JSON, vérifier le mot de passe, 
        // et mettre les données en PlayerPrefs si la vérification est OK.
        if (DataManager.Instance.VerifyLogin(email, password))
        {
            Debug.Log("Connexion réussie !");
            // Load soit Menu soit Mobile Menu selon le cas
            if (isMobile)
            {
                Debug.Log("Chargement Mobile Menu");
                SceneManager.LoadScene("Mobile Menu");
            }
            else
            {
                SceneManager.LoadScene("Menu");
            }
        }
        else
        {
            Debug.LogError("Email ou mot de passe incorrect.");
            // Afficher un message d'erreur à l'utilisateur
            if (errorMessageText != null) 
            {
                errorMessageText.text = "Email ou mot de passe incorrect.";
            }
        }
    }
}
