// Alien Wild West - Town Hall clock, bell and weathervane.
// Add this component to the Bldg_TownHall object in your scene and press Play.
// The clock has 13 hours. The bell swings once per hour struck (13 swings at the thirteenth hour).
using UnityEngine;
using UnityEngine.Events;

namespace AlienWildWest
{
    [DisallowMultipleComponent]
    public class TownHallClock : MonoBehaviour
    {
        [Header("Time")]
        [Tooltip("Clock time in hours, 0 to 13.")]
        [Range(0f, 13f)] public float hours = 4.6f;
        [Tooltip("How many game minutes pass per real second. 1 = one game hour per real minute.")]
        public float gameMinutesPerSecond = 1f;
        [Tooltip("The thin teal hand spins backwards, once per game minute. Untick for a normal seconds hand.")]
        public bool thirdHandRunsBackwards = true;

        [Header("Bell")]
        public bool ringOnTheHour = true;
        [Tooltip("Seconds for one full swing of the bell.")]
        public float swingPeriod = 1.4f;
        [Tooltip("Largest swing angle in degrees.")]
        public float swingAngle = 32f;
        [Tooltip("Optional: plays this clip once per swing.")]
        public AudioSource bellAudio;
        public AudioClip bellClip;

        [Header("Weathervane")]
        public bool spinWeathervane = true;

        [Header("Events")]
        [Tooltip("Called with the hour (1 to 13) each time the clock strikes.")]
        public UnityEvent<int> onHourStruck = new UnityEvent<int>();

        class Hand
        {
            public Transform t;
            public Vector3 axis;        // outward face direction, in the parent's space
            public float rest;          // angle the hand points at in the model, clockwise from 12
            public Quaternion restRot;
            public int kind;            // 0 hour, 1 minute, 2 third
        }

        Hand[] handList = new Hand[0];
        Transform bell, vane;
        Quaternion bellRest, vaneRest;
        Vector3 bellAxis, vaneAxis;
        int lastHour;
        int ringsLeft, ringsDone;
        float ringTime;

        void Start()
        {
            System.Collections.Generic.List<Transform> hands = new System.Collections.Generic.List<Transform>();
            foreach (Transform c in GetComponentsInChildren<Transform>(true))
            {
                if (c.name.Contains("Clock_Hour") || c.name.Contains("Clock_Minute") || c.name.Contains("Clock_Third")) hands.Add(c);
                else if (c.name.EndsWith("_Bell")) bell = c;
                else if (c.name.EndsWith("_Weathervane")) vane = c;
            }

            // the tower's centre line runs through the bell (or the building origin if the bell is missing)
            Vector3 towerWorld = bell != null ? bell.position : transform.position;
            handList = new Hand[hands.Count];
            for (int i = 0; i < hands.Count; i++)
            {
                Transform h = hands[i];
                Transform p = h.parent;
                Vector3 up = p.InverseTransformDirection(transform.up);
                Vector3 centre = p.InverseTransformPoint(h.position);
                Vector3 axis = Vector3.ProjectOnPlane(centre - p.InverseTransformPoint(towerWorld), up);
                if (axis.sqrMagnitude < 1e-6f) axis = p.InverseTransformDirection(transform.forward);
                axis.Normalize();

                Vector3 tip = Vector3.zero;
                MeshFilter mf = h.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null) tip = p.InverseTransformPoint(h.TransformPoint(mf.sharedMesh.bounds.center)) - centre;
                tip = Vector3.ProjectOnPlane(tip, axis);

                Hand hand = new Hand();
                hand.t = h;
                hand.axis = axis;
                hand.rest = tip.sqrMagnitude > 1e-8f ? Vector3.SignedAngle(Vector3.ProjectOnPlane(up, axis), tip, axis) : 0f;
                hand.restRot = h.localRotation;
                hand.kind = h.name.Contains("Clock_Hour") ? 0 : (h.name.Contains("Clock_Minute") ? 1 : 2);
                handList[i] = hand;
            }

            if (bell != null)
            {
                bellRest = bell.localRotation;
                bellAxis = bell.parent.InverseTransformDirection(transform.right).normalized;
            }
            if (vane != null)
            {
                vaneRest = vane.localRotation;
                vaneAxis = vane.parent.InverseTransformDirection(transform.up).normalized;
            }

            hours = Mathf.Repeat(hours, 13f);
            lastHour = Mathf.FloorToInt(hours);
            ApplyHands();
        }

        void Update()
        {
            hours = Mathf.Repeat(hours + Time.deltaTime * gameMinutesPerSecond / 60f, 13f);
            int hour = Mathf.FloorToInt(hours);
            if (hour != lastHour)
            {
                lastHour = hour;
                int struck = hour == 0 ? 13 : hour;
                if (ringOnTheHour) RingBell(struck);
                onHourStruck.Invoke(struck);
            }
            ApplyHands();
            UpdateBell();
            UpdateVane();
        }

        /// <summary>Jump the clock to a time (0 to 13 hours) without ringing.</summary>
        public void SetTime(float newHours)
        {
            hours = Mathf.Repeat(newHours, 13f);
            lastHour = Mathf.FloorToInt(hours);
            ApplyHands();
        }

        /// <summary>Swing the bell a number of times.</summary>
        public void RingBell(int times)
        {
            ringsLeft = Mathf.Max(0, times);
            ringsDone = 0;
            ringTime = 0f;
        }

        void ApplyHands()
        {
            float hourAngle = hours / 13f * 360f;
            float minutes = (hours - Mathf.Floor(hours)) * 60f;
            float minuteAngle = minutes / 60f * 360f;
            float thirdAngle = (minutes - Mathf.Floor(minutes)) * 360f * (thirdHandRunsBackwards ? -1f : 1f);
            for (int i = 0; i < handList.Length; i++)
            {
                Hand h = handList[i];
                if (h.t == null) continue;
                float a = h.kind == 0 ? hourAngle : (h.kind == 1 ? minuteAngle : thirdAngle);
                // a positive angle around the outward axis turns clockwise for someone facing the clock
                h.t.localRotation = Quaternion.AngleAxis(a - h.rest, h.axis) * h.restRot;
            }
        }

        void UpdateBell()
        {
            if (bell == null) return;
            if (ringsLeft <= 0) { bell.localRotation = bellRest; return; }
            ringTime += Time.deltaTime;
            float period = Mathf.Max(0.2f, swingPeriod);
            float total = ringsLeft * period;
            float ease = Mathf.Clamp01(ringTime / (period * 0.5f)) * Mathf.Clamp01((total - ringTime) / (period * 0.5f));
            float angle = swingAngle * ease * Mathf.Sin(ringTime / period * 2f * Mathf.PI);
            bell.localRotation = Quaternion.AngleAxis(angle, bellAxis) * bellRest;

            if (ringsDone < ringsLeft && ringTime >= (ringsDone + 0.25f) * period)
            {
                ringsDone++;
                if (bellAudio != null && bellClip != null) bellAudio.PlayOneShot(bellClip);
            }
            if (ringTime >= total) { ringsLeft = 0; bell.localRotation = bellRest; }
        }

        void UpdateVane()
        {
            if (vane == null || !spinWeathervane) return;
            float t = Time.time;
            float angle = 40f * Mathf.Sin(t * 0.35f) + 12f * Mathf.Sin(t * 1.7f) + 4f * Mathf.Sin(t * 5.3f);
            vane.localRotation = Quaternion.AngleAxis(angle, vaneAxis) * vaneRest;
        }
    }
}
