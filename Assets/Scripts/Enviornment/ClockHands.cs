using UnityEngine;
using System;

// Put this on Bldg_TownHall (or any parent of the clock hands).
// It finds every child whose name contains "Clock_Hour", "Clock_Minute" or "Clock_Third"
// (Clock_Third = the seconds hand) and turns them like a real clock.
public class ClockHands : MonoBehaviour
{
    [Header("Time")]
    [Tooltip("On = shows your computer's real time. Off = the clock runs on its own speed below.")]
    public bool useRealTime = false;

    [Tooltip("Only used when Use Real Time is off. 1 = a normal clock, 60 = a minute passes every second.")]
    public float speedMultiplier = 5f;

    [Tooltip("Only used when Use Real Time is off. Starting time in hours (0-12).")]
    public float startHour = 3f;

    [Header("How the hands turn")]
    [Tooltip("The hand's own local axis it spins around. Try (0,0,1). If it spins the wrong way, tick Reverse All. If it wobbles, try (0,1,0) or (1,0,0).")]
    public Vector3 spinAxis = new Vector3(0, 0, 1);

    [Tooltip("Tick this if the hands go anticlockwise.")]
    public bool reverseAll = false;

    [Tooltip("Hands whose names end with these are reversed. Handy if one face (like _L or _R) turns backwards. Example: _L")]
    public string[] reverseSuffixes = new string[0];

    [Tooltip("Smooth sweeping hands (on) or tick once per second (off).")]
    public bool smoothSeconds = true;

    [Header("Names to look for (part of the object name)")]
    public string hourKey = "Clock_Hour";
    public string minuteKey = "Clock_Minute";
    public string secondKey = "Clock_Third";

    class Hand
    {
        public Transform t;
        public Quaternion start;
        public int kind;      // 0 hour, 1 minute, 2 second
        public float sign;    // 1 or -1
    }

    Hand[] hands;
    double fakeSeconds;       // seconds since 12:00 when not using real time

    void Start()
    {
        FindHands();
        fakeSeconds = startHour * 3600.0;
    }

    [ContextMenu("Find Hands (prints what it found)")]
    void FindHands()
    {
        var list = new System.Collections.Generic.List<Hand>();
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t == transform) continue;
            string n = t.name.ToLower();
            int kind = -1;
            if (n.Contains(hourKey.ToLower())) kind = 0;
            else if (n.Contains(minuteKey.ToLower())) kind = 1;
            else if (n.Contains(secondKey.ToLower())) kind = 2;
            if (kind < 0) continue;

            float sign = reverseAll ? -1f : 1f;
            foreach (var s in reverseSuffixes)
                if (!string.IsNullOrEmpty(s) && t.name.ToLower().EndsWith(s.ToLower())) sign = -sign;

            list.Add(new Hand { t = t, start = t.localRotation, kind = kind, sign = sign });
        }
        hands = list.ToArray();
        if (hands.Length == 0)
            Debug.LogWarning("[ClockHands] No clock hands found under " + name + ". Check the names contain " + hourKey + ", " + minuteKey + " or " + secondKey + ".", this);
        else
            Debug.Log("[ClockHands] Found " + hands.Length + " hands.", this);
    }

    void Update()
    {
        if (hands == null || hands.Length == 0) return;

        double seconds; // seconds since 12:00 (0 - 43200)
        if (useRealTime)
        {
            var now = DateTime.Now;
            seconds = now.Hour % 12 * 3600.0 + now.Minute * 60.0 + now.Second + now.Millisecond / 1000.0;
        }
        else
        {
            fakeSeconds += Time.deltaTime * speedMultiplier;
            seconds = fakeSeconds % 43200.0;
        }

        float secondAngle = (float)((smoothSeconds ? seconds % 60.0 : Math.Floor(seconds % 60.0)) * 6.0);
        float minuteAngle = (float)((seconds % 3600.0) / 60.0 * 6.0);
        float hourAngle = (float)((seconds % 43200.0) / 3600.0 * 30.0);

        Vector3 axis = spinAxis.sqrMagnitude < 0.0001f ? Vector3.forward : spinAxis.normalized;

        foreach (var h in hands)
        {
            if (h.t == null) continue;
            float a = h.kind == 0 ? hourAngle : h.kind == 1 ? minuteAngle : secondAngle;
            h.t.localRotation = h.start * Quaternion.AngleAxis(a * h.sign, axis);
        }
    }
}
