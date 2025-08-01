using UnityEngine;

public class ArcanaSkilManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles acquisition of player skills (unique player effects) via Arcana purchase
    // This also manages the skill tree, a node network of skills with one parent each
    // This also manages communication to magic manager for purposes of 'casting' active magic skills

    // TODO: same engagement and state manaement pattern used in crafting, market, chicken race
    // (... this lives on Eden?)

    private PlayerControlManager currentPlayer;
    private bool skillPurchasing;
    private int currentSkillNode;

    private SkillTree skillTree;

    public struct SkillNode
    {
        public string name;
        public int nodeDepth;
        public int parent;
        public Vector2 position;
        public bool acquired;
        public bool available;
    }
    private SkillNode[] nodes;

    private bool displaySkillTree;

    private float playerCheckTimer;
    private PlayerControlManager foundPlayer;

    private string skillInstructions;
    private float feedbackTimer;

    const float PLAYERCHECKTIME = 1f;
    const float PLAYERPROXIMITY = 1f;
    const string DEFAULTINSTRUCTIONS = "WASD to explore skills\nPress ENTER to purchase";
    const float FEEDBACKTIME = 3f;



    void Start()
    {
        // validate
        // initialize
        if (enabled)
        {
            // build test tree
            InitializeSkillTree();
            InitializeNodes();

            playerCheckTimer = PLAYERCHECKTIME;
            skillInstructions = DEFAULTINSTRUCTIONS;
        }
    }

    void InitializeSkillTree()
    {
        skillTree = SkillSystem.InitializeSkillTree();
        SkillData skill = null;

        // farming skill branch
        skill = SkillSystem.InitializeSkill("Cover Crop",
            "Harvesting non-refruiting plants, automatically uproots stalk and replants if you hold seed.", PlayerEffect.SkillCoverCrop, 1);
        skillTree = SkillSystem.AddTreeNode(skillTree, skill, "root");
        skill = SkillSystem.InitializeSkill("Focus Flow",
            "While performing the same action repeatedly on multiple plots, your work goes twice as fast.", PlayerEffect.SkillFocusFlow, 1);
        skillTree = SkillSystem.AddTreeNode(skillTree, skill, "Cover Crop");
        skill = SkillSystem.InitializeSkill("Seed Fairy Magnet",
            "Each morning has a chance of a seed fairy visit, and she drops one unique plant seed.", PlayerEffect.SkillSeedFairy, 1);
        skillTree = SkillSystem.AddTreeNode(skillTree, skill, "Cover Crop");
        skill = SkillSystem.InitializeSkill("TBD Farming",
            "Pretty cool farming skill buff.", PlayerEffect.SkillFarmingTBD, 1);
        skillTree = SkillSystem.AddTreeNode(skillTree, skill, "Focus Flow");
        skill = SkillSystem.InitializeSkill("Dark Biomage",
            "Dark plant seeds you grow at night do very well even during the New Moon and low moonlight.", PlayerEffect.SkillDarkBiomage, 1);
        skillTree = SkillSystem.AddTreeNode(skillTree, skill, "Seed Fairy Magnet");

        // gold skill branch
        skill = SkillSystem.InitializeSkill("Friends of the Chicken",
            "Your bet attracts more wagers to the gold dish.", PlayerEffect.SkillFriendsChicken, 1);
        skillTree = SkillSystem.AddTreeNode(skillTree, skill, "root");
        skill = SkillSystem.InitializeSkill("Friends of the Merchant",
            "Selling at the market every 25 times earns you a 100% off coupon for a single item purchase.", PlayerEffect.SkillFriendsMerchant, 1);
        skillTree = SkillSystem.AddTreeNode(skillTree, skill, "Friends of the Chicken");
        skill = SkillSystem.InitializeSkill("Friends of the Gold Fairy",
            "The Gold Fairy visits your farm most nights.", PlayerEffect.SkillFriendsGoldFairy, 1);
        skillTree = SkillSystem.AddTreeNode(skillTree, skill, "Friends of the Chicken");
        skill = SkillSystem.InitializeSkill("Friends of the Salesman",
            "Your salesman purchases on island upgrades are at a 25% discount.", PlayerEffect.SkillFriendsSalesman, 1);
        skillTree = SkillSystem.AddTreeNode(skillTree, skill, "Friends of the Merchant");
        skill = SkillSystem.InitializeSkill("TBD Gold",
            "Pretty cool gold skill buff.", PlayerEffect.SkillGoldTBD, 1);
        skillTree = SkillSystem.AddTreeNode(skillTree, skill, "Friends of the Gold Fairy");

        // active magic skill branch
        skill = SkillSystem.InitializeSkill("Waste Management",
            "Covert one fertilizer to a random seed.", PlayerEffect.SkillWasteManagement, 1);
        skillTree = SkillSystem.AddTreeNode(skillTree, skill, "root");
        skill = SkillSystem.InitializeSkill("Clean Up",
            "All loose plant-based items within area are deposited in the compost bin.", PlayerEffect.SkillCleanUp, 1);
        skillTree = SkillSystem.AddTreeNode(skillTree, skill, "Waste Management");
        skill = SkillSystem.InitializeSkill("TBD Active Magic",
            "Pretty cool active magic skill buff.", PlayerEffect.SkillActiveMagicTBD, 1);
        skillTree = SkillSystem.AddTreeNode(skillTree, skill, "Waste Management");
        skill = SkillSystem.InitializeSkill("Cash In",
            "All loose items within area are sold at the market, turning them into a gold pouch.", PlayerEffect.SkillCashIn, 1);
        skillTree = SkillSystem.AddTreeNode(skillTree, skill, "Clean Up");
        skill = SkillSystem.InitializeSkill("Take Me Home",
            "Immediately teleports you back home from anywhere.", PlayerEffect.SkillTakeMeHome, 1);
        skillTree = SkillSystem.AddTreeNode(skillTree, skill, "TBD Active Magic");

        // passive magic skill branch
        skill = SkillSystem.InitializeSkill("Cool Cat",
            "All spell cooldowns go twice as fast.", PlayerEffect.SkillCoolCat, 1);
        skillTree = SkillSystem.AddTreeNode(skillTree, skill, "root");
        skill = SkillSystem.InitializeSkill("Mystic Forager",
            "You are likely to discover wild plots with any plant growing in them.", PlayerEffect.SkillMysticForager, 1);
        skillTree = SkillSystem.AddTreeNode(skillTree, skill, "Cool Cat");
        skill = SkillSystem.InitializeSkill("So Crafty",
            "All crafted spells yield two charges in your spell book.", PlayerEffect.SkillSoCrafty, 1);
        skillTree = SkillSystem.AddTreeNode(skillTree, skill, "Cool Cat");
        skill = SkillSystem.InitializeSkill("TBD Passive Magic",
            "Pretty cool passive magic skill buff.", PlayerEffect.SkillPassiveMagicTBD, 1);
        skillTree = SkillSystem.AddTreeNode(skillTree, skill, "Mystic Forager");
        skill = SkillSystem.InitializeSkill("Archmage",
            "All areas of effect on spells are twice as big.", PlayerEffect.SkillArchmage, 1);
        skillTree = SkillSystem.AddTreeNode(skillTree, skill, "So Crafty");
    }

    void InitializeNodes()
    {
        nodes = new SkillNode[skillTree.skills.Length];
        for (int i = 0; i < skillTree.skills.Length; i++)
        {
            nodes[i].name = skillTree.skills[i].name;
            nodes[i].nodeDepth = SkillSystem.GetSkillNodeDepth(skillTree, nodes[i].name);
            nodes[i].parent = skillTree.skills[i].parent;
            nodes[i].position = Vector2.zero;
            nodes[i].position.x = 0.1f + ((nodes[i].nodeDepth-1) * .2875f);
            nodes[i].position.y = .1625f;
            switch (nodes[i].nodeDepth - 1)
            {
                case 0:
                    nodes[i].position.y += .1125f;
                    nodes[i].position.y += (i / 5) * .15f;
                    break;
                case 1:
                    if (i % 5 == 1 || i % 5 == 3)
                        nodes[i].position.y += .075f;
                    else if (i % 5 == 2 || i % 5 == 4)
                        nodes[i].position.y += .15f;
                    nodes[i].position.y += (i / 5) * .15f;
                    break;
                case 2:
                    if (i % 5 == 1 || i % 5 == 3)
                        nodes[i].position.y += .075f;
                    else if (i % 5 == 2 || i % 5 == 4)
                        nodes[i].position.y += .15f;
                    nodes[i].position.y += (i / 5) * .15f;
                    break;
            }
            if (currentPlayer != null)
                nodes[i].acquired = PlayerSystem.PlayerHasEffect(currentPlayer.playerData, skillTree.skills[i].playerSkill);
        }
        // determine available
        for (int i = 0; i < skillTree.skills.Length; i++)
        {
            if (nodes[i].parent == -1)
                nodes[i].available = true;
            else
                nodes[i].available = nodes[nodes[i].parent].acquired;
        }
    }

    void Update()
    {
        // run player check timer
        if (playerCheckTimer > 0f)
        {
            playerCheckTimer -= Time.deltaTime;
            if (playerCheckTimer < 0f)
            {
                float closest = 999f;
                foundPlayer = null;
                PlayerControlManager[] pcms = GameObject.FindObjectsByType<PlayerControlManager>(FindObjectsSortMode.None);
                for (int i = 0; i < pcms.Length; i++)
                {
                    float dist = Vector3.Distance(gameObject.transform.position, pcms[i].gameObject.transform.position);
                    if (dist < PLAYERPROXIMITY && dist < closest)
                    {
                        closest = dist;
                        foundPlayer = pcms[i];
                    }
                }
                playerCheckTimer = PLAYERCHECKTIME;
                currentPlayer = foundPlayer;
            }
        }

        // detect skill purchasing
        if (skillPurchasing && currentPlayer != null)
        {
            if (!displaySkillTree)
            {
                InitializeNodes();
                currentSkillNode = 0;
                feedbackTimer = 0f;
                skillInstructions = DEFAULTINSTRUCTIONS;
                displaySkillTree = true;
            }
        }
        else if (displaySkillTree)
            displaySkillTree = false;

        if (!displaySkillTree)
            return;

        // run feedback timer
        if (feedbackTimer > 0f)
        {
            feedbackTimer -= Time.deltaTime;
            if (feedbackTimer < 0f)
            {
                feedbackTimer = 0f;
                skillInstructions = DEFAULTINSTRUCTIONS;
            }
        }

        // keyboard purchase of skill
        // enter
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (nodes[currentSkillNode].acquired)
            {
                skillInstructions = "Skill '" + nodes[currentSkillNode].name + "'\nalready acquired";
                feedbackTimer = FEEDBACKTIME;
                return;
            }
            if (currentPlayer.playerData.arcana < skillTree.skills[currentSkillNode].arcanaCost)
            {
                skillInstructions = "Not enough Arcana to\npurchase '"+ nodes[currentSkillNode].name + "'.";
                feedbackTimer = FEEDBACKTIME;
                return;
            }

            // TODO: confirm popup

            currentPlayer.playerData.arcana -= skillTree.skills[currentSkillNode].arcanaCost;
            PlayerSystem.AddPlayerEffect(currentPlayer.playerData, SkillSystem.GetSkillPlayerEffect(skillTree, nodes[currentSkillNode].name));
            // active magic skills add a permanently ready spell in player's spell book
            if (currentSkillNode >= 10 && currentSkillNode < 15)
            {
                MagicManager mm = currentPlayer.gameObject.GetComponent<MagicManager>();
                switch (currentSkillNode)
                {
                    case 10:
                        mm.AddActiveMagicSkill(SpellType.SkillWasteManagement);
                        break;
                    case 11:
                        mm.AddActiveMagicSkill(SpellType.SkillCleanup);
                        break;
                    case 12:
                        mm.AddActiveMagicSkill(SpellType.SkillTBD);
                        break;
                    case 13:
                        mm.AddActiveMagicSkill(SpellType.SkillCashIn);
                        break;
                    case 14:
                        mm.AddActiveMagicSkill(SpellType.SkillTakeMeHome);
                        break;
                }
            }
            InitializeNodes();
            return;
        }

        // keybvoard navigation of skill tree
        // WASD
        int move = -1;
        if (Input.GetKeyDown(KeyCode.W))
            move = 0;
        if (Input.GetKeyDown(KeyCode.S))
            move = 1;
        if (Input.GetKeyDown(KeyCode.A))
            move = 2;
        if (Input.GetKeyDown(KeyCode.D))
            move = 3;
        if (move < 0)
            return;

        if (nodes[currentSkillNode].parent == -1)
        {
            if (move == 0)
                currentSkillNode -= 5;
            if (currentSkillNode < 0)
                currentSkillNode = 0;
            if (move == 1)
                currentSkillNode += 5;
            if (currentSkillNode > 15)
                currentSkillNode = 15;
            if (move == 3 && nodes[currentSkillNode].acquired)
                currentSkillNode += 1;
            return;
        }
        if (nodes[currentSkillNode].nodeDepth == 2)
        {
            if (move == 0 && currentSkillNode % 5 == 2)
                currentSkillNode -= 1;
            if (move == 1 && currentSkillNode % 5 == 1)
                currentSkillNode += 1;
            if (move == 2)
            {
                if (currentSkillNode % 5 == 1)
                    currentSkillNode -= 1;
                else
                    currentSkillNode -= 2;
            }
            if (move == 3 && nodes[currentSkillNode].acquired)
                currentSkillNode += 2;
            return;
        }
        if (nodes[currentSkillNode].nodeDepth == 3)
        {
            if (move == 2)
                currentSkillNode -= 2;
        }
    }

    void OnGUI()
    {
        if (currentPlayer == null)
            return;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;

        if (!skillPurchasing)
        {        
            // skill tree activation button
            r.x = 0.4f * w;
            r.y = 0.9f * h;
            r.width = 0.2f * w;
            r.height = 0.05f * h;
            GUIStyle gs = new GUIStyle(GUI.skin.button);
            gs.fontSize = Mathf.RoundToInt(18 * (w / 1024f));
            gs.normal.textColor = Color.white;
            gs.hover.textColor = Color.white;
            gs.active.textColor = Color.yellow;
            string ls = "ENTER SKILL TREE";
            if (GUI.Button(r, ls, gs))
            {
                skillPurchasing = true;
                currentPlayer.characterFrozen = true;
                currentPlayer.freezeCharacterActions = true;
                currentPlayer.hidePlayerNameTag = true;
                playerCheckTimer = 0f;
            }
        }

        if (!displaySkillTree)
            return;

        // arcana skill tree display

        r.x = 0.05f * w;
        r.y = 0.15f * h;
        r.width = .9f * w;
        r.height = .825f * h;
        GUIStyle g = new GUIStyle(GUI.skin.box);
        g.fontSize = Mathf.RoundToInt(20 * (w / 1024f));
        g.fontStyle = FontStyle.Bold;
        g.padding = new RectOffset(0, 0, 20, 0);
        Texture2D t = Texture2D.whiteTexture; // bg tbd
        string s = "ARCANA SKILL TREE";
        GUI.Box(r, s, g);

        Color c = Color.white;

        // skill tree
        //  4 root nodes
        //  4 nodes per root, two branches
        //  3 columns, 4 rows
        //  box + label + checkbox per node
        for (int i = 0; i < skillTree.skills.Length; i++)
        {
            r.x = nodes[i].position.x * w;
            r.y = nodes[i].position.y * h;
            r.width = 0.25f * w;
            r.height = 0.05f * h;

            g = new GUIStyle(GUI.skin.box);
            GUI.Box(r, "", g);

            r.x += 0.01f * w;
            r.y += 0.00125f * h;
            r.width -= 0.005f * w;
            r.height -= 0.002f * h;

            g = new GUIStyle(GUI.skin.label);
            g.alignment = TextAnchor.MiddleCenter;
            g.fontSize = Mathf.RoundToInt(18 * (w / 1024f));
            g.fontStyle = FontStyle.Normal;
            if (currentSkillNode == i)
                g.fontStyle = FontStyle.Bold;
            c = Color.white;
            c *= 0.618f;
            g.normal.textColor = c;
            g.hover.textColor = c;
            g.active.textColor = c;
            if (nodes[i].available)
            {
                c = Color.white;
                c *= 0.9f;
                g.normal.textColor = c;
                g.hover.textColor = c;
                g.active.textColor = c;
            }
            else if (nodes[i].acquired)
            {
                g.normal.textColor = Color.white;
                g.hover.textColor = Color.white;
                g.active.textColor = Color.white;
            }
            if (currentSkillNode == i)
            {
                g.normal.textColor = Color.yellow;
                g.hover.textColor = Color.yellow;
                g.active.textColor = Color.yellow;
            }
            s = nodes[i].name;
            GUI.Label(r, s, g);

            // check boxes
            r.x -= 0.035f * w;
            r.y += 0.005f * h;
            r.width = 0.04f * h;
            r.height = 0.04f * h; // square
            g = new GUIStyle(GUI.skin.box);
            GUI.Box(r, "", g);
            // checks
            g = new GUIStyle(GUI.skin.label);
            g.alignment = TextAnchor.MiddleCenter;
            g.fontSize = Mathf.RoundToInt(20 * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            g.normal.textColor = Color.white;
            g.hover.textColor = Color.white;
            g.active.textColor = Color.white;
            GUI.Box(r, nodes[i].acquired ? "X" : "", g);
        }

        // skill description display
        r.x = 0.05f * w;
        r.y = 0.8325f * h;
        r.width = 0.9f * w;
        r.height = 0.05f * h;
        g = new GUIStyle(GUI.skin.label);
        g.alignment = TextAnchor.MiddleCenter;
        g.fontSize = Mathf.RoundToInt(18 * (w / 1024f));
        g.fontStyle = FontStyle.Italic;
        g.normal.textColor = Color.yellow;
        g.hover.textColor = Color.yellow;
        g.active.textColor = Color.yellow;
        s = skillTree.skills[currentSkillNode].description;
        GUI.Label(r, s, g);

        // arcana display
        r.x = 0.05f * w;
        r.y = 0.875f * h;
        r.width = 0.15f * w;
        r.height = 0.1f * h;
        g = new GUIStyle(GUI.skin.label);
        g.alignment = TextAnchor.MiddleCenter;
        g.fontSize = Mathf.RoundToInt(20 * (w / 1024f));
        g.fontStyle = FontStyle.Bold;
        s = "";
        if (currentPlayer != null)
            s = "ARCANA: " + currentPlayer.playerData.arcana;
        r.x += 0.001f * w;
        r.y += 0.001f * w;
        g.normal.textColor = Color.black;
        g.hover.textColor = Color.black;
        g.active.textColor = Color.black;
        GUI.Label(r, s, g);
        r.x -= 0.002f * w;
        r.y -= 0.002f * w;
        g.normal.textColor = Color.yellow;
        g.hover.textColor = Color.yellow;
        g.active.textColor = Color.yellow;
        GUI.Label(r, s, g);

        // instructions display
        r.x = 0.7f * w;
        r.y = 0.875f * h;
        r.width = 0.25f * w;
        r.height = 0.1f * h;
        g = new GUIStyle(GUI.skin.label);
        g.alignment = TextAnchor.MiddleCenter;
        g.fontSize = Mathf.RoundToInt(16 * (w / 1024f));
        g.fontStyle = FontStyle.Normal;
        g.wordWrap = true;
        s = "";
        if (currentPlayer != null)
            s = skillInstructions;
        g.normal.textColor = Color.white;
        g.hover.textColor = Color.white;
        g.active.textColor = Color.white;
        GUI.Label(r, s, g);

        // cancel button
        r.x = 0.4f * w;
        r.y = 0.9f * h;
        r.width = 0.2f * w;
        r.height = 0.05f * h;
        g = new GUIStyle(GUI.skin.button);
        g.fontSize = Mathf.RoundToInt(18 * (w/1024f));
        g.normal.textColor = Color.white;
        g.hover.textColor = Color.white;
        g.active.textColor = Color.yellow;
        s = "EXIT SKILL TREE";
        if (GUI.Button(r,s,g))
        {
            skillPurchasing = false;
            displaySkillTree = false;
            currentPlayer.freezeCharacterActions = false;
            currentPlayer.characterFrozen = false;
            currentPlayer.hidePlayerNameTag = false;
            currentPlayer = null;
            playerCheckTimer = PLAYERCHECKTIME;
        }
    }
}
