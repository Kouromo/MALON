using UnityEngine;
using TMPro;

public class UserInfoDisplay : MonoBehaviour
{
    [Header("Références UI")]
    public TMP_Text firstNameText;
    public TMP_Text lastNameText;
    public TMP_Text jobText;
    public TMP_Text vraiFauxDaysText;
    public TMP_Text devinePromptDaysText;
    public TMP_Text creerPromptDaysText;

    void Start()
    {
        string firstName = PlayerPrefs.GetString("User_FirstName", "");
        string lastName  = PlayerPrefs.GetString("User_LastName", "");
        string job       = PlayerPrefs.GetString("User_Position", "");

        if (firstNameText != null)
            firstNameText.text = firstName;

        if (lastNameText != null)
            lastNameText.text = lastName;

        if (jobText != null)
            jobText.text = job;
        
        int vfDays = PlayerPrefs.GetInt("VF_DaysCompleted", 0);
        int dpDays = PlayerPrefs.GetInt("DP_DaysCompleted", 0);
        int cpDays = PlayerPrefs.GetInt("CP_DaysCompleted", 0);

        if (vraiFauxDaysText != null)
            vraiFauxDaysText.text = $"Vrai/Faux : {vfDays}";
        if (devinePromptDaysText != null)
            devinePromptDaysText.text = $"Devine le prompt : {dpDays}";
        if (creerPromptDaysText != null)
            creerPromptDaysText.text = $"Créer un prompt : {cpDays}";
    }
}
