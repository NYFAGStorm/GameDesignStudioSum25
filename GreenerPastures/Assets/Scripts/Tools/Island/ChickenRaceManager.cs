using UnityEngine;

public class ChickenRaceManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the stall with the chicken race betting mini-game

    public enum GuestState
    {
        Default,        // ready to be approached by guest
        Activating,     // beginning to chance interface
        Active,         // in mini-game, handled by race state
        Deactivating    // end race interface, detect player left to reset
    }
    public GuestState guestState;
    public enum RaceState
    {
        Default,        // ready to enter mini-game
        Entering,       // instructions and option to exit here only
        Betting,        // placing bet of gold and selecting chicken
        PreRace,        // set chickens ready
        Race,           // chickens run race
        PostRace,       // race finish, winner circle celebration
        Rewarding,      // bets settled, gold awarded, return to entering state
        Exiting         // leaving chicken race stall
    }
    public RaceState raceState;

    public enum ChickenAnimSet
    {
        Idle,
        Run,
        Win
    }

    [System.Serializable]
    public struct ChickenFrames
    {
        public ChickenAnimSet anim;
        public Texture2D[] lineFrames;
        public Texture2D[] fillFrames;
        public Texture2D[] colorFrames;
        public float frameTime;
        public float rateVariance;
    }
    public ChickenFrames[] chickenAnimation;

    [System.Serializable]
    public struct ChickenRunner
    {
        public string chickenID;   
        public int animIndex;
        public int animFrame;
        public float animTimer;
        public Vector2 position;
        public bool faceLeft;
        public Vector2 resetVector;
        public float startX;
        public float finishX;
        public float lengthToFinish;
        public float speedMultiplier;
    }
    public ChickenRunner[] chickens;

    public Texture2D raceDishOL;
    public Texture2D[] goldOLs;
    public int goldOLIndex;
    public Texture2D raceOL;
    public Texture2D raceBG;

    private float playerCheckTimer;

    private PlayerControlManager currentGuest;
    private PlayerControlManager leavingGuest;

    private MultiGamepad padMgr;

    private QuitOnEscape qoe; // disable to suspend use of start button while in market

    private float guestStateTimer;
    public float raceStateTimer;
    private bool raceDisplay;

    private bool fadingOverlay;
    private bool fadingFromBlack;
    private Texture2D currentBackground;

    private Texture2D[] buttonTex;

    // -- chicken race variables --

    private int chickenPick = -1;
    private int betAmount;
    private int[] otherPicks = new int[5]; // picks by other wagers
    private int otherBets; // 1-5 other bets, each bet adds 1-OTHERWAGERMAX
    private int totalTake; // all gold in the dish
    private int[] eachChickenPicks = new int[3]; // each chicken has a number picks
    private int chickenWinner;
    private int totalWinners; // how many who picked the winner
    private float otherWagerTimer;
    private float rewardTimer;

    private AudioManager sfxAudio;
    private int currentIdleLoop;
    private bool finishSFX;

    const float PLAYERCHECKTIME = 1f;
    const float RACEPROXIMITYRANGE = .5f;
    const float GUESTSTATETIMERMAX = 1f;
    const float RACESTATETIMERMAX = 1f;
    const int MAXWAGERS = 5;
    const int OTHERWAGERMAX = 25;
    const float OTHERWAGERTIME = 1f;
    const float REWARDTICKTIME = 0.2f;


    void Start()
    {
        // validate
        padMgr = GameObject.FindFirstObjectByType<MultiGamepad>();
        if (padMgr == null)
            Debug.LogWarning("--- ChickenRaceManager [Start] : no multi gamepad found. will ignore.");
        qoe = GameObject.FindFirstObjectByType<QuitOnEscape>();
        if (qoe == null)
        {
            Debug.LogError("--- ChickenRaceManager [Start] : no quit on escape found in scene. aborting.");
            enabled = false;
        }
        GameObject sfxObj = GameObject.Find("AudioMgr ChickRace SFX");
        if (sfxObj != null)
            sfxAudio = sfxObj.GetComponent<AudioManager>();
        // initialize
        if (enabled)
        {
            playerCheckTimer = PLAYERCHECKTIME;

            // GUI Button Textures for build
            if (!Application.isEditor)
            {
                buttonTex = new Texture2D[3];
                buttonTex[0] = (Texture2D)Resources.Load("Button_Normal");
                buttonTex[1] = (Texture2D)Resources.Load("Button_Hover");
                buttonTex[2] = (Texture2D)Resources.Load("Button_Active");
            }

            // initialize all picks
            for (int i = 0; i < otherPicks.Length; i++)
            {
                otherPicks[i] = -1;
            }

            // chickens init
            chickens = new ChickenRunner[3];
            chickens[0] = InitializeChicken("larry", new Vector2(0.17f, 0.525f), 0.175f, 0.8f);
            chickens[1] = InitializeChicken("curly", new Vector2(0.09f, 0.6f), 0.15f, 0.8225f);
            chickens[2] = InitializeChicken("moe", new Vector2(0.2f, 0.67f), 0.127f, 0.8525f);
        }
    }

    string EnteringRaceInstructions()
    {
        string retString = "";

        retString = "Biomancers love to bet on the chicken races!\n";
        retString += "Just place your bet by putting gold in the dish and picking your chicken.\n";
        retString += "And choose wisely, as all bets are final. Then, watch the chickens race!\n";
        retString += "If your chicken wins the race, you get a share of the gold from the dish!\nGood luck!";

        return retString;
    }

    string BettingRaceInstructions()
    {
        string retString = "";

        retString = "Pick your chicken by the lane it's in: 1, 2 or 3.\n";
        retString += "Then, decide how much of your gold to wager on this race.\n";
        retString += "Notice how the others have picked their chicken.";

        return retString;
    }
    string RewardsRaceInstructions()
    {
        string retString = "";

        retString = "Lucky winners divide the gold in the dish equally.\n";
        retString += "If no one won, your gold is returned.\n";
        retString += "Otherwise the gold you bet will be taken now.\n";
        retString += "We hope you enjoyed the race no matter what.\n";
        retString += "Biomancer luck grows with time.";

        return retString;
    }

    void Update()
    {
        if (!DetectPlayerGuest())
            return;

        UpdateSFX();

        UpdateChickenFrames();

        HandleGuestStates();

        UpdateRaceStateTimer();

        HandleRaceStates();
    }

    void UpdateSFX()
    {
        if (sfxAudio == null)
            return;

        if (raceState < RaceState.PreRace)
        {
            // idle sfx
            if (sfxAudio.IsSoundPlaying("Race Idle 1"))
                currentIdleLoop = 1;
            else if (sfxAudio.IsSoundPlaying("Race Idle 2"))
                currentIdleLoop = 2;
            else if (sfxAudio.IsSoundPlaying("Race Idle 3"))
                currentIdleLoop = 3;
            else if (sfxAudio.IsSoundPlaying("Race Idle 4"))
                currentIdleLoop = 4;
            else
            {
                // play different idle sfx
                string idleSFX = "Race Idle ";
                int rnd = currentIdleLoop;
                int safety = 10;
                while (safety > 0 && rnd == currentIdleLoop)
                {
                    safety--;
                    rnd = GameSystem.RoundedResult(RandomSystem.FlatRandom01(), 4);
                }
                sfxAudio.StartSound(idleSFX + rnd.ToString());
                currentIdleLoop = rnd;
            }
            finishSFX = false;
        }
        else if (currentIdleLoop > -1)
        {
            sfxAudio.StopSound("Race Idle "+currentIdleLoop.ToString());
            currentIdleLoop = -1;
        }
        // race loop
        if (raceState == RaceState.Race && raceStateTimer == 0f)
        {
            if (!sfxAudio.IsSoundPlaying("Run Loop"))
                sfxAudio.StartSound("Run Loop");
        }
        else if (sfxAudio.IsSoundPlaying("Run Loop"))
            sfxAudio.StopSound("Run Loop");
        // race finish
        if (raceState == RaceState.PostRace)
        {
            // crowd
            if (!sfxAudio.IsSoundPlaying("Cheer 1") &&
                !sfxAudio.IsSoundPlaying("Cheer 2"))
            {
                if (RandomSystem.FlatRandom01() < .5f)
                    sfxAudio.StartSound("Cheer 1");
                else
                    sfxAudio.StartSound("Cheer 2");
            }
        }
        // winner sfx
        if (raceState == RaceState.Rewarding)
        {
            if (!finishSFX)
            {
                // chicken winner
                if (!sfxAudio.IsSoundPlaying("Finish"))
                    sfxAudio.StartSound("Finish");
                finishSFX = true;
            }
        }
    }

    bool DetectPlayerGuest()
    {
        if (guestState != GuestState.Default && guestState != GuestState.Deactivating)
            return true; // we must already have player engaged, skip

        // if no player, run player check timer
        if (currentGuest == null && playerCheckTimer > 0f)
        {
            playerCheckTimer -= Time.deltaTime;
            if (playerCheckTimer < 0f)
            {
                playerCheckTimer = 0f;
                // detect player in proximity
                PlayerControlManager[] pcms = GameObject.FindObjectsByType<PlayerControlManager>(FindObjectsSortMode.None);
                // 
                for (int i = 0; i < pcms.Length; i++)
                {
                    float dist = Vector3.Distance(gameObject.transform.position, pcms[i].gameObject.transform.position);
                    if (dist < RACEPROXIMITYRANGE)
                    {
                        currentGuest = pcms[i];
                        break;
                    }
                }
                // if no player, reset check timer
                if (currentGuest == null)
                {
                    playerCheckTimer = PLAYERCHECKTIME;
                    if (leavingGuest != null)
                    {
                        // customer has left, reset
                        // ensure leaving customer not stuck
                        leavingGuest.characterFrozen = false;
                        leavingGuest.hidePlayerNameTag = false;
                        leavingGuest.hidePlayerHUD = false;
                        leavingGuest = null;
                        guestState = GuestState.Default;
                        guestStateTimer = 0f;
                    }
                }
                else if (leavingGuest != null && currentGuest == leavingGuest)
                {
                    // REVIEW: remain active until player has left?
                    currentGuest = null;
                    playerCheckTimer = PLAYERCHECKTIME;
                }
                else
                {
                    // customer engagement, activate
                    currentGuest.characterFrozen = true;
                    currentGuest.hidePlayerNameTag = true;
                    currentGuest.hidePlayerHUD = true;
                    // disable almanac
                    InGameAlmanac iga = GameObject.FindFirstObjectByType<InGameAlmanac>();
                    if (iga != null)
                        iga.enabled = false;
                    // disable controls display
                    InGameControls igc = GameObject.FindFirstObjectByType<InGameControls>();
                    if (igc != null)
                        igc.enabled = false;
                    guestState = GuestState.Activating;
                    guestStateTimer = GUESTSTATETIMERMAX;
                }
            }
        }

        return (currentGuest != null);
    }

    void HandleGuestStates()
    {
        if (guestStateTimer == 0f)
            return;

        if (guestStateTimer > 0f)
        {
            guestStateTimer -= Time.deltaTime;
            if (guestStateTimer < 0f)
            {
                guestStateTimer = 0f;
                // handle state
                switch (guestState)
                {
                    case GuestState.Default:
                        // we should never be here
                        break;
                    case GuestState.Activating:
                        guestState = GuestState.Active;
                        raceDisplay = true;
                        raceState = RaceState.Entering;
                        raceStateTimer = RACESTATETIMERMAX;
                        fadingOverlay = true;
                        qoe.enabled = false;
                        break;
                    case GuestState.Active:
                        currentGuest.characterFrozen = false;
                        currentGuest.freezeCharacterActions = false;
                        currentGuest.hidePlayerHUD = false;
                        // re-able almanac
                        InGameAlmanac iga = GameObject.FindFirstObjectByType<InGameAlmanac>();
                        if (iga != null)
                            iga.enabled = true;
                        // show controls display hud item
                        InGameControls igc = GameObject.FindFirstObjectByType<InGameControls>();
                        if (igc != null)
                            igc.enabled = true;
                        guestState = GuestState.Deactivating;
                        guestStateTimer = GUESTSTATETIMERMAX;
                        break;
                    case GuestState.Deactivating:
                        if (currentGuest != null)
                        {
                            leavingGuest = currentGuest;
                            currentGuest = null;
                        }
                        // remain in this state until leaving guest not detected
                        playerCheckTimer = PLAYERCHECKTIME;
                        guestStateTimer = GUESTSTATETIMERMAX;
                        qoe.enabled = true;
                        break;
                    default:
                        Debug.LogWarning("--- ChickenRaceManager [HandleGuestStates] : guest state undefined. will ignore.");
                        break;
                }
            }
        }
    }

    void UpdateRaceStateTimer()
    {
        if (raceStateTimer > 0f)
        {
            raceStateTimer -= Time.deltaTime;
            if (raceStateTimer < (RACESTATETIMERMAX / 2f))
            {
                // configure race background images between overlay fades
                switch (raceState)
                {
                    case RaceState.Default:
                        break;
                    case RaceState.Entering:
                        if (!fadingFromBlack)
                        {
                            if (currentBackground == null && raceBG != null)
                                currentBackground = raceBG;
                            if (currentBackground == null)
                                currentBackground = Texture2D.whiteTexture; // TEMP
                            //currentNPCFrame = 0;
                            //npcFrameTimer = RACESTATETIMERMAX;
                        }
                        break;
                    case RaceState.Exiting:
                        if (!fadingFromBlack)
                            currentBackground = null;
                        break;
                }
                fadingFromBlack = true;
            }
            if (raceStateTimer < 0f)
            {
                raceStateTimer = 0f;
                fadingFromBlack = false;
                // handle race state changes
                switch (raceState)
                {
                    case RaceState.Default:
                        // we should never be here
                        break;
                    case RaceState.Entering:
                        fadingOverlay = false;
                        break;
                    case RaceState.Betting:
                        break;
                    case RaceState.PreRace:
                        // reset winner
                        chickenWinner = -1;
                        // get chickens ready
                        for (int i = 0; i < 3; i++)
                        {
                            chickens[i].animIndex = 0;
                            chickens[i].animTimer = 0.1f;
                            chickens[i].animFrame = 0;
                            chickens[i].faceLeft = false;
                            Vector3 pos = chickens[i].position;
                            pos.x = chickens[i].startX;
                            chickens[i].position = pos;
                        }
                        break;
                    case RaceState.Race:
                        for (int i = 0; i < 3; i++)
                        {
                            chickens[i].animIndex = 1;
                            chickens[i].animTimer = RandomSystem.GaussianRandom01() * 0.1f;
                            chickens[i].animFrame = Random.Range(0,2);
                            chickens[i].faceLeft = false;
                        }
                        break;
                    case RaceState.PostRace:
                        for (int i = 0; i < 3; i++)
                        {
                            if (i == chickenWinner)
                                chickens[i].animIndex = 2;
                            else
                                chickens[i].animIndex = 0;
                        }
                        break;
                    case RaceState.Rewarding:
                        rewardTimer = 2f;
                        break;
                    case RaceState.Exiting:
                        guestStateTimer = (GUESTSTATETIMERMAX / 2f); // exit faster
                        raceState = RaceState.Default;
                        raceDisplay = false;
                        fadingOverlay = false;
                        break;
                    default:
                        Debug.LogWarning("--- ChickenRaceManager [UpdateRaceStateTimer] : race state undefined. will ignore.");
                        break;
                }
            }
        }
    }

    void HandleRaceStates()
    {
        switch (raceState)
        {
            case RaceState.Default:
                // we should never be here
                break;
            case RaceState.Entering:
                // instructions and option to exit here only
                CheckGuestEngagement();
                break;
            case RaceState.Betting:
                // placing bet of gold and selecting chicken (allow bet 0 to just watch race)
                CheckGuestBet();
                break;
            case RaceState.PreRace:
                // set chickens ready
                UpdatePreRace();
                break;
            case RaceState.Race:
                // chickens run race
                UpdateChickenRun();
                break;
            case RaceState.PostRace:
                // race finish, winner circle celebration
                UpdatePostRace();
                break;
            case RaceState.Rewarding:
                // bets settled, gold awarded, return to entering state
                UpdateRewardGuest();
                break;
            case RaceState.Exiting:
                // leaving chicken race stall
                break;
        }
    }

    void CheckGuestEngagement()
    {
        PlayerControlManager.PlayerActions pa = currentGuest.GetPlayerActions();
    }

    void CheckGuestBet()
    {
        PlayerControlManager.PlayerActions pa = currentGuest.GetPlayerActions();

        // REVIEW: should others potentially change their picks before race?

        // run other wager timer
        if (otherWagerTimer > 0f)
        {
            otherWagerTimer -= Time.deltaTime;
            if (otherWagerTimer < 0f)
                otherWagerTimer = 0f;
        }

        float chanceOfMoreBets = 0.05f;
        // ARCANA SKILL : Friends Of The Chicken (x2 chance of bets in gold dish)
        if (PlayerSystem.PlayerHasEffect(currentGuest.playerData, PlayerEffect.SkillFriendsChicken))
            chanceOfMoreBets *= 2f;
        // include other biomancer wagers
        if (otherBets < 1 && 
            otherWagerTimer == 0f)
        {
            // always at least one other bet
            otherPicks[otherBets] = Random.Range(0, 3);
            otherBets = 1;
            totalTake += GameSystem.RoundedResult(RandomSystem.GaussianRandom01(), OTHERWAGERMAX);
            if (sfxAudio != null)
                sfxAudio.StartSound("Coin Drop 1");
        }
        else if ((otherBets+1) < MAXWAGERS && 
            (totalTake + betAmount) > ((otherBets-1) / OTHERWAGERMAX) && 
            otherWagerTimer == 0f && 
            RandomSystem.FlatRandom01() < chanceOfMoreBets)
        {
            // check other picks, favor non-picked chickens
            bool betPlaced = false;
            int thisPick = -1;
            int safety = 10;
            while (safety > 0 && !betPlaced)
            {
                safety--;
                thisPick = Random.Range(0, 3);
                if (eachChickenPicks[thisPick] == 0)
                    betPlaced = true;
            }
            // place bet
            otherPicks[otherBets] = thisPick;
            otherBets++;
            // additional gold in dish
            totalTake += GameSystem.RoundedResult(RandomSystem.GaussianRandom01(), OTHERWAGERMAX);
            if (sfxAudio != null)
            {
                if (totalTake > 30)
                    sfxAudio.StartSound("Coin Drop 4");
                else if (totalTake <= 30 && totalTake > 20)
                    sfxAudio.StartSound("Coin Drop 3");
                else
                    sfxAudio.StartSound("Coin Drop 2");
            }
        }

        // reset other wager timer
        if (otherWagerTimer == 0f)
            otherWagerTimer = OTHERWAGERTIME;

        // calculate each chicken picks
        eachChickenPicks = new int[3];
        if (chickenPick > -1)
            eachChickenPicks[chickenPick]++;
        for (int i = 0; i < otherPicks.Length; i++)
        {
            if (otherPicks[i] > -1)
                eachChickenPicks[otherPicks[i]]++;
        }

        // display gold in dish
        if (totalTake + betAmount > 0)
        {
            goldOLIndex = 0;
            goldOLIndex = Mathf.Clamp((totalTake + betAmount) / 10, 0, 2);
        }
        else
            goldOLIndex = -1;
    }

    void UpdatePreRace()
    {
        // hold chickens ready
        for (int i = 0; i < 3; i++)
        {
            chickens[i].animIndex = 0;
            chickens[i].animTimer = 1f;
            chickens[i].animFrame = 0;
            chickens[i].faceLeft = false;
            Vector3 pos = chickens[i].position;
            pos.x = chickens[i].startX;
            chickens[i].position = pos;
        }
        if (raceStateTimer > 0f)
            return;
        raceState = RaceState.Race;
        raceStateTimer = RACESTATETIMERMAX;
    }

    void UpdateChickenRun()
    {
        if (raceStateTimer > 0f)
            return;

        // all chickens run
        float chickenMoveSpeed = 0.2f;
        // find lane #1 length between start and finish
        float baseLaneLength = chickens[0].finishX - chickens[0].startX;
        float lengthChickenRuns; // each chicken lane length
        float laneCheatMultiplier = 1f; // (length/base)

        bool raceFinished = false;
        for (int i=0; i < 3; i++)
        {
            lengthChickenRuns = chickens[i].finishX - chickens[i].startX;
            laneCheatMultiplier = (lengthChickenRuns / baseLaneLength);

            chickenMoveSpeed = 0.2f;
            chickenMoveSpeed *= RandomSystem.GaussianRandom01();
            chickenMoveSpeed *= laneCheatMultiplier;

            chickens[i].lengthToFinish = chickens[i].finishX - chickens[i].position.x;
            chickens[i].speedMultiplier = laneCheatMultiplier;

            if (chickens[i].position.x > chickens[i].finishX)
                chickens[i].animIndex = 0; // chicken chill
            else
            {
                chickens[i].animIndex = 1;
                chickens[i].position.x += chickenMoveSpeed * Time.deltaTime;
                // check for winner
                if (chickens[i].position.x > chickens[i].finishX) // finish line
                    raceFinished = true;
            }
        }
        if (raceFinished && chickenWinner == -1)
        {
            float furthest = 0f;
            for (int i=0; i < 3; i++)
            {
                // PHOTO FINISH ACCURACY
                // if all we do is check furthest, lane #3 will win more often
                // lane #3 has the largest speed multiplier (lane cheat)
                // so, it's x position is furthest, and technically ...
                // even measuring distance past finish line will favor #3

                // but, ...
                // if we measure distance past finish line and then normalize for multiplier,
                // we can find actual winner
                float distancePastFinishLine = chickens[i].position.x - chickens[i].finishX;
                distancePastFinishLine /= chickens[i].speedMultiplier;
                // now normalized for speed multiplier

                if (distancePastFinishLine > furthest)
                {
                    furthest = distancePastFinishLine;
                    chickenWinner = i; // winner declared
                }
            }
            // let chickens run it out while timer runs
            raceState = RaceState.PostRace;
            raceStateTimer = RACESTATETIMERMAX;
        }
    }

    void UpdatePostRace()
    {
        if (raceStateTimer > 0f)
            return;

        // winner celebrates, others idle
        for (int i = 0; i < 3; i++)
        {
            if (i == chickenWinner)
                chickens[i].animIndex = 2;
            else
                chickens[i].animIndex = 0;
        }
        raceState = RaceState.Rewarding;
        raceStateTimer = RACESTATETIMERMAX * 2f;
    }

    void UpdateRewardGuest()
    {
        if (raceStateTimer > 0f)
            return;

        // determine how much dish is divided by
        // REVIEW: this okay to calculate every tick?
        int numberOfWinners = 0;
        if (chickenPick == chickenWinner)
            numberOfWinners = 1;
        for (int i = 0; i < otherPicks.Length; i++)
        {
            if (otherPicks[i] == chickenWinner)
                numberOfWinners++;
        }
        totalWinners = numberOfWinners;
        if (numberOfWinners == 0)
            betAmount = 0; // return bet amount to player
        // reduce total take in dish over time
        if (rewardTimer > 0f)
        {
            rewardTimer -= Time.deltaTime;
            if (rewardTimer < 0f)
            {
                rewardTimer = 0f;
                if (totalTake == 0)
                {
                    ResetChickenRace();
                    return;
                }
                else
                {
                    if (numberOfWinners == 0)
                        totalTake--;
                    else
                        totalTake -= numberOfWinners;
                    if (totalTake < 0)
                        totalTake = 0;
                    // display gold in dish
                    if (totalTake > 0)
                    {
                        goldOLIndex = 0;
                        goldOLIndex = Mathf.Clamp((totalTake+betAmount) / 10, 0, 2);
                    }
                    else
                        goldOLIndex = -1;
                    // give player gold if they won, take gold if they lost
                    if (chickenWinner == chickenPick)
                        currentGuest.playerData.gold++; // player always gets remainder
                    else
                    {
                        if (betAmount > 0)
                        {
                            betAmount--;
                            currentGuest.playerData.gold--;
                        }
                    }
                    if (totalTake == 0)
                    {
                        if (chickenWinner != chickenPick && betAmount > 0)
                        {
                            currentGuest.playerData.gold -= betAmount;
                            betAmount = 0;
                        }
                        rewardTimer = RACESTATETIMERMAX;
                    }
                    else
                        rewardTimer = REWARDTICKTIME;
                }
            }
        }        
    }

    void ResetChickenRace()
    {
        // clear all picks
        chickenPick = -1;
        otherPicks = new int[5];
        for (int i = 0; i < otherPicks.Length; i++)
        {
            otherPicks[i] = -1;
        }
        eachChickenPicks = new int[3];
        totalWinners = 0;
        // clear dish gold
        totalTake = 0;
        betAmount = 0;
        otherBets = 0;
        totalTake = 0;
        goldOLIndex = -1;
        // exit reward state, return to entering state
        raceState = RaceState.Entering;
        raceStateTimer = RACESTATETIMERMAX;
        // reset chickens here
        for (int i = 0; i < 3; i++)
        {
            chickens[i].position = chickens[i].resetVector;
            chickens[i].animIndex = 0;
            chickens[i].animFrame = 0;
            chickens[i].animTimer = 0.1f;
        }
    }

    ChickenRunner InitializeChicken(string name, Vector2 pos, float startLine, float finishLine )
    {
        return InitializeChicken(name, ChickenAnimSet.Idle, 0, RandomSystem.FlatRandom01(), pos, false, startLine, finishLine);
    }

    ChickenRunner InitializeChicken(string name, ChickenAnimSet set, int frame, float timer, Vector2 pos, bool faceLeft, float start, float finish)
    {
        ChickenRunner retChicken = new ChickenRunner();

        retChicken.chickenID = name;
        retChicken.animIndex = (int)set;
        retChicken.animFrame = frame;
        retChicken.animTimer = timer;
        retChicken.position = pos;
        retChicken.faceLeft = faceLeft;
        retChicken.resetVector = pos;
        retChicken.startX = start;
        retChicken.finishX = finish;

        return retChicken;
    }

    void UpdateChickenFrames()
    {
        for (int i = 0; i < chickens.Length; i++)
        {
            chickens[i].animTimer -= Time.deltaTime;
            if (chickens[i].animTimer < 0f)
            {
                // set timer
                chickens[i].animTimer = chickenAnimation[chickens[i].animIndex].frameTime;
                if (chickenAnimation[chickens[i].animIndex].rateVariance > 0f)
                {
                    float amt = RandomSystem.GaussianRandom01() * chickenAnimation[chickens[i].animIndex].rateVariance;
                    amt -= chickenAnimation[chickens[i].animIndex].rateVariance * 0.5f;
                    chickens[i].animTimer = chickenAnimation[chickens[i].animIndex].frameTime + amt;
                }
                // set frame (loop by default)
                chickens[i].animFrame++;
                if (chickens[i].animFrame >= chickenAnimation[chickens[i].animIndex].lineFrames.Length)
                    chickens[i].animFrame = 0;
                // win and idle may flip
                if (((ChickenAnimSet)chickens[i].animIndex == ChickenAnimSet.Idle || 
                    (ChickenAnimSet)chickens[i].animIndex == ChickenAnimSet.Win) && 
                    RandomSystem.FlatRandom01() < 0.381f)
                    chickens[i].faceLeft = !chickens[i].faceLeft;
                // idle variation
                if ((ChickenAnimSet)chickens[i].animIndex == ChickenAnimSet.Idle)
                    chickens[i].animFrame = Random.Range(0, 3);
            }
        }
    }

    void OnGUI()
    {
        if (!raceDisplay)
            return;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;

        r.x = 0f;
        r.y = 0f;
        r.width = w;
        r.height = h;

        GUIStyle g = new GUIStyle(GUI.skin.label);
        Texture2D t = Texture2D.whiteTexture;
        Color c = Color.white;
        string s = "";

        // crafting background image appears halfway through overlay fading
        if (currentBackground != null)
        {
            c = Color.white;
            GUI.color = c;
            // 
            if (raceState > RaceState.Default && raceState <= RaceState.Exiting) // REVIEW:
            {
                // chickens will be able to be safely placed on top of all this
                t = raceBG;
                GUI.DrawTexture(r, t); // race bg
                t = raceOL;
                GUI.DrawTexture(r, t); // race ol
                if (goldOLIndex > -1)
                {
                    t = goldOLs[goldOLIndex];
                    GUI.DrawTexture(r, t); // gold ol
                }
                t = raceDishOL;
                GUI.DrawTexture(r, t); // dish ol
            }
        }

        // handle fading to and from black for race state transitions
        if (fadingOverlay)
        {
            t = Texture2D.whiteTexture;
            c = Color.black;
            if (fadingFromBlack)
                c.a = ((raceStateTimer * 2f) / RACEPROXIMITYRANGE);
            else
                c.a = 1f - (((raceStateTimer * 2f) / RACESTATETIMERMAX) - 1f);
            GUI.color = c;
            GUI.DrawTexture(r, t);
            // if fading overlay, no other display
            return;
        }

        c = Color.white;
        GUI.color = c;

        // draw chickens
        for (int i = 0; i < chickens.Length; i++)
        {
            r.width = 0.2f * w;
            r.height = 0.2f * w; // square
            r.x = chickens[i].position.x * w - (r.width * 0.5f);
            r.y = chickens[i].position.y * h - (r.width);

            int frame = chickens[i].animFrame;
            if (chickens[i].faceLeft)
            {
                r.x += r.width;
                r.width = -r.width;
            }

            t = chickenAnimation[chickens[i].animIndex].fillFrames[frame];
            GUI.color = c;
            GUI.DrawTexture(r, t); // fill

            // REVIEW: color may not be the best way to identify chickens
            // but, they are in consistent chicken lanes, so we can use those
            /*
            t = chickenAnimation[chickens[i].animIndex].colorFrames[frame];
            switch (i)
            {
                case 0:
                    c = Color.blue;
                    break;
                case 1:
                    c = Color.green;
                    break;
                case 2:
                    c = Color.red;
                    break;
            }
            c.a = 0.381f;
            GUI.color = c;
            GUI.DrawTexture(r, t); // color
            */

            t = chickenAnimation[chickens[i].animIndex].lineFrames[frame];
            c = Color.white;
            GUI.color = c;
            GUI.DrawTexture(r, t); // line
        }

        if (raceStateTimer > 0f)
            return;

        // specific race ui

        // - entering race
        if (raceState == RaceState.Entering)
        {
            // title
            r.x = 0.35f * w;
            r.y = 0.05f * h;
            r.width = 0.3f * w;
            r.height = 0.1f * h;
            g = new GUIStyle(GUI.skin.label);
            g.fontSize = Mathf.RoundToInt(24f * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            g.alignment = TextAnchor.MiddleCenter;
            s = "CHICKEN RACES";
            // drop shadow
            r.x += 0.001f * w;
            r.y += 0.0015f * w;
            c = Color.black;
            GUI.color = c;
            GUI.Label(r, s, g);
            r.x -= 0.002f * w;
            r.y -= 0.003f * w;
            c = Color.white;
            GUI.color = c;
            GUI.Label(r, s, g);
            // instructions
            r.x = 0.2f * w;
            r.y = 0.125f * h;
            r.width = 0.6f * w;
            r.height = 0.225f * h;
            g = new GUIStyle(GUI.skin.label);
            g.fontSize = Mathf.RoundToInt(18f * (w / 1024f));
            g.alignment = TextAnchor.MiddleCenter;
            g.wordWrap = true;
            s = EnteringRaceInstructions();
            // drop shadow
            r.x += 0.00075f * w;
            r.y += 0.001f * w;
            c = Color.black;
            GUI.color = c;
            GUI.Label(r, s, g);
            r.x -= 0.0015f * w;
            r.y -= 0.002f * w;
            c = Color.white;
            GUI.color = c;
            GUI.Label(r, s, g);
            // bet button
            r.x = 0.4f * w;
            r.y = 0.4f * h;
            r.width = 0.2f * w;
            r.height = 0.075f * h;
            g = new GUIStyle(GUI.skin.button);
            g.fontSize = Mathf.RoundToInt(20f * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            g.normal.textColor = Color.white;
            g.hover.textColor = Color.white;
            g.active.textColor = Color.yellow;
            // TODO: gamepad 
            s = "BET ON CHICKEN";
            if (GUI.Button(r, s, g))
            {
                raceState = RaceState.Betting;
                raceStateTimer = 0.618f; //RACESTATETIMERMAX;
            }
        }

        // betting and selecting
        if (raceState == RaceState.Betting)
        {
            // title
            r.x = 0.35f * w;
            r.y = 0.05f * h;
            r.width = 0.3f * w;
            r.height = 0.1f * h;
            g = new GUIStyle(GUI.skin.label);
            g.fontSize = Mathf.RoundToInt(24f * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            g.alignment = TextAnchor.MiddleCenter;
            s = "PLACE YOUR BETS";
            // drop shadow
            r.x += 0.001f * w;
            r.y += 0.0015f * w;
            c = Color.black;
            GUI.color = c;
            GUI.Label(r, s, g);
            r.x -= 0.002f * w;
            r.y -= 0.003f * w;
            c = Color.white;
            GUI.color = c;
            GUI.Label(r, s, g);
            // instructions
            r.x = 0.3f * w;
            r.y = 0.125f * h;
            r.width = 0.4f * w;
            r.height = 0.225f * h;
            g = new GUIStyle(GUI.skin.label);
            g.fontSize = Mathf.RoundToInt(18f * (w / 1024f));
            g.alignment = TextAnchor.MiddleCenter;
            g.wordWrap = true;
            s = BettingRaceInstructions();
            // drop shadow
            r.x += 0.00075f * w;
            r.y += 0.001f * w;
            c = Color.black;
            GUI.color = c;
            GUI.Label(r, s, g);
            r.x -= 0.0015f * w;
            r.y -= 0.002f * w;
            c = Color.white;
            GUI.color = c;
            GUI.Label(r, s, g);
            // chicken pick label
            r.x = 0.175f * w;
            r.y = 0.3f * h;
            r.width = 0.2f * w;
            r.height = 0.1f * h;
            g = new GUIStyle(GUI.skin.label);
            g.fontSize = Mathf.RoundToInt(18f * (w / 1024f));
            g.fontStyle = FontStyle.BoldAndItalic;
            g.alignment = TextAnchor.MiddleCenter;
            s = "Pick Your Chicken!";
            if (chickenPick > -1)
                s = "You picked\nChicken #" + (chickenPick + 1);
            GUI.color = c;
            GUI.Label(r, s, g);
            // chicken selection buttons (lanes 1, 2, 3)
            r.x = 0.25f * w;
            r.y = 0.4f * h;
            r.width = 0.075f * w;
            r.height = 0.075f * h;
            g = new GUIStyle(GUI.skin.button);
            g.fontSize = Mathf.RoundToInt(20f * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            g.alignment = TextAnchor.MiddleCenter;
            g.normal.textColor = Color.white;
            g.hover.textColor = Color.white;
            g.active.textColor = Color.yellow;
            // TODO: gamepad 
            s = "1";
            if (GUI.Button(r, s, g))
            {
                chickenPick = 0;
            }
            // chicken #1 picks label
            r.x = 0.35f * w;
            r.y = 0.4f * h;
            g = new GUIStyle(GUI.skin.label);
            g.alignment = TextAnchor.MiddleCenter;
            g.fontSize = Mathf.RoundToInt(18f * (w / 1024f));
            
            s = eachChickenPicks[0] + " picks";
            GUI.color = c;
            GUI.Label(r, s, g);
            //
            r.x = 0.25f * w;
            r.y += .1f * h;
            g = new GUIStyle(GUI.skin.button);
            g.fontSize = Mathf.RoundToInt(20f * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            g.alignment = TextAnchor.MiddleCenter;
            g.normal.textColor = Color.white;
            g.hover.textColor = Color.white;
            g.active.textColor = Color.yellow;
            // TODO: gamepad 
            s = "2";
            if (GUI.Button(r, s, g))
            {
                chickenPick = 1;
            }
            // chicken #2 picks label
            r.x = 0.35f * w;
            r.y = 0.5f * h;
            g = new GUIStyle(GUI.skin.label);
            g.alignment = TextAnchor.MiddleCenter;
            g.fontSize = Mathf.RoundToInt(18f * (w / 1024f));
            s = eachChickenPicks[1] + " picks";
            GUI.color = c;
            GUI.Label(r, s, g);
            //
            r.x = 0.25f * w;
            r.y += .1f * h;
            g = new GUIStyle(GUI.skin.button);
            g.fontSize = Mathf.RoundToInt(20f * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            g.alignment = TextAnchor.MiddleCenter;
            g.normal.textColor = Color.white;
            g.hover.textColor = Color.white;
            g.active.textColor = Color.yellow;
            // TODO: gamepad 
            s = "3";
            if (GUI.Button(r, s, g))
            {
                chickenPick = 2;
            }            
            // chicken #3 picks label
            r.x = 0.35f * w;
            r.y = 0.6f * h;
            g = new GUIStyle(GUI.skin.label);
            g.alignment = TextAnchor.MiddleCenter;
            g.fontSize = Mathf.RoundToInt(18f * (w / 1024f));
            s = eachChickenPicks[2] + " picks";
            GUI.color = c;
            GUI.Label(r, s, g);
            //
            // bet amount label
            r.x = 0.5875f * w;
            r.y = 0.3f * h;
            r.width = 0.2f * w;
            r.height = 0.1f * h;
            g = new GUIStyle(GUI.skin.label);
            g.fontSize = Mathf.RoundToInt(18f * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            g.alignment = TextAnchor.MiddleCenter;
            s = "Your bet is\n" + betAmount + " gold";
            GUI.color = c;
            GUI.Label(r, s, g);
            // betting controls ( +, - )
            r.x = 0.6f * w;
            r.y = 0.4f * h;
            r.width = 0.075f * w;
            r.height = 0.075f * h;
            g = new GUIStyle(GUI.skin.button);
            g.fontSize = Mathf.RoundToInt(20f * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            g.normal.textColor = Color.white;
            g.hover.textColor = Color.white;
            g.active.textColor = Color.yellow;
            // TODO: gamepad 
            s = "-";
            if (GUI.Button(r, s, g))
            {
                betAmount--;
                if (betAmount < 0)
                    betAmount = 0;
            }
            r.x = 0.7f * w;
            g.normal.textColor = Color.white;
            g.hover.textColor = Color.white;
            g.active.textColor = Color.yellow;
            // TODO: gamepad 
            s = "+";
            if (GUI.Button(r, s, g))
            {
                betAmount++;
                if (betAmount > currentGuest.playerData.gold)
                    betAmount = currentGuest.playerData.gold;
                if (sfxAudio != null)
                    sfxAudio.StartSound("Coin Drop 1");
            }
            // total bets on race
            r.x = 0.5875f * w;
            r.y = 0.55f * h;
            r.width = 0.2f * w;
            r.height = 0.1f * h;
            g = new GUIStyle(GUI.skin.label);
            g.fontSize = Mathf.RoundToInt(18f * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            g.alignment = TextAnchor.MiddleCenter;
            s = "Biomancers betting\nthis race: " + (otherBets + 1);
            GUI.color = c;
            GUI.Label(r, s, g);
            // total in dish label
            r.x = 0.5875f * w;
            r.y = 0.65f * h;
            r.width = 0.2f * w;
            r.height = 0.1f * h;
            g = new GUIStyle(GUI.skin.label);
            g.fontSize = Mathf.RoundToInt(18f * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            g.alignment = TextAnchor.MiddleCenter;
            s = "Total gold in dish:\n" + (totalTake+betAmount) + " Gold";
            GUI.color = c;
            GUI.Label(r, s, g);
            // race button
            r.x = 0.25f * w;
            r.y = 0.8f * h;
            r.width = 0.2f * w;
            r.height = 0.075f * h;
            g = new GUIStyle(GUI.skin.button);
            g.fontSize = Mathf.RoundToInt(20f * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            g.normal.textColor = Color.white;
            g.hover.textColor = Color.white;
            g.active.textColor = Color.yellow;
            // TODO: gamepad 
            s = "LET'S RACE!";
            GUI.enabled = (chickenPick > -1 && betAmount > 0);
            if (GUI.Button(r, s, g))
            {
                raceState = RaceState.PreRace;
                raceStateTimer = 0.618f; //RACESTATETIMERMAX;
            }
            GUI.enabled = true;
            // cancel button
            r.x = 0.55f * w;
            r.y = 0.8f * h;
            r.width = 0.2f * w;
            r.height = 0.075f * h;
            g = new GUIStyle(GUI.skin.button);
            g.fontSize = Mathf.RoundToInt(20f * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            g.normal.textColor = Color.white;
            g.hover.textColor = Color.white;
            g.active.textColor = Color.yellow;
            // TODO: gamepad 
            s = "CANCEL BET";
            if (GUI.Button(r, s, g))
            {
                ResetChickenRace();
            }
            // player gold display
            r.x = 0.05f * w;
            r.y = 0.9f * h;
            r.width = 0.15f * w;
            r.height = 0.05f * h;
            g = new GUIStyle(GUI.skin.label);
            g.fontSize = Mathf.RoundToInt(18f * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            s = "GOLD: "+currentGuest.playerData.gold;
            // drop shadow
            r.x += 0.001f * w;
            r.y += 0.0015f * w;
            c = Color.black;
            GUI.color = c;
            GUI.Label(r, s, g);
            r.x -= 0.002f * w;
            r.y -= 0.003f * w;
            c = Color.yellow;
            GUI.color = c;
            GUI.Label(r, s, g);
        }

        // pre-race
        if (raceState == RaceState.PreRace)
        {
            // title
            r.x = 0.35f * w;
            r.y = 0.05f * h;
            r.width = 0.3f * w;
            r.height = 0.1f * h;
            g = new GUIStyle(GUI.skin.label);
            g.fontSize = Mathf.RoundToInt(24f * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            g.alignment = TextAnchor.MiddleCenter;
            s = "READY ... SET ... GO!";
            // drop shadow
            r.x += 0.001f * w;
            r.y += 0.0015f * w;
            c = Color.black;
            GUI.color = c;
            GUI.Label(r, s, g);
            r.x -= 0.002f * w;
            r.y -= 0.003f * w;
            c = Color.white;
            GUI.color = c;
            GUI.Label(r, s, g);
        }

        // race
        if (raceState == RaceState.Race)
        {
            // REVIEW: do nothing?
        }

        // post-race
        if (raceState == RaceState.PostRace)
        {
            // title
            r.x = 0.35f * w;
            r.y = 0.05f * h;
            r.width = 0.3f * w;
            r.height = 0.1f * h;
            g = new GUIStyle(GUI.skin.label);
            g.fontSize = Mathf.RoundToInt(24f * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            g.alignment = TextAnchor.MiddleCenter;
            s = "WINNER WINNER!";
            // drop shadow
            r.x += 0.001f * w;
            r.y += 0.0015f * w;
            c = Color.black;
            GUI.color = c;
            GUI.Label(r, s, g);
            r.x -= 0.002f * w;
            r.y -= 0.003f * w;
            c = Color.white;
            GUI.color = c;
            GUI.Label(r, s, g);
        }

        // rewarding
        if (raceState == RaceState.Rewarding)
        {
            // title
            r.x = 0.35f * w;
            r.y = 0.05f * h;
            r.width = 0.3f * w;
            r.height = 0.1f * h;
            g = new GUIStyle(GUI.skin.label);
            g.fontSize = Mathf.RoundToInt(24f * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            g.alignment = TextAnchor.MiddleCenter;
            s = "BETTER LUCK NEXT TIME";
            if (chickenPick == chickenWinner)
                s = "YOUR CHICKEN WON!";
            else if (totalWinners == 0)
                s = "ALL GOLD RETURNED";
            // drop shadow
            r.x += 0.001f * w;
            r.y += 0.0015f * w;
            c = Color.black;
            GUI.color = c;
            GUI.Label(r, s, g);
            r.x -= 0.002f * w;
            r.y -= 0.003f * w;
            c = Color.white;
            GUI.color = c;
            GUI.Label(r, s, g);
            // instructions
            r.x = 0.3f * w;
            r.y = 0.125f * h;
            r.width = 0.4f * w;
            r.height = 0.225f * h;
            g = new GUIStyle(GUI.skin.label);
            g.fontSize = Mathf.RoundToInt(18f * (w / 1024f));
            g.alignment = TextAnchor.MiddleCenter;
            g.wordWrap = true;
            s = RewardsRaceInstructions();
            // drop shadow
            r.x += 0.00075f * w;
            r.y += 0.001f * w;
            c = Color.black;
            GUI.color = c;
            GUI.Label(r, s, g);
            r.x -= 0.0015f * w;
            r.y -= 0.002f * w;
            c = Color.white;
            GUI.color = c;
            GUI.Label(r, s, g);

            // player gold display
            r.x = 0.05f * w;
            r.y = 0.9f * h;
            r.width = 0.15f * w;
            r.height = 0.05f * h;
            g = new GUIStyle(GUI.skin.label);
            g.fontSize = Mathf.RoundToInt(18f * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            s = "GOLD: " + currentGuest.playerData.gold;
            // drop shadow
            r.x += 0.001f * w;
            r.y += 0.0015f * w;
            c = Color.black;
            GUI.color = c;
            GUI.Label(r, s, g);
            r.x -= 0.002f * w;
            r.y -= 0.003f * w;
            c = Color.yellow;
            GUI.color = c;
            GUI.Label(r, s, g);
        }

        // exiting
        if (raceState == RaceState.Exiting)
        {
            // REVIEW: do nothing?
        }

        // ---

        // exit mini-game button
        r.x = 0.4f * w;
        r.y = 0.9f * h;
        r.width = 0.2f * w;
        r.height = 0.05f * h;
        g = new GUIStyle(GUI.skin.button);
        if (padMgr != null && padMgr.gamepads[0].isActive)
            g.fontSize = Mathf.RoundToInt(14 * (w / 1024f));
        else
            g.fontSize = Mathf.RoundToInt(16 * (w / 1024f));
        g.normal.textColor = Color.white;
        g.hover.textColor = Color.yellow;
        g.active.textColor = Color.white;
        if (!Application.isEditor)
        {
            g.normal.background = buttonTex[0];
            g.hover.background = buttonTex[1];
            g.active.background = buttonTex[2];
        }
        s = "EXIT MINI-GAME";
        if (padMgr != null && padMgr.gamepads[0].isActive)
            s += "\n[BACK BUTTON]";

        GUI.enabled = (raceState < RaceState.Betting);
        if (GUI.Button(r, s, g))
        {
            raceState = RaceState.Exiting;
            raceStateTimer = RACESTATETIMERMAX;
            fadingOverlay = true;
        }
    }
}
