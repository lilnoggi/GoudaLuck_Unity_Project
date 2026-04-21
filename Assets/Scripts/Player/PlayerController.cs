using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// === ** REFACTORED SCRIPT ** ===
/// <summary>
/// The core controller for the player character, heavily refactored for handheld deployment.
/// Utilises the Event-Driven Unity Input System (Command Pattern) to read hardware inputs
/// without relying on inefficient per-frame polling. Handles physics-based movement,
/// aiming abstraction (Gamepad vs. Mouse), and ultimate ability execution.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("The base movement speed of the player.")]
    [SerializeField] private float _moveSpeed = 5f;

    [Header("Dash Settings")]
    [Tooltip("The burst velocity applied during a dash.")]
    [SerializeField] private float _dashSpeed = 20f;
    [Tooltip("How long the dash velocity and i-frames last in seconds.")]
    [SerializeField] private float _dashDuration = 0.2f;
    [Tooltip("The required cooldown time before the player can dash again.")]
    [SerializeField] private float _dashCooldown = 1f;
    [SerializeField] private TrailRenderer _dashTrail;

    // --- DASH STATE TRACKING ---
    private bool _isDashing = false;
    private float _nextDashTime = 0f;
    private Vector3 _dashDirection;

    /// <summary>
    /// A public getter for the UI to read the dash cooldown percentage.
    /// Returns 1 (full) if ready, or a fraction (0.0 to 0.99) if currently on cooldown.
    /// </summary>
    public float DashCooldownRatio
    {
        get
        {
            if (Time.time >= _nextDashTime) return 1f;  // Ready to dash

            // Calculate the remaining time as a percentage
            float timeRemaining = _nextDashTime - Time.time;
            return 1f - (timeRemaining / _dashCooldown);
        }
    }

    [Header("Aiming Settings")]
    [Tooltip("The world-space crosshair transform.")]
    [SerializeField] private Transform _crosshair; 
    [Tooltip("The maximum distance the gamepad crosshair can project from the player.")]
    [SerializeField] private float _maxAimRadius = 8f; 
    [Tooltip("The interpolation speed for the crosshair to eliminate hardware jitter.")]
    [SerializeField] private float _crosshairSpeed = 25f;  

    [Header("Ultimate Settings")]
    [Tooltip("The prefab instantiated when the ultimate ability is triggered.")]
    [SerializeField] private GameObject _bigCheesePrefab;
    [SerializeField] private float _maxUltCharge = 100f;
    [Tooltip("The amount of ultimate charge passively gained per second.")]
    [SerializeField] private float _passiveChargeRate = 5f;  // Charges 5 points per second

    private float _currentUltCharge = 0f;

    // Public getter for the UI to read the ultimate charge percentage
    public float UltChargeRatio => _currentUltCharge / _maxUltCharge;
    
    // --- INPUT STATE VARIABLES ---
    // Stores the raw 2D input (X and Y) from the joystick or WASD keys.
    private Vector2 _inputVector;
    private Vector2 _aimVector;
    private Vector2 _mousePosition;      // Stores the physical screen position of the PC mouse

    private bool _isUsingMouse = false;  // Tracks which device is being used
    // A public "getter" so the UIManager can read this state safely
    public bool IsUsingMouse => _isUsingMouse; 
    
    // --- COMPONENT CACHING ---
    private Rigidbody _rb;
    private PlayerControls _controls;  // The auto-generated C# class from the Input System
    private Camera _mainCamera;        // The camera is needed to calculate mouse aiming
    private WeaponSystem _weaponSystem;
    private HealthSystem _healthSystem;
    private bool _isFiring;

    // ==============================================================================================================

    private void Awake()
    {
        InitialiseComponents();
        SubscribeToInputs();
    }

    // =========================================================================================
    // ======================== --- SETUP & HARDWARE SUBSCRIPTIONS ---  ========================
    // =========================================================================================
    // ===== Caches all required component dependencies during          ========================
    // ===== initialisation to avoid expensive runtime GetComponent     ========================
    // ===== calls.                                                     ========================
    // =========================================================================================
    private void InitialiseComponents()
    {
        _rb = GetComponent<Rigidbody>();
        _mainCamera = Camera.main;
        _weaponSystem = GetComponent<WeaponSystem>();
        _healthSystem = GetComponent<HealthSystem>();

        // Instantiate this as a new object because it is a generated C# class, not a MonoBehaviour component.
        _controls = new PlayerControls();
    }

    /// <summary>
    /// Acts as a central switchboard to connect all event-driven hardware inputs to their respective methods.
    /// </summary>
    private void SubscribeToInputs()
    {
        SetupMovementInput();
        SetupAimingInput();
        SetupActionInput();
    }

    private void SetupMovementInput()
    {
        // --- MOVEMENT INPUT ---
        /// <summary>
        /// This is the Event-Driven architecture.
        /// '+=' is used to "subscribe" to the Input System's events.
        /// When the Joystick is moved (performed), the system fires an event.
        /// 
        /// 'ctx' means "Context". It is a data package holding all the info about the input
        /// (e.g., how far the joystick is tilted).
        /// The '=>' (lambda) takes that context, reads its Vector2 value, and saves it to _inputVector.
        /// When the stick is released (canceled), the vector is reset to 0 so the player stops.
        /// 
        /// By subscribing directly to the Action events, the CPU only calculates input
        /// when the hardware state physically changes, bypassing empty per-frame polling loops.
        /// </summary>
        
        _controls.Player.Move.performed += ctx => _inputVector = ctx.ReadValue<Vector2>();
        _controls.Player.Move.canceled += ctx => _inputVector = Vector2.zero;
    }

    private void SetupAimingInput()
    {
        // --- GAMEPAD AIMING INPUT ---
        _controls.Player.Aim.performed += ctx =>
        {
            _aimVector = ctx.ReadValue<Vector2>();

            // STATE CHANGE: If transitioning from PC Mouse to Gamepad, hide the OS cursor
            if (_isUsingMouse)
            {
                _isUsingMouse = false;
                Cursor.visible = false;
            }
        };
        _controls.Player.Aim.canceled += ctx => _aimVector = Vector2.zero;

        // --- PC MOUSE AIMING ---
        _controls.Player.MousePosition.performed += ctx =>
        {
            _mousePosition = ctx.ReadValue<Vector2>();

            // STATE CHANGE: If transitioning from Gamepad to PC Mouse, reveal the OS cursor
            if (!_isUsingMouse)
            {
                _isUsingMouse = true;
                Cursor.visible = true;
            }
        };
    }

    private void SetupActionInput()
    {
        // --- FIRING ---
        // Listen for the Fire button being pressed and released BUT ignore if paused
        _controls.Player.Fire.performed += ctx =>
        {
            if (Time.timeScale > 0)  // DEFENSIVE CHECK: Ignore input if paused
            {
                _isFiring = true;
            }
        };
        _controls.Player.Fire.canceled += ctx => _isFiring = false;

        // === ACTIONS ===
        // --- RELOADING ---
        _controls.Player.Reload.performed += ctx =>
        {
            if (_weaponSystem != null) _weaponSystem.Reload();
        };
        // --- DASHING ---
        _controls.Player.Dash.performed += ctx => PerformDash();
        // --- ULTIMATE ---
        _controls.Player.Ultimate.performed += ctx => PerformUltimate();
        // --- WEAPON SWAP ---
        // Cycle Forward (R1 / E)
        _controls.Player.NextWeapon.performed += ctx =>
        {
            if (_weaponSystem != null)
            {
                _weaponSystem.CycleWeaponForward();
            }
        };
        // Cycle Backward (L1 / Q)
        _controls.Player.PreviousWeapon.performed += ctx =>
        {
            if (_weaponSystem != null)
            {
                _weaponSystem.CycleWeaponBackward();
            }
        };

        // --- PAUSE ---
        _controls.Player.Pause.performed += ctx =>
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.TogglePause();
            }
        };
    }

    // =========================================================================================
    // ============================= --- LIFECYCLE MANAGEMENT ---  =============================
    // =========================================================================================
    // =========================================================================================
    // --- ON ENABLED AND DISABLED ---                             ===========================================
    /// <summary>                                                  ==============================================
    /// The new Input System requires actions to be explicitly turned on and off.   =================================
    /// OnEnable runs when the script is turned on, allowing it to listen for button inputs.   ===========================
    /// OnDisable runs when the script is turned off (like if the player dies or pauses).   =================================
    /// Disabling prevents the script from trying to read input that don't exist, preventing memory leaks and crashes.   ====
    /// </summary>   ========================================================================================================
    // ======================================================================================================================

    private void OnEnable()
    {
        // Explicitly enable the Action Map
        _controls.Enable();
    }

    private void OnDisable()
    {
        // DEFENSIVE PROGRAMMING: Disable inputs when destroyed or disabled to prevent memory leaks and null reference callbacks
        _controls.Disable();
    }

    // ==========================================================================================
    // ================================ --- Core Loop Logic ---  ================================
    // ==========================================================================================
    // ==========================================================================================
    // --- MOVEMENT ---                                          =================================
    /// <summary>                                                ==================================
    /// Movement is handled in the Fixed Update because FixedUpdate runs on a reliable,   ===========
    /// fixed timer strictly meant for the Physics engine.                                ==============
    /// Moving Rigidbodies in the regular Update() loop causes the physics to calculate unevenly,   ====
    /// which leads to screen jittering (noted in TDD).                                         ========
    /// </summary> =====================================================================================
    /// // =============================================================================================

    private void FixedUpdate()
    {
        HandleMovement();
    }

    // --- AIMING ---
    // Goes in standard Update() because it handles visual rotation and UI, which should be frame-perfect.
    private void Update()
    {
        HandleAiming();

        // If the Fire button is held down, tell the Weapon System to fire!
        if (_isFiring && _weaponSystem != null)
        {
            _weaponSystem.FireWeapon();
        }

        // Passively charge the ultimate up to 100
        if (_currentUltCharge < _maxUltCharge)
        {
            _currentUltCharge += Time.deltaTime * _passiveChargeRate;
            _currentUltCharge = Mathf.Min(_currentUltCharge, _maxUltCharge);  // Clamp this so it doesn't exceed 100
        }
    }

    private void HandleMovement()
    {
        if (_isDashing)
        {
            // --- DASH MOVEMENT ---
            // Move very fast in the locked dash direction, ignoring the joystick temporarily
            _rb.MovePosition(_rb.position + _dashDirection * _dashSpeed * Time.fixedDeltaTime);
        }
        else
        {
            // --- STANDARD MOVEMENT ---
            // Convert the 2D input (X, Y) into 3D movement (X, 0, Z)
            Vector3 movement = new Vector3(_inputVector.x, 0f, _inputVector.y);

            // --- MOVING THE RIGIDBODY ---
            /// <summary>
            /// Time.fixedDeltaTime is used because it ensures the movement speed is entirely independent
            /// of the frame rate.
            /// Whether the Steam Deck is running at 30 FPS or 60 FPS, the player will cover
            /// the exact same physical distance in the game world per second.
            /// </summary>

            _rb.MovePosition(_rb.position + movement * _moveSpeed * Time.fixedDeltaTime);
        }
    }

    // =========================================================================================
    // ================================= --- AIMING LOGIC ---  =================================
    // =========================================================================================

    // --- AIMING ---
    private void HandleAiming()
    {
        if (_crosshair == null) return;

        // Hardware Abstraction Router
        if (_isUsingMouse)
        {
            HandleMouseAiming();
        }
        else
        {
            HandleGamepadAiming();
        }
    }

    // --- PC MOUSE AIMING ---
    private void HandleMouseAiming()
    {
        _crosshair.gameObject.SetActive(true);  // Mouse crosshair is always on

        // Create a mathmatical flat plane at the player's local Y position
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        // Shoot a ray from the camera through the mouse cursor on the screen
        Ray ray = _mainCamera.ScreenPointToRay(_mousePosition);

        // Calculate the intersection point between the ray and the flat plane
        if (groundPlane.Raycast(ray, out float hitDistance))
        {
            // Find the exact 3D point it hit
            Vector3 targetPoint = ray.GetPoint(hitDistance);

            // Interpolate crosshair position to smooth out variable framerates
            Vector3 desiredCrosshairPos = new Vector3(targetPoint.x, transform.position.y + 0.1f, targetPoint.z);
            _crosshair.position = Vector3.Lerp(_crosshair.position, desiredCrosshairPos, Time.deltaTime * _crosshairSpeed);

            // Calculate look rotation (locking the Y-axis to prevent pitching)
            Vector3 aimDirection = (targetPoint - transform.position).normalized;
            aimDirection.y = 0f;  

            if (aimDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(aimDirection);
                _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRotation, Time.deltaTime * 15f));
            }
        }
    }

    private void HandleGamepadAiming()
    {
        /// --- GAMEPAD AIMING ---
        // DEADZONE CHECK: Only calculate aim if the player is actually pushing the right stick
        if (_aimVector.sqrMagnitude > 0.01f)
        {
            _crosshair.gameObject.SetActive(true);

            // Convert the 2D joystick input into a 3D direction
            Vector3 aimDirection = new Vector3(_aimVector.x, 0f, _aimVector.y).normalized;

            // Scale the crosshair projection distance by the joystick's physical tilt 
            float currentRadius = _maxAimRadius * _aimVector.magnitude;
            Vector3 targetCrosshairPos = transform.position + (aimDirection * currentRadius) + (Vector3.up * 0.1f);

            // Interpolate the visual crosshair to mask hardware stick jitter
            _crosshair.position = Vector3.Lerp(_crosshair.position, targetCrosshairPos, Time.deltaTime * _crosshairSpeed);

            // Rotate the player to face the crosshair
            Quaternion targetRotation = Quaternion.LookRotation(aimDirection);
            _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRotation, Time.deltaTime * 15f));
        }
        else if (_crosshair != null)
        {
            // Auto-hide the crosshair when the joystick returns to center
            _crosshair.gameObject.SetActive(false);
        }
    }

    // ============================================================================================
    // ================================ --- SPECIAL ABILITIES ---  ================================
    // ============================================================================================

    // === DASHING ===
    private void PerformDash()
    {
        // SAFETY CHECK: Ignore input if the game is paused
        if (Time.timeScale == 0f) return;

        // Only dash if the cooldown is ready and not already dashing
        if (Time.time >= _nextDashTime && !_isDashing)
        {
            // Find out which way the player is currently pushing the movement stick/keys
            _dashDirection = new Vector3(_inputVector.x, 0f, _inputVector.y).normalized;

            // FALLBACK: Dash forward if no directional input is being supplied
            if (_dashDirection == Vector3.zero)
            {
                _dashDirection = transform.forward;
            }

            // Set the cooldown and start the dash
            _nextDashTime = Time.time + _dashCooldown;
            StartCoroutine(DashRoutine());
        }
    }

    private IEnumerator DashRoutine()
    {
        _isDashing = true;  // Turn on dash mode

        // Enable I-Frames
        if (_healthSystem != null)
        {
            _healthSystem.SetInvincible(true);
        }

        // Turn on the trail
        if (_dashTrail != null) _dashTrail.emitting = true;

        AudioManager.Instance.PlayDashSound();

        // Wait for a fraction of a second while the FixedUpdate handles the high-speed movement
        yield return new WaitForSeconds(_dashDuration);

        // Turn trail off
        if (_dashTrail != null) _dashTrail.emitting = false;

        // Turn I-Frames off
        if (_healthSystem != null)
        {
            _healthSystem.SetInvincible(false);
        }

        _isDashing = false;  // Turn off dash mode and return to normal movement
    }

    // --- ULTIMATE ABILITY ---
    private void PerformUltimate()
    {
        // SAFETY CHECK: Ignore input if the game is paused
        if (Time.timeScale == 0f) return;

        // Only fire if the meter is full
        if (_currentUltCharge >= _maxUltCharge && _bigCheesePrefab != null && _crosshair != null)
        {
            // Calculate a spot 20 units straight up into the air above the crosshair
            Vector3 dropPosition = new Vector3(_crosshair.position.x, 20f, _crosshair.position.z);

            // Spawn the cheese
            Instantiate(_bigCheesePrefab, dropPosition, Quaternion.identity);

            // Reset the meter
            _currentUltCharge = 0f;
        }
    }

    // ==========================================================================================
    // =============================== --- UPGRADE MODIFIERS ---  ===============================
    // ==========================================================================================
    public void IncreaseMoveSpeed(float amount)
    {
        _moveSpeed += amount;
    }

    public void DecreaseDashCooldown(float amount)
    {
        // MATHEMATICAL SAFETY: Prevent the cooldown from dropping below zero,
        // which would allow infinite dash spamming and break the game logic!
        _dashCooldown = Mathf.Max(0.1f, _dashCooldown - amount);
    }

    public void IncreaseUltChargeRate(float amount)
    {
        _passiveChargeRate += amount;
    }

    // =========================================================================================
    // =============================== --- EDITOR DEBUGGING ---  ===============================
    // =========================================================================================
    // --- Crosshair Radius --- 
    /// <summary>
    /// This draws a yellow circle in the Unity Editor scene view so you can
    /// easily see and adjust the maximum aiming radius without having to play the game.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        // Draws a wire sphere at the player's position using the max radius
        Gizmos.DrawWireSphere(transform.position, _maxAimRadius);
    }
}
