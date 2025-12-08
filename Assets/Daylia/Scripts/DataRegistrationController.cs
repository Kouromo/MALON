using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class RegistrationController : MonoBehaviour
{
    // Liez ces champs dans l'Inspecteur Unity à vos Input Fields
    public TMP_InputField inputFirstName;
    public TMP_InputField inputLastName;
    public TMP_InputField inputEmail;
    public TMP_InputField inputPassword;
    public TMP_InputField inputPosition;

    // Cette fonction sera appelée lorsque l'utilisateur cliquera sur le bouton "S'inscrire"
    public void OnRegisterButtonClicked()
    {
        // 1. Récupération des données (Validation et hachage à ajouter ici !)
        string firstName = inputFirstName.text;
        string lastName = inputLastName.text;
        string email = inputEmail.text;
        string passwordHash = inputPassword.text; 
        string position = inputPosition.text;

        // 2. Créer l'objet UserProfile
        UserProfile newProfile = new UserProfile(
            firstName, 
            lastName, 
            email, 
            passwordHash, 
            position
        );

        // 3. Sauvegarder et préparer la session via le DataManager
        DataManager.Instance.SaveProfile(newProfile);
        DataManager.Instance.SetSessionData(newProfile); 

        // 4. Passer à la scène suivante
        Debug.Log("Inscription réussie pour : " + email);
        SceneManager.LoadScene("Menu");
    }
}