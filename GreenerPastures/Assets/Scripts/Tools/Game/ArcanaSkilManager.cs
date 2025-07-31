using UnityEngine;

public class ArcanaSkilManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles acquisition of player skills (unique player effects) via Arcana purchase
    // This also manages the skill tree, a node network of skills with one parent each

    public SkillTree skillTree;

    private bool displaySkillTree;


    void Start()
    {
        // validate
        // initialize
        if (enabled)
        {
            // build test tree
            InitializeSkillTree();
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
        skill = SkillSystem.InitializeSkill("Friends of the Marchant",
            "Selling at the market every 25 times earns you a 100% off coupon for a single item purchase.", PlayerEffect.SkillFriendsMerchant, 1);
        skillTree = SkillSystem.AddTreeNode(skillTree, skill, "Friends of the Chicken");
        skill = SkillSystem.InitializeSkill("Friends of the Gold Fairy",
            "The Gold Fairy visits your farm most nights.", PlayerEffect.SkillFriendsGoldFairy, 1);
        skillTree = SkillSystem.AddTreeNode(skillTree, skill, "Friends of the Chicken");
        skill = SkillSystem.InitializeSkill("Friends of the Salesman",
            "Your salesman purchases on island upgrades are at a 25% discount.", PlayerEffect.SkillFriendsSalesman, 1);
        skillTree = SkillSystem.AddTreeNode(skillTree, skill, "Friends of the Marchant");
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

    void Update()
    {
        
    }

    void OnGUI()
    {
        if (!displaySkillTree)
            return;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;

        r.x = 0.5f * w;
        r.y = 0.5f * h;
        r.width = .9f * w;
        r.height = .9f * h;
        GUIStyle g = new GUIStyle(GUI.skin.box);
        Texture2D t = Texture2D.whiteTexture; // bg tbd
        string s = "ARCANA SKILL TREE";

        GUI.Box(r, s, g);

        // tree nodes
    }
}
