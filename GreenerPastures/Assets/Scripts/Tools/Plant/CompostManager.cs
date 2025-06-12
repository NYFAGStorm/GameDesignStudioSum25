using UnityEngine;

public class CompostManager : MonoBehaviour
{
    // Author: Glenn Storm
    // Handles reception of plant items (seed, fruit, stalk and plant) and produces fertilizer items

    private float compostAmount;
    private float cookedAmount;
    private float compostTimer;
    private ItemSpawnManager ism;

    const float ITEMCHECKRADIUS = 0.381f;
    const float COMPOSTCHECKTIME = 1f;
    const float COMPOSTCOOKRATE = 0.1f;


    void Start()
    {
        // validate
        ism = GameObject.FindFirstObjectByType<ItemSpawnManager>();
        if (ism == null)
        {
            Debug.LogError("--- CompostManager [Start] : no item spawm manager found. aborting.");
            enabled = false;
        }
        // intialization
        if ( enabled )
        {
            compostTimer = COMPOSTCHECKTIME;
        }
    }

    void Update()
    {
        // run compost timer
        if ( compostTimer >  0f )
        {
            compostTimer -= Time.deltaTime;
            if ( compostTimer < 0 )
            {
                compostTimer = COMPOSTCHECKTIME;
                // check for new dropped items
                CheckDroppedPlants();
                // check for spawn fertilizer
                if (compostAmount >= 1f && cookedAmount >= 1f)
                    SpawnFertilizer(); // spawn one at a time
            }
        }

        // cook compost
        cookedAmount += Time.deltaTime * COMPOSTCOOKRATE;
    }

    void CheckDroppedPlants()
    {
        LooseItemManager[] looseItems = GameObject.FindObjectsByType<LooseItemManager>(FindObjectsSortMode.None);
        // take all plant items within range, add to compost amount, remove items
        for (int i=0; i<looseItems.Length; i++)
        {
            float amountAdd = 0f;
            // seed worth .1, fruit worth .381, stalk with .618, plant worth 1
            switch (looseItems[i].looseItem.inv.items[0].type)
            {
                case ItemType.Seed:
                    amountAdd = 0.1f;
                    break;
                case ItemType.Fruit:
                    amountAdd = 0.381f;
                    break;
                case ItemType.Stalk:
                    amountAdd = 0.618f;
                    break;
                case ItemType.Plant:
                    amountAdd = 1f;
                    break;
                default:
                    amountAdd = 0f;
                    break;
            }
            if ( amountAdd > 0f )
            {
                float dist = Vector3.Distance(gameObject.transform.position, looseItems[i].transform.position);
                if (dist <= ITEMCHECKRADIUS)
                {
                    compostAmount += amountAdd;
                    looseItems[i].looseItem.deleteMe = true;
                }
            }
        }
    }

    void SpawnFertilizer()
    {
        Vector3 targ = gameObject.transform.position + (Vector3.right * RandomSystem.GaussianRandom01()) - (Vector3.left * 0.5f);
        ism.SpawnNewItem(ItemType.Fertilizer, gameObject.transform.position, targ);
        compostAmount -= 1f;
        cookedAmount = 0f;
    }
}
