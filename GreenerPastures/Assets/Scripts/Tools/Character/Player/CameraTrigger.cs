using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    // Author: Glenn Storm
    // This triggers the camera manager to change modes and behaviors upon player collision

    public enum CameraTriggerMode
    {
        Default,
        FollowMode,
        PanMode
    }

    public CameraTriggerMode mode;
    public Vector3 panModePositon;

    private CameraManager cm;


    void Start()
    {
        // validate
        cm = GameObject.FindFirstObjectByType<CameraManager>();
        if (cm == null)
        {
            Debug.LogError("--- CameraTrigger [Start] : "+gameObject.name+" no camera manager found in scene. aborting.");
            enabled = false;
        }
        if (mode == CameraTriggerMode.PanMode && panModePositon == Vector3.zero)
            Debug.LogWarning("--- CameraTrigger [Start] : " + gameObject.name + " pan mode configured and pan position defined as Vector3 zero. will ignore.");
        // initialize
        if ( enabled )
        {

        }
    }

    void Update()
    {
        
    }

    /// <summary>
    /// Triggers the configured camera mode and behavior
    /// </summary>
    public void TriggerCameraMode()
    {
        switch (mode)
        {
            case CameraTriggerMode.Default:
                // should never be here
                break;
            case CameraTriggerMode.FollowMode:
                cm.SetCameraFollowMode();
                break;
            case CameraTriggerMode.PanMode:
                cm.SetCameraPanMode(panModePositon);
                break;
            default:
                break;
        }
    }
}
