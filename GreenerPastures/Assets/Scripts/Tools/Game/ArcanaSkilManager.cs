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

        }
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

        r.x = 0f * w;
        r.y = 0f * h;
        r.width = 1f * w;
        r.height = 1f * h;
        GUIStyle g = new GUIStyle(GUI.skin.box);
        Texture2D t = Texture2D.whiteTexture; // bg tbd
        string s = "ARCANA SKILL TREE";

        GUI.Box(r, s, g);

        // tree nodes
    }
}
