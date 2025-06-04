using UnityEngine;

public class PlayerAnimManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the animation of a player character, as directed by player control


    private PlayerControlManager pcm;


    void Start()
    {
        // validate
        pcm = gameObject.transform.parent.GetComponent<PlayerControlManager>();
        if ( pcm == null )
        {
            Debug.LogError("--- PlayerAnimManager [Start] : "+gameObject.name+" no player control manager found in parent. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            
        }
    }

    void Update()
    {
        
    }
}
