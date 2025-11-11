using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightFlicker : MonoBehaviour
{
    public Light2D light2D;
    public float baseIntensity = 1.2f;
    public float amplitude = 0.25f;
    public float speed = 2.5f;

    void Reset() { light2D = GetComponent<Light2D>(); }

    void Update()
    {
        if (!light2D) return;
        float n = Mathf.PerlinNoise(Time.time * speed, 0f);
        light2D.intensity = baseIntensity + (n - 0.5f) * 2f * amplitude;
    }
}
