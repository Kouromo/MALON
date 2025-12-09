using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class DataLoginController : MonoBehaviour
{

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
            SceneManager.LoadScene("Menu");
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
