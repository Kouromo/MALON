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
}