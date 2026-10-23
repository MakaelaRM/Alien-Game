using UnityEngine;

// Makes an alien blink. Add it to the alien's root (the same object as NpcWalker / NpcProceduralAnim).
// The eyes are part of the head mesh (a dark material called Mat_Void), so a blink is done by swapping that
// one material for the skin colour for a split second. No model change is needed.
public class AlienBlink : MonoBehaviour
{
    [Header("Head (found automatically: a child whose name ends with _Head)")]
    public Renderer headRenderer;

    [Header("Which material is the eyes")]
    [Tooltip("Leave at -1 to find it automatically (the first material with 'void' in its name). Otherwise the element number, e.g. 1.")]
    public int eyeMaterialIndex = -1;
    [Tooltip("Material shown while blinking. Leave empty to use the head's skin material (Element 0).")]
    public Material blinkMaterial;

    [Header("Timing")]
    public float minSecondsBetweenBlinks = 2f;
    public float maxSecondsBetweenBlinks = 5f;
    [Tooltip("How long the eyes stay shut, in seconds.")]
    public float blinkDuration = 0.12f;
    [Range(0f, 1f)] public float doubleBlinkChance = 0.2f;

    Material[] openMaterials;
    Material[] closedMaterials;
    float timer;
    float closeTimer;
    bool closed;
    bool doubleBlinkPending;

    void Start()
    {
        if (headRenderer == null) headRenderer = FindHead();
        if (headRenderer == null)
        {
            Debug.LogWarning("[AlienBlink] No head found (a child whose name ends with _Head). Drag its renderer into Head Renderer.", this);
            enabled = false;
            return;
        }

        openMaterials = headRenderer.sharedMaterials;

        int eye = eyeMaterialIndex;
        if (eye < 0)
        {
            for (int i = 0; i < openMaterials.Length; i++)
            {
                if (openMaterials[i] != null && openMaterials[i].name.ToLower().Contains("void")) { eye = i; break; }
            }
        }
        if (eye < 0 || eye >= openMaterials.Length)
        {
            Debug.LogWarning("[AlienBlink] Couldn't find the eye material on " + headRenderer.name + ". Set Eye Material Index to the right element number.", this);
            enabled = false;
            return;
        }

        Material lid = blinkMaterial != null ? blinkMaterial : openMaterials[0];
        closedMaterials = (Material[])openMaterials.Clone();
        closedMaterials[eye] = lid;

        timer = Random.Range(minSecondsBetweenBlinks, maxSecondsBetweenBlinks) * Random.value + 0.5f; // so a crowd doesn't blink together
    }

    void Update()
    {
        if (closed)
        {
            closeTimer -= Time.deltaTime;
            if (closeTimer <= 0f)
            {
                headRenderer.sharedMaterials = openMaterials;
                closed = false;
                if (doubleBlinkPending) { doubleBlinkPending = false; timer = 0.1f; }
                else timer = Random.Range(minSecondsBetweenBlinks, maxSecondsBetweenBlinks);
            }
            return;
        }

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            headRenderer.sharedMaterials = closedMaterials;
            closed = true;
            closeTimer = blinkDuration;
            if (!doubleBlinkPending && Random.value < doubleBlinkChance) doubleBlinkPending = true;
        }
    }

    void OnDisable()
    {
        // Always leave the eyes open.
        if (headRenderer != null && openMaterials != null) headRenderer.sharedMaterials = openMaterials;
        closed = false;
    }

    Renderer FindHead()
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t == transform) continue;
            if (t.name.StartsWith("Wpn_", System.StringComparison.OrdinalIgnoreCase)) continue;
            if (t.name.ToLower().EndsWith("_head"))
            {
                var r = t.GetComponent<Renderer>();
                if (r == null) r = t.GetComponentInChildren<Renderer>();
                if (r != null) return r;
            }
        }
        return null;
    }
}
