using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the color of the far background layer elements

    public GameObject[] childObjects;
    public Color[] elementColors;

    private Color[] currentColors;
    private Color lowColor;

    const float LOWRED = 0.01f;
    const float LOWGREEN = 0.015f;
    const float LOWBLUE = 0.025f;
    const float LOWALPHA = 1f;


    void Start()
    {
        // validate
        if (gameObject.transform.childCount == 0)
        {
            Debug.LogError("--- BackgroundManager [Start] : no child objects found on this object. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            childObjects = new GameObject[gameObject.transform.childCount];
            elementColors = new Color[gameObject.transform.childCount];
            for ( int i = 0; i < childObjects.Length; i++ )
            {
                childObjects[i] = gameObject.transform.GetChild(i).gameObject;
                elementColors[i] = childObjects[i].GetComponent<Renderer>().material.color;
            }
            currentColors = elementColors;
            lowColor = new Color(LOWRED,LOWGREEN,LOWBLUE,LOWALPHA);
        }
    }

    void Update()
    {
        // update current colors
        for (int i = 0; i < childObjects.Length; i++)
        {
            childObjects[i].GetComponent<Renderer>().material.color = Color.Lerp((lowColor * elementColors[i]), elementColors[i], RenderSettings.ambientIntensity);
        }
    }
}
