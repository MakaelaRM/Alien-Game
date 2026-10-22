using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// Screen-space HUD shown while walking around town
/// Create it with Tools > HUD > Create Town HUD, which builds the layout and fills in these fields.
[RequireComponent(typeof(CanvasGroup))]
public class TownHUD : MonoBehaviour
{
    [Header("Stat Counters (top left)")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI foodText;
    [SerializeField] private TextMeshProUGUI drinksText;

    [Header("Reputation Bar (top right)")]
    [Tooltip("The bar's fill. Its width is driven by its right anchor, so no special sprite is needed.")]
    [SerializeField] private RectTransform reputationFill;
    [SerializeField] private TextMeshProUGUI reputationLevelText;

    [Header("Animation")]
    [Tooltip("How fast the HUD fades in and out (alpha per second).")]
    [SerializeField] private float fadeSpeed = 6f;
    [Tooltip("Seconds to wait after the HUD fades back in before changes start animating.")]
    [SerializeField] private float revealDelay = 0.5f;
    [Tooltip("How fast the reputation bar fills (full bars per second).")]
    [SerializeField] private float fillSpeed = 0.6f;
    [Tooltip("How big a number pops when it changes.")]
    [SerializeField] private float punchScale = 1.35f;
    [SerializeField] private float punchDuration = 0.55f;
    [Tooltip("Color a stat number flashes when it goes up.")]
    [SerializeField] private Color increaseColor = new Color(0.4f, 1f, 0.45f);

    [Header("Popups")]
    [Tooltip("Text under the bar showing +/- rep. Created automatically if left empty.")]
    [SerializeField] private TextMeshProUGUI reputationPopupText;
    [SerializeField] private TextMeshProUGUI moneyPopupText;
    [SerializeField] private TextMeshProUGUI foodPopupText;
    [SerializeField] private TextMeshProUGUI drinksPopupText;
    [SerializeField] private Color reputationGainColor = new Color(0.4f, 1f, 0.45f);
    [SerializeField] private Color reputationLossColor = new Color(1f, 0.35f, 0.3f);
    [Tooltip("How long the popup stays on screen, including its fade out.")]
    [SerializeField] private float popupDuration = 1.8f;
    [Tooltip("How far the popup drifts down while it's showing (pixels).")]
    [SerializeField] private float popupDrift = 14f;
    [SerializeField] private float statPopupFontSize = 22f;

    private CanvasGroup group;
    private PlayerInteract player;
    private PlayerStats stats;

    private int shownMoney, shownFood, shownDrinks, shownLevel, shownTotalRep;
    private float shownFill;
    private bool synced;
    private float visibleTime;

    private Punch moneyPunch, foodPunch, drinksPunch, levelPunch;
    private Popup repPopup, moneyPopup, foodPopup, drinksPopup;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        group.interactable = false;
        group.blocksRaycasts = false;

        moneyPunch  = new Punch(moneyText);
        foodPunch   = new Punch(foodText);
        drinksPunch = new Punch(drinksText);
        levelPunch  = new Punch(reputationLevelText);

        if (reputationPopupText == null) reputationPopupText = CreateRepPopupText();
        if (moneyPopupText == null)  moneyPopupText  = CreateStatPopupText(moneyText,  "MoneyPopup");
        if (foodPopupText == null)   foodPopupText   = CreateStatPopupText(foodText,   "FoodPopup");
        if (drinksPopupText == null) drinksPopupText = CreateStatPopupText(drinksText, "DrinksPopup");

        repPopup    = new Popup(reputationPopupText, " REP");
        moneyPopup  = new Popup(moneyPopupText, "");
        foodPopup   = new Popup(foodPopupText, "");
        drinksPopup = new Popup(drinksPopupText, "");
    }

