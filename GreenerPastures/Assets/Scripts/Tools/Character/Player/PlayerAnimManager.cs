using UnityEngine;

public class PlayerAnimManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the animation of a player character, as directed by player control

    public bool imageFlipped;

    private Renderer rend;


    void Start()
    {
        // validate
        rend = gameObject.GetComponent<Renderer>();
        if ( rend == null )
        {
            Debug.LogError("--- PlayerAnimManager [Start] : " + gameObject.name + " no renderer found. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            
        }
    }

    void Update()
    {
        // handle image flip
        Vector2 flipVec = new Vector2(1f,1f);
        if (imageFlipped)
            flipVec.x = -1f;
        rend.material.SetTextureScale("_MainTex",flipVec);
    }
}
