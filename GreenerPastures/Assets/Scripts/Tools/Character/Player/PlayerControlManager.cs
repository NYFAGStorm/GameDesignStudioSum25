using UnityEditor.UI;
using UnityEngine;

public class PlayerControlManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the local player controls for their character



    private CameraManager cam;
    private PlayerAnimManager pam;


    void Start()
    {
        // validate
        cam = GameObject.FindFirstObjectByType<CameraManager>();
        if ( cam == null )
        {
            Debug.LogError("--- PlayerControlManager [Start] : " + gameObject.name + " no camera manager found in scene. aborting.");
            enabled = false;
        }
        pam = gameObject.transform.GetComponentInChildren<PlayerAnimManager>();
        if ( pam == null )
        {
            Debug.LogError("--- PlayerControlManager [Start] : "+gameObject.name+" no player anim manager found in children. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            cam.SetPlayer(this);
        }
    }

    void Update()
    {
        
    }
}
