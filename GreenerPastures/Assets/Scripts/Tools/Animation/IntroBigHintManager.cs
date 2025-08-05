using UnityEngine;

public class IntroBigHintManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This manages a big hint on the plot the player should work on in front of Eden

    private GameObject cursor;
    private GameObject arrow;
    private Vector3 savedArrowPos;

    void Start()
    {
        cursor = transform.GetChild(0).gameObject;
        arrow = transform.GetChild(1).gameObject;
        savedArrowPos = arrow.transform.localPosition;

        cursor.GetComponent<Renderer>().material.color = Color.yellow;
        arrow.GetComponent<Renderer>().material.color = Color.yellow;
    }

    void Update()
    {
        Color c = Color.yellow;
        c.a = (Mathf.Sin(Time.time * 6.18f) + 1f) * 0.5f;
        cursor.GetComponent<Renderer>().material.color = c;
        Vector3 pos = savedArrowPos;
        pos.y += (Mathf.Sin(Time.time * 3.81f) + 1f) * 0.25f;
        arrow.transform.localPosition = pos;
    }
}
