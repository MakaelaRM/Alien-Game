#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// Editor tool: Tools > HUD > Create Town HUD builds the screen-space HUD in the open scene
/// and wires it to a TownHUD component. Everything it makes can be edited by hand afterwards.
public static class TownHUDBuilder
{
    private const float Margin = 32f;

    private static readonly Color PanelColor  = new Color(0f, 0f, 0f, 0.6f);
    private static readonly Color TrackColor  = new Color(0f, 0f, 0f, 0.7f);
    private static readonly Color FillColor   = new Color32(100, 255, 50, 255);
    private static readonly Color BadgeRim    = new Color32(100, 255, 50, 255);
    private static readonly Color BadgeCenter = new Color(0.08f, 0.08f, 0.08f, 1f);

    // Placeholder icon colors until real sprites are dropped in
    private static readonly Color MoneyTint  = new Color(1f, 0.82f, 0.2f);
    private static readonly Color FoodTint   = new Color(0.9f, 0.3f, 0.25f);
    private static readonly Color DrinksTint = new Color(0.3f, 0.6f, 1f);

    [MenuItem("Tools/HUD/Create Town HUD")]
    private static void Create()
    {
        Sprite rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        Sprite circle  = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        TMP_FontAsset font = FindFont("kleader");

        var root = new GameObject("TownHUD", typeof(RectTransform), typeof(Canvas),
                                  typeof(CanvasScaler), typeof(CanvasGroup));
        Undo.RegisterCreatedObjectUndo(root, "Create Town HUD");

        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var group = root.GetComponent<CanvasGroup>();
        group.interactable = false;
        group.blocksRaycasts = false;

        RectTransform stats = CreateRect("Stats", root.transform);
        PinToCorner(stats, new Vector2(0f, 1f), new Vector2(Margin, -Margin));
        Style(stats.gameObject.AddComponent<Image>(), rounded, PanelColor);

        var row = stats.gameObject.AddComponent<HorizontalLayoutGroup>();
        row.padding = new RectOffset(16, 24, 10, 10);
        row.spacing = 28f;
        ConfigureRow(row);

        var fit = stats.gameObject.AddComponent<ContentSizeFitter>();
        fit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        TextMeshProUGUI moneyText  = CreateStat(stats, "Money",  LoadIcon("Icon_Money"),  circle, MoneyTint,  font);
        TextMeshProUGUI foodText   = CreateStat(stats, "Food",   LoadIcon("Icon_Food"),   circle, FoodTint,   font);
        TextMeshProUGUI drinksText = CreateStat(stats, "Drinks", LoadIcon("Icon_Drinks"), circle, DrinksTint, font);

        const float barHeight = 40f;
        const float badgeSize = 64f;

        RectTransform rep = CreateRect("Reputation", root.transform);
        PinToCorner(rep, new Vector2(1f, 1f), new Vector2(-Margin, -Margin - (badgeSize - barHeight) * 0.5f));
        rep.sizeDelta = new Vector2(440f, barHeight);

        RectTransform track = CreateRect("Track", rep);
        Stretch(track, 0f, 0f, 0f, 0f);
        Style(track.gameObject.AddComponent<Image>(), rounded, TrackColor);

        RectTransform fillArea = CreateRect("FillArea", track);
        Stretch(fillArea, badgeSize * 0.5f + 4f, 6f, 6f, 6f);

        RectTransform fill = CreateRect("Fill", fillArea);
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = new Vector2(0.35f, 1f);
        fill.offsetMin = Vector2.zero;
        fill.offsetMax = Vector2.zero;
        Style(fill.gameObject.AddComponent<Image>(), rounded, FillColor);

        RectTransform badge = CreateRect("LevelBadge", rep);
        badge.anchorMin = badge.anchorMax = new Vector2(0f, 0.5f);
        badge.pivot = new Vector2(0.5f, 0.5f);
        badge.anchoredPosition = Vector2.zero;
        badge.sizeDelta = new Vector2(badgeSize, badgeSize);
        Style(badge.gameObject.AddComponent<Image>(), circle, BadgeRim);

        RectTransform badgeInner = CreateRect("Center", badge);
        Stretch(badgeInner, 5f, 5f, 5f, 5f);
        Style(badgeInner.gameObject.AddComponent<Image>(), circle, BadgeCenter);

        TextMeshProUGUI levelText = CreateText("LevelText", badge, font, 32f, TextAlignmentOptions.Center);
        Stretch(levelText.rectTransform, 0f, 0f, 0f, 0f);

        var hud = root.AddComponent<TownHUD>();
        var so = new SerializedObject(hud);
        so.FindProperty("moneyText").objectReferenceValue = moneyText;
        so.FindProperty("foodText").objectReferenceValue = foodText;
        so.FindProperty("drinksText").objectReferenceValue = drinksText;
        so.FindProperty("reputationFill").objectReferenceValue = fill;
        so.FindProperty("reputationLevelText").objectReferenceValue = levelText;
        so.ApplyModifiedPropertiesWithoutUndo();

        EnsurePlayerStats();

        Selection.activeGameObject = root;
        Debug.Log("Town HUD created. Drag your icon sprites onto Stats > MoneyIcon, FoodIcon and DrinksIcon.", root);
    }

