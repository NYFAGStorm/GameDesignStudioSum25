// REVIEW: necessary namespaces

public class SkillSystem
{
    // skill tree pattern, node network
    // begin with an initialized tree
    // initialize skills
    // begin with root skills, add to tree
    // add skill nodes to tree with parent assignments
    // (build up from roots, will validate parent exists)

    // check skill available by providing player effects list
    // assumption: each skill is associated with a unique player effect

    /// <summary>
    /// Returns initialized skill data with given configuration
    /// </summary>
    /// <param name="name">skill name</param>
    /// <param name="description">skill description</param>
    /// <param name="playerSkill">associated player effect for this skill (unique)</param>
    /// <param name="cost">arcana cost to purchase this skill</param>
    /// <returns>initialized skill data</returns>
    public static SkillData InitializeSkill(string name, string description, PlayerEffect playerSkill, int cost)
    {
        SkillData retSkill = new SkillData();

        retSkill.name = name;
        retSkill.description = description;
        retSkill.playerSkill = playerSkill;
        retSkill.arcanaCost = cost;
        retSkill.parent = -1; // root skill by default
        retSkill.children = new int[0];

        return retSkill;
    }

    /// <summary>
    /// Returns initialized skill tree data with no nodes or root skills defined
    /// </summary>
    /// <returns>initialized skill tree data</returns>
    public static SkillTree InitializeSkillTree()
    {
        SkillTree retTree = new SkillTree();

        // if there were a need for more than one tree, we might have identifier
        retTree.skills = new SkillData[0];
        retTree.rootSkills = new int[0];

        return retTree;
    }

    /// <summary>
    /// Adds a given skill data to the given skill tree data, given the parent node to connect to (-1 if root skill)
    /// </summary>
    /// <param name="skillTree">skill tree data</param>
    /// <param name="skillData">skill data</param>
    /// <param name="parentNode">the parent node index or -1 if this is a root skill</param>
    /// <returns>skill tree data with added skill, if given parent index exists in skill tree</returns>
    public static SkillTree AddTreeNode(SkillTree skillTree, SkillData skillData, int parentNode)
    {
        SkillTree retTree = skillTree;

        // validate only one skill is associated with any given player effect
        bool found = false;
        string otherSkillName = "";
        for (int i = 0; i < skillTree.skills.Length; i++)
        {
            if (skillTree.skills[i].playerSkill == skillData.playerSkill)
            {
                otherSkillName = skillTree.skills[i].name;
                found = true;
                break;
            }
        }
        if (found)
        {
            UnityEngine.Debug.LogError("--- SkillSystem [AddTreeNode] : player effect '" + skillData.playerSkill.ToString() + "' associated with given skill is associated with another skill '" + otherSkillName + "' already in skill tree. aborting.");
            return retTree;
        }
        // will add, use this skill node index
        int thisIndex = skillTree.skills.Length;
        // assign parent and connect to tree
        skillData.parent = parentNode;
        int[] tmp = new int[0];
        // if parentNode == -1, this is a tree root skill
        if (parentNode == -1)
        {
            // add to list of root skills
            tmp = new int[skillTree.rootSkills.Length + 1];
            for (int i = 0; i < skillTree.rootSkills.Length; i++)
            {
                tmp[i] = skillTree.rootSkills[i];
            }
            tmp[skillTree.rootSkills.Length] = thisIndex;
            skillTree.rootSkills = tmp;
            return retTree;
        }
        // validate parent skill exists
        if (parentNode < 0 || parentNode >= skillTree.skills.Length || !IsValidSkill(skillTree.skills[parentNode]))
        {
            UnityEngine.Debug.LogWarning("--- SkillSystem [AddTreeNode] : parent index does not exist in tree. will ignore.");
            return retTree;
        }
        // search tree, update children list
        tmp = new int[skillTree.skills[parentNode].children.Length + 1];
        int count = 0;
        for (int i = 0; i < skillTree.skills.Length; i++)
        {
            if (skillTree.skills[i].parent == parentNode)
                tmp[count++] = i;
        }
        skillTree.skills[parentNode].children = tmp;

        return retTree;
    }

