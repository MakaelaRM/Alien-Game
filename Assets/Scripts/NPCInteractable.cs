using UnityEngine;

public class NPCInteractable : MonoBehaviour
{
    [Header("Name Prompt")]
    [SerializeField] private string npcName;
    [SerializeField] private GameObject chatBubblePrefab;
    [Tooltip("Where the tip of the name bubble's tail sits, relative to the top of the head. X follows the camera's right, Y is up.")]
    [SerializeField] private Vector2 promptAnchorOffset = new Vector2(0f, 0.12f);
    [Tooltip("How far the name bubble leans back toward the camera. 0 = upright, 1 = faces the camera fully.")]
    [SerializeField, Range(0f, 1f)] private float promptTilt = 0.5f;

    [Header("Camera Focus")]
    [SerializeField, Range(0f, 1f)] private float focusHeight = 0.75f; // where the camera looks, as a fraction of model height

    [Header("Dialogue")]
    [SerializeField, TextArea(2, 5)]
    private string dialogueText = "You're not from around these parts, are you Partner? I'd advise you keep moving.";
    [SerializeField] private GameObject dialogueBubblePrefab;
    [Tooltip("Where the tip of the dialogue bubble's tail sits, relative to the top of the head. X follows the camera's right (negative = left), Y is up.")]
    [SerializeField] private Vector2 dialogueAnchorOffset = new Vector2(-0.2f, 0.06f);
    [SerializeField] private float dialogueScale = 1f;
    [Tooltip("Shrinks the dialogue bubble if it would run off the edge of the screen.")]
    [SerializeField] private bool fitDialogueToView = true;
    [Tooltip("Gap to keep between the bubble and the screen edge, as a fraction of the screen.")]
    [SerializeField, Range(0f, 0.2f)] private float viewMargin = 0.04f;

    private SpeechBubble3D promptBubble;
    private SpeechBubble3D dialogueBubble;
    private float promptBaseScale = 1f;
    private float dialogueBaseScale = 1f;
    private float modelHeight;

    /// The point the camera looks at during an interaction (roughly face height).
    public Vector3 FocusPoint => transform.position + Vector3.up * (modelHeight * focusHeight);

    private Vector3 HeadTop => transform.position + Vector3.up * modelHeight;

    void Reset()
    {
        #if UNITY_EDITOR
            chatBubblePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/UI/ChatBubble.prefab");
            dialogueBubblePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/UI/DialogueBubble.prefab");
        #endif
    }

    void Awake()
    {
        if (string.IsNullOrEmpty(npcName))
            npcName = gameObject.name;

        // Measure the model before the bubbles are added, so they don't count toward its height
        modelHeight = GetTopOfModel() - transform.position.y;

        promptBubble = SpawnBubble(chatBubblePrefab, "Chat Bubble Prefab", out promptBaseScale);
        dialogueBubble = SpawnBubble(dialogueBubblePrefab, "Dialogue Bubble Prefab", out dialogueBaseScale);
    }

    private SpeechBubble3D SpawnBubble(GameObject prefab, string fieldName, out float baseScale)
    {
        baseScale = 1f;
        if (prefab == null)
        {
            Debug.LogWarning(name + " has no " + fieldName + " assigned.", this);
            return null;
        }

        GameObject instance = Instantiate(prefab, transform);
        instance.name = prefab.name;
        if (!instance.TryGetComponent(out SpeechBubble3D bubble))
        {
            Debug.LogError(prefab.name + " isn't a 3D speech bubble yet. Run Tools > Speech Bubbles > Rebuild Bubble Prefabs.", prefab);
            Destroy(instance);
            return null;
        }

        baseScale = prefab.transform.localScale.x; // lets you resize every bubble by scaling the prefab root
        bubble.HideImmediate();
        return bubble;
    }

    private float GetTopOfModel()
    {
        float top = transform.position.y;
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            top = Mathf.Max(top, r.bounds.max.y);
        return top;
    }

    // ---------------------------------------------------------------------------------------------
    // Name prompt
    // ---------------------------------------------------------------------------------------------

    public void ShowPrompt(string keyName)
    {
        if (promptBubble == null) return;

        promptBubble.SetContent(npcName, "Press " + promptBubble.HighlightKey(keyName) + " to interact");

        Transform view = Camera.main != null ? Camera.main.transform : null;
        GetViewAxes(view, out Vector3 right, out Vector3 flatForward);

        // Lean the bubble back partway toward the (raised) camera so it reads well but still looks 3D
        Vector3 facing = flatForward;
        if (view != null) facing = Vector3.Slerp(flatForward, view.forward, promptTilt).normalized;

        Vector3 anchor = HeadTop + right * promptAnchorOffset.x + Vector3.up * promptAnchorOffset.y;
        PlaceBubble(promptBubble, anchor, Quaternion.LookRotation(facing, Vector3.up), promptBaseScale);
        promptBubble.Show();
    }

