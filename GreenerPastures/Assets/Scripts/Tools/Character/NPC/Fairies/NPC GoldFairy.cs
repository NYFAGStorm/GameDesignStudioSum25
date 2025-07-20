using UnityEngine;

public class NPCGoldFairy : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles a gold fairy sent to help a player down on their luck

    public float wingFrameTime = 0.02f;
    public Texture2D[] wingFrames;
    public float fairyFrameTime = 0.1f;
    public Texture2D[] fairyFrames;

    private Vector3 moveVector;
    private bool faceLeft;
    private Renderer render;
    private float wingFrameTimer;
    private float fairyFrameTimer;
    private int wingFrame;
    private int fairyFrame;

    private float flyTime;
    private Vector2 noiseVector;

    private Vector3 moveTarget;
    private float targetTimer;
    private Vector2 fairyRange;

    private int islandIndex; // island that needs this fairy
    private int playerIndex; // player that needs this fairy
    private bool fairyActive;
    private Vector3 startLocation;
    private PositionData islandPos;
    private int goldToDrop;
    private bool reachedIslandCenter;
    private bool playerNear;
    private float goldDropTimer;
    private GreenerGameManager ggm;
    private ItemSpawnManager ism;

    const float MAXMOVESPEED = 6.18f;
    const float ZIPFACTOR = 0.618f;
    const float ZIPRANGE = 3.81f;


    void Start()
    {
        // validate
        render = gameObject.GetComponent<Renderer>();
        if (render == null)
        {
            Debug.LogError("--- NPC GoldFairy [Start] : no renderer found on this object. aborting.");
            enabled = false;
        }
        ggm = GameObject.FindFirstObjectByType<GreenerGameManager>();
        if (ggm == null)
        {
            Debug.LogError("--- NPC GoldFairy [Start] : no game manager found in scene. aborting.");
            enabled = false;
        }
        ism = GameObject.FindFirstObjectByType<ItemSpawnManager>();
        if (ism == null)
        {
            Debug.LogError("--- NPC GoldFairy [Start] : no item spawn manager found. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            wingFrameTimer = wingFrameTime;
            // seed flytime
            flyTime = RandomSystem.GaussianRandom01() * 61.8f;
            noiseVector = new Vector2(RandomSystem.GaussianRandom01(), RandomSystem.GaussianRandom01());
            targetTimer = 1f + RandomSystem.GaussianRandom01() * 3.81f;
            fairyRange = new Vector2(7f, 7f);
        }
    }

    // REVIEW: shouldn't this be player index and gold?
    public void ActivateFairy( int island, int gold )
    {
        // validate
        islandIndex = island;
        goldToDrop = gold;
        if (ggm == null)
            ggm = GameObject.FindFirstObjectByType<GreenerGameManager>();
        if (ggm == null || ggm.game == null || ggm.game.islands == null)
        {
            Debug.LogError("---NPC GoldFairy [ActivateFairy] : invalid game manager, game data or island data. aborting.");
            return;
        }
        if (island < 0 || island >= ggm.game.islands.Length || gold <= 0)
        {
            Debug.LogError("---NPC GoldFairy [ActivateFairy] : invalid island and gold parameters. aborting.");
            return;
        }

        // initialize
        for (int i = 0; i < ggm.game.players.Length; i++)
        {
            if (ggm.game.players[i].playerIsland == island)
            {
                playerIndex = i;
                break;
            }
        }
        islandPos = ggm.game.islands[island].location;
        // adjust island pos by average plot position
        Vector3 islandVec = GameSystem.GetVector(islandPos);
        Vector3 avgPlotPos = GameSystem.GetVector(islandPos);
        int count = 1;
        PlotManager[] foundPlots = GameObject.FindObjectsByType<PlotManager>(FindObjectsSortMode.None);
        float islandRange = islandPos.w * 7f;
        for (int i = 0; i < foundPlots.Length; i++)
        {
            if ( Vector3.Distance( foundPlots[i].gameObject.transform.position, islandVec ) < islandRange )
            {
                avgPlotPos += foundPlots[i].gameObject.transform.position;
                count++;
            }
        }
        avgPlotPos /= count;
        islandPos = GameSystem.GetPositionData(avgPlotPos);
        //
        fairyRange.x = islandPos.w * 7f;
        fairyRange.y = islandPos.w * 7f;
        Vector3 pos = Vector3.zero;
        pos.x = islandPos.x;
        pos.y = islandPos.y;
        pos.z = islandPos.z;
        // from center of island, move fairy off
        Vector3 rand = Random.onUnitSphere;
        rand *= 38.1f;
        rand.y = Mathf.Abs(rand.y);
        rand.z = -38.1f; // behind player
        pos += rand;
        gameObject.transform.position = pos;
        startLocation = pos;
        moveTarget = pos;
        // 
        fairyActive = true;
        reachedIslandCenter = false;
        playerNear = false;
    }

    void Update()
    {
        if (fairyActive)
        {
            // detect reached island center
            if (!reachedIslandCenter)
            {
                Vector3 islandVec = GameSystem.GetVector(ggm.game.islands[islandIndex].location);
                if (Vector3.Distance(gameObject.transform.position, islandVec) < 1f)
                    reachedIslandCenter = true;
            }

            // detect player nearby
            if (reachedIslandCenter && !playerNear)
            {
                // REVIEW: this would only work for local host player
                Vector3 playerVec = GameSystem.GetVector(ggm.game.players[playerIndex].location);
                PlayerControlManager pcm = GameObject.FindFirstObjectByType<PlayerControlManager>();
                if (pcm != null)
                    playerVec = pcm.gameObject.transform.position;
                if (Vector3.Distance(gameObject.transform.position, playerVec) < 1f)
                    playerNear = true;
            }

            // run gold drop timer
            if (goldDropTimer > 0f)
            {
                goldDropTimer -= Time.deltaTime;
                if (goldDropTimer < 0f)
                {
                    goldToDrop--;
                    if (goldToDrop <= 0)
                    {
                        goldDropTimer = 0f;
                        fairyActive = false;
                        Destroy(gameObject, 10f);
                    }
                    else
                        goldDropTimer = RandomSystem.GaussianRandom01();
                    // drop
                    Vector3 spawnPos = gameObject.transform.position;
                    spawnPos.y = ggm.game.islands[islandIndex].location.y;
                    Vector3 targetPos = spawnPos;
                    targetPos.x += -0.5f + RandomSystem.GaussianRandom01();
                    targetPos.z += -0.5f + RandomSystem.GaussianRandom01();
                    ism.SpawnNewItem(ItemType.GoldCoin, spawnPos, targetPos, true);
                    // TODO: find this item ref and attach gold sparkleys
                }
            }

            // detect time to drop gold for poor player
            if (reachedIslandCenter && playerNear && goldDropTimer == 0f)
                goldDropTimer = RandomSystem.GaussianRandom01();
        }

        // run wing frame timer
        if (wingFrameTimer > 0f)
        {
            wingFrameTimer -= Time.deltaTime;
            if (wingFrameTimer < 0f)
            {
                wingFrameTimer = wingFrameTime;
                // wing frame increment
                wingFrame++;
                if (wingFrame >= wingFrames.Length)
                    wingFrame = 0;
                render.material.mainTexture = wingFrames[wingFrame];
            }
        }

        // run firefly frame time
        if (fairyFrameTimer > 0f)
        {
            fairyFrameTimer -= Time.deltaTime;
            if (fairyFrameTimer < 0f)
            {
                // increment firefly frame
                fairyFrame++;
                if (fairyFrame >= fairyFrames.Length)
                {
                    fairyFrame = 0;
                    fairyFrameTimer = 0f;
                    Vector2 facing = Vector2.one;
                    if (faceLeft)
                        facing.x = -1f;
                    render.material.SetTextureScale("_LineArt", facing);
                    render.material.SetTextureScale("_MainTex", facing);
                }
                else
                    fairyFrameTimer = fairyFrameTime;
                render.material.SetTexture("_LineArt", fairyFrames[fairyFrame]);
            }
        }

        // run target time
        if (targetTimer > 0f)
        {
            targetTimer -= Time.deltaTime;
            if (targetTimer < 0f)
            {
                targetTimer = 1f + RandomSystem.GaussianRandom01() * 3.81f;
                Vector3 newTarget = Vector3.zero;
                if (fairyActive)
                {
                    // make way to center of island
                    newTarget.x = islandPos.x - gameObject.transform.position.x;
                    newTarget.y = islandPos.y - gameObject.transform.position.y;
                    newTarget.z = islandPos.z - gameObject.transform.position.z;
                }
                else
                {
                    // return to start location
                    newTarget.x = startLocation.x - gameObject.transform.position.x;
                    newTarget.y = startLocation.y - gameObject.transform.position.y;
                    newTarget.z = startLocation.z - gameObject.transform.position.z;
                }
                // clamp magnitude to zip range
                newTarget.Normalize();
                newTarget *= ZIPRANGE;
                newTarget = gameObject.transform.position + newTarget;
                // add move target noise
                newTarget.x += RandomSystem.GaussianRandom01() * fairyRange.x;
                newTarget.x -= fairyRange.x * 0.5f;
                newTarget.z += RandomSystem.GaussianRandom01() * fairyRange.y;
                newTarget.z -= fairyRange.y * 0.5f;
                moveTarget = newTarget;
            }
        }

        // detect direction change
        if (moveVector.x < 0f && !faceLeft)
        {
            faceLeft = true;
            fairyFrameTimer = fairyFrameTime;
        }
        else if (moveVector.x > 0f && faceLeft)
        {
            faceLeft = false;
            fairyFrameTimer = fairyFrameTime;
        }

        // handle movement
        Vector3 pos = gameObject.transform.position;
        pos.x += moveVector.x * MAXMOVESPEED * Time.deltaTime;
        pos.y += moveVector.y * MAXMOVESPEED * Time.deltaTime;
        pos.z += moveVector.z * MAXMOVESPEED * Time.deltaTime;
        gameObject.transform.position = pos;

        // flytime increase
        flyTime += Time.deltaTime;

        // set move vector
        Vector3 m = Vector3.zero;
        m.x = Mathf.PerlinNoise(flyTime, noiseVector.y) - 0.5f;
        m.y = 0.381f * Mathf.PerlinNoise(flyTime, 0f);
        m.z = Mathf.PerlinNoise(noiseVector.x, flyTime) - 0.5f;
        // center vector within range of island
        m.x += ggm.game.islands[islandIndex].location.x;
        m.y += ggm.game.islands[islandIndex].location.y;
        m.z += ggm.game.islands[islandIndex].location.z;
        // zip
        m.x -= ZIPFACTOR * (gameObject.transform.position.x - moveTarget.x);
        m.y -= ZIPFACTOR * (gameObject.transform.position.y - islandPos.y);
        m.z -= ZIPFACTOR * (gameObject.transform.position.z - moveTarget.y);
        moveVector = m;
    }
}
