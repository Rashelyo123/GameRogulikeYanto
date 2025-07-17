

using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; // Player transform
    public Vector3 offset = new Vector3(0, 0, -10); // Camera offset from player

    [Header("Follow Settings")]
    public float followSpeed = 5f; // How fast camera follows
    public float lookAheadDistance = 2f; // Distance to look ahead based on movement
    public float lookAheadSmoothing = 3f; // How smooth the look ahead is

    [Header("Smoothing")]
    public bool useSmoothDamping = true;
    public float smoothTime = 0.3f; // For SmoothDamp

    [Header("Boundaries (Optional)")]
    public bool useBoundaries = false;
    public float minX = -50f;
    public float maxX = 50f;
    public float minY = -50f;
    public float maxY = 50f;

    [Header("Camera Shake")]
    public bool enableCameraShake = true;
    public float shakeDecay = 2f;

    // Private variables
    private Vector3 velocity = Vector3.zero;
    private Vector3 lookAheadPosition;
    private Vector3 currentLookAhead;
    private PlayerController playerController;

    // Camera shake variables
    private float shakeIntensity = 0f;
    private float shakeTimer = 0f;
    private Vector3 shakeOffset;

    void Start()
    {
        // Auto-find player if not assigned
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                playerController = player.GetComponent<PlayerController>();
            }
        }

        // Set initial position
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Calculate look ahead based on player movement
        CalculateLookAhead();

        // Calculate target position
        Vector3 targetPosition = target.position + offset + currentLookAhead;

        // Apply boundaries if enabled
        if (useBoundaries)
        {
            targetPosition = ApplyBoundaries(targetPosition);
        }

        // Move camera towards target
        MoveCamera(targetPosition);

        // Apply camera shake if active
        ApplyCameraShake();
    }

    void CalculateLookAhead()
    {
        if (playerController != null)
        {
            // Get player movement direction
            Vector2 moveDirection = playerController.GetMoveDirection();

            // Calculate look ahead position
            lookAheadPosition = moveDirection * lookAheadDistance;

            // Smooth the look ahead transition
            currentLookAhead = Vector3.Lerp(currentLookAhead, lookAheadPosition,
                                          lookAheadSmoothing * Time.deltaTime);
        }
    }

    void MoveCamera(Vector3 targetPosition)
    {
        if (useSmoothDamping)
        {
            // Use SmoothDamp for very smooth movement
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition,
                                                  ref velocity, smoothTime);
        }
        else
        {
            // Use Lerp for more responsive movement
            transform.position = Vector3.Lerp(transform.position, targetPosition,
                                            followSpeed * Time.deltaTime);
        }
    }

    Vector3 ApplyBoundaries(Vector3 targetPosition)
    {
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
        return targetPosition;
    }

    void ApplyCameraShake()
    {
        if (!enableCameraShake) return;

        if (shakeTimer > 0)
        {
            // Generate random shake offset
            shakeOffset = Random.insideUnitSphere * shakeIntensity;
            shakeOffset.z = 0; // Keep Z at 0 for 2D

            // Apply shake to camera position
            transform.position += shakeOffset;

            // Reduce shake over time
            shakeTimer -= Time.deltaTime;
            shakeIntensity = Mathf.Lerp(shakeIntensity, 0f, shakeDecay * Time.deltaTime);
        }
    }

    // Public methods for camera shake
    public void ShakeCamera(float intensity, float duration)
    {
        if (enableCameraShake)
        {
            shakeIntensity = intensity;
            shakeTimer = duration;
        }
    }

    public void ShakeCamera(float intensity)
    {
        ShakeCamera(intensity, 0.5f); // Default duration
    }

    // Public methods for camera control
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            playerController = target.GetComponent<PlayerController>();
        }
    }

    public void SetFollowSpeed(float speed)
    {
        followSpeed = speed;
    }

    public void SetLookAheadDistance(float distance)
    {
        lookAheadDistance = distance;
    }

    // Teleport camera to target instantly (useful for scene transitions)
    public void TeleportToTarget()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
            currentLookAhead = Vector3.zero;
            velocity = Vector3.zero;
        }
    }

    // Debug visualization
    void OnDrawGizmosSelected()
    {
        if (target == null) return;

        // Draw follow range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(target.position, 1f);

        // Draw look ahead
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(target.position, target.position + currentLookAhead);

        // Draw boundaries
        if (useBoundaries)
        {
            Gizmos.color = Color.red;
            Vector3 bottomLeft = new Vector3(minX, minY, 0);
            Vector3 topRight = new Vector3(maxX, maxY, 0);
            Vector3 topLeft = new Vector3(minX, maxY, 0);
            Vector3 bottomRight = new Vector3(maxX, minY, 0);

            Gizmos.DrawLine(bottomLeft, topLeft);
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
        }
    }
}
