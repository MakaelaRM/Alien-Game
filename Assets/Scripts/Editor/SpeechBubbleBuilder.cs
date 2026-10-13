#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using TMPro;

/// Tools > Speech Bubbles > Rebuild Bubble Prefabs
///
/// Converts ChatBubble.prefab and DialogueBubble.prefab into 3D speech bubbles (SpeechBubble3D).
/// The prefabs are edited in place, so every NPC that already references them keeps working.
/// Safe to run again at any time: it rebuilds the parts and re-applies the default styling,
/// but keeps the BubbleFace / BubbleRim materials so your color tweaks survive.
public static class SpeechBubbleBuilder
{
    private const string DefaultFolder = "Assets/UI";

    [MenuItem("Tools/Speech Bubbles/Rebuild Bubble Prefabs")]
    public static void RebuildBubblePrefabs()
    {
        string chatPath = FindPrefabPath("ChatBubble") ?? DefaultFolder + "/ChatBubble.prefab";
        string dialoguePath = FindPrefabPath("DialogueBubble") ?? DefaultFolder + "/DialogueBubble.prefab";

        string folder = Path.GetDirectoryName(chatPath).Replace('\\', '/');
        EnsureFolder(folder);
        EnsureFolder(Path.GetDirectoryName(dialoguePath).Replace('\\', '/'));
        string materialFolder = folder + "/Materials";
        EnsureFolder(materialFolder);

        TMP_FontAsset font = FindFont(chatPath, dialoguePath);
        if (font == null)
            Debug.LogWarning("Speech bubbles: couldn't find the Kleader font, so TextMeshPro's default font will be used.");

        Material face = GetOrCreateMaterial(materialFolder + "/BubbleFace.mat",
            new Color(0.06f, 0.055f, 0.07f), 0.3f, Color.black);
        Material rim = GetOrCreateMaterial(materialFolder + "/BubbleRim.mat",
            new Color(0.92f, 0.84f, 0.68f), 0.45f, new Color(0.10f, 0.085f, 0.06f));

        bool chatOk = Build(chatPath, SpeechBubble3D.Preset.NameTag, font, face, rim, false);
        bool dialogueOk = Build(dialoguePath, SpeechBubble3D.Preset.Dialogue, font, face, rim, true);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (chatOk && dialogueOk)
            Debug.Log("Speech bubbles rebuilt as 3D:\n" + chatPath + "\n" + dialoguePath);

        GameObject result = AssetDatabase.LoadAssetAtPath<GameObject>(dialoguePath);
        if (result != null) EditorGUIUtility.PingObject(result);
    }

    private static bool Build(string path, SpeechBubble3D.Preset preset, TMP_FontAsset font,
                              Material face, Material rim, bool withDivider)
    {
        bool exists = AssetDatabase.LoadAssetAtPath<GameObject>(path) != null;

        // Editing the existing prefab's contents (instead of overwriting the file) keeps its
        // GUID and root object, so references from NPCs stay connected.
        GameObject root = exists
            ? PrefabUtility.LoadPrefabContents(path)
            : new GameObject(Path.GetFileNameWithoutExtension(path));

        bool success = false;
        try
        {
            StripOldContent(root);

            Transform rootTransform = root.transform;
            rootTransform.localPosition = Vector3.zero;
            rootTransform.localRotation = Quaternion.identity;
            rootTransform.localScale = Vector3.one;
            root.SetActive(true);

            Transform body = CreateChild("Body", rootTransform).transform;

            GameObject shape = CreateChild("Shape", body);
            MeshFilter filter = shape.AddComponent<MeshFilter>();
            MeshRenderer shapeRenderer = shape.AddComponent<MeshRenderer>();
            shapeRenderer.sharedMaterials = new[] { face, rim };
            shapeRenderer.shadowCastingMode = ShadowCastingMode.Off;
            shapeRenderer.receiveShadows = false;

            TMP_Text bodyText = CreateText("BodyText", body, font);
            TMP_Text promptText = CreateText("PromptText", body, font);
            Transform divider = withDivider ? CreateDivider(body, rim) : null;

            SpeechBubble3D bubble = root.GetComponent<SpeechBubble3D>();
            if (bubble == null) bubble = root.AddComponent<SpeechBubble3D>();
            bubble.AssignParts(body, filter, shapeRenderer, bodyText, promptText, divider);
            bubble.ApplyPreset(preset);
            bubble.ReleaseGeneratedMesh();

            PrefabUtility.SaveAsPrefabAsset(root, path, out success);
            if (!success) Debug.LogError("Speech bubbles: failed to save " + path);
        }
        finally
        {
            if (exists) PrefabUtility.UnloadPrefabContents(root);
            else Object.DestroyImmediate(root);
        }
        return success;
    }