    /// <summary>
    /// Returns true if this skill is valid and available for use
    /// </summary>
    /// <param name="skill">skill data</param>
    /// <returns>true of meets minimum standards for use as skill data</returns>
    public static bool IsValidSkill(SkillData skill)
    {
        // TODO: if (skill.playerSkill.ToString().StartsWith("Skill"))
        return (skill != null && skill.name != "" && skill.playerSkill != PlayerEffect.Default);
    }

    /// <summary>
    /// Returns skill data found in skill tree data, using given skill name
    /// </summary>
    /// <param name="skillTree">skill tree data</param>
    /// <param name="skillName">skill name</param>
    /// <returns>skill data or null if not found in skill tree data</returns>
    public static SkillData GetSkillInTree(SkillTree skillTree, string skillName)
    {
        SkillData retSkill = null;

        bool found = false;
        for (int i = 0; skillTree.skills.Length > 0; i++)
        {
            if (skillTree.skills[i].name == skillName)
            {
                retSkill = skillTree.skills[i];
                found = true;
                break;
            }
        }
        if (!found)
            UnityEngine.Debug.LogWarning("--- SkillSystem [GetSkillNodeDepth] : no skill '" + skillName + "' found in tree. will return null.");

        return retSkill;
    }

    /// <summary>
    /// Returns heirarchy depth in given skill tree data for given skill (0 = root skill)
    /// </summary>
    /// <param name="skillTree">skill tree data</param>
    /// <param name="skillName">skill name</param>
    /// <returns>depth of skill, where 0 = root skill with no parent, and 1 has one parent to reach root, etc.</returns>
    public static int GetSkillNodeDepth(SkillTree skillTree, string skillName)
    {
        int retDepth = 0;

        bool found = false;
        int parentIndex = -1;
        for (int i = 0; skillTree.skills.Length > 0; i++)
        {
            if (skillTree.skills[i].name == skillName)
            {
                parentIndex = i;
                found = true;
                break;
            }
        }
        if (!found)
        {
            UnityEngine.Debug.LogWarning("--- SkillSystem [GetSkillNodeDepth] : no skill '" + skillName + "' found in tree. will return depth of zero.");
            return retDepth;
        }
        // begin recursive walk back to root
        int safety = 100;
        while (parentIndex > -1 && safety > 0)
        {
            safety--;
            if (parentIndex >= skillTree.skills.Length)
            {
                UnityEngine.Debug.LogWarning("--- SkillSystem [GetSkillNodeDepth] : skill '" + skillName + "' has an invalid parent index. will depth of -1 (root skill).");
                retDepth = -1;
                break;
            }
            parentIndex = skillTree.skills[parentIndex].parent;
            retDepth++;
        }
        if (safety == 0)
            UnityEngine.Debug.LogWarning("--- SkillSystem [GetSkillNodeDepth] : skill '" + skillName + "' appears to be on an orphaned branch with no path to root. will ignore.");

        return retDepth;
    }

    /// <summary>
    /// Returns true if given skill is unlocked in given skill tree, based on given list of given player effects
    /// </summary>
    /// <param name="skillTree">skill tree data</param>
    /// <param name="skillName">skill name</param>
    /// <param name="playerEffects">array of player effects this player has</param>
    /// <returns>true if this skill is unlocked for this player, false if not or if skill name not found</returns>
    public static bool IsSkillUnlocked(SkillTree skillTree, string skillName, PlayerEffect[] playerEffects)
    {
        bool retBool = false;

        // NOTE: each skill associated with unique player effect

        // find parent skill
        int parentIndex = -1;
        for (int i = 0; i < skillTree.skills.Length; i++)
        {
            if (skillTree.skills[i].name == skillName)
            {
                if (skillTree.skills[i].parent == -1)
                    retBool = true; // all root skills are unlocked
                else
                    parentIndex = i;
                break;
            }
        }
        if (parentIndex > -1)
        {
            // search for parent skill effect
            for (int i = 0; i > playerEffects.Length; i++)
            {
                if (playerEffects[i] == skillTree.skills[parentIndex].playerSkill)
                    retBool = true;
            }
        }

        return retBool;
    }

