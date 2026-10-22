using System.IO;
using UnityEditor;
using UnityEngine;

// Mine interior setup tools. Adds a "Mine" menu to the Unity menu bar.
//   Mine > 1 - Create Materials and Fix Models
//   Mine > 2 - Create Prefabs
//   Mine > 3 - Build Test Tunnel In Scene
//   Mine > 4 - Create Floor Plane (40 x 40)
//   Mine > 5 - Create Mine Exit Prefab (with return trigger)
//   Mine > Apply Ground Texture To Selected Planes
public static class MineSetup
{
    const string Root = "Assets/MineInterior";
    const string ModelsDir = Root + "/Models";
    const string MatsDir = Root + "/Materials";
    const string TexDir = Root + "/Textures";
    const string PrefabDir = Root + "/Prefabs";

    // One ground texture tile covers this many metres.
    const float GroundTileMetres = 4f;

    class Def
    {
        public string Name, Color, Texture, Emission;
        public float EmissionIntensity, Smoothness, Metallic;
    }

    static Def D(string name, string color, string tex = null, string emission = null,
                 float ei = 0f, float smooth = 0.1f, float metal = 0f)
    {
        return new Def
        {
            Name = name, Color = color, Texture = tex, Emission = emission,
            EmissionIntensity = ei, Smoothness = smooth, Metallic = metal
        };
    }

    static readonly Def[] Defs =
    {
        D("Mine_Ground", "#FFFFFF", "Mine_Ground", smooth: 0.05f),
        D("Mine_Rock_Top", "#6A5EB4", smooth: 0.05f),
        D("Mine_Rock_Side", "#4A3F8C", smooth: 0.05f),
        D("Mine_Rock_SideAlt", "#3D3580", smooth: 0.05f),
        D("Mine_Rock_Under", "#2B2460", smooth: 0.05f),
        D("Mine_Timber", "#6E5063"),
        D("Mine_TimberDark", "#4D3646"),
        D("Mine_Iron", "#4A5272", smooth: 0.35f, metal: 0.3f),
        D("Mine_IronDark", "#2C3150", smooth: 0.3f, metal: 0.4f),
        D("Mine_RailSteel", "#8A93B8", smooth: 0.45f, metal: 0.3f),
        D("Mine_CrystalCyan", "#7FE8F0", emission: "#7FE8F0", ei: 2.0f, smooth: 0.8f),
        D("Mine_CrystalTeal", "#2FC4D6", emission: "#2FC4D6", ei: 1.6f, smooth: 0.8f),
        D("Mine_LanternGlow", "#FFC46B", emission: "#FFC46B", ei: 3.0f, smooth: 0.5f),
        D("Mine_Amber", "#FFC46B"),
        D("Mine_DaylightGlow", "#FFE9B8", emission: "#FFE9B8", ei: 4.0f),
        D("Mine_Rock", "#2B2562", smooth: 0.05f),
        D("Mine_RockLight", "#4B3F85", smooth: 0.05f),
        D("Mine_Sack", "#8A7FA8"),
        D("Mine_Dark", "#141733"),
    };

    static readonly string[] BoxColliderProps =
    {
        "Mine_Crate", "Mine_Barrel", "Mine_Cart", "Mine_Cart_Loaded", "Mine_Ore_Pile", "Mine_Ore_Sack",
        "Mine_Crystal_Cluster_Large"
    };

    static bool IsRockPiece(string n)
    {
        return n.StartsWith("Mine_Wall_Rock") || n.StartsWith("Mine_Ceiling_Rock") || n.StartsWith("Mine_Rock_") ||
               n.StartsWith("Mine_Exit");
    }

