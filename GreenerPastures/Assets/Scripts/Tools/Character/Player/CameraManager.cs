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
        PanFollow,
        CloseUp,
        Medium,
        Long,
        World
    }

    public CameraMode mode;
    public Vector3 cameraTargetPosition;
    public Vector3 cameraTargetRotation;

    public CameraMode modeAfterHold = CameraMode.Follow;
    public CameraMode modeAfterMove = CameraMode.Hold;

    public bool allowPlayerControlCam = true;

    private GameObject playerObject;
    private PlayerControlManager pcm;
    private MultiGamepad padMgr;
    private CameraClip cc;

    private float cameraPauseTimer;
    private float cameraMoveTimer;
    private float cameraMoveDuration;
    private Vector3 savedPostion;
    private Vector3 savedRotation;

    private AnimationCurve easeCurve; // basic ease-in-out curve

    private PositionData[] offsetPositions;
    private PositionData[] offsetRotations;

    private GameObject rainBox;
    private ParticleSystem rainVFX;
    private bool rainOn;

    const float CAMERAPAUSEDURATION = 0.381f;
    const float CAMERAMOVEDURATION = 0.618f;
    const float GLIDEMULTIPLIER = 0.0381f;
    const float PANCRANETARGETVERTICALOFFSET = 0.618f;
    const float MAXPANCRANEDIST = 10f;
    const float MINPANCRANEHEIGHT = 0.5f;
    const float LATERALPANMULTIPLIER = 0.618f;
    const float INTROMOVEDURATION = 4f;


    void Awake()
    {
        // NOTE: do this here so recovered position and settings from player data can work
        // ensure unparented
        gameObject.transform.parent = null;
        // configure cam offsets
        ConfigureCamOffsets();
    }

    void Start()
    {
        // validate
        padMgr = GameObject.FindFirstObjectByType<MultiGamepad>();
        if (padMgr == null)
        {
            Debug.LogWarning("--- CameraManager[Start] : no gamepad manager found. will ignore.");
            // enabled = false;
        }
        // initialize
        if (enabled)
        {
            if (savedPostion == Vector3.zero)
            {
                // ensure unparented
                gameObject.transform.parent = null;
                // start paused
                SetDefaultTimers();
                mode = CameraMode.Hold;
                modeAfterHold = CameraMode.Follow;
                SavePosAndRot();
                // config curve
                easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            }
            if (playerObject == null)
            {
                PlayerControlManager pcm = GameObject.FindFirstObjectByType<PlayerControlManager>();
                if (pcm != null)
                    playerObject = pcm.gameObject;
            }
            // rain box
            rainBox = GameObject.Instantiate((GameObject)Resources.Load("VFX Rain Box"));
            rainBox.name = "VFX Rain Box";
            rainBox.transform.position = gameObject.transform.position;
            rainBox.transform.parent = gameObject.transform;
            rainVFX = rainBox.GetComponent<ParticleSystem>();
            rainVFX.Stop();
            // config weather manager
            WeatherManager wm = GameObject.FindFirstObjectByType<WeatherManager>();
            if (wm != null)
                wm.ConfigCameraManager(this);
            else
                Debug.LogWarning("--- CameraManager [Start] : no weather manager found in scene. will ignore.");
        }
    }

    public void SetRain( float rainAmount, float windAmount, bool windLeft )
    {
        // start colors
        ParticleSystem.MainModule rainMain = rainVFX.main;
        ParticleSystem.MinMaxGradient grad = new ParticleSystem.MinMaxGradient();
        Color mn = Color.white;
        Color mx = Color.white;
        // change color by light level
        mn.r = 0.618f;
        mn.g = 0.925f;
        mn.b = 1f;
        mx.r = 0.75f;
        mx.g = 0.75f;
        mx.b = 0.9f;
        float intensity = RenderSettings.sun.intensity;
        mn *= Mathf.Clamp01(intensity + 0.618f);
        mn.a = 1f;
        mx *= Mathf.Clamp01(intensity + 0.618f);
        mx.a = 1f;
        grad.colorMin = mn;
        grad.colorMax = mx;
        rainMain.startColor = grad;

        // (emission rate over time = 618 * rain amount)
        // linear vel x countered by shape position x
        // every 10 lin vel x means -3 shape pos

        ParticleSystem.EmissionModule rainEmission = rainVFX.emission;
        rainEmission.rateOverTime = rainAmount * 3810f;

        ParticleSystem.VelocityOverLifetimeModule rainVel = rainVFX.velocityOverLifetime;
        float wForce = windAmount * 100f;
        if (windLeft)
            wForce *= -1f;
        rainVel.x = wForce;
        ParticleSystem.ShapeModule rainShape = rainVFX.shape;
        Vector3 pos = new Vector3((wForce * -.3f), 6.18f, -10f);
        rainShape.position = pos;

        if (rainAmount > 0f && !rainVFX.isPlaying)
            rainVFX.Play();
        else if (rainVFX.isPlaying && rainAmount == 0f)
            rainVFX.Stop();
    }

    void ConfigureCamOffsets()
    {
        offsetPositions = new PositionData[8];
        offsetRotations = new PositionData[8];

        // DEFAULT
        offsetPositions[0].x = 0f;
        offsetPositions[0].y = 2.5f;
        offsetPositions[0].z = -5f;
        offsetRotations[0].x = 20f;
        offsetRotations[0].y = 0f;
        offsetRotations[0].z = 0f;
        // FOLLOW
        offsetPositions[1].x = 0f;
        offsetPositions[1].y = 2.5f;
        offsetPositions[1].z = -5f;
        offsetRotations[1].x = 20f;
        offsetRotations[1].y = 0f;
        offsetRotations[1].z = 0f;
        // HOLD
        offsetPositions[2].x = 0f;
        offsetPositions[2].y = 2.5f;
        offsetPositions[2].z = -5f;
        offsetRotations[2].x = 20f;
        offsetRotations[2].y = 0f;
        offsetRotations[2].z = 0f;
        // PANFOLLOW
        offsetPositions[3].x = 0f;
        offsetPositions[3].y = 2.5f;
        offsetPositions[3].z = -5f;
        offsetRotations[3].x = 20f;
        offsetRotations[3].y = 0f;
        offsetRotations[3].z = 0f;
        // CLOSEUP
        offsetPositions[4].x = 0f;
        offsetPositions[4].y = 1f;
        offsetPositions[4].z = -1f;
        offsetRotations[4].x = 18f;
        offsetRotations[4].y = 0f;
        offsetRotations[4].z = 0f;
        // MEDIUM
        offsetPositions[5].x = 0f;
        offsetPositions[5].y = 1.75f;
        offsetPositions[5].z = -3f;
        offsetRotations[5].x = 20f;
        offsetRotations[5].y = 0f;
        offsetRotations[5].z = 0f;
        // LONG
        offsetPositions[6].x = 0f;
        offsetPositions[6].y = 7.5f;
        offsetPositions[6].z = -15f;
        offsetRotations[6].x = 22f;
        offsetRotations[6].y = 0f;
        offsetRotations[6].z = 0f;
        // WORLD
        offsetPositions[7].x = 0f;
        offsetPositions[7].y = 20f;
        offsetPositions[7].z = -45f;
        offsetRotations[7].x = 30f;
        offsetRotations[7].y = 0f;
        offsetRotations[7].z = 0f;
    }

    Vector3 GetPosOffset( CameraMode mode )
    {
        Vector3 retPos = Vector3.zero;
        PositionData offsetPos = new PositionData();

        offsetPos = offsetPositions[(int)mode];

        retPos.x = offsetPos.x;
        retPos.y = offsetPos.y;
        retPos.z = offsetPos.z;

        return retPos;
    }

    Vector3 GetRotOffset( CameraMode mode )
    {
        Vector3 retRot = Vector3.zero;
        PositionData offsetRot = new PositionData();

        offsetRot = offsetRotations[(int)mode];

        retRot.x = offsetRot.x;
        retRot.y = offsetRot.y;
        retRot.z = offsetRot.z;

        return retRot;
    }

    /// <summary>
    /// Sets the local player character to follow
    /// </summary>
    /// <param name="player">player control manager reference</param>
    public void SetPlayer(PlayerControlManager player)
    {
        playerObject = player.gameObject;
        pcm = player;

        cc = UnityEngine.Object.FindFirstObjectByType<CameraClip>();
        cc.ConnectPlayer(playerObject.transform);
    }

    /// <summary>
    /// Sets the camera mode to travel with the player plus offset
    /// </summary>
    public void SetCameraFollowMode()
    {
        mode = CameraMode.Follow;
        GetFollowTarget();
        gameObject.transform.eulerAngles = savedRotation;

        if (rainBox != null)
        {
            // rain vfx config (on)
            rainBox.SetActive(true);
            if (rainOn)
                rainVFX.Play();
        }
    }

    /// <summary>
    /// Sets the camera mode to lock in a position and pan to follow player
    /// </summary>
    /// <param name="camPosition"></param>
    public void SetCameraPanMode( Vector3 camPosition )
    {
        gameObject.transform.position = camPosition;
        savedPostion = camPosition;
        mode = CameraMode.PanFollow;
        GetPanTarget();

        if (rainBox != null)
        {
            // rain vfx config (off)
            rainOn = rainVFX.isPlaying;
            rainBox.SetActive(false);
        }
    }

    public void SetWorldViewIntro()
    {
        mode = CameraMode.World;
        gameObject.transform.position = GetPosOffset(mode);
        gameObject.transform.eulerAngles = GetRotOffset(mode);
        Vector3 introPos = new Vector3(20.5f, 0, -26.5f);
        gameObject.transform.position += introPos;
        savedPostion = transform.position;
        savedRotation = transform.eulerAngles;
        cameraPauseTimer = 1f;
        cameraMoveTimer = INTROMOVEDURATION;
        cameraMoveDuration = cameraMoveTimer;
        cameraTargetPosition = introPos + GetPosOffset(CameraMode.CloseUp);
        cameraTargetRotation = GetRotOffset(CameraMode.CloseUp);
        modeAfterMove = CameraMode.Default;
    }

    public void SetCameraViewIntro( CameraMode cMode )
    {
        modeAfterHold = cMode;
        SavePosAndRot();
        // use modeAfterHold for target acquisition
        mode = modeAfterHold;
        GetFollowTarget();
        cameraPauseTimer = 1f;
        cameraMoveTimer = 2f;
        cameraMoveDuration = cameraMoveTimer;
        mode = CameraMode.Hold;
        modeAfterMove = modeAfterHold; // stay there
    }

    void SavePosAndRot()
    {
        savedPostion = transform.position;
        savedRotation = transform.eulerAngles;
    }

    public Vector3 GetSavedPosition()
    {
        return savedPostion;
    }

    void GetFollowTarget()
    {
        cameraTargetPosition = GetPosOffset(mode);
        cameraTargetPosition += playerObject.transform.position;
        cameraTargetRotation = GetRotOffset(mode);
    }

    void GetPanTarget()
    {
        Vector3 lateralCam = gameObject.transform.position;
        Vector3 lateralPlayer = playerObject.transform.position;
        float heightDist = (savedPostion.y - lateralPlayer.y);
        float sideMove = (savedPostion.x - lateralPlayer.x);
        sideMove *= LATERALPANMULTIPLIER;
        lateralCam.y = 0f;
        lateralPlayer.y = 0f;
        float dist = Vector3.Distance(lateralCam,lateralPlayer);
        dist = Mathf.Clamp(dist,0f,MAXPANCRANEDIST);
        float craneMultiplier = 1f-(dist/MAXPANCRANEDIST);
        float craneHeight = ( craneMultiplier * heightDist ) - MINPANCRANEHEIGHT;
        cameraTargetPosition = savedPostion + (Vector3.down * craneHeight) + (Vector3.left * sideMove);
        Transform camTrans = gameObject.transform;
        camTrans.LookAt(playerObject.transform.position + (Vector3.up * PANCRANETARGETVERTICALOFFSET));
        cameraTargetRotation = camTrans.eulerAngles;
    }

    void SetDefaultTimers()
    {
        cameraPauseTimer = CAMERAPAUSEDURATION;
        cameraMoveTimer = CAMERAMOVEDURATION;

        cameraMoveDuration = cameraMoveTimer;
    }

    void PerformMove()
    {
        Vector3 pos = gameObject.transform.position;
        Vector3 rot = gameObject.transform.eulerAngles;

        pos += (cameraTargetPosition - pos) * GLIDEMULTIPLIER;
        rot += (cameraTargetRotation - rot) * GLIDEMULTIPLIER;

        gameObject.transform.position = pos;
        gameObject.transform.eulerAngles = rot;

        rot.x = 90f - rot.x;
        rainBox.transform.localEulerAngles = rot;
    }

    void Update()
    {
        if (pcm == null)
            return;

        // detect player cam controls
        if (allowPlayerControlCam && 
            (mode == CameraMode.Follow || mode > CameraMode.PanFollow) && 
            cameraPauseTimer == 0f && cameraMoveTimer == 0f)
        {
            int camModeChange = 0;
            camModeChange += (int)Input.mouseScrollDelta.y;
            // gamepad controls (dpad up and down)
            if (padMgr != null && padMgr.gamepads[0].isActive)
            {
                if (padMgr.gPadDown[0].DpadUp)
                    camModeChange = 1;
                else if (padMgr.gPadDown[0].DpadDown)
                    camModeChange = -1;
            }
            if (camModeChange != 0)
            {
                if (mode == CameraMode.Follow)
                    modeAfterHold = CameraMode.CloseUp;
                else
                    modeAfterHold -= camModeChange;
                if (modeAfterHold == CameraMode.PanFollow)
                    modeAfterHold = CameraMode.Follow;
                if (modeAfterHold > CameraMode.World)
                    modeAfterHold = CameraMode.World;
                else
                {
                    SavePosAndRot();
                    // use modeAfterHold for target acquisition
                    mode = modeAfterHold;
                    GetFollowTarget();
                    SetDefaultTimers();
                    mode = CameraMode.Hold;
                    modeAfterMove = modeAfterHold; // stay there
                }
            }
        }

        if (mode == CameraMode.Follow)
        {
            GetFollowTarget();
            // follow move camera
            PerformMove();
            // reset move timer
            cameraMoveTimer = 0f;
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
                    // handle mode after hold
                    mode = modeAfterHold;
                    return;
                }
                else
                    return;
            }
        }

        if ( mode == CameraMode.PanFollow )
        {
            GetPanTarget();
            // pan move camera
            PerformMove();
            return;
        }

        // CINEMATIC CAMERA MOVES

        // run move timer
        if (cameraMoveTimer > 0f)
        {
            cameraMoveTimer -= Time.deltaTime;
            if (cameraMoveTimer < 0f)
                cameraMoveTimer = 0f;
        }
        else
        {
            // at end of cinematic moves, allow follow behavior
            // (close up and medium only)
            if (mode == CameraMode.CloseUp || mode == CameraMode.Medium)
            {
                GetFollowTarget();
                // follow move camera
                PerformMove();
            }
            return;
        }

        // smooth progress to target
        float progress = ((cameraMoveDuration - cameraMoveTimer) / cameraMoveDuration);
        if (easeCurve == null)
            easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        if (easeCurve != null)
            progress = easeCurve.Evaluate(progress);

        // move camera
        gameObject.transform.position = Vector3.Lerp(savedPostion, cameraTargetPosition, progress);
        gameObject.transform.eulerAngles = Vector3.Lerp(savedRotation, cameraTargetRotation, progress);
        Vector3 rot = gameObject.transform.eulerAngles;
        rot.x = 90f - rot.x;
        rainBox.transform.localEulerAngles = rot;

        // progress done
        if (progress >= 1f)
        {
            SavePosAndRot();
            // handle mode after move
            mode = modeAfterMove;
        }
    }

    /*
    void OnGUI()
    {
        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;

        r.x = 0.1f * w;
        r.y = 0.3f * h;
        r.width = 0.2f * w;
        r.height = 0.2f * h;
        GUIStyle g = new GUIStyle(GUI.skin.label);
        g.fontSize = Mathf.RoundToInt(16f * (w / 1024f));
        g.wordWrap = true;
        string s = "camera mode: ";
        s += mode.ToString() + "\n";
        s += "pause: " + cameraPauseTimer + "\n";
        s += "move: " + cameraMoveTimer + "\n";
        Color c = Color.white;
        c.r = 0.381f;
        c.g = 0.618f;

        GUI.color = c;
        GUI.Label(r, s, g);
    }
    */
}
