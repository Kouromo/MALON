using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuScene : MonoBehaviour 
{
    [Header("UI")]
    public TextMeshProUGUI welcomeText;
    public Button vraiFauxButton;
    public Button devinePromptButton;
    public Button creerPromptButton;
    public Button logoutButton;
    
    void Start() 
    {
        SetupUI();
        SetupButtons();
    }
    
    void SetupUI()
    {
        welcomeText.text = $"Bonjour {GlobalGameState.FirstName} {GlobalGameState.LastName} !\n" +
                          $"({GlobalGameState.Job})\n\n" +
                          "Choisis ton mini-jeu du jour :";
    }
    
    void SetupButtons()
    {
        vraiFauxButton.onClick.AddListener(() => LoadMiniGame("Scene_VraiFaux"));
        devinePromptButton.onClick.AddListener(() => LoadMiniGame("Scene_DevinePrompt"));
        creerPromptButton.onClick.AddListener(() => LoadMiniGame("Scene_CreerPrompt"));
        logoutButton.onClick.AddListener(() => SceneManager.LoadScene("Scene_Login"));
    }
    
    void LoadMiniGame(string sceneName)
    {
        GlobalGameState.CurrentMiniGame = sceneName;
        Debug.Log($"🎮 Chargement: {sceneName} pour {GlobalGameState.Job}");
        SceneManager.LoadScene(sceneName);
    }
}