    static void EnsureFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + name))
            AssetDatabase.CreateFolder(parent, name);
    }

    static Color Hex(string hex)
    {
        Color c;
        if (!ColorUtility.TryParseHtmlString(hex, out c)) c = Color.white;
        return c;
    }

    // ------------------------------------------------------------------
    // Step 1
    // ------------------------------------------------------------------
    [MenuItem("Mine/1 - Create Materials and Fix Models")]
    public static void CreateMaterialsAndFixModels()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            Debug.LogError("[Mine] Could not find the 'Universal Render Pipeline/Lit' shader. Is URP installed and active?");
            return;
        }

        EnsureFolder(Root, "Materials");
        var made = new System.Collections.Generic.Dictionary<string, Material>();

        foreach (Def d in Defs)
        {
            string path = MatsDir + "/" + d.Name + ".mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.shader = shader;
            }

            mat.SetColor("_BaseColor", Hex(d.Color));
            mat.SetFloat("_Smoothness", d.Smoothness);
            mat.SetFloat("_Metallic", d.Metallic);

            if (!string.IsNullOrEmpty(d.Texture))
            {
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexDir + "/" + d.Texture + ".png");
                if (tex != null) mat.SetTexture("_BaseMap", tex);
                else Debug.LogWarning("[Mine] Missing texture " + d.Texture);
            }

            if (!string.IsNullOrEmpty(d.Emission))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Hex(d.Emission) * d.EmissionIntensity);
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            }

            EditorUtility.SetDirty(mat);
            made[d.Name] = mat;
        }

        AssetDatabase.SaveAssets();

        int count = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { ModelsDir }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var imp = AssetImporter.GetAtPath(path) as ModelImporter;
            if (imp == null) continue;

            string n = Path.GetFileNameWithoutExtension(path);
            imp.globalScale = 1f;
            imp.importNormals = ModelImporterNormals.Import;
            imp.importBlendShapes = false;
            imp.importCameras = false;
            imp.importLights = false;
            imp.meshCompression = ModelImporterMeshCompression.Off;
            imp.addCollider = IsRockPiece(n);
            imp.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            foreach (var kv in made)
                imp.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), kv.Key), kv.Value);
            imp.SaveAndReimport();
            count++;
        }

        Debug.Log("[Mine] Created " + made.Count + " materials and set up " + count + " models. Next: Mine > 2 - Create Prefabs");
    }

    // ------------------------------------------------------------------
    // Step 2
    // ------------------------------------------------------------------
    [MenuItem("Mine/2 - Create Prefabs")]
    public static void CreatePrefabs()
    {
        EnsureFolder(Root, "Prefabs");
        int count = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { ModelsDir }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (model == null) continue;

            string n = Path.GetFileNameWithoutExtension(path);
            GameObject go = (GameObject)Object.Instantiate(model);
            go.name = n;
            Decorate(go, n);
            PrefabUtility.SaveAsPrefabAsset(go, PrefabDir + "/" + n + ".prefab");
            Object.DestroyImmediate(go);
            count++;
        }
        AssetDatabase.SaveAssets();
        Debug.Log("[Mine] Created " + count + " prefabs in " + PrefabDir + ". Next: Mine > 3 - Build Test Tunnel In Scene");
    }

    static void Decorate(GameObject go, string n)
    {
        switch (n)
        {
            case "Mine_Lantern_Post":
                AddLight(go, new Vector3(0.52f, 1.66f, 0f), "#FFC46B", 4f, 9f, true);
                break;
            case "Mine_Crystal_Cluster_Large":
                AddLight(go, new Vector3(0f, 0.8f, 0f), "#7FE8F0", 3f, 7f, false);
                break;
            case "Mine_Crystal_Shards_Small":
                AddLight(go, new Vector3(0f, 0.3f, 0f), "#7FE8F0", 1.5f, 3.5f, false);
                break;
            case "Mine_Cart_Loaded":
                AddLight(go, new Vector3(0f, 1.7f, 0f), "#7FE8F0", 1.5f, 3f, false);
                break;
            case "Mine_Wall_Rock_Vein":
                AddLight(go, new Vector3(0.6f, 2.2f, 2f), "#7FE8F0", 2.5f, 6f, false);
                break;
            case "Mine_Ceiling_Rock_Crystals":
                AddLight(go, new Vector3(0f, -0.5f, 2f), "#7FE8F0", 2.5f, 6f, false);
                break;
            case "Mine_Exit":
                // daylight spilling in from the far end, the hanging lantern, and a little cyan from the crystals
                AddLight(go, new Vector3(0f, 1.8f, 2.6f), "#FFE2A8", 8f, 14f, false);
                AddLight(go, new Vector3(-0.6f, 2.4f, -0.5f), "#FFC46B", 3f, 7f, true);
                AddLight(go, new Vector3(0f, 4.5f, 0f), "#7FE8F0", 2f, 6f, false);
                break;
        }

        if (System.Array.IndexOf(BoxColliderProps, n) >= 0) AddBox(go);

        if (IsRockPiece(n) || n.StartsWith("Mine_Support") || n.StartsWith("Mine_Track"))
            go.isStatic = true;
    }

    static void AddLight(GameObject parent, Vector3 localPos, string hex, float intensity, float range, bool flicker)
    {
        var l = new GameObject("Light");
        l.transform.SetParent(parent.transform, false);
        l.transform.localPosition = localPos;
        var light = l.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = Hex(hex);
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.None;
        if (flicker) l.AddComponent<MineLightFlicker>();
    }

    static void AddBox(GameObject go)
    {
        var rs = go.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return;
        Bounds b = rs[0].bounds;
        foreach (var r in rs) b.Encapsulate(r.bounds);
        var bc = go.AddComponent<BoxCollider>();
        bc.center = go.transform.InverseTransformPoint(b.center);
        bc.size = b.size;
    }

    // ------------------------------------------------------------------
    // Floor: a plain Unity Plane with the ground texture tiled across it
    // ------------------------------------------------------------------
    static Material GroundMaterialFor(float width, float length)
    {
        var baseMat = AssetDatabase.LoadAssetAtPath<Material>(MatsDir + "/Mine_Ground.mat");
        if (baseMat == null)
        {
            Debug.LogWarning("[Mine] Run Mine > 1 first so Mine_Ground exists.");
            return null;
        }
        string path = MatsDir + "/Mine_Ground_" + Mathf.RoundToInt(width) + "x" + Mathf.RoundToInt(length) + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(baseMat);
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.SetTextureScale("_BaseMap", new Vector2(width / GroundTileMetres, length / GroundTileMetres));
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static GameObject MakeFloor(Transform parent, Vector3 center, float width, float length)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
        go.name = "Mine_Floor";
        go.transform.SetParent(parent, false);
        go.transform.position = center;
        go.transform.localScale = new Vector3(width / 10f, 1f, length / 10f);
        var mat = GroundMaterialFor(width, length);
        if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
        go.isStatic = true;
        return go;
    }

    [MenuItem("Mine/4 - Create Floor Plane (40 x 40)")]
    public static void CreateFloorPlane()
    {
        var go = MakeFloor(null, Vector3.zero, 40f, 40f);
        Undo.RegisterCreatedObjectUndo(go, "Create mine floor");
        Selection.activeGameObject = go;
    }

    [MenuItem("Mine/Apply Ground Texture To Selected Planes")]
    public static void ApplyGroundToSelection()
    {
        int n = 0;
        foreach (var go in Selection.gameObjects)
        {
            var r = go.GetComponent<Renderer>();
            if (r == null) continue;
            Vector3 size = r.bounds.size;
            var mat = GroundMaterialFor(size.x, size.z);
            if (mat == null) return;
            Undo.RecordObject(r, "Apply mine ground");
            r.sharedMaterial = mat;
            n++;
        }
        AssetDatabase.SaveAssets();
        Debug.Log("[Mine] Applied the ground texture to " + n + " object(s), one tile per " + GroundTileMetres + " m.");
    }

    // ------------------------------------------------------------------
    // Mine exit with a trigger that loads the town
    // ------------------------------------------------------------------
    // Change this if your town scene has a different name.
    const string TownSceneName = "Town";

    [MenuItem("Mine/5 - Create Mine Exit Prefab (with return trigger)")]
    public static void CreateExitPrefab()
    {
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/Mine_Exit.prefab");
        if (source == null)
        {
            Debug.LogWarning("[Mine] Run Mine > 2 - Create Prefabs first.");
            return;
        }

        var go = (GameObject)PrefabUtility.InstantiatePrefab(source);
        go.name = "Mine_Exit_WithTrigger";
        PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

        var trig = new GameObject("ExitTrigger");
        trig.transform.SetParent(go.transform, false);
        trig.transform.localPosition = new Vector3(0f, 1.3f, 1.6f);
        var box = trig.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(2.0f, 2.6f, 0.8f);

        // Uses your own SceneTransition script (the one from the mine entrance) if it exists in the project.
        var type = System.Type.GetType("SceneTransition, Assembly-CSharp");
        if (type != null)
        {
            var comp = trig.AddComponent(type);
            var so = new SerializedObject(comp);
            var prop = so.FindProperty("sceneToLoad");
            if (prop != null)
            {
                prop.stringValue = TownSceneName;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            Debug.Log("[Mine] Added SceneTransition to the exit trigger, loading scene '" + TownSceneName + "'.");
        }
        else
        {
            Debug.LogWarning("[Mine] No SceneTransition script found. Add your transition script to Mine_Exit_WithTrigger/ExitTrigger by hand.");
        }

        PrefabUtility.SaveAsPrefabAsset(go, PrefabDir + "/Mine_Exit_WithTrigger.prefab");
        Object.DestroyImmediate(go);
        AssetDatabase.SaveAssets();
        Debug.Log("[Mine] Created " + PrefabDir + "/Mine_Exit_WithTrigger.prefab");
    }

    // ------------------------------------------------------------------
    // Step 3: test tunnel
    // ------------------------------------------------------------------
    static GameObject Spawn(string name, Vector3 pos, float yaw, Transform parent)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/" + name + ".prefab");
        if (prefab == null) prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ModelsDir + "/" + name + ".obj");
        if (prefab == null)
        {
            Debug.LogWarning("[Mine] Could not find " + name);
            return null;
        }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        return go;
    }

    static Bounds GetBounds(GameObject go)
    {
        var rs = go.GetComponentsInChildren<Renderer>();
        Bounds b = rs[0].bounds;
        foreach (var r in rs) b.Encapsulate(r.bounds);
        return b;
    }

    // Wall rock: the rock mass must sit on the far side of the wall line, away from the tunnel centre.
    // Unity can mirror OBJ files on import, so check the real bounds and turn the piece if needed.
    static void Wall(string name, float wallX, int side, float z, Transform parent)
    {
        var g = Spawn(name, new Vector3(wallX, 0f, z), 0f, parent);
        if (g == null) return;
        Bounds b = GetBounds(g);
        bool massOutward = (b.center.x - wallX) * side > 0f;
        if (!massOutward)
        {
            g.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            g.transform.position = new Vector3(wallX, 0f, z + 4f);
        }
    }

    // Turn a prop around if its body ends up on the wrong side of its pivot.
    static void FaceDirX(GameObject g, float dirX)
    {
        if (g == null) return;
        Bounds b = GetBounds(g);
        if ((b.center.x - g.transform.position.x) * dirX < 0f)
            g.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
    }

    [MenuItem("Mine/3 - Build Test Tunnel In Scene")]
    public static void BuildTestTunnel()
    {
        var root = new GameObject("Mine_TestTunnel");
        Undo.RegisterCreatedObjectUndo(root, "Build mine test tunnel");
        Transform t = root.transform;

        MakeFloor(t, new Vector3(0f, 0f, 12f), 8f, 24f);

        string[] leftWalls = { "Mine_Wall_Rock_A", "Mine_Wall_Rock_Vein", "Mine_Wall_Rock_B", "Mine_Wall_Rock_C" };
        string[] rightWalls = { "Mine_Wall_Rock_C", "Mine_Wall_Rock_A", "Mine_Wall_Rock_B", "Mine_Wall_Rock_Vein" };
        string[] ceilings = { "Mine_Ceiling_Rock_A", "Mine_Ceiling_Rock_Crystals", "Mine_Ceiling_Rock_B" };

        const int segments = 6;
        for (int i = 0; i < segments; i++)
        {
            float z = 4f * i;
            Spawn(ceilings[i % ceilings.Length], new Vector3(0f, 4f, z), 0f, t);
            Wall(leftWalls[i % leftWalls.Length], -2f, -1, z, t);
            Wall(rightWalls[(i + 1) % rightWalls.Length], 2f, 1, z, t);
            Spawn("Mine_Track_Straight_4m", new Vector3(0f, 0f, z), 0f, t);
            if (i % 2 == 0) Spawn("Mine_Support_Frame", new Vector3(0f, 0f, z), 0f, t);
        }

        // the exit closes the far end of the tunnel, with the rails running out through it
        var exitName = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/Mine_Exit_WithTrigger.prefab") != null
            ? "Mine_Exit_WithTrigger" : "Mine_Exit";
        Spawn(exitName, new Vector3(0f, 0f, 24.2f), 0f, t);
        Spawn("Mine_Track_Straight_4m", new Vector3(0f, 0f, 24.2f), 0f, t);
        Spawn("Mine_Warning_Sign", new Vector3(-1.6f, 0f, 3f), 0f, t);

        FaceDirX(Spawn("Mine_Lantern_Post", new Vector3(-1.5f, 0f, 6f), 0f, t), 1f);
        FaceDirX(Spawn("Mine_Lantern_Post", new Vector3(1.5f, 0f, 14f), 0f, t), -1f);

        Spawn("Mine_Cart_Loaded", new Vector3(0f, 0f, 9f), 0f, t);
        Spawn("Mine_Cart", new Vector3(0f, 0f, 15f), 0f, t);

        Spawn("Mine_Crystal_Cluster_Large", new Vector3(1.35f, 0f, 11f), 20f, t);
        Spawn("Mine_Crystal_Shards_Small", new Vector3(-1.4f, 0f, 3f), 0f, t);
        Spawn("Mine_Crystal_Shards_Small", new Vector3(1.2f, 0f, 18f), 70f, t);

        Spawn("Mine_Crate", new Vector3(-1.3f, 0f, 15f), 15f, t);
        Spawn("Mine_Barrel", new Vector3(-1.3f, 0f, 16.4f), 0f, t);
        Spawn("Mine_Ore_Sack", new Vector3(-0.95f, 0f, 14f), 40f, t);
        Spawn("Mine_Ore_Pile", new Vector3(1.2f, 0f, 4.5f), 0f, t);
        Spawn("Mine_Tool_Rack", new Vector3(-1.9f, 0f, 11f), 90f, t);

        Spawn("Mine_Rock_Boulder_A", new Vector3(1.6f, 0f, 7.5f), 80f, t);

        Selection.activeGameObject = root;
        Debug.Log("[Mine] Built a test tunnel. Try it in a new empty scene, away from your town.");
    }
}
