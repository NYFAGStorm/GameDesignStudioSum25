using UnityEngine;

public class PlantManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles a single plant object

    private float plantTimer;
    private Renderer plantImage;
    private PlotManager plot;

    private bool forceReFruit;

    private AudioManager sfxAudio;

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
        GameObject sfxObj = GameObject.Find("AudioMgr SFX");
        if (sfxObj != null)
            sfxAudio = sfxObj.GetComponent<AudioManager>();
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

                TimeManager tm = GameObject.FindFirstObjectByType<TimeManager>();
                float moonPhaseLight = 1f;
                if (tm != null)
                    moonPhaseLight = tm.moonPhase;
                // ARCANA SKILL : Dark Biomage
                if (PlantSystem.PlantHasEffect(plot.data.plant, PlantEffect.DarkBiomagePlanted))
                    moonPhaseLight = 1f; // full moon light regardless of phase
                
                // find resources amount as an average of sun, water and soil quality
                // if even a little (25%) sun is available, this counts as 100% sun resource
                float sunResource = Mathf.Clamp01(plot.data.sun * 4f);
                if (plot.data.plant.isDarkPlant)
                {
                    sunResource = Mathf.Clamp01(1f - sunResource);
                    // adjust to moonlight intensity due to moon phases
                    sunResource *= moonPhaseLight;
                }
                // PLANT EFFECTS:
                if (PlantSystem.PlantHasEffect(plot.data.plant, PlantEffect.DayNightPlant))
                {
                    sunResource = 1f;
                    // if night, adjust to moonlight intensity due to moon phases
                    if (plot.data.sun == 0f)
                        sunResource *= moonPhaseLight;
                }
                float resources = (sunResource + plot.data.water + plot.data.soil) / 3f;
                // calculate vitality delta             
                float vitalityDelta = (0.667f - resources) * -0.1f;
                // ARCANA SKILL : Plant Doctor (always 100% seasonal vitality)
                if (!PlantSystem.PlantHasEffect(plot.data.plant, PlantEffect.PlantDoctorPlanted))
                {
                    // adjust vitality for current season
                    vitalityDelta *= plot.GetPlantSeasonalVitality();
                }
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
                        plot.data.plant.quality += growthDelta * plot.data.plant.vitality * plot.data.plant.adjustedGrowthRate;
                }
                plot.data.plant.quality = Mathf.Clamp01(plot.data.plant.quality);
                plantTimer = PLANTCHECKINTERVAL;
            }
        }
    }

    public void SetForceReFruit( bool reFruit )
    {
        forceReFruit = reFruit;
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
    }

    void Grow( PlantData pData )
    {
        int growNumber = Mathf.RoundToInt(pData.growth * 4f);
        // if a re-fruiting plant, and has harvested, keep image near top end
        if ((pData.canReFruit || forceReFruit) && pData.isHarvested)
            growNumber = Mathf.Clamp(growNumber, 2, 4);

        string plantTextureName = "";
        bool plantHadTexture = (plantImage.material.mainTexture.name != "Util_Clear");
        
        switch (growNumber)
        {
            case 0:
                break;
            case 1:
                plantTextureName = "Seedling";
                plantImage.material.mainTexture = (Texture2D)Resources.Load("ProtoPlant01");
                break;
            case 2:
                plantTextureName = "Shoot_";
                plantTextureName += GetPlantShootSize(pData.type);
                plantImage.material.mainTexture = (Texture2D)Resources.Load("ProtoPlant02");
                break;
            case 3:
                plantTextureName = "Bulb_";
                plantTextureName += pData.rarity.ToString() + "_";
                plantTextureName += pData.type.ToString();
                plantImage.material.mainTexture = (Texture2D)Resources.Load("ProtoPlant03");
                break;
            case 4:
                plantTextureName = "Plant_";
                plantTextureName += pData.rarity.ToString() + "_";
                plantTextureName += pData.type.ToString();
                plantImage.material.mainTexture = (Texture2D)Resources.Load("ProtoPlant04");
                break;
        }
        // show 'stalk' if re-fruiting plant is harvested (instead of 'shoot')
        if ((pData.isHarvested && !pData.canReFruit && !forceReFruit) ||
            (pData.isHarvested && (pData.canReFruit || forceReFruit) && growNumber == 2))
        {
            plantTextureName = "Stalk_";
            plantTextureName += pData.rarity.ToString() + "_";
            plantTextureName += pData.type.ToString();
            plantImage.material.mainTexture = (Texture2D)Resources.Load("ProtoPlant_Stalk");
        }

        // set plant texture
        //print("[SET PLANT TEX] : plant texture name = '" + plantTextureName + "'");
        if (plantTextureName != "" &&
            (pData.rarity == PlantRarity.Common || 
            pData.rarity == PlantRarity.Uncommon ||
            pData.rarity == PlantRarity.Rare ||
            pData.rarity == PlantRarity.Special ||
            pData.rarity == PlantRarity.Unique))
            plantImage.material.mainTexture = (Texture2D)Resources.Load(plantTextureName);

        if (sfxAudio != null && !plantHadTexture && plantTextureName == "Seedling")
            sfxAudio.StartSound("Plant Seedling Appear", plot.gameObject, 0f, 6.18f);
    }

    string GetPlantShootSize( PlantType plantType )
    {
        string retString = "Short";

        if (plantType == PlantType.Rose ||
            plantType == PlantType.Apple ||
            plantType == PlantType.Orange ||
            plantType == PlantType.Lemon ||
            plantType == PlantType.Magnolia ||
            plantType == PlantType.Chrystalia ||
            plantType == PlantType.Underbloom ||
            plantType == PlantType.GoldenApple ||
            plantType == PlantType.Nightshade ||
            plantType == PlantType.Yarrow ||
            plantType == PlantType.WinterRose ||
            plantType == PlantType.FleurDeLis ||
            plantType == PlantType.BlastApple ||
            plantType == PlantType.PixiePlumeria ||
            plantType == PlantType.SplatBerry ||
            plantType == PlantType.Jazzmyne ||
            plantType == PlantType.BettingHedge ||
            plantType == PlantType.GenesisSapling)
            retString = "Medium";

        if (plantType == PlantType.Corn ||
            plantType == PlantType.Tomato ||
            plantType == PlantType.Sunflower ||
            plantType == PlantType.Snowgrace ||
            plantType == PlantType.Popcorn ||
            plantType == PlantType.EclipseFlower ||
            plantType == PlantType.Banana ||
            plantType == PlantType.Coconut ||
            plantType == PlantType.Mysteria ||
            plantType == PlantType.CrystalRose ||
            plantType == PlantType.Tropicus ||
            plantType == PlantType.HerbalPert ||
            plantType == PlantType.WillowWisp ||
            plantType == PlantType.WalkingStick)
            retString = "Tall";

        return retString;
    }
}
