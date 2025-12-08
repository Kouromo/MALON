using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LoginScene : MonoBehaviour 
{
    [Header("UI")]
    public TMP_InputField firstNameInput;
    public TMP_InputField lastNameInput;
    public TMP_Dropdown jobDropdown;
    public Button continueButton;
    public TextMeshProUGUI errorText;
    
    void Start() 
    {
        SetupDropdown();
        continueButton.onClick.AddListener(OnContinue);
        errorText.text = "";
    }
    
    void SetupDropdown()
    {
        jobDropdown.ClearOptions();
        jobDropdown.AddOptions(new List<string> { "Vendeur", "Technicien", "Manager" });
        jobDropdown.value = 0;
        jobDropdown.RefreshShownValue();
    }
    
    void OnContinue()
    {
        string firstName = firstNameInput.text.Trim();
        string lastName = lastNameInput.text.Trim();
        string job = jobDropdown.options[jobDropdown.value].text;
        
        if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
        {
            errorText.text = "Veuillez renseigner nom et prénom.";
            return;
        }
        
        // Sauvegarde globale
        GlobalGameState.FirstName = firstName;
        GlobalGameState.LastName = lastName;
        GlobalGameState.Job = job;
        
        Debug.Log($"Connexion: {firstName} {lastName} - {job}");
        SceneManager.LoadScene("Scene_Menu");
    }
}
