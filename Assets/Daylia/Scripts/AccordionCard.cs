using UnityEngine;
using UnityEngine.UI;

public class AccordionCard : MonoBehaviour
{
    [Header("Glisse ici l'objet Scroll_View")]
    public GameObject contentScrollArea;

    [Header("Glisse ici l'image de la flèche (optionnel)")]
    public RectTransform arrowIcon;

    private bool isOpen = true; // Ouvert par défaut ?

    private void Start()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent as RectTransform);
    }

    public void ToggleCard()
    {
        isOpen = !isOpen;

        // 1. Activer ou Désactiver le contenu
        contentScrollArea.SetActive(isOpen);
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent as RectTransform);

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
