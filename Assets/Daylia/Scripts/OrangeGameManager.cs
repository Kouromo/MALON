using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class OrangeGameManager : MonoBehaviour 
{
    [Header("UI Écran Principal")]
    public TMP_Dropdown jobDropdown;
    public Button startButton;
    public TextMeshProUGUI welcomeText;
    
    [Header("Navigation Mini-Jeux")]
    public Button vraiFauxButton;
    public Button devinePromptButton;
    public Button creerPromptButton;
    
    [Header("Game State")]
    public string currentJob = "Vendeur";
    private bool gameStarted = false;
    
    void Start() 
    {
        SetupUI();
        SetupButtons();
    }
    
    void SetupUI()
    {
        // Dropdown métiers
        jobDropdown.ClearOptions();
        jobDropdown.AddOptions(new List<string> { 
            "Vendeur", "Technicien", "Manager" 
        });
        jobDropdown.value = 0;
        jobDropdown.RefreshShownValue();
        
        // Texte bienvenue
        welcomeText.text = "Bienvenue dans ton Serious Game IA Orange !\n\nChoisis ton métier et commence ta session quotidienne.";
        
        // Désactive boutons mini-jeux au début
        vraiFauxButton.interactable = false;
        devinePromptButton.interactable = false;
        creerPromptButton.interactable = false;
    }
    
    void SetupButtons()
    {
        startButton.onClick.AddListener(StartGame);
        
        vraiFauxButton.onClick.AddListener(() => LoadMiniGame("VraiFaux"));
        devinePromptButton.onClick.AddListener(() => LoadMiniGame("DevinePrompt"));
        creerPromptButton.onClick.AddListener(() => LoadMiniGame("CreerPrompt"));
    }
    
    void StartGame()
    {
        currentJob = jobDropdown.options[jobDropdown.value].text;
        gameStarted = true;
        
        // UI feedback
        startButton.interactable = false;
        jobDropdown.interactable = false;
        welcomeText.text = $"Bonjour {currentJob} Orange !\n\nChoisis ton mini-jeu du jour :";
        
        // Active boutons mini-jeux
        vraiFauxButton.interactable = true;
        devinePromptButton.interactable = true;
        creerPromptButton.interactable = true;
    }
    
    void LoadMiniGame(string miniGameType)
    {
        // TODO: Charger la scène mini-jeu correspondante
        Debug.Log($"Charger mini-jeu: {miniGameType} pour métier: {currentJob}");
        
        // Passer currentJob au mini-jeu via PlayerPrefs ou singleton
        PlayerPrefs.SetString("CurrentJob", currentJob);
        PlayerPrefs.SetString("MiniGameType", miniGameType);
        PlayerPrefs.Save();
        
        // Exemple: UnityEngine.SceneManagement.SceneManager.LoadScene(miniGameType);
    }
    
    // Méthodes utilitaires pour tous les mini-jeux
    public string GetCurrentJob() => currentJob;
    public bool IsGameStarted() => gameStarted;
}
