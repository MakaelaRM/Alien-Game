using UnityEngine;

// Subtle lantern flicker. Added automatically to the lantern prefab by Mine > 2 - Create Prefabs.
[RequireComponent(typeof(Light))]
public class MineLightFlicker : MonoBehaviour
{
    [Range(0f, 0.5f)] public float amount = 0.12f;
    public float speed = 2.2f;

    Light _light;
    float _base;
    float _seed;

    void Awake()
    {
        _light = GetComponent<Light>();
        _base = _light.intensity;
        _seed = Random.value * 100f;
    }

    void Update()
    {
        float n = Mathf.PerlinNoise(_seed, Time.time * speed);
        _light.intensity = _base * (1f + (n - 0.5f) * 2f * amount);
    }
}
