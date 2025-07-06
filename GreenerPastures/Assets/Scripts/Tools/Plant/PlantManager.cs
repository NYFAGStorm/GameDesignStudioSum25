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
                float sunResource = Mathf.Clamp01(plot.data.sun * 4f);
                // [ ] if this is a dark plant (grows in moonlight) invert sun
                // REVIEW: when we have moon phases, adjust to moonlight intensity
                if (plot.data.plant.isDarkPlant)
                    sunResource = Mathf.Clamp01(1f - sunResource);
                // PLANT EFFECTS:
                if (PlantSystem.PlantHasEffect(plot.data.plant, PlantEffect.DayNightPlant))
                    sunResource = 1f;
                float resources = (sunResource + plot.data.water + plot.data.soil) / 3f;
                // calculate vitality delta             
                float vitalityDelta = (0.667f - resources) * -0.1f;
                // adjust vitality for current season
                vitalityDelta *= plot.GetPlantSeasonalVitality();
                // calculate current vitality
                plot.data.plant.vitality = Mathf.Clamp01(plot.data.plant.vitality + vitalityDelta);
                // calculate health
                float healthDelta = (0.5f - plot.data.plant.vitality) + (0.5f - resources);
                healthDelta *= -0.001f;
                plot.data.plant.health = Mathf.Clamp01(0.01f + plot.data.plant.health+healthDelta);
                // calculate growth
                float growthDelta = resources * 0.2f * progress * 
                    plot.data.plant.vitality * plot.data.plant.growthRate * 
                    plot.data.plant.adjustedGrowthRate;
                if (plot.data.plant.growth < 1f)
                {
                    plot.data.plant.growth = Mathf.Clamp01(plot.data.plant.growth + growthDelta);
                    // show growth change
                    Grow(plot.data.plant);
                    // calculate quality
                    if (!plot.data.plant.isHarvested)
                        plot.data.plant.quality += growthDelta * plot.data.plant.vitality;
                }
                plot.data.plant.quality = Mathf.Clamp01(plot.data.plant.quality);
                plantTimer = PLANTCHECKINTERVAL;
            }
        }
    }

    /// <summary>
    /// Calls the routine to set this plant image based on given growth amount
    /// </summary>
    /// <param name="growthAmount">growth amount</param>
    /// <param name="harvested">is this plant harvested</param>
    public void ForceGrowthImage( PlantData plantData )
    {
        // if called from data distribution routine, this needs to be established
        if (plantImage == null)
            plantImage = transform.Find("Plant Image").gameObject.GetComponent<Renderer>();
        Grow(plantData);
        if (plantData.isHarvested)
            plantImage.material.mainTexture = (Texture2D)Resources.Load("ProtoPlant_Stalk");
    }

    void Grow( PlantData pData )
    {
        int growNumber = Mathf.RoundToInt(pData.growth * 4f);
        // if a re-fruiting plant, and has harvested, keep image near top end
        if (pData.canReFruit && pData.isHarvested)
            growNumber = Mathf.Clamp(growNumber, 3, 4);
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