    void Start()
    {
        player = FindFirstObjectByType<PlayerInteract>();
        stats = PlayerStats.Instance;
        if (stats == null)
            Debug.LogWarning("TownHUD can't find a PlayerStats. Add one to your Player.", this);
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;

        // Hide while in interact mode
        bool visible = player == null || !player.IsInteracting;
        group.alpha = Mathf.MoveTowards(group.alpha, visible ? 1f : 0f, fadeSpeed * dt);

        if (stats == null)
        {
            stats = PlayerStats.Instance;
            if (stats == null) return;
        }

        if (!synced)
        {
            SnapToStats();
            synced = true;
        }

        if (visible && group.alpha >= 0.99f) visibleTime += dt;
        else visibleTime = 0f;

        if (visibleTime >= revealDelay)
        {
            UpdateCounter(moneyText,  stats.Money,  ref shownMoney,  moneyPunch,  moneyPopup);
            UpdateCounter(foodText,   stats.Food,   ref shownFood,   foodPunch,   foodPopup);
            UpdateCounter(drinksText, stats.Drinks, ref shownDrinks, drinksPunch, drinksPopup);
            UpdateReputation(dt);
            CheckReputationPopup();
        }

        moneyPunch.Tick(dt, punchDuration, punchScale);
        foodPunch.Tick(dt, punchDuration, punchScale);
        drinksPunch.Tick(dt, punchDuration, punchScale);
        levelPunch.Tick(dt, punchDuration, punchScale);

        repPopup.Tick(dt, popupDuration, popupDrift);
        moneyPopup.Tick(dt, popupDuration, popupDrift);
        foodPopup.Tick(dt, popupDuration, popupDrift);
        drinksPopup.Tick(dt, popupDuration, popupDrift);
    }

    private void SnapToStats()
    {
        shownMoney = stats.Money;
        shownFood = stats.Food;
        shownDrinks = stats.Drinks;
        shownLevel = stats.ReputationLevel;
        shownFill = stats.ReputationProgress;
        shownTotalRep = stats.TotalReputation;

        SetText(moneyText, shownMoney);
        SetText(foodText, shownFood);
        SetText(drinksText, shownDrinks);
        SetText(reputationLevelText, shownLevel);
        SetFill(shownFill);
    }

    private void UpdateCounter(TextMeshProUGUI text, int value, ref int shown, Punch punch, Popup popup)
    {
        if (value == shown) return;
        int delta = value - shown;
        shown = value;
        SetText(text, value);

        if (delta > 0) punch.Play(increaseColor);
        else punch.Play();

        popup.Show(delta, popupDuration, reputationGainColor, reputationLossColor);
    }

    private void UpdateReputation(float dt)
    {
        int targetLevel = stats.ReputationLevel;
        float targetFill = stats.ReputationProgress;
        float step = fillSpeed * dt;

        if (shownLevel < targetLevel)
        {
            shownFill = Mathf.MoveTowards(shownFill, 1f, step);
            if (shownFill >= 1f)
            {
                shownLevel++;
                shownFill = 0f;
                SetText(reputationLevelText, shownLevel);
                levelPunch.Play();
            }
        }
        else if (shownLevel > targetLevel)
        {
            shownLevel = targetLevel;
            shownFill = targetFill;
            SetText(reputationLevelText, shownLevel);
        }
        else
        {
            shownFill = Mathf.MoveTowards(shownFill, targetFill, step);
        }

        SetFill(shownFill);
    }

    private void SetFill(float amount)
    {
        if (reputationFill == null) return;
        reputationFill.anchorMin = Vector2.zero;
        reputationFill.anchorMax = new Vector2(Mathf.Clamp01(amount), 1f);
        reputationFill.offsetMin = Vector2.zero;
        reputationFill.offsetMax = Vector2.zero;
    }

    private static void SetText(TextMeshProUGUI text, int value)
    {
        if (text != null) text.text = value.ToString();
    }

    private void CheckReputationPopup()
    {
        int total = stats.TotalReputation;
        if (total == shownTotalRep) return;

        int delta = total - shownTotalRep;
        shownTotalRep = total;
        repPopup.Show(delta, popupDuration, reputationGainColor, reputationLossColor);
    }

