using UnityEngine;
using UnityEngine.SceneManagement;

public class DiscoverableElement : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles a static screen clickable element

    public enum DiscoverMode
    {
        Default,
        Clickable,
        Firefly
    }

    public enum RevealTransition
    {
        Default,
        SildeOff,
        FadeOut,
        ScaleDown,
        ScaleUpFade
    }

    public enum RewardType
    {
        Default,
        Gold,
        XP,
        Arcana,
        MagicItem
    }

    public enum DiscoverableState
    {
        Default,
        Ready,
        Appear,
        Discovered,
        Rewarded
    }

    public Texture2D[] randomizedTextures;

    public Texture2D elementTexture;
    [Tooltip("The viewport space this element exists. (percentage of screen space")]
    public Rect elementSpace;
    public RevealTransition revealMode;
    public float revealTime = 0.618f;
    [Tooltip("This animation curve describes the transition timing between start and end. (if not defined, will be linear)")]
    public AnimationCurve revealAnimation;
    [Tooltip("The viewport space this element will move toward during reveal. (percentage of screen space)")]
    public Vector2 elementTarget;
    public RewardType reward;
    public int rewardAmount;

    private bool elementDiscovered;
    private float revealTimer;
    private float revealProgress;

    private GreenerGameManager ggm;
    private PlayerData playerData;
    private float playerCheckTimer;
    private float rewardTimer;

    private DiscoverableState state;

    private RewardType storedType;
    private int storedAmount;

    private AudioManager sfxAudio;

    private MultiGamepad padMgr;

    private GameObject npcFirefly;
    private GameObject glassJar;
    private Renderer jarRenderer;
    private bool controlJar;
    private float fireFlyStartTimer;
    private bool fireFlyActive;
    private float fireFlyTimer;
    private float jarCaughtFrameTimer;
    private int jarCaughtFrame;
    private Vector3 caughtPosition;

    private DiscoverMode discoverMode;

    public Texture2D jarClosed;
    public Texture2D[] jarCaught;

    private CreditsScreen cs;

    const float CHANCEOFAPPEARANCE = 0.05f; // big rewards mean rare chance of appearance
    const float PLAYERCHECKTIME = 10f;
    const float REWARDTIME = 10f;
    const float FIREFLYTIME = 6.18f;
    const float JARSPEED = 3.81f;
    const float JARPROXIMITY = 1f;
    const float CAUGHTFRAMETIME = 0.1f;


    void Start()
    {
        // validate
        if (elementTexture == null)
        {
            Debug.LogError("--- DiscoverableElement [Start] : " + gameObject.name + " no element texture defined. aborting.");
            enabled = false;
        }
        if (elementSpace == Rect.zero)
        {
            Debug.LogError("--- DiscoverableElement [Start] : " + gameObject.name + " no element space defined. aborting.");
            enabled = false;
        }
        if (revealMode == RevealTransition.Default)
        {
            Debug.LogError("--- DiscoverableElement [Start] : " + gameObject.name + " no reveal mode defined. aborting.");
            enabled = false;
        }
        if (revealTime <= 0f)
        {
            Debug.LogError("--- DiscoverableElement [Start] : " + gameObject.name + " invalid reveal time. aborting.");
            enabled = false;
        }
        if (elementTarget == Vector2.zero)
        {
            Debug.LogError("--- DiscoverableElement [Start] : " + gameObject.name + " no element target defined. aborting.");
            enabled = false;
        }
        if (reward == RewardType.Default)
        {
            Debug.LogError("--- DiscoverableElement [Start] : " + gameObject.name + " no reward defined. aborting.");
            enabled = false;
        }
        if (rewardAmount <= 0)
        {
            Debug.LogError("--- DiscoverableElement [Start] : " + gameObject.name + " no reward amount defined. aborting.");
            enabled = false;
        }
        padMgr = GameObject.FindFirstObjectByType<MultiGamepad>();
        if (padMgr == null)
        {
            Debug.LogError("--- DiscoverableElement [Start] : no multi gamepad manager found in scene. aborting.");
            enabled = false;
        }
        GameObject sfxObject = GameObject.Find("AudioMgr SFX");
        if (sfxObject != null)
            sfxAudio = sfxObject.GetComponent<AudioManager>();
        // initialize
        if (enabled)
        {
            if (revealAnimation.keys == null || revealAnimation.keys.Length == 0)
                revealAnimation = AnimationCurve.Linear(0f,0f,1f,1f);

            state = DiscoverableState.Default;

            RandomizeReward();
            RandomizePosition();
            RandomizeTexture();
            RandomizeReveal();

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    void RandomizeReward()
    {
        float rnd = RandomSystem.FlatRandom01();
        if ( rnd < .25f )
        {
            // gold
            reward = RewardType.Gold;
            rnd = RandomSystem.WeightedRandom01();
            rewardAmount = GameSystem.RoundedResult(rnd, 25);
            rewardAmount *= 10;
        }
        else if ( rnd < .5f )
        {
            // xp
            reward = RewardType.XP;
            rnd = RandomSystem.WeightedRandom01();
            rewardAmount = GameSystem.RoundedResult(rnd, 10);
            rewardAmount *= 10;
        }
        else if ( rnd < .618f )
        {
            // arcana
            reward = RewardType.Arcana;
            rewardAmount = 1;
        }
        else
        {
            // magic item
            reward = RewardType.MagicItem;
            rnd = RandomSystem.WeightedRandom01();
            rewardAmount = GameSystem.RoundedResult(rnd, 3);
        }
    }

    void RandomizePosition()
    {
        float rnd = RandomSystem.FlatRandom01();
        if (rnd > .5f)
        {
            // h flip
            elementSpace.x = 1f - elementSpace.x;
            elementSpace.x -= elementSpace.width;
            elementTarget.x = 1f - elementTarget.x;
            elementTarget.x -= elementSpace.width;
        }
        rnd = RandomSystem.FlatRandom01();
        if (rnd > .5f)
        {
            // v flip
            elementTarget.y -= 1f - elementSpace.y;
            elementSpace.y = 1f - elementSpace.y;
            elementSpace.y -= elementSpace.height;
        }
    }

    void RandomizeTexture()
    {
        if (randomizedTextures == null || randomizedTextures.Length == 0)
            return;
        int rnd = Random.Range(0, randomizedTextures.Length);
        elementTexture = randomizedTextures[rnd];
    }

    void RandomizeReveal()
    {
        revealMode = (RevealTransition)(Random.Range(0, 4)+1);
    }

    void Update()
    {
        if (storedType == RewardType.Default)
        {
            // determine discover mode
            discoverMode = DiscoverMode.Clickable;
            if (padMgr.gamepads[0].isActive)
                discoverMode = DiscoverMode.Firefly;
        }

        if (discoverMode == DiscoverMode.Firefly && fireFlyActive)
        {
            if (fireFlyStartTimer > 0f)
            {
                if (cs == null)
                {
                    cs = GameObject.FindFirstObjectByType<CreditsScreen>();
                    cs.enabled = false;
                }
                fireFlyStartTimer -= Time.deltaTime;
                if (fireFlyStartTimer < 0f)
                {
                    fireFlyStartTimer = 0f;
                    // begin firefly catch
                    SpawnFirefly();
                    SpawnGlassJar();
                }
                return;
            }

            if (controlJar)
            {
                Vector3 pos = glassJar.transform.position;
                pos.x += padMgr.gamepads[0].XaxisL * JARSPEED * Time.deltaTime;
                pos.y += padMgr.gamepads[0].YaxisL * JARSPEED * Time.deltaTime;
                glassJar.transform.position = pos;

                if (padMgr.gamepads[0].aButton)
                {
                    controlJar = false;
                    float dist = Vector3.Distance(glassJar.transform.position, npcFirefly.transform.position);
                    if (dist < JARPROXIMITY)
                    {
                        jarCaughtFrame = 0;
                        jarRenderer.material.mainTexture = jarCaught[jarCaughtFrame];
                        jarCaughtFrameTimer = CAUGHTFRAMETIME;
                        // catch gets reward
                        StoreReward();
                        state = DiscoverableState.Discovered;
                        elementDiscovered = true;
                        //
                        caughtPosition = Camera.main.WorldToViewportPoint(caughtPosition);
                        caughtPosition.y = 1f - caughtPosition.y;
                        caughtPosition.z = 0f;
                    }
                    else
                        jarRenderer.material.mainTexture = jarClosed;
                    fireFlyTimer = 3.81f; // completion timer
                    Destroy(npcFirefly);
                }
            }
            else
            {
                // run jar caught frame timer
                if (jarCaughtFrameTimer > 0f)
                {
                    jarCaughtFrameTimer -= Time.deltaTime;
                    if (jarCaughtFrameTimer < 0f)
                    {
                        jarCaughtFrame++;
                        if (jarCaughtFrame >= jarCaught.Length)
                            jarCaughtFrame = 0;
                        jarRenderer.material.mainTexture = jarCaught[jarCaughtFrame];
                        jarCaughtFrameTimer = CAUGHTFRAMETIME;
                    }
                }
            }
            // run firefly timer
            if (fireFlyTimer > 0f)
            {
                fireFlyTimer -= Time.deltaTime;
                if (fireFlyTimer < 0f)
                {
                    fireFlyTimer = 0f;
                    if (npcFirefly != null)
                        Destroy(npcFirefly);
                    Destroy(glassJar);
                    fireFlyActive = false;
                    if (cs != null)
                        cs.enabled = true;
                }
            }
        }

        if (!elementDiscovered)
            return;

        // run player check timer
        if (playerCheckTimer > 0f)
        {
            playerCheckTimer -= Time.deltaTime;
            if (playerCheckTimer < 0f)
            {
                playerCheckTimer = PLAYERCHECKTIME;
                ggm = GameObject.FindFirstObjectByType<GreenerGameManager>();
                if (ggm != null)
                {
                    // local player gets reward
                    if (ggm.game != null && ggm.game.players != null && ggm.game.players.Length > 0)
                    {
                        playerCheckTimer = 0f;
                        playerData = ggm.game.players[0];
                        rewardTimer = REWARDTIME;
                    }
                }
            }
        }

        // run reward timer
        if (rewardTimer > 0f)
        {
            rewardTimer -= Time.deltaTime;
            if (rewardTimer < 0f)
            {
                rewardTimer = REWARDTIME;
                // check game manager still up
                if (ggm == null)
                {
                    playerCheckTimer = PLAYERCHECKTIME;
                    playerData = null;
                    rewardTimer = 0f;
                }
                else
                {
                    // check player intro not running
                    PlayerIntroduction pIntro = GameObject.FindFirstObjectByType<PlayerIntroduction>();
                    if (pIntro != null)
                    {
                        if (!pIntro.introRunning)
                        {
                            ProvideReward();
                            string[] discoverNotifs = new string[2];
                            discoverNotifs[0] = "You are rewarded for discovering a rare menu element!";
                            switch (reward)
                            {
                                case RewardType.Gold:
                                    discoverNotifs[1] = "You are gifted\n" + rewardAmount + " GOLD!";
                                    break;
                                case RewardType.XP:
                                    discoverNotifs[1] = "You are gifted\n" + rewardAmount + " additional XP!";
                                    break;
                                case RewardType.Arcana:
                                    discoverNotifs[1] = "You are gifted\n" + rewardAmount + " ARCANA!";
                                    break;
                                case RewardType.MagicItem:
                                    discoverNotifs[1] = "You are gifted\n" + rewardAmount + " magic scroll!";
                                    if (rewardAmount > 1)
                                        discoverNotifs[1] = "You are gifted\n" + rewardAmount + " magic scrolls!";
                                    break;
                            }
                            ggm.StackNotifications(discoverNotifs);
                            if (sfxAudio != null)
                                sfxAudio.StartSound("Player Arcana Skill");
                            rewardTimer = 0f;
                        }
                    }
                    state = DiscoverableState.Rewarded;
                }
            }
        }

        // run reveal timer
        if (revealTimer > 0f)
        {
            revealTimer -= Time.deltaTime;
            if (revealTimer < 0f)
                revealTimer = 0f;
            revealProgress = Mathf.Clamp01(1f - (revealTimer/revealTime));
            revealProgress = revealAnimation.Evaluate(revealProgress);
            if (revealProgress == 1f)
            {
                StoreReward();
                state = DiscoverableState.Discovered;
            }
        }
    }

    void OnSceneLoaded( Scene scene, LoadSceneMode mode )
    {
        // activate on credits scene
        if (scene.name == "Credits" && state == DiscoverableState.Default)
        {
            state = DiscoverableState.Ready;
            // chance of element appearance
            if (state == DiscoverableState.Ready &&
                RandomSystem.FlatRandom01() < CHANCEOFAPPEARANCE)
            {
                state = DiscoverableState.Appear;
                if (discoverMode == DiscoverMode.Firefly)
                {
                    fireFlyActive = true;
                    fireFlyStartTimer = 1f;
                }
            }
        }
        // reward in greenergame scene
        if (scene.name == "GreenerGame" && state == DiscoverableState.Discovered)
            playerCheckTimer = PLAYERCHECKTIME;
        // reset on splash scene
        if (scene.name == "Splash")
        {
            state = DiscoverableState.Default;
            revealProgress = 0f;
            ggm = null;
            playerData = null;
            elementDiscovered = false;
            RandomizeReward();
            RandomizePosition();
            RandomizeTexture();
            RandomizeReveal();
        }
    }

    void RevealElement()
    {
        if (elementDiscovered)
            return;
        revealTimer = revealTime;
        elementDiscovered = true;
    }

    void SpawnFirefly()
    {
        npcFirefly = GameObject.Instantiate((GameObject)Resources.Load("NPC Firefly"));
        Vector3 nudge = Vector3.right;
        Vector2 rnd = Random.insideUnitCircle * 3.81f;
        nudge.x += rnd.x;
        nudge.y += rnd.y;
        npcFirefly.transform.position = nudge;
        fireFlyTimer = FIREFLYTIME;
    }

    void SpawnGlassJar()
    {
        glassJar = GameObject.Instantiate((GameObject)Resources.Load("Glass Jar"));
        Vector3 nudge = Vector3.forward * 0.01f;
        nudge += Vector3.left;
        Vector2 rnd = Random.insideUnitCircle * 3.81f;
        nudge.x += rnd.x;
        nudge.y += rnd.y;
        glassJar.transform.position = nudge;
        jarRenderer = glassJar.GetComponentInChildren<Renderer>();

        controlJar = true;
    }

    void StoreReward()
    {
        // REVIEW: store indefinitely?
        storedType = reward;
        storedAmount = rewardAmount;

        playerCheckTimer = PLAYERCHECKTIME;
    }

    void ProvideReward()
    {
        if (playerData == null)
        {
            Debug.LogWarning("--- DiscoverableElement [ProvideReward] : player data not found on this tool. will ignore.");
            return;
        }

        PlayerControlManager pcm = GameObject.FindFirstObjectByType<PlayerControlManager>();
        if (pcm != null)
            pcm.AwardXP(PlayerData.XP_FINDCLICKABLE);

        switch (storedType)
        {
            case RewardType.Default:
                // we should never be here
                break;
            case RewardType.Gold:
                playerData.gold += storedAmount;
                break;
            case RewardType.XP:
                playerData.xp += storedAmount;
                break;
            case RewardType.Arcana:
                playerData.arcana += storedAmount;
                break;
            case RewardType.MagicItem:
                // unknown spell scroll (weighted random)
                ItemData scroll = InventorySystem.InitializeItem(ItemType.Scroll);
                scroll.name += " (Unknown)";
                scroll.effects = new ItemEffect[1];
                scroll.effects[0] = ItemEffect.ScrollRandomSpellCharge;
                // provide item as either inventory (if open slot) or loose item
                for (int i = 0; i < storedAmount; i++)
                {
                    if (InventorySystem.InvHasSlot(playerData.inventory))
                        InventorySystem.AddToInventory(playerData.inventory, scroll);
                    else
                    {
                        LooseItemData looseScroll = InventorySystem.CreateItem(ItemType.Scroll);
                        looseScroll.inv.items[0] = scroll;
                        ItemSpawnManager ism = GameObject.FindFirstObjectByType<ItemSpawnManager>();
                        if (ism != null)
                        {
                            Vector3 spawnSpot = Vector3.zero;
                            Vector3 targetSpot = Vector3.zero;
                            // if pcm available, spawn at player, if not ... ? at player island?
                            if (pcm != null)
                                spawnSpot = pcm.gameObject.transform.position;
                            else
                                spawnSpot = GameSystem.GetVector(ggm.game.islands[playerData.playerIsland].location);
                            targetSpot = spawnSpot + (Vector3.right * 1f * RandomSystem.GaussianRandom01()) - (Vector3.left * 0.5f);
                            ism.SpawnItem(looseScroll, spawnSpot, targetSpot, true);
                        }
                    }
                }
                break;
            default:
                Debug.LogWarning("--- DiscoverableElement [ProvideReward] : " + gameObject.name + " reward type undefined. will ignore.");
                break;
        }

        storedType = RewardType.Default;
        storedAmount = 0;
    }

    void OnGUI()
    {
        Rect r = elementSpace;
        float w = Screen.width;
        float h = Screen.height;

        GUIStyle g = new GUIStyle();
        g.normal.background = null;
        g.hover.background = null;
        g.active.background = null;
        Color c = Color.white;
        Texture2D t = elementTexture;
        string s = "";

        if (discoverMode == DiscoverMode.Firefly && state == DiscoverableState.Discovered &&
            fireFlyTimer > 0f)
        {
            r.x = caughtPosition.x + .025f;
            r.y = caughtPosition.y + (fireFlyTimer / 61.8f);
            r.x *= w;
            r.y *= h;
            r.width *= w;
            r.height = r.width; // square
            g = new GUIStyle(GUI.skin.label);
            g.fontSize = Mathf.RoundToInt(20f * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            g.alignment = TextAnchor.MiddleCenter;
            s = "+" + rewardAmount + "\n";
            if (reward == RewardType.Gold)
                s += "GOLD!";
            else if (reward == RewardType.XP)
                s += "XP!";
            else if (reward == RewardType.Arcana)
                s += "ARCANA!";
            else
                s += "MAGIC ITEM!";
            // drop shadow (yellow over black)
            r.x += 0.0008f;
            r.y += 0.001f;
            g.normal.textColor = Color.black;
            g.hover.textColor = Color.black;
            g.active.textColor = Color.black;
            GUI.Label(r, s, g);
            r.x -= 0.0016f;
            r.y -= 0.002f;
            g.normal.textColor = Color.yellow;
            g.hover.textColor = Color.yellow;
            g.active.textColor = Color.yellow;
            GUI.Label(r, s, g);
            return;
        }

        if (discoverMode != DiscoverMode.Clickable || state < DiscoverableState.Appear || 
            ( elementDiscovered && revealProgress == 1f ))
            return;

        if (!elementDiscovered)
        {
            r.x *= w;
            r.y *= h;
            r.width *= w;
            r.height = r.width; // square

            GUI.DrawTexture(r, t);

            if (GUI.Button(r, "", g))
                RevealElement();

            return;
        }

        // reward label display under element
        r.x *= w;
        r.y *= h;
        r.width *= w;
        r.height = r.width; // square
        g = new GUIStyle(GUI.skin.label);
        g.fontSize = Mathf.RoundToInt( 14f * (w/1024f) );
        g.fontStyle = FontStyle.Bold;
        g.alignment = TextAnchor.MiddleCenter;
        s = "+" + rewardAmount + "\n";
        if (reward == RewardType.Gold)
            s += "GOLD!";
        else if (reward == RewardType.XP)
            s += "XP!";
        else if (reward == RewardType.Arcana)
            s += "ARCANA!";
        else
            s += "MAGIC ITEM!";
        // drop shadow (yellow over black)
        r.x += 0.0008f;
        r.y += 0.001f;
        g.normal.textColor = Color.black;
        g.hover.textColor = Color.black;
        g.active.textColor = Color.black;
        GUI.Label(r, s, g);
        r.x -= 0.0016f;
        r.y -= 0.002f;
        g.normal.textColor = Color.yellow;
        g.hover.textColor = Color.yellow;
        g.active.textColor = Color.yellow;
        GUI.Label(r, s, g);

        // 
        r = elementSpace;
        c = Color.white;

        // adjust for lerp to target
        r.x = Mathf.Lerp(elementSpace.x, elementTarget.x, revealProgress);
        r.y = Mathf.Lerp(elementSpace.y, elementTarget.y, revealProgress);

        // 
        Vector2 elementCenter = Vector2.zero;
        elementCenter.x = elementSpace.x + (elementSpace.width * 0.5f);
        elementCenter.y = elementSpace.y + (elementSpace.height * 0.5f);

        switch (revealMode)
        {
            case RevealTransition.Default:
                // we should never be here
                break;
            case RevealTransition.SildeOff:
                break;
            case RevealTransition.FadeOut:
                c.a = 1f - revealProgress;
                break;
            case RevealTransition.ScaleDown:
                r.x += (elementCenter.x - elementSpace.x) * revealProgress;
                r.y += (elementCenter.y - elementSpace.y) * revealProgress;
                r.width -= revealProgress * elementSpace.width;
                r.height -= revealProgress * elementSpace.height;
                break;
            case RevealTransition.ScaleUpFade:
                r.x -= (elementCenter.x - elementSpace.x) * .381f * revealProgress;
                r.y -= (elementCenter.y - elementSpace.y) * .381f * revealProgress;
                r.width += revealProgress * 0.381f * elementSpace.width;
                r.height += revealProgress * 0.381f * elementSpace.height;
                c.a = 1f - revealProgress;
                break;
            default:
                break;
        }

        GUI.color = c;

        r.x *= w;
        r.y *= h;
        r.width *= w;
        r.height = r.width; // square

        GUI.DrawTexture(r, t);
    }
}
