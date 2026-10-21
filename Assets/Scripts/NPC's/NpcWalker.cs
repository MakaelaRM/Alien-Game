using UnityEngine;

// Walks an NPC back and forth along a list of points (for example up and down the town road).
// Add it to the NPC, make an empty GameObject for each stop along the road, and drag them into "Points".
public class NpcWalker : MonoBehaviour
{
    [Tooltip("Stops to walk to, in order. Do NOT make these children of the NPC.")]
    public Transform[] points;
    public float speed = 1.5f;
    [Tooltip("Seconds to stand still at each stop.")]
    public float waitAtPoint = 2f;
    [Tooltip("On: walk A to B and back again. Off: walk A to B, then jump to A's direction again (loop).")]
    public bool pingPong = true;
    public float turnSpeed = 8f;

    [Header("Facing")]
    [Tooltip("On: turns to face the camera while standing still (at start and at each stop).")]
    public bool faceCameraWhenWaiting = true;
    [Tooltip("Turn this many degrees extra if his front is not the +Z side of the model. Try 180 if he shows his back to the camera.")]
    public float facingOffsetY = 0f;

    [Header("Optional animation")]
    public Animator animator;
    [Tooltip("Name of a Float parameter in the Animator that is 0 when idle and above 0 when walking.")]
    public string speedParam = "Speed";

    int index;
    int dir = 1;
    float waitTimer;
    bool hasSpeedParam;

    void Start()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            foreach (var p in animator.parameters)
                if (p.name == speedParam && p.type == AnimatorControllerParameterType.Float) hasSpeedParam = true;
        }
        waitTimer = Random.Range(0f, waitAtPoint); // so a group of NPCs doesn't move in sync
    }

    void Update()
    {
        if (points == null || points.Length == 0) return;

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            SetAnim(0f);
            FaceCamera();
            return;
        }

        Vector3 target = points[index].position;
        target.y = transform.position.y;
        Vector3 to = target - transform.position;

        if (to.sqrMagnitude < 0.01f)
        {
            waitTimer = waitAtPoint;
            NextPoint();
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        Quaternion look = Quaternion.LookRotation(to.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);
        SetAnim(speed);
    }

    void FaceCamera()
    {
        if (!faceCameraWhenWaiting) return;
        var cam = Camera.main;
        if (cam == null) return;
        Vector3 to = cam.transform.position - transform.position;
        to.y = 0f;
        if (to.sqrMagnitude < 0.001f) return;
        Quaternion look = Quaternion.LookRotation(to.normalized, Vector3.up) * Quaternion.Euler(0f, facingOffsetY, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);
    }

    void NextPoint()
    {
        if (points.Length < 2) return;
        if (pingPong)
        {
            if (index + dir < 0 || index + dir >= points.Length) dir = -dir;
            index += dir;
        }
        else
        {
            index = (index + 1) % points.Length;
        }
    }

    void SetAnim(float v)
    {
        if (animator != null && hasSpeedParam) animator.SetFloat(speedParam, v);
    }
}