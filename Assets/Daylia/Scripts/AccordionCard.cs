using UnityEngine;

public class AccordionCard : MonoBehaviour
{
    [Header("Glisse ici l'objet Scroll_View")]
    public GameObject contentScrollArea;

    [Header("Glisse ici l'image de la flèche (optionnel)")]
    public RectTransform arrowIcon;

    private bool isOpen = true; // Ouvert par défaut ?

    public void ToggleCard()
    {
        isOpen = !isOpen;

        // 1. Activer ou Désactiver le contenu
        contentScrollArea.SetActive(isOpen);

        // 2. Faire tourner la flèche (Visuel)
        if (arrowIcon != null)
        {
            if (isOpen)
                arrowIcon.localRotation = Quaternion.Euler(0, 0, 0); // Flèche vers le haut
            else
                arrowIcon.localRotation = Quaternion.Euler(0, 0, 180); // Flèche vers le bas
        }
    }
}
