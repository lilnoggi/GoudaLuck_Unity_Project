using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// A dynamic presentation component that smoothly elevates and oscillates UI elements upon focus.
/// Natively implements Unity Event System interfaces to guarantee identical visual feedback
/// whether the player is navigating via PC Mouse (Pointer) or Steam Deck Gamepad (Select).
/// </summary>

public class HoverAnimationUI : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Animation Settings")]
    [Tooltip("The total vertical distance the UI element travels when highlighted.")]
    [SerializeField] private float _hoverHeight = 30f;  // How high the card lifts
    [Tooltip("The interpolation speed dictating how quickly the element snaps to its hover height.")]
    [SerializeField] private float _liftSpeed = 10f;    // How fast it snaps up
    [Tooltip("The frequency of the continous sine-wave oscillation.")]
    [SerializeField] private float _bobSpeed = 2f;      // How fast it bobs up and down
    [Tooltip("The amplitude (vertical stretch) of the sine-wave oscillation.")]
    [SerializeField] private float _bobAmount = 2f;     // How far it bobs

    // --- STATE TRACKING ---
    private RectTransform _rectTransform;
    private float _originalY;
    private float _targetY;
    private bool _isSelected = false;

    // ==============================================================================================================

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        // SPATIAL PARTITIONINGL: Because this script is attatched to a child "Visual Layer"
        // rather than the parent "Layout Layer", the anchored Y position will safely default to 0.
        _originalY = _rectTransform.anchoredPosition.y;
        _targetY = _originalY;
    }

    void Update()
    {
        float currentTarget = _targetY;

        // --- OSCILLATION MATHS ---
        if (_isSelected)
        {
            // UnscaledTime is strictly required because the global timeScale is set to 0 during the Upgrade Phase.
            // Mathf.Sin generates a smooth, continous wave between -1 and 1.
            currentTarget += Mathf.Sin(Time.unscaledTime * _bobSpeed) * _bobAmount;
        }

        // --- INTERPOLATION ---
        // Smoothly glide the UI element towards the dynamically calculating target height.
        float newY = Mathf.Lerp(_rectTransform.anchoredPosition.y, currentTarget, Time.unscaledDeltaTime * _liftSpeed);

        _rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x, newY);
    }

    // ============================================================================================
    // ============================== --- EVENT SYSTEM TRIGGERS ---  ==============================
    // ============================================================================================

    // --- CONTROLLER / KEYBOARD NAVIGATION ---
    public void OnSelect(BaseEventData eventData) => SetHoverState(true);
    public void OnDeselect(BaseEventData eventData) => SetHoverState(false);

    // --- MOUSE HOVER ---
    public void OnPointerEnter(PointerEventData eventData) => SetHoverState(true);
    public void OnPointerExit(PointerEventData eventData) => SetHoverState(false);

    // ============================================================================================

    /// <summary>
    /// Centralised helper method to handle state transitions and audio feedback.
    /// </summary>
    private void SetHoverState(bool isHovering)
    {
        _isSelected = isHovering;

        if (isHovering)
        {
            _targetY = _originalY + _hoverHeight;  // Set the target height higher
            AudioManager.Instance.PlayHoverButtonSound(); // Play hover sound effect
        }
        else
        {
            _targetY = _originalY;  // Reset target height to original
        }
    }
}
