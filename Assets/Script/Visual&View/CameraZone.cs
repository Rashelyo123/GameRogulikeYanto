using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZone : MonoBehaviour
{
    [Header("Zone Settings")]
    public Vector3 zoneOffset = Vector3.zero;
    public float zoneFollowSpeed = 3f;
    public float zoneLookAheadDistance = 1f;
    public bool useZoneBoundaries = false;
    public Bounds zoneBoundaries;

    private CameraFollow cameraFollow;
    private Vector3 originalOffset;
    private float originalFollowSpeed;
    private float originalLookAheadDistance;
    private bool originalUseBoundaries;

    void Start()
    {
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && cameraFollow != null)
        {
            // Store original settings
            originalOffset = cameraFollow.offset;
            originalFollowSpeed = cameraFollow.followSpeed;
            originalLookAheadDistance = cameraFollow.lookAheadDistance;
            originalUseBoundaries = cameraFollow.useBoundaries;

            // Apply zone settings
            StartCoroutine(TransitionToZoneSettings());
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && cameraFollow != null)
        {
            // Restore original settings
            StartCoroutine(TransitionToOriginalSettings());
        }
    }

    System.Collections.IEnumerator TransitionToZoneSettings()
    {
        float transitionTime = 1f;
        float elapsed = 0f;

        Vector3 startOffset = cameraFollow.offset;
        float startFollowSpeed = cameraFollow.followSpeed;
        float startLookAheadDistance = cameraFollow.lookAheadDistance;

        while (elapsed < transitionTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionTime;

            cameraFollow.offset = Vector3.Lerp(startOffset, zoneOffset, t);
            cameraFollow.followSpeed = Mathf.Lerp(startFollowSpeed, zoneFollowSpeed, t);
            cameraFollow.lookAheadDistance = Mathf.Lerp(startLookAheadDistance, zoneLookAheadDistance, t);

            yield return null;
        }

        if (useZoneBoundaries)
        {
            cameraFollow.useBoundaries = true;
            cameraFollow.minX = zoneBoundaries.min.x;
            cameraFollow.maxX = zoneBoundaries.max.x;
            cameraFollow.minY = zoneBoundaries.min.y;
            cameraFollow.maxY = zoneBoundaries.max.y;
        }
    }

    System.Collections.IEnumerator TransitionToOriginalSettings()
    {
        float transitionTime = 1f;
        float elapsed = 0f;

        Vector3 startOffset = cameraFollow.offset;
        float startFollowSpeed = cameraFollow.followSpeed;
        float startLookAheadDistance = cameraFollow.lookAheadDistance;

        while (elapsed < transitionTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionTime;

            cameraFollow.offset = Vector3.Lerp(startOffset, originalOffset, t);
            cameraFollow.followSpeed = Mathf.Lerp(startFollowSpeed, originalFollowSpeed, t);
            cameraFollow.lookAheadDistance = Mathf.Lerp(startLookAheadDistance, originalLookAheadDistance, t);

            yield return null;
        }

        cameraFollow.useBoundaries = originalUseBoundaries;
    }

    void OnDrawGizmosSelected()
    {
        if (useZoneBoundaries)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(zoneBoundaries.center, zoneBoundaries.size);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, GetComponent<Collider2D>().bounds.size);
    }
}
