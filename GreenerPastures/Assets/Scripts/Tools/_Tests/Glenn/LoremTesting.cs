using UnityEngine;

public class LoremTesting : MonoBehaviour
{
    // Author: Glenn Storm
    // Goofy stupid fun, converting any string to lorem ipsum text

    public bool goLorem;
    public string playString;

    public AlmanacData almanac;


    void Start()
    {
        almanac = AlmanacSystem.InitializeAlmanac();
    }

    void Update()
    {
        if (goLorem)
        {
            playString = AlmanacSystem.ConvertToLorem(playString);
            goLorem = false;
        }
    }
}
