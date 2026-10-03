using UnityEngine;

/// Fixed-angle perspective camera that follows a target without ever rotating.
/// Attach to the Camera object and assign the player as Target.
[RequireComponent(typeof(Camera))]
public class FixedAngleCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Vector3 targetOffset = new Vector3(0f, 1f, 0f);

    [Header("Framing")]
    public float distance = 12f;
    [Range(10f, 80f)] public float pitch = 40f;   // ~25-30 = low/dramatic, ~45 = top-down-ish
    public float yaw = 0f;                         // rotate the whole view around the Y axis

    [Header("Follow")]
    public float smoothTime = 0.15f;               // 0 = snap to target

    [Header("Room Bounds (optional)")]
    public bool useBounds = false;
    public Bounds bounds = new Bounds(Vector3.zero, new Vector3(20f, 0f, 20f));

    Vector3 velocity;

    void LateUpdate()
    {
        if (target == null) return;

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focus = target.position + targetOffset;

        if (useBounds)
            focus = bounds.ClosestPoint(focus);

        Vector3 desired = focus - rot * Vector3.forward * distance;

        transform.rotation = rot;
        transform.position = smoothTime > 0f
            ? Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime)
            : desired;
    }

    void OnDrawGizmosSelected()
    {
        if (!useBounds) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