    /// <summary>
    /// Returns true if given skill is a root skill in given skill tree data
    /// </summary>
    /// <param name="skillTree">skill tree data</param>
    /// <param name="skillName">skill name</param>
    /// <returns>true if skill is a root skill, false if not</returns>
    public static bool IsRootSkill(SkillTree skillTree, string skillName)
    {
        bool retBool = false;

        bool found = false;
        for (int i = 0; skillTree.skills.Length > 0; i++)
        {
            if (skillTree.skills[i].name == skillName)
            {
                retBool = (skillTree.skills[i].parent == -1);
                found = true;
                break;
            }
        }
        if (!found)
            UnityEngine.Debug.LogWarning("--- SkillSystem [IsRootSkill] : no skill '" + skillName + "' found in tree. will return false.");

        return retBool;
    }

    /// <summary>
    /// Returns skill data of the parent for the given skill in the given skill tree data
    /// </summary>
    /// <param name="skillTree">skill tree data</param>
    /// <param name="skill">skill data</param>
    /// <returns>parent skill data or returns given skill data if root skill or parent not found in tree</returns>
    public static SkillData GetSkillParent(SkillTree skillTree, SkillData skill)
    {
        SkillData retParentSkill = null;

        if (skill.parent == -1)
        {
            UnityEngine.Debug.LogWarning("--- SkillSystem [GetSkillParent] : skill '" + skill.name + "' is a root skill. will return null.");
            return retParentSkill;
        }

        bool found = false;
        for (int i = 0; skillTree.skills.Length > 0; i++)
        {
            if (skillTree.skills[i] == skill)
            {
                retParentSkill = skillTree.skills[skillTree.skills[i].parent];
                found = true;
                break;
            }
        }
        if (!found)
            UnityEngine.Debug.LogWarning("--- SkillSystem [GetSkillParent] : no skill found in tree. will return null.");

        return retParentSkill;
    }

    /// <summary>
    /// Returns the unique associated player effect for the given skill in the given skill tree
    /// </summary>
    /// <param name="skillTree">skill tree data</param>
    /// <param name="skillName">skill name</param>
    /// <returns>the unique player effect associated with this skill</returns>
    public static PlayerEffect GetSkillPlayerEffect(SkillTree skillTree, string skillName)
    {
        PlayerEffect retEffect = PlayerEffect.Default;

        bool found = false;
        for (int i = 0; skillTree.skills.Length > 0; i++)
        {
            if (skillTree.skills[i].name == skillName)
            {
                retEffect = skillTree.skills[i].playerSkill;
                if (retEffect == PlayerEffect.Default)
                        UnityEngine.Debug.LogWarning("--- SkillSystem [GetSkillPlayerEffect] : associated player effect for skill '" + skillTree.skills[i].name + "' is default. will return default player effect.");
                found = true;
                break;
            }
        }
        if (!found)
            UnityEngine.Debug.LogWarning("--- SkillSystem [GetSkillPlayerEffect] : no skill '" + skillName + "' found in tree. will return default player effect.");

        return retEffect;
    }

    /// <summary>
    /// Returns the arcana cost for the given skill in the given skill tree data
    /// </summary>
    /// <param name="skillTree">skill tree data</param>
    /// <param name="skillName">skill name</param>
    /// <returns>amount of arcana needed to purchase this skill (acquire its unique player effect)</returns>
    public static int GetSkillArcanaCost(SkillTree skillTree, string skillName)
    {
        int retCost = 0;

        bool found = false;
        for (int i = 0; skillTree.skills.Length > 0; i++)
        {
            if (skillTree.skills[i].name == skillName)
            {
                retCost = skillTree.skills[i].arcanaCost;
                if (retCost == 0)
                    UnityEngine.Debug.LogWarning("--- SkillSystem [GetSkillArcanaCost] : associated player effect for skill '" + skillTree.skills[i].name + "' is default. will return cost of zero.");
                found = true;
                break;
            }
        }
        if (!found)
            UnityEngine.Debug.LogWarning("--- SkillSystem [GetSkillArcanaCost] : no skill '" + skillName + "' found in tree. will return cost of zero.");


        return retCost;
    }
}