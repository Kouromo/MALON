using UnityEngine;

public class ThemeButton : MonoBehaviour
{
    // Clé PlayerPrefs utilisée pour stocker le mode (0, 1 ou 2)
    private const string ThemeKey = "CurrentThemeMode"; 

    public void CycleAndSaveTheme()
    {
        // 1. Lire la valeur actuelle (0 par défaut si non trouvée)
        // 0 = Sombre, 1 = Bleu, 2 = Clair
        int currentMode = PlayerPrefs.GetInt(ThemeKey, 0); 

        // 2. Calculer le prochain mode (la modulo assure que 2 + 1 = 0)
        int nextMode = (currentMode + 1) % 3;

        // 3. Sauvegarder le nouveau mode
        PlayerPrefs.SetInt(ThemeKey, nextMode);
        PlayerPrefs.Save();

        Debug.Log($"Nouveau mode sauvegardé : {nextMode}.");

        // 4. Appliquer immédiatement le nouveau thème via le ThemeManager
        ThemeInitializer initializer = FindObjectOfType<ThemeInitializer>();
        if (initializer != null)
        {
            initializer.ApplyCurrentTheme();
        }
    }
}