    private const string IconFolder = "Assets/UI/Icons/";

    private static Sprite LoadIcon(string name)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(IconFolder + name + ".png");
        if (sprite == null)
            Debug.LogWarning("Couldn't find a sprite at " + IconFolder + name + ".png, using a placeholder. " +
                             "Make sure its Texture Type is Sprite (2D and UI).");
        return sprite;
    }

    private static TextMeshProUGUI CreateStat(Transform parent, string label, Sprite iconSprite,
                                              Sprite placeholder, Color tint, TMP_FontAsset font)
    {
        if (iconSprite != null)
        {
            placeholder = iconSprite;
            tint = Color.white;
        }

        RectTransform entry = CreateRect(label, parent);
        var layout = entry.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 10f;
        ConfigureRow(layout);

        RectTransform iconRect = CreateRect(label + "Icon", entry);
        var icon = iconRect.gameObject.AddComponent<Image>();
        Style(icon, placeholder, tint);
        icon.preserveAspect = true;
        var iconSize = iconRect.gameObject.AddComponent<LayoutElement>();
        iconSize.minWidth = iconSize.preferredWidth = 44f;
        iconSize.minHeight = iconSize.preferredHeight = 44f;

        TextMeshProUGUI text = CreateText(label + "Text", entry, font, 36f, TextAlignmentOptions.MidlineLeft);
        var textSize = text.gameObject.AddComponent<LayoutElement>();
        textSize.minWidth = 48f;
        return text;
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, TMP_FontAsset font,
                                              float size, TextAlignmentOptions alignment)
    {
        RectTransform rect = CreateRect(name, parent);
        var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        if (font != null) text.font = font;
        text.fontSize = size;
        text.color = Color.white;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
        text.text = "0";
        return text;
    }

    private static void ConfigureRow(HorizontalLayoutGroup row)
    {
        row.childAlignment = TextAnchor.MiddleLeft;
        row.childControlWidth = true;
        row.childControlHeight = true;
        row.childForceExpandWidth = false;
        row.childForceExpandHeight = false;
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        return rect;
    }

    private static void PinToCorner(RectTransform rect, Vector2 corner, Vector2 offset)
    {
        rect.anchorMin = rect.anchorMax = corner;
        rect.pivot = corner;
        rect.anchoredPosition = offset;
    }

    private static void Stretch(RectTransform rect, float left, float right, float top, float bottom)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    private static void Style(Image image, Sprite sprite, Color color)
    {
        image.sprite = sprite;
        image.type = sprite != null && sprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
        image.color = color;
        image.raycastTarget = false;
    }

    private static TMP_FontAsset FindFont(string search)
    {
        foreach (string guid in AssetDatabase.FindAssets(search + " t:TMP_FontAsset"))
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
            if (font != null) return font;
        }
        Debug.LogWarning("Couldn't find the Kleader font asset, using TextMeshPro's default font.");
        return TMP_Settings.defaultFontAsset;
    }

    private static void EnsurePlayerStats()
    {
        if (Object.FindFirstObjectByType<PlayerStats>() != null) return;

        var player = Object.FindFirstObjectByType<PlayerInteract>();
        if (player != null)
        {
            Undo.AddComponent<PlayerStats>(player.gameObject);
            Debug.Log("Added PlayerStats to " + player.name + ".", player);
        }
        else
        {
            Debug.LogWarning("No PlayerStats in the scene. Add one to your Player so the HUD has numbers to show.");
        }
    }
}
#endif