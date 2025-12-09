using UnityEngine;

public class ThemeInitializer : MonoBehaviour
{
    private const string ThemeKey = "CurrentThemeMode"; // Clé utilisée par le bouton

    [Header("Assignation des GameObjects Locaux")]
    [Tooltip("Le GameObject Empty qui contient les éléments du thème Sombre")]
    [SerializeField] private GameObject darkThemeGO;
    [Tooltip("Le GameObject Empty qui contient les éléments du thème Bleu")]
    [SerializeField] private GameObject blueThemeGO;
    [Tooltip("Le GameObject Empty qui contient les éléments du thème Clair")]
    [SerializeField] private GameObject lightThemeGO;


    void Start()
    {
        // Applique le thème dès que la scène est chargée
        ApplyCurrentTheme();
    }

    // Rendu public pour que le bouton puisse l'appeler pour une mise à jour immédiate
    public void ApplyCurrentTheme()
    {
        // 1. Lire la valeur sauvegardée (par défaut 0 = Sombre)
        // 0 = Sombre, 1 = Bleu, 2 = Clair
        int currentMode = PlayerPrefs.GetInt(ThemeKey, 0);

        // Désactiver tous les thèmes
        // Les vérifications 'if (go != null)' sont omises ici pour la concision, 
        // mais assurez-vous de toujours assigner les GOs dans l'Inspecteur.
        darkThemeGO.SetActive(false);
        blueThemeGO.SetActive(false);
        lightThemeGO.SetActive(false);

        // Activer le thème sélectionné
        switch (currentMode)
        {
            case 0: // Mode Sombre
                darkThemeGO.SetActive(true);
                break;
            case 1: // Mode Bleu
                blueThemeGO.SetActive(true);
                break;
            case 2: // Mode Clair
                lightThemeGO.SetActive(true);
                break;
            default:
                // Fallback de sécurité
                darkThemeGO.SetActive(true);
                break;
        }
    }
}