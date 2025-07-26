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
    public float lightVal;
    public float medVal;
    public float heavyVal;
    [Range(0f,1f)]
    public float drivingValue;
    //

    // -- float rounding
    [Range(0f, 1f)]
    public float valueToRound;
    public float valueMultiplier;
    public int roundedResult;
    //

    void Start()
    {
        
    }

    void Update()
    {
        // - float rounding
        roundedResult = Mathf.RoundToInt((valueToRound * valueMultiplier) + 0.5f);
        roundedResult = Mathf.Clamp(roundedResult, 1, (int)valueMultiplier);

        // - dynamic audio blending
        lightVal = Mathf.Sin(Mathf.Clamp01( drivingValue / .5f ) * Mathf.PI);
        if (lightVal < 0.01f)
            lightVal = 0f;
        medVal = Mathf.Sin(Mathf.Clamp01( (drivingValue - .25f) / .5f ) * Mathf.PI);
        if (medVal < 0.01f)
            medVal = 0f;
        heavyVal = Mathf.Sin(Mathf.Clamp01( drivingValue - .5f ) * Mathf.PI);
        if (heavyVal < 0.01f)
            heavyVal = 0f;

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
