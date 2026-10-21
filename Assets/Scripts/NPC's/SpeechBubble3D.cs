using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using TMPro;

/// A chunky 3D speech bubble: a bevelled, extruded rounded rectangle with a curved tail,
/// sized automatically to fit its TextMeshPro text.
///
/// The object's pivot is the TIP OF THE TAIL, so position it where the bubble should come from
/// (e.g. just above a character's head). The bubble grows upward from there however long the text is.
/// The front face points along -Z, like a world-space canvas, so rotate it to face the camera.
[ExecuteAlways]
[DisallowMultipleComponent]
public class SpeechBubble3D : MonoBehaviour
{
    public enum Preset { NameTag, Dialogue }

    private const string GeneratedMeshName = "SpeechBubble3D (generated)";

    [Header("Parts")]
    [SerializeField] private Transform body;
    [SerializeField] private MeshFilter shapeFilter;
    [SerializeField] private MeshRenderer shapeRenderer;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private Transform divider;

    [Header("Layout (meters)")]
    [Tooltip("Text wraps onto a new line past this width.")]
    [SerializeField] private float maxTextWidth = 3.6f;
    [SerializeField] private Vector2 padding = new Vector2(0.3f, 0.2f);
    [Tooltip("Space between the main text and the prompt line.")]
    [SerializeField] private float lineGap = 0.1f;
    [SerializeField] private float minWidth = 1.2f;
    [Tooltip("Large values give a pill shape.")]
    [SerializeField] private float cornerRadius = 0.25f;
    [SerializeField] private float dividerThickness = 0.012f;
    [SerializeField, Range(0.1f, 1f)] private float dividerWidth = 0.55f;

    [Header("Shape")]
    [SerializeField] private float depth = 0.1f;
    [SerializeField] private float bevel = 0.03f;
    [SerializeField, Range(1, 8)] private int bevelSegments = 4;
    [SerializeField, Range(2, 24)] private int cornerSegments = 14;

    [Header("Tail")]
    [Tooltip("Where the tail sits along the bottom edge. 0 = left, 1 = right.")]
    [SerializeField, Range(0f, 1f)] private float tailPosition = 0.5f;
    [SerializeField] private float tailWidth = 0.3f;
    [SerializeField] private float tailLength = 0.22f;
    [Tooltip("Sideways lean of the tail tip, in meters. Positive leans right.")]
    [SerializeField] private float tailSlant = 0f;
    [SerializeField, Range(0f, 0.8f)] private float tailCurve = 0.25f;

    [Header("Text")]
    [Tooltip("Color used by HighlightKey() for key names like \"E\" or \"Esc\".")]
    [SerializeField] private Color keyColor = new Color(0.96f, 0.78f, 0.42f);
    [SerializeField] private Color levelColor = new Color(0.392f, 1f, 0.196f);
    [Tooltip("How far the text floats in front of the bubble's face.")]
    [SerializeField] private float textLift = 0.004f;

    [Header("Animation")]
    [SerializeField] private float showDuration = 0.28f;
    [SerializeField] private float hideDuration = 0.14f;
    [SerializeField] private float bobHeight = 0.015f;
    [Tooltip("Bobs per second.")]
    [SerializeField] private float bobSpeed = 0.5f;

    [Header("Editor Preview")]
    [SerializeField, TextArea(1, 4)] private string previewBody = "Shopkeeper";
    [SerializeField] private string previewPrompt = "Press E to interact";

    private string currentBody = string.Empty;
    private string currentPrompt = string.Empty;
    private bool hasContent;
    private bool dirty = true;

    private Mesh mesh;
    private Rect localBounds;

    private bool targetVisible;
    private float visibility;
    private float bobPhase;

    // Reusable buffers so rebuilding doesn't allocate every time
    private readonly List<Vector2> outline = new List<Vector2>(128);
    private readonly List<Vector2> capPoints = new List<Vector2>(128);
    private readonly List<Vector2> edgeNormals = new List<Vector2>(128);
    private readonly List<float> miters = new List<float>(128);
    private readonly List<int> capTriangles = new List<int>(384);
    private readonly List<int> earWork = new List<int>(128);
    private readonly List<Vector3> vertices = new List<Vector3>(1024);
    private readonly List<Vector3> normals = new List<Vector3>(1024);
    private readonly List<Vector2> uvs = new List<Vector2>(1024);
    private readonly List<int> faceTriangles = new List<int>(512);
    private readonly List<int> rimTriangles = new List<int>(6144);

