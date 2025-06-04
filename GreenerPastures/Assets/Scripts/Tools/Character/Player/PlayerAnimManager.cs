using UnityEngine;

public class PlayerAnimManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the animation of a player character, as directed by player control

    public bool spriteFlipped;

    private PlayerControlManager pcm;
    private Renderer rend;


    void Start()
    {
        // validate
        pcm = gameObject.transform.parent.GetComponent<PlayerControlManager>();
        if ( pcm == null )
        {
            Debug.LogError("--- PlayerAnimManager [Start] : "+gameObject.name+" no player control manager found in parent. aborting.");
            enabled = false;
        }
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
        // handle sprite flip
        Vector2 flipVec = new Vector2(1f,1f);
        if (spriteFlipped)
            flipVec.x = -1f;
        rend.material.SetTextureScale("_MainTex",flipVec);
    }
}
