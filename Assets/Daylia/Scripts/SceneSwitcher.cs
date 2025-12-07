using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    // Nous utiliserons une chaîne de caractères pour stocker le nom de la scène
    [SerializeField] public string sceneName;

    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            // Charge la scène en utilisant le nom
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Aucune scène assignée au SceneSwitcher!");
        }
    }

    public void LoadSceneIfLoggedIn(string sceneName)
    {
        // Vérifie si l'utilisateur est connecté
        if (AuthManager.isLoggedIn)
        {
            // Si oui, charge la scène
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            // Sinon, l'empêche de charger la scène
            Debug.LogWarning("Connexion requise pour charger cette scène!");
        }
    }
}