    /// The bubble's outline in its own local space (before scaling), with the tail tip at (0, 0).
    public Rect LocalBounds => localBounds;

    /// True while the bubble is visible or animating in.
    public bool IsShown => targetVisible && gameObject.activeSelf;

    // ---------------------------------------------------------------------------------------------
    // Public API
    // ---------------------------------------------------------------------------------------------

    /// Sets the main text and the smaller prompt line, then resizes the bubble to fit.
    /// Either can be empty.
    public void SetContent(string bodyContent, string promptContent)
    {
        currentBody = bodyContent ?? string.Empty;
        currentPrompt = promptContent ?? string.Empty;
        hasContent = true;
        Rebuild();
    }

    /// Wraps a key name in this bubble's highlight color, e.g. "Press " + HighlightKey("E").
    public string HighlightKey(string key)
    {
        return "<color=#" + ColorUtility.ToHtmlStringRGB(keyColor) + ">" + key + "</color>";
    }

    public string HighlightLevel(int level)
    {
        return "<color=#" + ColorUtility.ToHtmlStringRGB(levelColor) + ">" + level.ToString() + "</color>";
    }

    /// Pops the bubble in from the tail tip.
    public void Show()
    {
        if (!gameObject.activeSelf)
        {
            visibility = 0f;
            gameObject.SetActive(true);
        }
        targetVisible = true;
        if (!Application.isPlaying || showDuration <= 0f) visibility = 1f;
        ApplyAnimation();
    }

    /// Shrinks the bubble away, then deactivates it.
    public void Hide()
    {
        targetVisible = false;
        if (!gameObject.activeSelf) return;
        if (!Application.isPlaying || hideDuration <= 0f) HideImmediate();
    }

    public void HideImmediate()
    {
        targetVisible = false;
        visibility = 0f;
        gameObject.SetActive(false);
    }

    // ---------------------------------------------------------------------------------------------
    // Unity messages
    // ---------------------------------------------------------------------------------------------

    void Awake()
    {
        bobPhase = Random.value * Mathf.PI * 2f;
        if (Application.isPlaying) EnsureMaterials();
    }

    void OnEnable()
    {
        if (mesh == null || !Application.isPlaying) dirty = true;
    }

    void OnValidate()
    {
        // Rebuilding inside OnValidate isn't allowed for TextMeshPro, so flag it for the next Update.
        dirty = true;
    }

    void OnDestroy()
    {
        if (mesh == null) return;
        if (Application.isPlaying) Destroy(mesh);
        else DestroyImmediate(mesh);
    }

    void Update()
    {
        if (dirty) Rebuild();

        if (!Application.isPlaying)
        {
            // In the editor, always show the bubble at full size so it's easy to tune.
            if (body != null)
            {
                SetLocalScale(body, Vector3.one);
                SetLocalPosition(body, Vector3.zero);
            }
            return;
        }

        float dt = Time.unscaledDeltaTime;
        if (targetVisible)
        {
            visibility = showDuration > 0f ? Mathf.MoveTowards(visibility, 1f, dt / showDuration) : 1f;
        }
        else
        {
            visibility = hideDuration > 0f ? Mathf.MoveTowards(visibility, 0f, dt / hideDuration) : 0f;
            if (visibility <= 0f)
            {
                gameObject.SetActive(false);
                return;
            }
        }
        ApplyAnimation();
    }

    // ---------------------------------------------------------------------------------------------
    // Animation
    // ---------------------------------------------------------------------------------------------

    private void ApplyAnimation()
    {
        if (body == null) return;

        float scale = EaseOutBack(Mathf.Clamp01(visibility));
        body.localScale = Vector3.one * Mathf.Max(scale, 0.0001f);

        float bob = Application.isPlaying
            ? Mathf.Sin(Time.unscaledTime * bobSpeed * Mathf.PI * 2f + bobPhase) * bobHeight
            : 0f;
        body.localPosition = new Vector3(0f, bob, 0f);
    }

