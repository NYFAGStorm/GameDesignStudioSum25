// REVIEW: necessary namespaces

[System.Serializable]
public class SkillData
{
    public string name;
    public string description;
    public int arcanaCost;
    public PlayerEffect playerSkill; // represents skill acquired
    public int parent; // node network hierarchy
    public int[] children; // use parent pass, determine
}

[System.Serializable]
public class SkillTree
{
    public SkillData[] skills;
    public int[] rootSkills; // base parent skill(s) of heirarchy
}
