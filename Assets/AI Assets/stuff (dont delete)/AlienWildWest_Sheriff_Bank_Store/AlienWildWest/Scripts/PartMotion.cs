// Alien Wild West - simple looping motion for the separate moving parts of a model.
// Add it to a child part (for example Bldg_Fountain_Saucer), pick a motion and press Play.
using UnityEngine;

namespace AlienWildWest
{
    public class PartMotion : MonoBehaviour
    {
        public enum Motion { Spin, Swing, Bob, LookAround }

        public Motion motion = Motion.Spin;
        [Tooltip("Axis in the parent's space. (0,1,0) is up.")]
        public Vector3 axis = Vector3.up;
        [Tooltip("Spin: degrees per second. Swing and Look Around: largest angle in degrees. Bob: height in metres.")]
        public float amount = 45f;
        [Tooltip("Seconds for one swing, bob or glance.")]
        public float period = 2f;
        [Tooltip("Offset in seconds, so copies of the same model don't move in step.")]
        public float phase = 0f;

        Quaternion restRot;
        Vector3 restPos;
        float spinAngle;
        Quaternion lookFrom = Quaternion.identity;
        Quaternion lookTo = Quaternion.identity;
        float lookTimer;

        void Start()
        {
            restRot = transform.localRotation;
            restPos = transform.localPosition;
        }

        void Update()
        {
            float t = Time.time + phase;
            Vector3 ax = axis.sqrMagnitude > 1e-6f ? axis.normalized : Vector3.up;
            float p = Mathf.Max(0.05f, period);
            float wave = Mathf.Sin(t / p * 2f * Mathf.PI);

            switch (motion)
            {
                case Motion.Spin:
                    spinAngle = Mathf.Repeat(spinAngle + amount * Time.deltaTime, 360f);
                    transform.localRotation = Quaternion.AngleAxis(spinAngle, ax) * restRot;
                    break;
                case Motion.Swing:
                    transform.localRotation = Quaternion.AngleAxis(amount * wave, ax) * restRot;
                    break;
                case Motion.Bob:
                    transform.localPosition = restPos + ax * (amount * wave);
                    break;
                case Motion.LookAround:
                    lookTimer -= Time.deltaTime;
                    if (lookTimer <= 0f)
                    {
                        lookFrom = lookTo;
                        lookTo = Quaternion.Euler(Random.Range(-amount, amount) * 0.5f, Random.Range(-amount, amount), 0f);
                        lookTimer = p;
                    }
                    float k = Mathf.SmoothStep(0f, 1f, 1f - lookTimer / p);
                    transform.localRotation = Quaternion.Slerp(lookFrom, lookTo, k) * restRot;
                    break;
            }
        }
    }
}
