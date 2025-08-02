using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the camera movement for one player (the local client player)

    public enum CameraMode
    {
        Default,
        Hold,
        PanFollow,
        CloseUp,
        Medium,
        Follow,
        Long,
        World
    }

    public CameraMode mode = CameraMode.Follow;
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

    private Vector3 persistentFollowTarget;

    private AnimationCurve easeCurve; // basic ease-in-out curve

    private PositionData[] offsetPositions;
    private PositionData[] offsetRotations;

    private GameObject rainBox;
    private ParticleSystem rainVFX;
    private bool rainOn;
    private WeatherManager wm;

    const float CAMERAPAUSEDURATION = 0.0618f;
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
            Debug.LogWarning("--- CameraManager [Start] : no gamepad manager found. will ignore.");
            // enabled = false;
        }
        wm = GameObject.FindFirstObjectByType<WeatherManager>();
        if (wm == null)
        {
            Debug.LogError("--- CameraManager [Start] : no weather manager found. will ignore.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            if (savedPostion == Vector3.zero)
            {
                // ensure unparented
                gameObject.transform.parent = null;
                // start paused
                mode = CameraMode.Follow;
                modeAfterHold = CameraMode.Follow;
                modeAfterMove = CameraMode.Follow;
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

        if (mode == CameraMode.PanFollow)
        {
            // play no weather vfx (indoors)
            if (rainBox != null)
            {
                // rain vfx config (off)
                rainOn = rainVFX.isPlaying; // REVIEW:
                rainBox.SetActive(false);
            }
            if (wm != null)
            {
                // play all weather vfx as indoors
                wm.SFXForIndoors(true);
            }
        }
        else
        {
            if (rainAmount > 0f && !rainVFX.isPlaying)
                rainVFX.Play();
            else if (rainVFX.isPlaying && rainAmount == 0f)
                rainVFX.Stop();
        }
        rainOn = rainAmount > 0f;
    }

    void ConfigureCamOffsets()
    {
        offsetPositions = new PositionData[8];
        offsetRotations = new PositionData[8];

        // DEFAULT
        offsetPositions[(int)CameraMode.Default].x = 0f;
        offsetPositions[(int)CameraMode.Default].y = 2.5f;
        offsetPositions[(int)CameraMode.Default].z = -5f;
        offsetRotations[(int)CameraMode.Default].x = 20f;
        offsetRotations[(int)CameraMode.Default].y = 0f;
        offsetRotations[(int)CameraMode.Default].z = 0f;
        // FOLLOW
        offsetPositions[(int)CameraMode.Follow].x = 0f;
        offsetPositions[(int)CameraMode.Follow].y = 2.5f;
        offsetPositions[(int)CameraMode.Follow].z = -5f;
        offsetRotations[(int)CameraMode.Follow].x = 20f;
        offsetRotations[(int)CameraMode.Follow].y = 0f;
        offsetRotations[(int)CameraMode.Follow].z = 0f;
        // HOLD
        offsetPositions[(int)CameraMode.Hold].x = 0f;
        offsetPositions[(int)CameraMode.Hold].y = 2.5f;
        offsetPositions[(int)CameraMode.Hold].z = -5f;
        offsetRotations[(int)CameraMode.Hold].x = 20f;
        offsetRotations[(int)CameraMode.Hold].y = 0f;
        offsetRotations[(int)CameraMode.Hold].z = 0f;
        // PANFOLLOW
        offsetPositions[(int)CameraMode.PanFollow].x = 0f;
        offsetPositions[(int)CameraMode.PanFollow].y = 2.5f;
        offsetPositions[(int)CameraMode.PanFollow].z = -5f;
        offsetRotations[(int)CameraMode.PanFollow].x = 20f;
        offsetRotations[(int)CameraMode.PanFollow].y = 0f;
        offsetRotations[(int)CameraMode.PanFollow].z = 0f;
        // CLOSEUP
        offsetPositions[(int)CameraMode.CloseUp].x = 0f;
        offsetPositions[(int)CameraMode.CloseUp].y = 1f;
        offsetPositions[(int)CameraMode.CloseUp].z = -1f;
        offsetRotations[(int)CameraMode.CloseUp].x = 18f;
        offsetRotations[(int)CameraMode.CloseUp].y = 0f;
        offsetRotations[(int)CameraMode.CloseUp].z = 0f;
        // MEDIUM
        offsetPositions[(int)CameraMode.Medium].x = 0f;
        offsetPositions[(int)CameraMode.Medium].y = 1.75f;
        offsetPositions[(int)CameraMode.Medium].z = -3f;
        offsetRotations[(int)CameraMode.Medium].x = 20f;
        offsetRotations[(int)CameraMode.Medium].y = 0f;
        offsetRotations[(int)CameraMode.Medium].z = 0f;
        // LONG
        offsetPositions[(int)CameraMode.Long].x = 0f;
        offsetPositions[(int)CameraMode.Long].y = 7.5f;
        offsetPositions[(int)CameraMode.Long].z = -15f;
        offsetRotations[(int)CameraMode.Long].x = 22f;
        offsetRotations[(int)CameraMode.Long].y = 0f;
        offsetRotations[(int)CameraMode.Long].z = 0f;
        // WORLD
        offsetPositions[(int)CameraMode.World].x = 0f;
        offsetPositions[(int)CameraMode.World].y = 20f;
        offsetPositions[(int)CameraMode.World].z = -45f;
        offsetRotations[(int)CameraMode.World].x = 30f;
        offsetRotations[(int)CameraMode.World].y = 0f;
        offsetRotations[(int)CameraMode.World].z = 0f;
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
        if (pcm != null)
            return;

        playerObject = player.gameObject;
        pcm = player;

        // camera clip config
        cc = GameObject.FindFirstObjectByType<CameraClip>();
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
        // play weather vfx as outdoors
        if (rainBox != null)
        {
            // rain vfx config (on)
            rainBox.SetActive(true);
            if (rainOn)
            {
                rainVFX.Play();
            }
        }
        if (wm != null)
        {
            // play all weather sfx as outdoors
            wm.SFXForIndoors(false);
        }
    }

    /// <summary>
    /// Sets the camera mode to lock in a position and pan to follow player
    /// </summary>
    /// <param name="camPosition"></param>
    public void SetCameraPanMode(Vector3 camPosition)
    {
        gameObject.transform.position = camPosition;
        savedPostion = camPosition;
        mode = CameraMode.PanFollow;
        GetPanTarget();
        // play no weather vfx (indoors)
        if (rainBox != null)
        {
            // rain vfx config (off)
            rainOn = rainVFX.isPlaying;
            rainBox.SetActive(false);
        }
        if (wm != null)
        {
            // play all weather vfx as indoors
            wm.SFXForIndoors(true);
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
        cameraTargetPosition = GetPosOffset(CameraMode.CloseUp);
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
        persistentFollowTarget = playerObject.transform.position;
        cameraTargetRotation = GetRotOffset(mode);
    }

    Vector3 GetFollowDelta()
    {
        if (playerObject == null)
            return Vector3.zero;
        Vector3 retDelta = playerObject.transform.position - persistentFollowTarget;
        persistentFollowTarget = playerObject.transform.position;
        return retDelta;
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

        if (mode == CameraMode.Hold)
        {
            // update target by follow
            cameraTargetPosition += GetFollowDelta();
            // follow anyway
            gameObject.transform.position += GetFollowDelta();
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

        if (mode == CameraMode.PanFollow)
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
            // update target by follow
            cameraTargetPosition += GetFollowDelta();
            // follow anyway
            gameObject.transform.position += GetFollowDelta();
            // run timer
            cameraMoveTimer -= Time.deltaTime;
            if (cameraMoveTimer < 0f)
                cameraMoveTimer = 0f;
        }
        else
        {
            if (mode == CameraMode.Follow)
            {
                GetFollowTarget();
                // follow move camera
                PerformMove();
            }

            // at end of cinematic moves, allow follow behavior
            // (close up and medium only)
            if (mode == CameraMode.CloseUp || mode == CameraMode.Medium)
            {
                GetFollowTarget();
                // follow move camera
                PerformMove();
            }

            // detect player cam controls
            if (allowPlayerControlCam && mode > CameraMode.PanFollow &&
                cameraPauseTimer == 0f)
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
                    modeAfterHold = mode - camModeChange;
                    // clamp to valid cam modes
                    if (modeAfterHold < CameraMode.CloseUp)
                        modeAfterHold = CameraMode.CloseUp;
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
