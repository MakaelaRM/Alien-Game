using UnityEngine;
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
    [Tooltip("How fast the reputation bar fills (full bars per second).")]
    [SerializeField] private float fillSpeed = 1.2f;
    [Tooltip("How big a number pops when it changes.")]
    [SerializeField] private float punchScale = 1.35f;
    [SerializeField] private float punchDuration = 0.3f;
    [Tooltip("Color a stat number flashes when it goes up.")]
    [SerializeField] private Color increaseColor = new Color(0.4f, 1f, 0.45f);

    private CanvasGroup group;
    private PlayerInteract player;
    private PlayerStats stats;

    private int shownMoney, shownFood, shownDrinks, shownLevel;
    private float shownFill;
    private bool synced;

    private Punch moneyPunch, foodPunch, drinksPunch, levelPunch;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        group.interactable = false;
        group.blocksRaycasts = false;

        moneyPunch  = new Punch(moneyText);
        foodPunch   = new Punch(foodText);
        drinksPunch = new Punch(drinksText);
        levelPunch  = new Punch(reputationLevelText);
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

        // Rewards earned during an interaction wait until the HUD is back on screen,
        // so the player sees the numbers pop and the bar fill as they walk away.
        if (visible && group.alpha > 0.9f)
        {
            UpdateCounter(moneyText,  stats.Money,  ref shownMoney,  moneyPunch);
            UpdateCounter(foodText,   stats.Food,   ref shownFood,   foodPunch);
            UpdateCounter(drinksText, stats.Drinks, ref shownDrinks, drinksPunch);
            UpdateReputation(dt);
        }

        moneyPunch.Tick(dt, punchDuration, punchScale);
        foodPunch.Tick(dt, punchDuration, punchScale);
        drinksPunch.Tick(dt, punchDuration, punchScale);
        levelPunch.Tick(dt, punchDuration, punchScale);
    }

    private void SnapToStats()
    {
        shownMoney = stats.Money;
        shownFood = stats.Food;
        shownDrinks = stats.Drinks;
        shownLevel = stats.ReputationLevel;
        shownFill = stats.ReputationProgress;

        SetText(moneyText, shownMoney);
        SetText(foodText, shownFood);
        SetText(drinksText, shownDrinks);
        SetText(reputationLevelText, shownLevel);
        SetFill(shownFill);
    }

    private void UpdateCounter(TextMeshProUGUI text, int value, ref int shown, Punch punch)
    {
        if (value == shown) return;
        bool increased = value > shown;
        shown = value;
        SetText(text, value);

        if (increased) punch.Play(increaseColor);
        else punch.Play();
    }

    private void UpdateReputation(float dt)
    {
        int targetLevel = stats.ReputationLevel;
        float targetFill = stats.ReputationProgress;
        float step = fillSpeed * dt;

        if (shownLevel < targetLevel)
        {
            // Fill the bar to the end, then level up and start again from empty
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
            // Level went down (e.g. a reset): no animation
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

            // Stay fully colored for most of the pop, then blend back at the end
            if (flashing)
                text.color = Color.Lerp(flashColor, baseColor, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.6f, 1f, k)));
        }
    }
}