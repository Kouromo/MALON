using UnityEngine;
using UnityEngine.UI; // Nécessaire pour le composant Button

public class OptionsUIController : MonoBehaviour
{
    // === Boutons Gérés par l'état de Connexion ===
    [Header("Boutons Connexion/Profil")]
    public GameObject connectButton;
    public GameObject registerButton;

    public GameObject logoutButton;
    public GameObject profitButton;


    // === Boutons de Menu à Désactiver hors ligne ===
    [Header("Boutons de Navigation (Désactivation onClick)")]
    public Button menuButton1; 
    public Button menuButton2;  

    void Start()
    {
        Debug.Log("Etat de connexion" + PlayerPrefs.GetInt("IsLoggedIn", 0));
        UpdateButtonsState();
    }

    public void UpdateButtonsState()
    {
        // Lire l'état de la session (0 = déconnecté, 1 = connecté)
        bool isLoggedIn = PlayerPrefs.GetInt("IsLoggedIn", 0) == 1;
        
        if (connectButton != null)
        {
            // Si CONNECTÉ: Masquer 'Se connecter'
            connectButton.SetActive(!isLoggedIn); 
        }
        if (registerButton != null)
        {
            // Si CONNECTÉ: Masquer 'S'inscrire'
            registerButton.SetActive(!isLoggedIn); 
        }   

        if (logoutButton != null)
        {
            // Si CONNECTÉ: Afficher 'Se déconnecter'
            logoutButton.SetActive(isLoggedIn); 
        }
        if (profitButton != null)
        {
            // Si CONNECTÉ: Afficher 'Profil'
            profitButton.SetActive(isLoggedIn); 
        }

        bool canInteract = isLoggedIn;

        if (menuButton1 != null)
        {
            menuButton1.interactable = canInteract;
            // Optionnel : Changer la couleur si non interactif pour indiquer l'état
            // Vous pouvez aussi changer la couleur de l'image du bouton ici.
        }

        if (menuButton2 != null)
        {
            menuButton2.interactable = canInteract;
        }

        Debug.Log("État des boutons mis à jour. Connecté : " + isLoggedIn);
    }
}