using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    public float minIntensity = 1f;
    public float maxIntensity = 3.81f;
    public float noiseSpeed = 1f;

    private Light thisLight;
    private Vector2 offset;


    void Start()
    {
        // validate
        thisLight = GetComponent<Light>();
        if (thisLight == null)
        {
            Debug.LogError("--- LightFlicker [Start] : " + gameObject.name + " light component not found on this object. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            // find random location on perlin noise landscape
            offset = new Vector2(RandomSystem.GaussianRandom01(), RandomSystem.FlatRandom01());
            offset *= RandomSystem.WeightedRandom01() * 6.18f;
        }
    }

    void Update()
    {
        float flickerIntensity = minIntensity;
        float flickerAmp = maxIntensity - minIntensity;
        flickerAmp *= Mathf.PerlinNoise( offset.x, offset.y + (Time.time * noiseSpeed) );
        flickerIntensity += flickerAmp;
        thisLight.intensity = flickerIntensity;
    }
}
