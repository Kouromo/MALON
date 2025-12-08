using UnityEngine;
using UnityEngine.UI;

public class AccordionSansReset : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button headerButton;        // Bouton pour ouvrir/fermer
    [SerializeField] private GameObject scrollView;      // Bloc du texte
    [SerializeField] private ScrollRect scrollRect;      // ScrollRect du texte
    [SerializeField] private RectTransform arrowIcon;    // Icône flèche (optionnelle)

    [Header("Options")]
    [SerializeField] private bool startOpened = false;   // L'accordéon démarre-t-il ouvert ?

    // Évite que Unity réinitialise le scroll pendant les mises à jour de layout
    private bool hasJustOpened = false;

    private bool _isOpen = false;

    private void Awake()
    {
        // Applique l’état initial
        _isOpen = startOpened;
        scrollView.SetActive(_isOpen);

        // Met la flèche au bon angle dès le départ
        UpdateArrow();

        // On écoute le clic
        headerButton.onClick.AddListener(ToggleAccordion);
    }

    private void Update()
    {
        // Empêche le scroll de remonter juste après ouverture
        if (hasJustOpened)
        {
            // Sécurise la position en haut uniquement une fois
            if (scrollRect != null)
                scrollRect.verticalNormalizedPosition = 1f;

            hasJustOpened = false;
        }
    }

    /// <summary>
    /// Ouverture / Fermeture du bloc
    /// </summary>
    private void ToggleAccordion()
    {
        _isOpen = !_isOpen;
        scrollView.SetActive(_isOpen);

        // On vient juste d'ouvrir → positionner le scroll en haut UNE SEULE fois
        if (_isOpen)
            hasJustOpened = true;

        UpdateArrow();
    }

    /// <summary>
    /// Rotation de la flèche pour feedback visuel
    /// </summary>
    private void UpdateArrow()
    {
        if (arrowIcon == null) return;

        float angle = _isOpen ? 180f : 0f;
        arrowIcon.localRotation = Quaternion.Euler(0, 0, angle);
    }
}
