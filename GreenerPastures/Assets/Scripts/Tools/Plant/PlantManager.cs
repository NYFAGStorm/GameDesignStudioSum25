using UnityEngine;

public class PlantManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles a single plant object

    private float plantTimer;
    private Renderer plantImage;
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
        plantImage = transform.Find("Plant Image").gameObject.GetComponent<Renderer>();
        if ( plantImage == null )
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

                // find resources amount as an average of sun, water and soil quality
                // if even a little (25%) sun is available, this counts as 100% sun resource
                float resources = (Mathf.Clamp01(plot.data.sun*4f) + plot.data.water + plot.data.soil) / 3f;
                // calculate vitality
                float vitalityDelta = (0.667f - resources) * -0.1f;
                plot.data.plant.vitality = Mathf.Clamp01(plot.data.plant.vitality + vitalityDelta);
                // calculate health
                float healthDelta = (0.5f - plot.data.plant.vitality) + (0.5f - resources);
                healthDelta *= -0.001f;
                plot.data.plant.health = Mathf.Clamp01(plot.data.plant.health+healthDelta);
                // calculate growth
                float growthDelta = resources * 0.2f * progress * 
                    plot.data.plant.vitality * plot.data.plant.growthRate * 
                    plot.data.plant.adjustedGrowthRate;
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
                plantImage.material.mainTexture = (Texture2D)Resources.Load("ProtoPlant01"); ;
                break;
            case 2:
                plantImage.material.mainTexture = (Texture2D)Resources.Load("ProtoPlant02"); ;
                break;
            case 3:
                plantImage.material.mainTexture = (Texture2D)Resources.Load("ProtoPlant03"); ;
                break;
            case 4:
                plantImage.material.mainTexture = (Texture2D)Resources.Load("ProtoPlant04"); ;
                break;
        }
    }
}