    // Builds the popup text under the bar if one wasn't assigned in the Inspector
    private TextMeshProUGUI CreateRepPopupText()
    {
        if (reputationFill == null) return null;

        Transform bar = reputationFill;
        while (bar != null && bar.name != "Reputation") bar = bar.parent;
        if (bar == null) bar = reputationFill.parent;

        TextMeshProUGUI text = CreatePopupText("ReputationPopup", bar, 28f, TextAlignmentOptions.Right);
        RectTransform rect = text.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-4f, -8f);
        rect.sizeDelta = new Vector2(220f, 36f);
        return text;
    }

    private TextMeshProUGUI CreateStatPopupText(TextMeshProUGUI statText, string name)
    {
        if (statText == null || statText.transform.parent == null) return null;

        TextMeshProUGUI text = CreatePopupText(name, statText.transform.parent, statPopupFontSize, TextAlignmentOptions.Center);
        text.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;

        RectTransform rect = text.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -16f);
        rect.sizeDelta = new Vector2(120f, 28f);
        return text;
    }

    private TextMeshProUGUI CreatePopupText(string name, Transform parent, float size, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var text = go.AddComponent<TextMeshProUGUI>();
        if (reputationLevelText != null) text.font = reputationLevelText.font;
        text.fontSize = size;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
        return text;
    }

    // Briefly scales a number up and eases it back down, optionally flashing a color
    private class Punch
    {
        private readonly TextMeshProUGUI text;
        private readonly Color baseColor;
        private Color flashColor;
        private bool flashing;
        private float time = float.MaxValue;

        public Punch(TextMeshProUGUI text)
        {
            this.text = text;
            if (text != null) baseColor = text.color;
        }

        public void Play()
        {
            time = 0f;
            flashing = false;
            if (text != null) text.color = baseColor;
        }

        public void Play(Color flash)
        {
            time = 0f;
            flashing = true;
            flashColor = flash;
        }

        public void Tick(float dt, float duration, float scale)
        {
            if (text == null || time >= duration) return;

            time += dt;
            float k = Mathf.Clamp01(time / duration);
            float easeOut = 1f - (1f - k) * (1f - k);
            text.transform.localScale = Vector3.one * Mathf.Lerp(scale, 1f, easeOut);

            if (flashing)
                text.color = Color.Lerp(flashColor, baseColor, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.6f, 1f, k)));
        }
    }

    private class Popup
    {
        private readonly TextMeshProUGUI text;
        private readonly string suffix;
        private readonly Vector2 home;
        private int amount;
        private float time = float.MaxValue;

        public Popup(TextMeshProUGUI text, string suffix)
        {
            this.text = text;
            this.suffix = suffix;
            if (text != null)
            {
                home = text.rectTransform.anchoredPosition;
                text.alpha = 0f;
            }
        }

        public void Show(int delta, float duration, Color gain, Color loss)
        {
            if (text == null || delta == 0) return;

            bool active = time < duration;
            if (active && (delta > 0) == (amount > 0)) amount += delta;
            else amount = delta;

            text.text = (amount > 0 ? "+" : "") + amount + suffix;
            text.color = amount > 0 ? gain : loss;
            text.alpha = 1f;
            time = 0f;
        }

        public void Tick(float dt, float duration, float drift)
        {
            if (text == null || time >= duration) return;

            time += dt;
            float s = time;

            float scale = s < 0.12f ? Mathf.Lerp(0.6f, 1.15f, s / 0.12f)
                        : s < 0.24f ? Mathf.Lerp(1.15f, 1f, (s - 0.12f) / 0.12f)
                        : 1f;
            float fadeStart = duration * 0.6f;
            float alpha = s < fadeStart ? 1f : 1f - Mathf.InverseLerp(fadeStart, duration, s);

            text.transform.localScale = Vector3.one * scale;
            text.alpha = alpha;
            text.rectTransform.anchoredPosition = home + Vector2.down * (drift * Mathf.Clamp01(s / duration));
        }
    }
}