    // Removes the old canvas-based bubble: all children plus the Canvas/UI components on the root.
    private static void StripOldContent(GameObject root)
    {
        for (int i = root.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(root.transform.GetChild(i).gameObject);

        // Order matters: components that require a Canvas have to go before the Canvas itself.
        DestroyAll<GraphicRaycaster>(root);
        DestroyAll<CanvasScaler>(root);
        DestroyAll<Graphic>(root);
        DestroyAll<CanvasGroup>(root);
        DestroyAll<Canvas>(root);
        DestroyAll<CanvasRenderer>(root);

        // A Billboard would fight the rotation NPCInteractable sets
        foreach (MonoBehaviour behaviour in root.GetComponents<MonoBehaviour>())
        {
            if (behaviour != null && behaviour.GetType().Name == "Billboard")
                Object.DestroyImmediate(behaviour);
        }
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
    }

    private static void DestroyAll<T>(GameObject go) where T : Component
    {
        foreach (T component in go.GetComponents<T>())
            Object.DestroyImmediate(component);
    }

    private static GameObject CreateChild(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go;
    }

    private static TMP_Text CreateText(string name, Transform parent, TMP_FontAsset font)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        TextMeshPro text = go.AddComponent<TextMeshPro>(); // 3D text, not the UI version
        if (font != null)
        {
            text.font = font;
            text.fontSharedMaterial = font.material;
        }
        text.text = string.Empty;
        return text;
    }

    private static Transform CreateDivider(Transform parent, Material material)
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "Divider";
        Object.DestroyImmediate(quad.GetComponent<Collider>());
        quad.transform.SetParent(parent, false);

        MeshRenderer quadRenderer = quad.GetComponent<MeshRenderer>();
        quadRenderer.sharedMaterial = material;
        quadRenderer.shadowCastingMode = ShadowCastingMode.Off;
        quadRenderer.receiveShadows = false;
        return quad.transform;
    }

    private static Material GetOrCreateMaterial(string path, Color color, float smoothness, Color emission)
    {
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;

        Shader shader = SpeechBubble3D.FindLitShader();
        if (shader == null)
        {
            Debug.LogError("Speech bubbles: couldn't find a lit shader for " + path);
            return null;
        }

        Material material = new Material(shader);
        SpeechBubble3D.SetBaseColor(material, color);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness); // URP / HDRP
        if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", smoothness); // built-in
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
        if (emission.maxColorComponent > 0f && material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emission);
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
        }

        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static string FindPrefabPath(string prefabName)
    {
        string best = null;
        foreach (string guid in AssetDatabase.FindAssets(prefabName + " t:Prefab"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.StartsWith("Assets/")) continue;
            if (Path.GetFileNameWithoutExtension(path) != prefabName) continue;
            if (best == null || path.StartsWith(DefaultFolder + "/")) best = path;
        }
        return best;
    }

    // Uses the font from the existing bubble prefabs (Kleader), falling back to a project search.
    private static TMP_FontAsset FindFont(params string[] prefabPaths)
    {
        foreach (string path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;
            TMP_Text text = prefab.GetComponentInChildren<TMP_Text>(true);
            if (text != null && text.font != null) return text.font;
        }

        foreach (string guid in AssetDatabase.FindAssets("Kleader t:TMP_FontAsset"))
        {
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
            if (font != null) return font;
        }

        return TMP_Settings.defaultFontAsset;
    }

    private static void EnsureFolder(string folder)
    {
        if (string.IsNullOrEmpty(folder) || AssetDatabase.IsValidFolder(folder)) return;
        string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
    }
}
#endif
