using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the camera movement for one player (the local client player)

    public enum CameraMode
    {
        Default,
        Follow,
        Hold,
        CloseUp,
        Medium,
        Long,
        World
    }

    public CameraMode mode;
    public Vector3 cameraTargetPosition;
    public Vector3 cameraTargetRotation;

    private GameObject playerObject;
    private PlayerControlManager pcm;

    private float cameraPauseTimer;
    private float cameraMoveTimer;
    private Vector3 savedPostion;
    private Vector3 savedRotation;

    private Vector3 followMoveOffset = new Vector3(0f, 2.5f, -5f);
    private Vector3 followRotOffset = new Vector3(20f, 0f, 0f);

    // TODO: utilitze an animation curve for ease-in and ease-out motion
    // TODO: migrate the calculations done below to world system script

    const float CAMERAPAUSEDURATION = 0.381f;
    const float CAMERAMOVEDURATION = 1f;
    const float GLIDEMULTIPLIER = 0.0381f;


    void Start()
    {
        // validate
        // initialize
        if (enabled)
        {
            // ensure unparented
            gameObject.transform.parent = null;
            // start paused
            cameraPauseTimer = 1f;
            mode = CameraMode.Hold;
        }
    }

    /// <summary>
    /// Sets the local player character to follow
    /// </summary>
    /// <param name="player">player control manager reference</param>
    public void SetPlayer(PlayerControlManager player)
    {
        playerObject = player.gameObject;
        pcm = player;
    }

    void SavePosAndRot()
    {
        savedPostion = transform.position;
        savedRotation = transform.eulerAngles;
    }

    void GetFollowTarget()
    {
        cameraTargetPosition = playerObject.transform.position + followMoveOffset;
        cameraTargetRotation = followRotOffset;
    }

    void SetTimers()
    {
        cameraPauseTimer = CAMERAPAUSEDURATION;
        cameraMoveTimer = CAMERAMOVEDURATION;
    }

    void PerformMove( Vector3 posOffset, Vector3 rotOffset )
    {
        Vector3 pos = gameObject.transform.position;
        Vector3 rot = gameObject.transform.eulerAngles;

        pos += (cameraTargetPosition - pos) * GLIDEMULTIPLIER;
        rot += (cameraTargetRotation - rot) * GLIDEMULTIPLIER;

        gameObject.transform.position = pos;
        gameObject.transform.eulerAngles = rot;
    }

    void Update()
    {
        if (pcm == null)
            return;

        if (mode == CameraMode.Follow)
        {
            GetFollowTarget();
            // move camera
            PerformMove( followMoveOffset, followRotOffset );
            return;
        }

        if (mode == CameraMode.Hold)
        {
            // run pause timer
            if (cameraPauseTimer > 0f)
            {
                cameraPauseTimer -= Time.deltaTime;
                if (cameraPauseTimer < 0f)
                {
                    cameraPauseTimer = 0f;
                    // handle end of pause
                    
                    // temp
                    mode = CameraMode.Follow;
                    return;
                }
                else
                    return;
            }
        }

        // NOTE: for cinematic camera use (not default follow player mode)

        // run move timer
        if (cameraMoveTimer > 0f)
        {
            cameraMoveTimer -= Time.deltaTime;
            if (cameraMoveTimer < 0f)
            {
                cameraMoveTimer = 0f;
            }
        }
        float progress = ((CAMERAMOVEDURATION - cameraMoveTimer)/CAMERAMOVEDURATION);

        // move camera
        gameObject.transform.position = Vector3.Lerp(savedPostion,cameraTargetPosition,progress);
        gameObject.transform.eulerAngles = Vector3.Lerp(savedRotation, cameraTargetRotation, progress);

        // reset timers
        if (progress >= 1f)
            SetTimers();
    }
}
