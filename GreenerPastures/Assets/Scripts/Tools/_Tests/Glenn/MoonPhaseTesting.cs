using UnityEngine;

public class MoonPhaseTesting : MonoBehaviour
{
    public int dayOfMonth = 1;
    public float moonPhase;

    public Renderer moonRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
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
