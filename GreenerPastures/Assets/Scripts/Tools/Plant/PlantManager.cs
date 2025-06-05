using UnityEngine;

public class PlantManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles a single plant object


    private float plantTimer;
    private Renderer plantSprite;
    private PlotManager plot;

    const float PLANTSTAGEDURATION = 5f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // validate
        plot = transform.parent.gameObject.GetComponent<PlotManager>();
        if ( plot == null )
        {
            Debug.LogError("--- PlantManager [Start] : "+gameObject.name+" no parent plot found. aborting.");
            enabled = false;
        }
        plantSprite = transform.Find("Plant Sprite").gameObject.GetComponent<Renderer>();
        if ( plantSprite == null )
        {
            Debug.LogError("--- PlantManager [Start] : " + gameObject.name + " no renderer found in children. aborting.");
            enabled = false;
        }
        // initialize
        if ( enabled )
        {
            plantTimer = PLANTSTAGEDURATION;
        }
    }

    void Update()
    {
        // run plant timer
        if ( plantTimer > 0f )
        {
            plantTimer -= Time.deltaTime;
            if ( plantTimer < 0f )
            {
                // temp
                plot.data.plant.plantGrowth = Mathf.Clamp01(plot.data.plant.plantGrowth+0.2f);
                Grow(plot.data.plant.plantGrowth);
                if (plot.data.plant.plantGrowth < 1f)
                    plantTimer = PLANTSTAGEDURATION;
            }
        }
    }

    void Grow( float growth )
    {
        int growNumber = Mathf.RoundToInt(growth * 5f);
        switch (growNumber)
        {
            case 0:
                break;
            case 1:
                plantSprite.material.mainTexture = (Texture2D)Resources.Load("ProtoPlant01"); ;
                break;
            case 2:
                plantSprite.material.mainTexture = (Texture2D)Resources.Load("ProtoPlant02"); ;
                break;
            case 3:
                plantSprite.material.mainTexture = (Texture2D)Resources.Load("ProtoPlant03"); ;
                break;
            case 4:
                plantSprite.material.mainTexture = (Texture2D)Resources.Load("ProtoPlant04"); ;
                break;
        }
    }
}