    // Overshoots slightly past 1 before settling, for a springy pop.
    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float u = t - 1f;
        return 1f + c3 * u * u * u + c1 * u * u;
    }

    // ---------------------------------------------------------------------------------------------
    // Layout
    // ---------------------------------------------------------------------------------------------

    private void Rebuild()
    {
        dirty = false;
        if (body == null || shapeFilter == null) return;

        bool usePreview = !Application.isPlaying || !hasContent;
        string bodyString = usePreview ? previewBody : currentBody;
        string promptString = usePreview ? previewPrompt : currentPrompt;

        bool hasBody = PrepareText(bodyText, bodyString, out Rect bodyInk);
        bool hasPrompt = PrepareText(promptText, promptString, out Rect promptInk);

        float contentWidth = Mathf.Max(hasBody ? bodyInk.width : 0f, hasPrompt ? promptInk.width : 0f);
        float bodyHeight = hasBody ? bodyInk.height : 0f;
        float promptHeight = hasPrompt ? promptInk.height : 0f;
        bool showDivider = divider != null && hasBody && hasPrompt;
        float gap = hasBody && hasPrompt ? (showDivider ? lineGap * 2f + dividerThickness : lineGap) : 0f;

        // Bubble rectangle size
        float width = Mathf.Max(minWidth, contentWidth + padding.x * 2f);
        float height = Mathf.Max(bodyHeight + promptHeight + gap + padding.y * 2f, 0.1f);
        float radius = Mathf.Min(cornerRadius, height * 0.5f, width * 0.5f);

        // Tail placement: keep the tail's base on the straight part of the bottom edge
        float tail = Mathf.Max(0f, tailLength);
        float tailBase = Mathf.Min(tailWidth, Mathf.Max(0.01f, width - 2f * radius - 0.02f));
        float minBaseX = -width * 0.5f + radius + tailBase * 0.5f;
        float maxBaseX = width * 0.5f - radius - tailBase * 0.5f;
        float baseOffset = minBaseX <= maxBaseX ? Mathf.Lerp(minBaseX, maxBaseX, tailPosition) : 0f;

        // Shift the rectangle so the tail tip lands exactly on the pivot (0, 0)
        float centerX = -(baseOffset + tailSlant);
        float bottom = tail;
        float top = tail + height;
        float left = centerX - width * 0.5f;
        float right = centerX + width * 0.5f;
        float baseX = centerX + baseOffset;

        float bevelSize = Mathf.Min(bevel, depth * 0.5f * 0.95f);
        float tipRadius = Mathf.Min(bevelSize * 1.1f + 0.005f, tailBase * 0.25f);

        localBounds = Rect.MinMaxRect(left, 0f, right, top);

        // Stack the text from the top down
        float frontZ = -depth * 0.5f;
        float y = top - padding.y;
        if (hasBody)
        {
            Place(bodyText, bodyInk, new Vector2(centerX, y - bodyHeight * 0.5f), frontZ - textLift);
            y -= bodyHeight;
        }
        if (hasBody && hasPrompt)
        {
            y -= lineGap;
            if (showDivider)
            {
                float dividerLength = Mathf.Max(contentWidth * dividerWidth, 0.2f);
                SetLocalPosition(divider, new Vector3(centerX, y - dividerThickness * 0.5f, frontZ - textLift * 0.5f));
                if (divider.localRotation != Quaternion.identity) divider.localRotation = Quaternion.identity;
                SetLocalScale(divider, new Vector3(dividerLength, dividerThickness, 1f));
                y -= dividerThickness + lineGap;
            }
        }
        if (divider != null && divider.gameObject.activeSelf != showDivider)
            divider.gameObject.SetActive(showDivider);
        if (hasPrompt)
            Place(promptText, promptInk, new Vector2(centerX, y - promptHeight * 0.5f), frontZ - textLift);

        BuildOutline(left, right, bottom, top, radius, baseX, tailBase, tail, tipRadius);
        BuildMesh(bevelSize);
    }

    // Sets the text, lets TextMeshPro lay it out, and measures the visible glyphs.
    private bool PrepareText(TMP_Text text, string content, out Rect ink)
    {
        ink = default;
        if (text == null) return false;

        bool has = !string.IsNullOrEmpty(content);
        if (text.gameObject.activeSelf != has) text.gameObject.SetActive(has);
        if (!has) return false;

        RectTransform rect = text.rectTransform;
        Vector2 size = new Vector2(Mathf.Max(0.1f, maxTextWidth), 50f);
        if (rect.pivot != new Vector2(0.5f, 0.5f)) rect.pivot = new Vector2(0.5f, 0.5f);
        if (rect.sizeDelta != size) rect.sizeDelta = size;
        if (rect.localRotation != Quaternion.identity) rect.localRotation = Quaternion.identity;
        SetLocalScale(rect, Vector3.one);

        if (text.text != content) text.text = content;
        text.ForceMeshUpdate(true, true);
        ink = InkBounds(text);
        return ink.width > 0f && ink.height > 0f;
    }

    // Bounds of the actual letter shapes (not line metrics), so text is optically centered.
    private static Rect InkBounds(TMP_Text text)
    {
        TMP_TextInfo info = text.textInfo;
        if (info == null) return default;

        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);
        bool any = false;
        for (int i = 0; i < info.characterCount; i++)
        {
            TMP_CharacterInfo c = info.characterInfo[i];
            if (!c.isVisible) continue;
            min = Vector2.Min(min, c.bottomLeft);
            max = Vector2.Max(max, c.topRight);
            any = true;
        }
        return any ? Rect.MinMaxRect(min.x, min.y, max.x, max.y) : default;
    }

    private static void Place(TMP_Text text, Rect ink, Vector2 center, float z)
    {
        SetLocalPosition(text.rectTransform, new Vector3(center.x - ink.center.x, center.y - ink.center.y, z));
    }

    // Only write transforms that actually changed, so editing the prefab doesn't keep re-dirtying it.
    private static void SetLocalPosition(Transform t, Vector3 value)
    {
        if (t.localPosition != value) t.localPosition = value;
    }

    private static void SetLocalScale(Transform t, Vector3 value)
    {
        if (t.localScale != value) t.localScale = value;
    }

    // ---------------------------------------------------------------------------------------------
    // Outline (2D, counter-clockwise, x right / y up as seen from the front)
    // ---------------------------------------------------------------------------------------------

    private void BuildOutline(float left, float right, float bottom, float top, float radius,
                              float baseX, float tailBase, float tail, float tipRadius)
    {
        outline.Clear();
        int steps = Mathf.Max(2, cornerSegments);

        AddArc(new Vector2(left + radius, bottom + radius), radius, 180f, 270f, steps);
        if (tail > 0.001f && tailBase > 0.001f)
            AddTail(baseX, bottom, tailBase, tipRadius);
        AddArc(new Vector2(right - radius, bottom + radius), radius, 270f, 360f, steps);
        AddArc(new Vector2(right - radius, top - radius), radius, 0f, 90f, steps);
        AddArc(new Vector2(left + radius, top - radius), radius, 90f, 180f, steps);

        RemoveDuplicatePoints();
    }

    private void AddArc(Vector2 center, float radius, float fromDegrees, float toDegrees, int steps)
    {
        for (int i = 0; i <= steps; i++)
        {
            float a = Mathf.Lerp(fromDegrees, toDegrees, i / (float)steps) * Mathf.Deg2Rad;
            outline.Add(center + radius * new Vector2(Mathf.Cos(a), Mathf.Sin(a)));
        }
    }

    // Tail from the left base point, down to a rounded tip at (0, 0), and back up to the right base point.
    private void AddTail(float baseX, float baseY, float tailBase, float tipRadius)
    {
        const int sideSteps = 10;
        const int tipSteps = 6;

        Vector2 leftBase = new Vector2(baseX - tailBase * 0.5f, baseY);
        Vector2 rightBase = new Vector2(baseX + tailBase * 0.5f, baseY);
        Vector2 baseCenter = new Vector2(baseX, baseY);
        Vector2 tipCenter = new Vector2(0f, tipRadius);

        Vector2 axis = (tipCenter - baseCenter).normalized;
        Vector2 perp = new Vector2(axis.y, -axis.x);
        if (Vector2.Dot(perp, leftBase - baseCenter) < 0f) perp = -perp;
        Vector2 tipLeft = tipCenter + perp * tipRadius;
        Vector2 tipRight = tipCenter - perp * tipRadius;

        // Left side
        Vector2 control = TailControl(leftBase, tipLeft, baseCenter, axis);
        for (int i = 0; i <= sideSteps; i++)
            outline.Add(QuadraticBezier(leftBase, control, tipLeft, i / (float)sideSteps));

        // Rounded tip
        float a0 = Mathf.Atan2(tipLeft.y - tipCenter.y, tipLeft.x - tipCenter.x) * Mathf.Rad2Deg;
        float a1 = Mathf.Atan2(tipRight.y - tipCenter.y, tipRight.x - tipCenter.x) * Mathf.Rad2Deg;
        if (a1 <= a0) a1 += 360f;
        for (int i = 1; i < tipSteps; i++)
        {
            float a = Mathf.Lerp(a0, a1, i / (float)tipSteps) * Mathf.Deg2Rad;
            outline.Add(tipCenter + tipRadius * new Vector2(Mathf.Cos(a), Mathf.Sin(a)));
        }

        // Right side
        control = TailControl(tipRight, rightBase, baseCenter, axis);
        for (int i = 0; i <= sideSteps; i++)
            outline.Add(QuadraticBezier(tipRight, control, rightBase, i / (float)sideSteps));
    }

    // Bezier control point pulled toward the tail's center line, so both sides curve inward.
    private Vector2 TailControl(Vector2 from, Vector2 to, Vector2 axisOrigin, Vector2 axis)
    {
        Vector2 mid = (from + to) * 0.5f;
        Vector2 onAxis = axisOrigin + axis * Vector2.Dot(mid - axisOrigin, axis);
        return mid + (onAxis - mid) * tailCurve;
    }

    private static Vector2 QuadraticBezier(Vector2 p0, Vector2 c, Vector2 p1, float t)
    {
        float u = 1f - t;
        return u * u * p0 + 2f * u * t * c + t * t * p1;
    }

    private void RemoveDuplicatePoints()
    {
        const float minSpacingSqr = 1e-8f;
        int write = 0;
        for (int i = 0; i < outline.Count; i++)
        {
            if (write > 0 && (outline[i] - outline[write - 1]).sqrMagnitude <= minSpacingSqr) continue;
            outline[write++] = outline[i];
        }
        outline.RemoveRange(write, outline.Count - write);
        while (outline.Count > 2 && (outline[0] - outline[outline.Count - 1]).sqrMagnitude <= minSpacingSqr)
            outline.RemoveAt(outline.Count - 1);
    }

    // ---------------------------------------------------------------------------------------------
    // Mesh: flat front/back caps (submesh 0, "face") + rounded bevel and sides (submesh 1, "rim")
    // ---------------------------------------------------------------------------------------------

    private void BuildMesh(float bevelSize)
    {
        EnsureMesh();
        int n = outline.Count;
        if (n < 3)
        {
            mesh.Clear();
            return;
        }

        // Outward 2D normal at each point, plus how far to push it so insets stay parallel at corners
        edgeNormals.Clear();
        miters.Clear();
        for (int i = 0; i < n; i++)
        {
            Vector2 prev = outline[(i + n - 1) % n];
            Vector2 cur = outline[i];
            Vector2 next = outline[(i + 1) % n];
            Vector2 e1 = (cur - prev).normalized;
            Vector2 e2 = (next - cur).normalized;
            Vector2 n1 = new Vector2(e1.y, -e1.x);
            Vector2 n2 = new Vector2(e2.y, -e2.x);
            Vector2 nv = n1 + n2;
            if (nv.sqrMagnitude < 1e-8f) nv = n1;
            nv.Normalize();
            edgeNormals.Add(nv);
            miters.Add(1f / Mathf.Max(Vector2.Dot(nv, n1), 0.35f));
        }

        float half = depth * 0.5f;
        vertices.Clear();
        normals.Clear();
        uvs.Clear();
        faceTriangles.Clear();
        rimTriangles.Clear();

        // Caps are the outline shrunk inward by the bevel. Triangulating the shrunk shape
        // (rather than the outer one) avoids slivers flipping inside the narrow tail.
        capPoints.Clear();
        for (int i = 0; i < n; i++)
            capPoints.Add(outline[i] - edgeNormals[i] * (bevelSize * miters[i]));
        Triangulate(capPoints, capTriangles);

        int frontStart = vertices.Count;
        for (int i = 0; i < n; i++)
            AddVertex(new Vector3(capPoints[i].x, capPoints[i].y, -half), Vector3.back);
        for (int t = 0; t < capTriangles.Count; t += 3)
            AddTriangle(faceTriangles, frontStart + capTriangles[t], frontStart + capTriangles[t + 1], frontStart + capTriangles[t + 2], Vector3.back);

        int backStart = vertices.Count;
        for (int i = 0; i < n; i++)
            AddVertex(new Vector3(capPoints[i].x, capPoints[i].y, half), Vector3.forward);
        for (int t = 0; t < capTriangles.Count; t += 3)
            AddTriangle(faceTriangles, backStart + capTriangles[t], backStart + capTriangles[t + 1], backStart + capTriangles[t + 2], Vector3.forward);

        // Rings of the rounded bevel profile: front edge -> side wall -> back edge
        int segments = Mathf.Max(1, bevelSegments);
        int ringCount = 2 * (segments + 1);
        int ringStart = vertices.Count;
        for (int k = 0; k < ringCount; k++)
        {
            float theta, z, nz;
            if (k <= segments)
            {
                theta = 90f * (1f - k / (float)segments) * Mathf.Deg2Rad;
                z = -half + bevelSize * (1f - Mathf.Sin(theta));
                nz = -Mathf.Sin(theta);
            }
            else
            {
                int j = k - (segments + 1);
                theta = 90f * (j / (float)segments) * Mathf.Deg2Rad;
                z = half - bevelSize * (1f - Mathf.Sin(theta));
                nz = Mathf.Sin(theta);
            }
            float inset = bevelSize * (1f - Mathf.Cos(theta));
            float nxy = Mathf.Cos(theta);

            for (int i = 0; i < n; i++)
            {
                Vector2 p = outline[i] - edgeNormals[i] * (inset * miters[i]);
                Vector3 normal = new Vector3(edgeNormals[i].x * nxy, edgeNormals[i].y * nxy, nz).normalized;
                AddVertex(new Vector3(p.x, p.y, z), normal);
            }
        }

        for (int k = 0; k < ringCount - 1; k++)
        {
            for (int i = 0; i < n; i++)
            {
                int i2 = (i + 1) % n;
                int a = ringStart + k * n + i;
                int b = ringStart + k * n + i2;
                int c = ringStart + (k + 1) * n + i2;
                int d = ringStart + (k + 1) * n + i;
                Vector3 expected = normals[a] + normals[b] + normals[c] + normals[d];
                AddTriangle(rimTriangles, a, b, c, expected);
                AddTriangle(rimTriangles, a, c, d, expected);
            }
        }

        mesh.Clear();
        mesh.indexFormat = vertices.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.subMeshCount = 2;
        mesh.SetTriangles(faceTriangles, 0);
        mesh.SetTriangles(rimTriangles, 1);
        mesh.RecalculateBounds();
    }

    private void AddVertex(Vector3 position, Vector3 normal)
    {
        vertices.Add(position);
        normals.Add(normal);
        uvs.Add(new Vector2(position.x, position.y));
    }

    // Adds a triangle wound so its front side faces 'expected' (Unity treats clockwise as front).
    private void AddTriangle(List<int> list, int a, int b, int c, Vector3 expected)
    {
        Vector3 cross = Vector3.Cross(vertices[b] - vertices[a], vertices[c] - vertices[a]);
        list.Add(a);
        if (Vector3.Dot(cross, expected) < 0f)
        {
            list.Add(c);
            list.Add(b);
        }
        else
        {
            list.Add(b);
            list.Add(c);
        }
    }

    // Ear-clipping triangulation for a simple polygon. Writes index triples into 'result'.
    private void Triangulate(List<Vector2> points, List<int> result)
    {
        result.Clear();
        int n = points.Count;
        if (n < 3) return;

        earWork.Clear();
        for (int i = 0; i < n; i++) earWork.Add(i);
        if (SignedArea(points) < 0f) earWork.Reverse();

        int guard = 0;
        while (earWork.Count > 3 && guard++ < 10000)
        {
            int count = earWork.Count;
            bool clipped = false;
            for (int i = 0; i < count; i++)
            {
                int ia = earWork[(i + count - 1) % count];
                int ib = earWork[i];
                int ic = earWork[(i + 1) % count];
                Vector2 a = points[ia], b = points[ib], c = points[ic];
                if (Cross(b - a, c - b) <= 1e-10f) continue; // reflex or flat corner

                bool blocked = false;
                for (int j = 0; j < count; j++)
                {
                    int ij = earWork[j];
                    if (ij == ia || ij == ib || ij == ic) continue;
                    if (PointInTriangle(points[ij], a, b, c)) { blocked = true; break; }
                }
                if (blocked) continue;

                result.Add(ia);
                result.Add(ib);
                result.Add(ic);
                earWork.RemoveAt(i);
                clipped = true;
                break;
            }

            if (!clipped)
            {
                // Degenerate leftovers: clip anyway so we always finish
                result.Add(earWork[count - 1]);
                result.Add(earWork[0]);
                result.Add(earWork[1]);
                earWork.RemoveAt(0);
            }
        }

        if (earWork.Count == 3)
        {
            result.Add(earWork[0]);
            result.Add(earWork[1]);
            result.Add(earWork[2]);
        }
    }

    private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

    private static float SignedArea(List<Vector2> points)
    {
        float area = 0f;
        for (int i = 0; i < points.Count; i++)
            area += Cross(points[i], points[(i + 1) % points.Count]);
        return area * 0.5f;
    }

    private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        const float eps = 1e-7f;
        return Cross(b - a, p - a) >= -eps && Cross(c - b, p - b) >= -eps && Cross(a - c, p - c) >= -eps;
    }

    private void EnsureMesh()
    {
        if (mesh == null)
        {
            Mesh existing = shapeFilter.sharedMesh;
            if (!Application.isPlaying && existing != null && existing.name == GeneratedMeshName)
            {
                mesh = existing; // reuse the editor preview mesh after a script reload
            }
            else
            {
                mesh = new Mesh { name = GeneratedMeshName, hideFlags = HideFlags.DontSave };
                mesh.MarkDynamic();
            }
        }
        if (shapeFilter.sharedMesh != mesh) shapeFilter.sharedMesh = mesh;
    }

    // ---------------------------------------------------------------------------------------------
    // Materials
    // ---------------------------------------------------------------------------------------------

    // Fallback so the bubble still renders if the prefab's materials go missing.
    private void EnsureMaterials()
    {
        if (shapeRenderer == null) return;
        Material[] current = shapeRenderer.sharedMaterials;
        Material faceMat = current.Length > 0 ? current[0] : null;
        Material rimMat = current.Length > 1 ? current[1] : null;
        if (faceMat != null && rimMat != null) return;

        Shader shader = FindLitShader();
        if (shader == null) return;
        if (faceMat == null)
        {
            faceMat = new Material(shader) { name = "Bubble Face (runtime)" };
            SetBaseColor(faceMat, new Color(0.06f, 0.055f, 0.07f));
        }
        if (rimMat == null)
        {
            rimMat = new Material(shader) { name = "Bubble Rim (runtime)" };
            SetBaseColor(rimMat, new Color(0.92f, 0.84f, 0.68f));
        }
        shapeRenderer.sharedMaterials = new[] { faceMat, rimMat };
    }

    /// The lit shader for whichever render pipeline the project uses (URP, HDRP or built-in).
    public static Shader FindLitShader()
    {
        RenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline;
        Shader shader = null;
        if (pipeline != null)
        {
            string pipelineType = pipeline.GetType().Name;
            if (pipelineType.Contains("HDRenderPipeline")) shader = Shader.Find("HDRP/Lit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Simple Lit");
        }
        if (shader == null) shader = Shader.Find("Standard");
        return shader;
    }

    public static void SetBaseColor(Material material, Color color)
    {
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
    }

    // ---------------------------------------------------------------------------------------------
    // Setup helpers used by the editor's prefab builder
    // ---------------------------------------------------------------------------------------------

    public void AssignParts(Transform bodyRoot, MeshFilter filter, MeshRenderer meshRenderer,
                            TMP_Text mainText, TMP_Text prompt, Transform dividerLine)
    {
        body = bodyRoot;
        shapeFilter = filter;
        shapeRenderer = meshRenderer;
        bodyText = mainText;
        promptText = prompt;
        divider = dividerLine;
        dirty = true;
    }

    public void ApplyPreset(Preset preset)
    {
        Color titleColor = new Color(0.98f, 0.95f, 0.88f);
        Color promptColor = new Color(0.80f, 0.76f, 0.70f);
        keyColor = new Color(0.96f, 0.78f, 0.42f);

        bevelSegments = 4;
        cornerSegments = 14;
        dividerThickness = 0.012f;
        dividerWidth = 0.55f;
        showDuration = 0.28f;
        hideDuration = 0.14f;

        if (preset == Preset.NameTag)
        {
            maxTextWidth = 4f;
            padding = new Vector2(0.3f, 0.17f);
            lineGap = 0.07f;
            minWidth = 1.2f;
            cornerRadius = 0.6f; // pill
            depth = 0.1f;
            bevel = 0.03f;
            tailPosition = 0.5f;
            tailWidth = 0.34f;
            tailLength = 0.2f;
            tailSlant = 0f;
            tailCurve = 0.2f;
            textLift = 0.005f;
            bobHeight = 0.02f;
            bobSpeed = 0.45f;
            StyleText(bodyText, 3.2f, titleColor, 2f, 0f);
            StyleText(promptText, 2f, promptColor, 4f, 0f);
            previewBody = "Shopkeeper";
            previewPrompt = "Press " + HighlightKey("E") + " to interact";
        }
        else
        {
            maxTextWidth = 3.8f;
            padding = new Vector2(0.32f, 0.22f);
            lineGap = 0.1f;
            minWidth = 1.6f;
            cornerRadius = 0.26f;
            depth = 0.12f;
            bevel = 0.035f;
            tailPosition = 0.4f;
            tailWidth = 0.32f;
            tailLength = 0.24f;
            tailSlant = 0.1f;
            tailCurve = 0.3f;
            textLift = 0.004f;
            bobHeight = 0.008f;
            bobSpeed = 0.35f;
            StyleText(bodyText, 1.25f, titleColor, 1f, -35f);
            StyleText(promptText, 0.85f, promptColor, 5f, 0f);
            previewBody = "You're not from around these parts, are you Partner? I'd advise you keep moving.";
            previewPrompt = "Press " + HighlightKey("Esc") + " to escape";
        }
        dirty = true;
    }

    /// Removes the generated mesh from the MeshFilter so it isn't saved into the prefab.
    public void ReleaseGeneratedMesh()
    {
        if (shapeFilter != null && shapeFilter.sharedMesh != null && shapeFilter.sharedMesh.name == GeneratedMeshName)
            shapeFilter.sharedMesh = null;
        if (mesh != null)
        {
            if (Application.isPlaying) Destroy(mesh);
            else DestroyImmediate(mesh);
            mesh = null;
        }
        dirty = true;
    }

    private static void StyleText(TMP_Text text, float size, Color color, float characterSpacing, float lineSpacing)
    {
        if (text == null) return;
        text.enableAutoSizing = false;
        text.fontSize = size;
        text.color = color;
        text.characterSpacing = characterSpacing;
        text.lineSpacing = lineSpacing;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Overflow;
        text.richText = true;
        text.isOrthographic = false;

        Renderer textRenderer = text.GetComponent<Renderer>();
        if (textRenderer != null)
        {
            textRenderer.shadowCastingMode = ShadowCastingMode.Off;
            textRenderer.receiveShadows = false;
        }
    }
}
