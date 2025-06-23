using UnityEngine;

public class GreenerGameManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the highest level of game data during game scenes

    public GameData game;
    // TODO: indicate who is the server (owner of game data)

    private SaveLoadManager saveMgr;
    private ArtLibraryManager alm;
    private bool looseItemsDistributed;


    void Awake()
    {
        saveMgr = GameObject.FindFirstObjectByType<SaveLoadManager>();
        if (saveMgr != null)
        {
            game = saveMgr.GetCurrentGameData();
            Debug.Log("--- GreenerGameManager [Awake] : game data loaded for '" + game.gameName + "'");
        }
    }

    void OnDestroy()
    {
        CollectLooseItemData();
        if (saveMgr != null)
            saveMgr.SetCurrentGameData(game);
    }

    void Start()
    {
        // validate
        if (saveMgr == null)
        {
            Debug.LogError("--- GreenerGameManager [Start] : no save load manager found in scene. aborting.");
            enabled = false;
        }
        alm = GameObject.FindFirstObjectByType<ArtLibraryManager>();
        if (alm == null)
        {
            Debug.LogError("--- GreenerGameManager [Start] : no art library manager found in scene. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {

        }
    }

    void Update()
    {
        if (!looseItemsDistributed && alm != null)
        {
            DistributeLooseItems();
            looseItemsDistributed = true;
        }
    }

    void CollectLooseItemData()
    {
        LooseItemManager[] lItems = GameObject.FindObjectsByType<LooseItemManager>(FindObjectsSortMode.None);
        if (lItems.Length == 0)
            return;

        game.looseItems = new LooseItemData[lItems.Length];
        for (int i = 0; i < lItems.Length; i++)
        {
            lItems[i].looseItem.location.x = lItems[i].transform.position.x;
            lItems[i].looseItem.location.y = lItems[i].transform.position.y;
            lItems[i].looseItem.location.z = lItems[i].transform.position.z;
            game.looseItems[i] = lItems[i].looseItem;
        }
        Debug.Log("--- GreenerGameManager [CollectLooseItems] : " + game.looseItems.Length + " loose items collected.");
    }

    void DistributeLooseItems()
    {
        if (game == null || game.looseItems == null ||
            game.looseItems.Length == 0)
            return;
        for (int i = 0; i < game.looseItems.Length; i++)
        {
            GameObject lItem = GameObject.Instantiate((GameObject)Resources.Load("Loose Item"));
            LooseItemManager lim = lItem.GetComponent<LooseItemManager>();
            if (lim != null)
            {
                Vector3 pos = Vector3.zero;
                lim.looseItem = game.looseItems[i];
                pos.x = lim.looseItem.location.x;
                pos.y = lim.looseItem.location.y;
                pos.z = lim.looseItem.location.z;
                lim.transform.position = pos;
                // get art (first try from plant type)
                ArtData aData = new ArtData();
                lim.frames = new Texture2D[1];
                if (lim.looseItem.inv.items[0].plant != PlantType.Default)
                {
                    aData = alm.GetArtData(lim.looseItem.inv.items[0].type, lim.looseItem.inv.items[0].plant);
                }
                if (aData.type == ItemType.Default)
                    aData = alm.GetArtData(lim.looseItem.inv.items[0].type);
                lim.frames[0] = alm.itemImages[aData.artIndexBase];
            }
        }
        Debug.Log("--- GreenerGameManager [DistributeLooseItems] : " + game.looseItems.Length + " loose items distributed.");
        game.looseItems = null;
    }
}