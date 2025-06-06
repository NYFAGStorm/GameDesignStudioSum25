using UnityEngine;

public class PlantManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles a single plant object


    private float plantTimer;
    private Renderer plantSprite;
    private PlotManager plot;

    // temp (use time manager multiplier)
    const float PLANTSTAGEDURATION = 10f;
    const float PLANTCHECKINTERVAL = 1f;


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
            plantTimer = PLANTCHECKINTERVAL;
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
                float progress = ( PLANTCHECKINTERVAL / PLANTSTAGEDURATION );

                // find resources amount as 50% sun and 50% water of plot
                float resources = ((0.5f * plot.data.sun) + (0.5f * plot.data.water));
                // calculate vitality
                float vitalityDelta = (0.5f - resources) * -0.1f;
                plot.data.plant.vitality = Mathf.Clamp01(plot.data.plant.vitality + vitalityDelta);
                // calculate health
                float healthDelta = (0.5f - plot.data.plant.vitality) + (0.5f - resources);
                healthDelta *= -0.001f;
                plot.data.plant.health = Mathf.Clamp01(plot.data.plant.health+healthDelta);
                // calculate growth
                float growthDelta = resources * 0.2f * progress * plot.data.plant.vitality;
                if (plot.data.plant.growth < 1f)
                {
                    plot.data.plant.growth = Mathf.Clamp01(plot.data.plant.growth + growthDelta);
                    // show growth change
                    Grow(plot.data.plant.growth);
                    // calculate quality
                    plot.data.plant.quality += growthDelta * plot.data.plant.vitality;
                }

                plantTimer = PLANTCHECKINTERVAL;
            }
        }
    }

    void Grow( float growth )
    {
        int growNumber = Mathf.RoundToInt(growth * 4f);
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
