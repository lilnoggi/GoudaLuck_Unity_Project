using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Smoothly raises the UI element when selected/hovered and adds a subtle "bobbing" motion.
/// Implements Unity's Event System interfaces to automatically detect controller focus and mouse hover.
/// </summary>

public class HoverAnimationUI : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Animation Settings")]
    [SerializeField] private float _hoverHeight = 30f;  // How high the card lifts
    [SerializeField] private float _liftSpeed = 10f;    // How fast it snaps up
    [SerializeField] private float _bobSpeed = 2f;      // How fast it bobs up and down
    [SerializeField] private float _bobAmount = 2f;     // How far it bobs

    private RectTransform _rectTransform;
    private float _originalY;
    private float _targetY;
    private bool _isSelected = false;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        // Record the exact position the Horizontal Layout Group assigned this card
        _originalY = _rectTransform.anchoredPosition.y;
        _targetY = _originalY;
    }

    void Update()
    {
        float currentTarget = _targetY;

        // If the card is currently highlighted, add the sine-wave bobbing effect
        if (_isSelected)
        {
            currentTarget += Mathf.Sin(Time.unscaledTime * _bobSpeed) * _bobAmount;
        }

        // Smoothly glide the card towards its target height using Time.unscaledDeltaTime
        // !!! (Unscaled is critical because the game is paused during the upgrade screen) !!!
        float newY = Mathf.Lerp(_rectTransform.anchoredPosition.y, currentTarget, Time.unscaledDeltaTime * _liftSpeed);

        _rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x, newY);
    }

    // === EVENT SYSTEM TRIGGERS ===

    // --- CONTROLLER / KEYBOARD NAVIGATION ---
    public void OnSelect(BaseEventData eventData) => SetHoverState(true);
    public void OnDeselect(BaseEventData eventData) => SetHoverState(false);

    // --- MOUSE HOVER ---
    public void OnPointerEnter(PointerEventData eventData) => SetHoverState(true);
    public void OnPointerExit(PointerEventData eventData) => SetHoverState(false);

    // --- HELPER METHOD ---
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
