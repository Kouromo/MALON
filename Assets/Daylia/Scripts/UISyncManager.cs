using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UISyncManager : MonoBehaviour
{
    // --- CLASSE POUR DÉFINIR CE QUI DOIT ÊTRE SYNCHRONISÉ ---
    // [System.Serializable] permet d'afficher la classe dans l'Inspecteur.
    [System.Serializable]
    public class SynchronizedTextInput
    {
        public TMP_InputField inputField;
    }

    [Header("A. Synchronisation des Champs de Texte")]
    public SynchronizedTextInput[] darkTextInputs;
    public SynchronizedTextInput[] blueTextInputs;
    public SynchronizedTextInput[] lightTextInputs;

    [System.Serializable]
    public class SynchronizedSimpleText
    {
        public TextMeshProUGUI simpleText;
    }

    [Header("C. Synchronisation du Texte Simple")]
    public SynchronizedSimpleText[] darkSimpleTexts;
    public SynchronizedSimpleText[] blueSimpleTexts;
    public SynchronizedSimpleText[] lightSimpleTexts;

    [System.Serializable]
    public class SynchronizedButtonSelection
    {
        [Tooltip("La liste des 4 boutons dans ce thème. Le premier est l'index 0, etc.")]
        public GameObject[] selectionButtons; 
    }

    [System.Serializable]
    public class ButtonAnswerData
    {
        [Tooltip("Texte affiché sur le bouton")]
        public string answerText;
        [Tooltip("Indique si c'est la bonne réponse")]
        public bool isCorrectAnswer;
    }

    [System.Serializable]
    public class SynchronizedButtonAnswers
    {
        [Tooltip("Les données des 4 réponses pour ce thème")]
        public ButtonAnswerData[] answers = new ButtonAnswerData[4];
    }
    
    [Header("B. Synchronisation des Sélections de Bouton")]
    [Tooltip("Chaque élément contient les 4 boutons de sélection de couleur (ex: B1, B2, B3, B4) pour le thème")]
    public SynchronizedButtonSelection[] darkButtonSets;
    public SynchronizedButtonSelection[] blueButtonSets;
    public SynchronizedButtonSelection[] lightButtonSets;

    [Header("D. Synchronisation des Réponses de Bouton")]
    [Tooltip("Les données des réponses (texte et correctness) pour chaque ensemble de boutons")]
    public SynchronizedButtonAnswers[] darkButtonAnswers;
    public SynchronizedButtonAnswers[] blueButtonAnswers;
    public SynchronizedButtonAnswers[] lightButtonAnswers;
    
    private const string ThemeKey = "CurrentThemeMode";
    private const string ButtonSelectionKey = "SelectedButtonIndex_"; // Clé pour PlayerPrefs (on ajoutera l'index de la liste)

    // --- Fonction de Synchronisation ---
    public void SyncAllInputs()
    {
        // 1. Déterminer quel ensemble d'éléments est actuellement visible
        int currentMode = PlayerPrefs.GetInt(ThemeKey, 0); 
        SynchronizedTextInput[] activeElements;

        switch (currentMode)
        {
            case 0: activeElements = darkTextInputs; break;
            case 1: activeElements = blueTextInputs; break;
            case 2: activeElements = lightTextInputs; break;
            default: return;
        }

        // Vérification de sécurité: assurez-vous que les listes ont la même taille.
        if (darkTextInputs.Length != blueTextInputs.Length || darkTextInputs.Length != lightTextInputs.Length)
        {
            Debug.LogError("Les listes d'éléments de thème DOIVENT avoir la même taille pour la synchronisation!");
            return;
        }

        // 2. Parcourir et synchroniser les valeurs
        for (int i = 0; i < activeElements.Length; i++)
        {
            // Récupérer la valeur du champ ACTIF
            string activeValue = activeElements[i].inputField.text;

            // Écrire cette valeur dans les champs INACTIFS
            // Ceci garantit que la donnée est toujours répliquée dans tous les thèmes
            
            // Synchronisation pour le thème Sombre
            if (darkTextInputs[i].inputField != null)
            {
                darkTextInputs[i].inputField.text = activeValue;
            }
            
            // Synchronisation pour le thème Bleu
            if (blueTextInputs[i].inputField != null)
            {
                blueTextInputs[i].inputField.text = activeValue;
            }
            
            // Synchronisation pour le thème Clair
            if (lightTextInputs[i].inputField != null)
            {
                lightTextInputs[i].inputField.text = activeValue;
            }
            
            // Note : L'écriture sur le champ actif n'est pas nécessaire mais n'est pas nuisible.
        }

        Debug.Log($"Synchronisation de {activeElements.Length} éléments effectuée.");
    }

    public void SyncAllSimpleTexts()
    {
        // 1. Déterminer quel ensemble d'éléments est actuellement visible
        int currentMode = PlayerPrefs.GetInt(ThemeKey, 0); 

        // Vérification de sécurité: assurez-vous que les listes ont la même taille.
        if (darkSimpleTexts.Length != blueSimpleTexts.Length || darkSimpleTexts.Length != lightSimpleTexts.Length)
        {
            Debug.LogError("Les listes d'éléments de thème DOIVENT avoir la même taille pour la synchronisation!");
            return;
        }

        // 2. Parcourir et synchroniser les valeurs
        for (int i = 0; i < darkSimpleTexts.Length; i++)
        {
            string valueToSync = "";
            
            // Récupérer la valeur du thème ACTIF
            switch (currentMode)
            {
                case 0: 
                    if (darkSimpleTexts[i].simpleText != null)
                        valueToSync = darkSimpleTexts[i].simpleText.text;
                    break;
                case 1: 
                    if (blueSimpleTexts[i].simpleText != null)
                        valueToSync = blueSimpleTexts[i].simpleText.text;
                    break;
                case 2: 
                    if (lightSimpleTexts[i].simpleText != null)
                        valueToSync = lightSimpleTexts[i].simpleText.text;
                    break;
            }

            // Écrire cette valeur dans TOUS les thèmes (y compris l'actif, pour la cohérence)
            if (darkSimpleTexts[i].simpleText != null)
            {
                darkSimpleTexts[i].simpleText.text = valueToSync;
            }
            
            if (blueSimpleTexts[i].simpleText != null)
            {
                blueSimpleTexts[i].simpleText.text = valueToSync;
            }
            
            if (lightSimpleTexts[i].simpleText != null)
            {
                lightSimpleTexts[i].simpleText.text = valueToSync;
            }
        }

        Debug.Log($"Synchronisation de {darkSimpleTexts.Length} éléments effectuée.");
    }

    public void SyncAllButtonAnswers()
    {
        // 1. Déterminer quel ensemble d'éléments est actuellement visible
        int currentMode = PlayerPrefs.GetInt(ThemeKey, 0);

        // Vérification de sécurité: assurez-vous que les listes ont la même taille.
        if (darkButtonAnswers.Length != blueButtonAnswers.Length || darkButtonAnswers.Length != lightButtonAnswers.Length)
        {
            Debug.LogError("Les listes de réponses de thème DOIVENT avoir la même taille pour la synchronisation!");
            return;
        }

        // 2. Parcourir et synchroniser les valeurs
        for (int i = 0; i < darkButtonAnswers.Length; i++)
        {
            // Récupérer les données du thème ACTIF
            SynchronizedButtonAnswers sourceAnswers = null;
            
            switch (currentMode)
            {
                case 0: sourceAnswers = darkButtonAnswers[i]; break;
                case 1: sourceAnswers = blueButtonAnswers[i]; break;
                case 2: sourceAnswers = lightButtonAnswers[i]; break;
            }

            if (sourceAnswers == null || sourceAnswers.answers.Length != 4)
                continue;

            // Synchroniser vers TOUS les thèmes
            SyncButtonAnswersForSet(i, sourceAnswers);
        }

        Debug.Log($"Synchronisation de {darkButtonAnswers.Length} ensembles de réponses effectuée.");
    }

    private void SyncButtonAnswersForSet(int setIndex, SynchronizedButtonAnswers sourceAnswers)
    {
        // Copier les données source vers tous les thèmes
        for (int i = 0; i < 4; i++)
        {
            if (sourceAnswers.answers[i] == null)
                continue;

            // Synchroniser vers Dark
            if (darkButtonAnswers[setIndex].answers[i] != null)
            {
                darkButtonAnswers[setIndex].answers[i].answerText = sourceAnswers.answers[i].answerText;
                darkButtonAnswers[setIndex].answers[i].isCorrectAnswer = sourceAnswers.answers[i].isCorrectAnswer;
            }

            // Synchroniser vers Blue
            if (blueButtonAnswers[setIndex].answers[i] != null)
            {
                blueButtonAnswers[setIndex].answers[i].answerText = sourceAnswers.answers[i].answerText;
                blueButtonAnswers[setIndex].answers[i].isCorrectAnswer = sourceAnswers.answers[i].isCorrectAnswer;
            }

            // Synchroniser vers Light
            if (lightButtonAnswers[setIndex].answers[i] != null)
            {
                lightButtonAnswers[setIndex].answers[i].answerText = sourceAnswers.answers[i].answerText;
                lightButtonAnswers[setIndex].answers[i].isCorrectAnswer = sourceAnswers.answers[i].isCorrectAnswer;
            }
        }
    }
    
    // Fonction appelée par les 4 boutons de sélection
    public void SelectAndSyncButton(int listIndex, int selectedIndex)
    {
        // 1. Définir la clé pour cette sélection spécifique (ex: "SelectedButtonIndex_0" pour le premier ensemble)
        string key = ButtonSelectionKey + listIndex;
        
        // 2. Sauvegarder l'index du bouton sélectionné
        PlayerPrefs.SetInt(key, selectedIndex);
        PlayerPrefs.Save();
        
        // 3. Mettre à jour l'affichage de tous les thèmes dans la scène actuelle
        // On appelle une méthode qui gère l'application visuelle pour tous les thèmes
        ApplySelectionState(listIndex, selectedIndex);
    }
    
    // Fonction qui met à jour l'état visuel de TOUS les ensembles de boutons
    private void ApplySelectionState(int listIndex, int selectedIndex)
    {
        // On utilise la même couleur pour simuler l'état sélectionné
        Color selectedColor = Color.yellow; 
        Color normalColor = Color.white; 
        
        // Récupérer les ensembles de boutons qui doivent être synchronisés à cet index
        GameObject[] darkSet = darkButtonSets[listIndex].selectionButtons;
        GameObject[] blueSet = blueButtonSets[listIndex].selectionButtons;
        GameObject[] lightSet = lightButtonSets[listIndex].selectionButtons;
        
        // Liste de tous les ensembles à mettre à jour
        GameObject[][] allSets = { darkSet, blueSet, lightSet };
        
        foreach (var buttonSet in allSets)
        {
            if (buttonSet.Length != 4) continue; // Sûreté

            for (int i = 0; i < buttonSet.Length; i++)
            {
                // Changer la couleur du composant Image du bouton
                Image buttonImage = buttonSet[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    // Si l'index correspond au bouton sélectionné, on le met en jaune
                    buttonImage.color = (i == selectedIndex) ? selectedColor : normalColor;
                }
            }
        }
    }

    // Fonction d'Initialisation des Boutons au démarrage de la scène
    public void InitializeButtonSelections()
    {
        // Parcourir tous les ensembles de boutons (par exemple, si vous avez 2 ensembles de 4 boutons)
        for (int i = 0; i < darkButtonSets.Length; i++)
        {
            string key = ButtonSelectionKey + i;
            // Charger l'index sauvegardé (0 par défaut)
            int savedIndex = PlayerPrefs.GetInt(key, 0); 
            
            // Appliquer l'état visuel
            ApplySelectionState(i, savedIndex);
        }
    }
}