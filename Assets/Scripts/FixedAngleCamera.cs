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

    [Header("Interaction Focus")]
    public float focusDistance = 2.5f;               // how far in front of the NPC the camera ends up
    public float focusTransitionTime = 1f;         // seconds for the zoom-in

    Vector3 velocity;

    bool isFocusing;
    float focusTimer;
    Vector3 focusStartPos;
    Quaternion focusStartRot;
    Vector3 focusEndPos;
    Quaternion focusEndRot;
    public bool IsFocusComplete => isFocusing && focusTimer >= focusTransitionTime;

    /// Smoothly moves the camera in front of the NPC, looking at lookPoint.
    public void FocusOn(Transform npc, Vector3 lookPoint)
    {
        // Flatten the NPC's facing direction so the camera stays level with their face
        Vector3 npcForward = npc.forward;
        npcForward.y = 0f;
        npcForward.Normalize();

        focusStartPos = transform.position;
        focusStartRot = transform.rotation;
        focusEndPos = lookPoint + npcForward * focusDistance;
        focusEndRot = Quaternion.LookRotation(lookPoint - focusEndPos);

        focusTimer = 0f;
        isFocusing = true;
    }

    /// Returns control to the normal follow camera.
    public void StopFocus()
    {
        isFocusing = false;
        velocity = Vector3.zero; // clear stale follow velocity so it doesn't lurch
    }

    void LateUpdate()
    {
        if (isFocusing)
        {
            UpdateFocus();
        }
        else
        {
            // ---------- Original follow camera (unchanged) ----------
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
    }

    /// Updates each step of the focus.
    void UpdateFocus()
    {
        focusTimer += Time.deltaTime;

        float t = focusTransitionTime > 0f
            ? Mathf.Clamp01(focusTimer / focusTransitionTime)
            : 1f;
        t = Mathf.SmoothStep(0f, 1f, t); // ease in and out

        transform.position = Vector3.Lerp(focusStartPos, focusEndPos, t);
        transform.rotation = Quaternion.Slerp(focusStartRot, focusEndRot, t);
    }

    void OnDrawGizmosSelected()
    {
        if (!useBounds) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}