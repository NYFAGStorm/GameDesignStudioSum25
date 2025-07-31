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
        
        // gold branch
        SkillData skill = SkillSystem.InitializeSkill("Friends Of The Chicken", 
            "Your bet attracts more wagers in the gold dish.", PlayerEffect.EdenLetterOne, 1);
        skillTree = SkillSystem.AddTreeNode(skillTree, skill, "root");
        skill = SkillSystem.InitializeSkill("Friends Of The Merchant", 
            "Every 25 purchases at the market earn you a 100% off coupon.", PlayerEffect.EdenLetterTwo, 1);
        skillTree = SkillSystem.AddTreeNode(skillTree, skill, "Friends Of The Chicken");
        skill = SkillSystem.InitializeSkill("Friends Of The Salesman", 
            "The traveling salesman gives you a 25% discount on all his wares.", PlayerEffect.EdenLetterThree, 1);
        skillTree = SkillSystem.AddTreeNode(skillTree, skill, "Friends Of The Merchant");

        // magic branch
        skill = SkillSystem.InitializeSkill("Cool Cat", 
            "All spell book cooldowns are half the duration.", PlayerEffect.EdenLetterFour, 1);
        skillTree = SkillSystem.AddTreeNode(skillTree, skill, "root");
        skill = SkillSystem.InitializeSkill("So Crafty", 
            "All crafted spells yeild two charges in your spell book.", PlayerEffect.EdenLetterFive, 1);
        skillTree = SkillSystem.AddTreeNode(skillTree, skill, "Cool Cat");
        skill = SkillSystem.InitializeSkill("Archmage", 
            "All spells have an area of effect twice as large.", PlayerEffect.EdenLetterSix, 1);
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