    public void HidePrompt()
    {
        if (promptBubble != null) promptBubble.Hide();
    }

    // ---------------------------------------------------------------------------------------------
    // Dialogue
    // ---------------------------------------------------------------------------------------------

    /// Shows the dialogue bubble above the NPC's head, facing the given viewer (the camera).
    public void ShowDialogue(string exitKeyName, Transform viewer)
    {
        if (dialogueBubble == null) return;

        dialogueBubble.SetContent(dialogueText, "Press " + dialogueBubble.HighlightKey(exitKeyName) + " to escape");

        GetViewAxes(viewer, out Vector3 right, out Vector3 flatForward);
        Vector3 anchor = HeadTop + right * dialogueAnchorOffset.x + Vector3.up * dialogueAnchorOffset.y;
        Quaternion rotation = Quaternion.LookRotation(flatForward, Vector3.up);

        float scale = dialogueBaseScale * dialogueScale;
        if (fitDialogueToView && viewer != null && viewer.TryGetComponent(out Camera cam))
            scale = FitScaleToView(cam, dialogueBubble.LocalBounds, anchor, rotation, scale);

        PlaceBubble(dialogueBubble, anchor, rotation, scale);
        dialogueBubble.Show();
    }

    public void HideDialogue()
    {
        if (dialogueBubble != null) dialogueBubble.Hide();
    }

    public void Interact()
    {
        Debug.Log("Interacting with: " + npcName);
    }

    // ---------------------------------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------------------------------

    // The camera's right and forward directions, flattened so bubbles stay upright.
    private static void GetViewAxes(Transform view, out Vector3 right, out Vector3 flatForward)
    {
        right = Vector3.right;
        flatForward = Vector3.forward;
        if (view == null) return;

        Vector3 forward = Vector3.ProjectOnPlane(view.forward, Vector3.up);
        if (forward.sqrMagnitude < 1e-4f) forward = Vector3.ProjectOnPlane(view.up, Vector3.up);
        if (forward.sqrMagnitude < 1e-4f) return;

        flatForward = forward.normalized;
        right = Vector3.Cross(Vector3.up, flatForward);
    }

    private static void PlaceBubble(SpeechBubble3D bubble, Vector3 position, Quaternion rotation, float worldScale)
    {
        Transform t = bubble.transform;
        t.SetPositionAndRotation(position, rotation);

        // Bubbles are children of the NPC, so cancel out the NPC's own scale
        Vector3 parentScale = t.parent != null ? t.parent.lossyScale : Vector3.one;
        t.localScale = new Vector3(
            worldScale / NonZero(parentScale.x),
            worldScale / NonZero(parentScale.y),
            worldScale / NonZero(parentScale.z));
    }

    private static float NonZero(float value) => Mathf.Abs(value) < 1e-5f ? 1f : value;

    // Largest scale (up to 'desired') at which the whole bubble stays on screen. The bubble shrinks
    // toward its tail tip, so it never pulls away from the NPC.
    private float FitScaleToView(Camera cam, Rect bounds, Vector3 anchor, Quaternion rotation, float desired)
    {
        if (!PointInView(cam, anchor, 0f)) return desired;
        if (BoundsInView(cam, bounds, anchor, rotation, desired)) return desired;

        float low = desired * 0.5f;
        float high = desired;
        for (int i = 0; i < 12; i++)
        {
            float mid = (low + high) * 0.5f;
            if (BoundsInView(cam, bounds, anchor, rotation, mid)) low = mid;
            else high = mid;
        }
        return low;
    }

    private bool BoundsInView(Camera cam, Rect bounds, Vector3 anchor, Quaternion rotation, float scale)
    {
        return PointInView(cam, anchor + rotation * new Vector3(bounds.xMin * scale, bounds.yMin * scale, 0f), viewMargin)
            && PointInView(cam, anchor + rotation * new Vector3(bounds.xMax * scale, bounds.yMin * scale, 0f), viewMargin)
            && PointInView(cam, anchor + rotation * new Vector3(bounds.xMin * scale, bounds.yMax * scale, 0f), viewMargin)
            && PointInView(cam, anchor + rotation * new Vector3(bounds.xMax * scale, bounds.yMax * scale, 0f), viewMargin);
    }

    private static bool PointInView(Camera cam, Vector3 worldPoint, float margin)
    {
        Vector3 v = cam.WorldToViewportPoint(worldPoint);
        return v.z > 0f && v.x >= margin && v.x <= 1f - margin && v.y >= margin && v.y <= 1f - margin;
    }
}