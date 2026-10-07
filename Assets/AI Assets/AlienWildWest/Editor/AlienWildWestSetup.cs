// Alien Wild West asset pack: one-time Unity setup.
//
// What it does (menu: Tools > Alien Wild West):
//   1. Set Up Materials   Creates one shared material per palette color in AlienWildWest/Materials
//                         and points every model in the pack at those shared materials.
//   2. Apply Sky, Fog and Lights to Open Scene   Sets the Deep Purple skybox, fog, ambient light and two suns.
//
// "Set Up Materials" also runs by itself the first time the pack is imported.
// This file must stay inside a folder named "Editor". It is safe to delete: the models keep working
// with the materials Unity imports from the FBX files.
#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AlienWildWest
{
    public static class AlienWildWestSetup
    {
        struct Entry
        {
            public string hex; public float glow; public float alpha; public bool metal;
            public Entry(string hex, float glow, float alpha, bool metal) { this.hex = hex; this.glow = glow; this.alpha = alpha; this.metal = metal; }
        }

        // name, color, glow strength (HDR emission multiplier, 0 = no glow), opacity, metallic
        static readonly Dictionary<string, Entry> Palette = new Dictionary<string, Entry>
        {
            { "Mat_Sand", new Entry("#E3B56F", 0f, 1f, false) },
            { "Mat_Terracotta", new Entry("#D2602F", 0f, 1f, false) },
            { "Mat_RustRock", new Entry("#A6402B", 0f, 1f, false) },
            { "Mat_ShadowRock", new Entry("#5A2E3A", 0f, 1f, false) },
            { "Mat_FadedWood", new Entry("#B08A5E", 0f, 1f, false) },
            { "Mat_DarkWood", new Entry("#6B4A35", 0f, 1f, false) },
            { "Mat_PaintedWood", new Entry("#6A3FB0", 0f, 1f, false) },
            { "Mat_Cactus", new Entry("#5E9E4A", 0f, 1f, false) },
            { "Mat_Foliage", new Entry("#9AA34F", 0f, 1f, false) },
            { "Mat_Bone", new Entry("#F2E6CF", 0f, 1f, false) },
            { "Mat_Metal", new Entry("#8D99A6", 0f, 1f, true) },
            { "Mat_DarkMetal", new Entry("#3A3F4B", 0f, 1f, true) },
            { "Mat_Chrome", new Entry("#D5DCE4", 0f, 1f, true) },
            { "Mat_Brass", new Entry("#D9A441", 0f, 1f, true) },
            { "Mat_SolarBlue", new Entry("#23406E", 0f, 1f, false) },
            { "Mat_Glass", new Entry("#BFE9F2", 0f, 0.35f, false) },
            { "Mat_Void", new Entry("#221A2B", 0f, 1f, false) },
            { "Mat_NeonLaser", new Entry("#19D8C4", 3f, 1f, false) },
            { "Mat_NeonGreen", new Entry("#5CFF3B", 3f, 1f, false) },
            { "Mat_NeonCyan", new Entry("#2EF2FF", 3f, 1f, false) },
            { "Mat_NeonCrystal", new Entry("#3DE0D0", 2.5f, 1f, false) },
            { "Mat_NeonAmber", new Entry("#FFB02E", 1.5f, 1f, false) },
            { "Mat_SlimeGreen", new Entry("#A6FF2E", 2f, 1f, false) },
            { "Mat_TractorBeam", new Entry("#9BFFF0", 1.5f, 0.4f, false) },
            { "Mat_HeroWhite", new Entry("#F1ECE0", 0f, 1f, false) },
            { "Mat_HeroRed", new Entry("#E2473B", 0f, 1f, false) },
            { "Mat_HeroBlue", new Entry("#2D6CDF", 0f, 1f, false) },
            { "Mat_HeroYellow", new Entry("#FFC83D", 0f, 1f, false) },
        };

        const string SkyTexture = "Tex_Sky_DeepPurple.png";

        static string PackRoot()
        {
            string[] guids = AssetDatabase.FindAssets("AlienWildWestSetup t:MonoScript");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith("/Editor/AlienWildWestSetup.cs")) continue;
                return path.Substring(0, path.Length - "/Editor/AlienWildWestSetup.cs".Length);
            }
            return null;
        }

        static Color Hex(string hex)
        {
            Color c;
            if (!ColorUtility.TryParseHtmlString(hex, out c)) c = Color.magenta;
            return c;
        }

        [InitializeOnLoadMethod]
        static void RunOnceAfterImport()
        {
            EditorApplication.delayCall += () =>
            {
                string root = PackRoot();
                if (root == null) return;
                if (File.Exists(root + "/Materials/Mat_Sand.mat")) return;   // already set up
                SetUpMaterials();
            };
        }

        [MenuItem("Tools/Alien Wild West/Set Up Materials")]
        public static void SetUpMaterials()
        {
            string root = PackRoot();
            if (root == null) { Debug.LogWarning("Alien Wild West: could not find the pack folder."); return; }
            string folder = root + "/Materials";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder(root, "Materials");

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) { Debug.LogWarning("Alien Wild West: no Lit shader found."); return; }

            int created = 0;
            foreach (KeyValuePair<string, Entry> kv in Palette)
            {
                string path = folder + "/" + kv.Key + ".mat";
                if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) continue;
                AssetDatabase.CreateAsset(BuildMaterial(shader, kv.Key, kv.Value), path);
                created++;
            }
            AssetDatabase.SaveAssets();

            // textures: full-size sky, no mipmaps on the sky
            TextureImporter sky = AssetImporter.GetAtPath(root + "/Textures/" + SkyTexture) as TextureImporter;
            if (sky != null && (sky.maxTextureSize != 4096 || sky.mipmapEnabled))
            {
                sky.maxTextureSize = 4096;
                sky.mipmapEnabled = false;
                sky.wrapMode = TextureWrapMode.Repeat;
                sky.SaveAndReimport();
            }

            // models: use the shared materials, found by name in AlienWildWest/Materials
            string[] modelGuids = AssetDatabase.FindAssets("t:Model", new[] { root + "/Models" });
            int remapped = 0;
            foreach (string guid in modelGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null) continue;
                importer.importAnimation = false;
                importer.importCameras = false;
                importer.importLights = false;
                importer.animationType = ModelImporterAnimationType.None;
                importer.SearchAndRemapMaterials(ModelImporterMaterialName.BasedOnMaterialName, ModelImporterMaterialSearch.RecursiveUp);
                importer.SaveAndReimport();
                remapped++;
            }
            Debug.Log("Alien Wild West: created " + created + " materials and set up " + remapped + " models.");
        }

        static Material BuildMaterial(Shader shader, string name, Entry e)
        {
            Material m = new Material(shader);
            m.name = name;
            Color c = Hex(e.hex);
            c.a = e.alpha;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            if (m.HasProperty("_Color")) m.SetColor("_Color", c);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", e.metal ? 0.7f : 0f);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", e.metal ? 0.5f : 0.1f);
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", e.metal ? 0.5f : 0.1f);

            if (e.glow > 0f)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", new Color(c.r, c.g, c.b, 1f) * e.glow);
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            }

            if (e.alpha < 1f)
            {
                // see-through, drawn from both sides
                if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 1f);
                if (m.HasProperty("_Blend")) m.SetFloat("_Blend", 0f);
                if (m.HasProperty("_SrcBlend")) m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                if (m.HasProperty("_DstBlend")) m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                if (m.HasProperty("_ZWrite")) m.SetFloat("_ZWrite", 0f);
                if (m.HasProperty("_Cull")) m.SetFloat("_Cull", 0f);
                m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                m.SetOverrideTag("RenderType", "Transparent");
                m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
            return m;
        }

        [MenuItem("Tools/Alien Wild West/Apply Sky, Fog and Lights to Open Scene")]
        public static void ApplySceneLook()
        {
            string root = PackRoot();
            if (root == null) { Debug.LogWarning("Alien Wild West: could not find the pack folder."); return; }

            // skybox
            string skyMatPath = root + "/Materials/Mat_Sky_DeepPurple.mat";
            Material skyMat = AssetDatabase.LoadAssetAtPath<Material>(skyMatPath);
            if (skyMat == null)
            {
                Shader skyShader = Shader.Find("Skybox/Panoramic");
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(root + "/Textures/" + SkyTexture);
                if (skyShader != null && tex != null)
                {
                    skyMat = new Material(skyShader);
                    skyMat.SetTexture("_MainTex", tex);
                    if (!AssetDatabase.IsValidFolder(root + "/Materials")) AssetDatabase.CreateFolder(root, "Materials");
                    AssetDatabase.CreateAsset(skyMat, skyMatPath);
                }
            }
            if (skyMat != null) RenderSettings.skybox = skyMat;

            // fog and ambient light
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = Hex("#5A3A9A");
            RenderSettings.fogStartDistance = 70f;
            RenderSettings.fogEndDistance = 260f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = Hex("#544C6C");
            RenderSettings.ambientEquatorColor = Hex("#3E354C");
            RenderSettings.ambientGroundColor = Hex("#281F2B");

            // two suns
            Light sun = FindOrCreateLight("AWW Sun");
            sun.color = Hex("#FFE0B4");
            sun.intensity = 1.0f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(35f, 211f, 0f);

            Light second = FindOrCreateLight("AWW Second Sun");
            second.color = Hex("#A07CFF");
            second.intensity = 0.35f;
            second.shadows = LightShadows.None;
            second.transform.rotation = Quaternion.Euler(19f, 59f, 0f);

            RenderSettings.sun = sun;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("Alien Wild West: sky, fog and lights applied to the open scene.");
        }

        static Light FindOrCreateLight(string name)
        {
            GameObject go = GameObject.Find(name);
            if (go == null)
            {
                go = new GameObject(name);
                Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            }
            Light light = go.GetComponent<Light>();
            if (light == null) light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            return light;
        }
    }
}
#endif
