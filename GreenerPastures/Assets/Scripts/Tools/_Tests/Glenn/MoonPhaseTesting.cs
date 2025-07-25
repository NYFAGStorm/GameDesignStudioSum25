using UnityEngine;

public class MoonPhaseTesting : MonoBehaviour
{
    // Author: Glenn Storm
    // A general testing script, mostly to fine tune forumlae that would take too long to test in game

    // -- moon phase
    public int dayOfMonth = 1;
    public float moonPhase;

    public Renderer moonRenderer;
    // 

    // -- dynamic audio blend
    public float light;
    public float med;
    public float heavy;
    [Range(0f,1f)]
    public float drivingValue;
    //

    void Start()
    {
        
    }

    void Update()
    {
        // - dynamic audio blending
        light = Mathf.Sin(Mathf.Clamp01( drivingValue / .5f ) * Mathf.PI);
        if (light < 0.01f)
            light = 0f;
        med = Mathf.Sin(Mathf.Clamp01( (drivingValue - .25f) / .5f ) * Mathf.PI);
        if (med < 0.01f)
            med = 0f;
        heavy = Mathf.Sin(Mathf.Clamp01( drivingValue - .5f ) * Mathf.PI);
        if (heavy < 0.01f)
            heavy = 0f;

        // - moon phase -
        if (Input.GetKeyDown(KeyCode.Space))
        {
            dayOfMonth++;
            if (dayOfMonth > 30)
                dayOfMonth = 1;
            moonPhase = Mathf.Sin(((dayOfMonth) / (float)30) * Mathf.PI);
            if (moonPhase < 0.001f)
                moonPhase = 0f;
            moonPhase = 1f - moonPhase; // makes 15th new moon, 30th full moon
            moonRenderer.material.SetFloat("_MoonPhase", ((float)dayOfMonth / 30f));
        }
    }
}
