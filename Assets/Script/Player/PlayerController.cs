using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 10f;

    [Header("Dodge Settings")]
    public float dodgeSpeed = 10f; // Kecepatan saat dodge
    public float dodgeDuration = 0.3f; // Durasi dodge
    public float dodgeCooldown = 2f; // Cooldown antar dodge
    public int maxDodgeCount = 2; // Maksimum dodge
    private int currentDodgeCount;
    private float lastDodgeTime;
    private bool isDodging;

    [Header("Flip Settings")]
    public bool useMovementFlip = true;
    public bool allowWeaponFlip = true;

    [Header("Components")]
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private PlayerStats playerStats;
    private PlayerHealth playerHealth;

    // Input variables
    private Vector2 moveInput;
    private Vector2 currentVelocity;

    // Flip control
    private bool weaponControlsFlip = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        playerStats = GetComponent<PlayerStats>();
        playerHealth = GetComponent<PlayerHealth>();

        if (rb == null) Debug.LogWarning("Rigidbody2D not found on PlayerController!");
        if (spriteRenderer == null) Debug.LogWarning("SpriteRenderer not found on PlayerController!");
        if (animator == null) Debug.LogWarning("Animator not found on PlayerController!");
        if (playerStats == null) Debug.LogWarning("PlayerStats not found on PlayerController!");

        rb.gravityScale = 0f;
        rb.drag = 0f;

        // Initialize dodge
        currentDodgeCount = maxDodgeCount;
        maxDodgeCount = playerStats.GetDodgeCount();

        // Check if any weapon wants to control flip
        // CheckWeaponFlipControl();
    }

    void Update()
    {
        HandleInput();
        HandleDodge();
        HandleAnimation();

        if (!weaponControlsFlip && useMovementFlip)
        {
            HandleMovementFlip();
        }
    }

    void FixedUpdate()
    {
        if (!isDodging)
        {
            HandleMovement();
        }
    }

    void HandleInput()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;

        // Dodge input
        if (Input.GetKeyDown(KeyCode.Space) && currentDodgeCount > 0 && Time.time >= lastDodgeTime + dodgeCooldown)
        {
            StartDodge();
        }
    }

    void HandleMovement()
    {
        Vector2 targetVelocity = moveInput * moveSpeed;
        float accelRate = (moveInput != Vector2.zero) ? acceleration : deceleration;
        currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, accelRate * Time.fixedDeltaTime);
        rb.velocity = currentVelocity;
    }

    void HandleDodge()
    {
        // Reset dodge count after cooldown
        if (currentDodgeCount < maxDodgeCount && Time.time >= lastDodgeTime + dodgeCooldown)
        {
            currentDodgeCount = maxDodgeCount;
        }
    }

    void StartDodge()
    {
        if (moveInput == Vector2.zero) return; // No dodge if not moving
        isDodging = true;
        currentDodgeCount--;
        lastDodgeTime = Time.time;

        // Apply dodge velocity
        rb.velocity = moveInput * dodgeSpeed;

        // Set invincibility
        if (playerHealth != null)
        {
            playerHealth.SetInvincible(true);
        }

        // Trigger dodge animation if exists
        if (animator != null)
        {
            animator.SetTrigger("Dodge");
        }

        // End dodge after duration
        Invoke(nameof(EndDodge), dodgeDuration);
    }

    void EndDodge()
    {
        isDodging = false;
        rb.velocity = moveInput * moveSpeed; // Back to normal movement
        if (playerHealth != null)
        {
            playerHealth.SetInvincible(false);
        }
    }

    void HandleMovementFlip()
    {
        if (moveInput.x > 0)
            spriteRenderer.flipX = false;
        else if (moveInput.x < 0)
            spriteRenderer.flipX = true;
    }

    void HandleAnimation()
    {
        if (animator != null)
        {
            bool isMoving = moveInput != Vector2.zero && !isDodging;
            animator.SetBool("isRunning", isMoving);
        }
    }

    // void CheckWeaponFlipControl()
    // {
    //     MouseSliceWeapon[] sliceWeapons = GetComponentsInChildren<MouseSliceWeapon>();
    //     foreach (var weapon in sliceWeapons)
    //     {
    //         if (weapon.flipPlayerSprite)
    //         {
    //             weaponControlsFlip = true;
    //             break;
    //         }
    //     }
    // }

    public void SetWeaponFlipControl(bool enabled)
    {
        weaponControlsFlip = enabled;
    }

    public void SetMaxDodgeCount(int newCount)
    {
        maxDodgeCount = Mathf.Max(1, newCount);
        currentDodgeCount = Mathf.Min(currentDodgeCount, maxDodgeCount);
    }

    public Vector2 GetMoveDirection() => moveInput;
    public Vector3 GetPosition() => transform.position;
    public SpriteRenderer GetSpriteRenderer() => spriteRenderer;
}