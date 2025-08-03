using UnityEngine;

public class CompostManager : MonoBehaviour
{
    // Author: Glenn Storm
    // Handles reception of plant items (seed, fruit, stalk and plant) and produces fertilizer items

    private float compostAmount;
    private float cookedAmount;
    private float compostTimer;
    private ItemSpawnManager ism;

    private bool compostDisplay;
    PlayerControlManager pcm;
    private bool playerClose;

    const float ITEMCHECKRADIUS = 0.618f;
    const float PLAYERCHECKRADIUS = 1f;
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
        if ( compostAmount > cookedAmount )
            cookedAmount += Time.deltaTime * COMPOSTCOOKRATE;

        // grab player when avaiable
        if (pcm == null)
            pcm = GameObject.FindFirstObjectByType<PlayerControlManager>();
        if (pcm == null)
        {
            playerClose = false;
            compostDisplay = false;
            return;
        }

        // check player proximity
        playerClose = (Vector3.Distance(gameObject.transform.position, pcm.gameObject.transform.position) < PLAYERCHECKRADIUS);

        // handle display when player arrives / leaves
        if (playerClose && !compostDisplay && !pcm.hidePlayerHUD)
        {
            compostDisplay = true;
            pcm.hidePlayerNameTag = true;
        }
        else if ((!playerClose || pcm.hidePlayerHUD) && compostDisplay)
        {
            compostDisplay = false;
            pcm.hidePlayerNameTag = false;
        }
    }

    void CheckDroppedPlants()
    {
        LooseItemManager[] looseItems = GameObject.FindObjectsByType<LooseItemManager>(FindObjectsSortMode.None);
        // take all plant items within range, add to compost amount, remove items
        for (int i=0; i<looseItems.Length; i++)
        {
            if (looseItems[i] == null || looseItems[i].looseItem == null ||
                looseItems[i].looseItem.inv == null || looseItems[i].looseItem.inv.items == null ||
                looseItems[i].looseItem.inv.items.Length == 0)
                continue;
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
        ism.SpawnNewItem(ItemType.Fertilizer, gameObject.transform.position, targ, true);
        compostAmount -= 1f;
        cookedAmount = 0f;
    }

    void OnGUI()
    {
        if (!compostDisplay)
            return;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;
        GUIStyle g = new GUIStyle(GUI.skin.label);
        g.fontSize = Mathf.RoundToInt(12f * (w / 1024f));
        g.fontStyle = FontStyle.Bold;
        g.alignment = TextAnchor.MiddleCenter;
        Texture2D t = Texture2D.whiteTexture;
        Color c = Color.white;

        // locate display over plot
        Vector3 disp = Camera.main.WorldToViewportPoint(gameObject.transform.position);
        disp.y = (1f - disp.y);

        // position display
        r.x = (disp.x - 0.05f) * w;
        r.y = (disp.y - 0.2f) * h;
        r.width = 0.1f * w;
        r.height = 0.1f * h;

        // display stats background
        c = Color.gray;
        c.a = .8f;
        GUI.color = c;
        GUI.depth = 2;

        GUI.DrawTexture(r, t);

        // compost stats display
        r.x += 0.00825f * w;
        r.y -= 0.025f * h;
        g.fontSize = Mathf.RoundToInt(10f * (w / 1024f));
        g.fontStyle = FontStyle.Bold;
        g.alignment = TextAnchor.MiddleLeft;

        GUI.color = Color.white;
        c.a = 1f;
        GUI.depth = 0;

        // stats drop shadow first, then text
        GUI.color = Color.black;
        r.x += 0.0005f * w;
        r.y += 0.001f * h;

        string s = "Compost Bin";
        GUI.Label(r, s, g);

        r.y += 0.025f * h;
        s = "Compost : ";
        s += Mathf.RoundToInt((compostAmount * 100f));
        s += "%";
        GUI.Label(r, s, g);

        r.y += 0.025f * h;
        s = "Cooked   : ";
        s += Mathf.RoundToInt((cookedAmount * 100f));
        s += "%";
        GUI.Label(r, s, g);

        // reset to top to draw text again
        r.y -= 0.05f * h;
        GUI.color = Color.white;
        r.x -= 0.001f * w;
        r.y -= 0.002f * h;

        s = "Compost Bin";
        GUI.Label(r, s, g);

        r.y += 0.025f * h;
        s = "Compost : ";
        s += Mathf.RoundToInt((compostAmount * 100f));
        s += "%";
        GUI.Label(r, s, g);

        r.y += 0.025f * h;
        s = "Cooked   : ";
        s += Mathf.RoundToInt((cookedAmount * 100f));
        s += "%";
        GUI.Label(r, s, g);
    }
}
