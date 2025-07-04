using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuLayerManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles bg layer movement for menus
    // (NOTE: while not in menus, layers hidden)

    public enum CinematicFade
    {
        Default,
        FadeWhite,
        FadeBlack
    }

    [System.Serializable]
    public struct BGLayer
    {
        public GameObject layerObj;
        public float layerMoveMultiplier;
        public float savedVerticalPos;
    }

    public BGLayer[] layers;
    public float verticalMovement;
    public AnimationCurve animCurve;
    public int targetKey = 0;

    public CinematicFade fadeType;
    public bool fadingDownFromColor;
    public float pauseTimer;
    public float fadeTimer;

    private float currentAnimProgress;
    private string previousSceneName;
    private string currentSceneName;

    const int TOTALANIMKEYS = 3;
    const float ANIMATIONINTERPDURATION = 3f;
    const float FADETIME = 1f;
    const float LAUNCHPAUSETIME = 2f;


    void Start()
    {
        // validate
        if ( layers == null || layers.Length == 0 )
        {
            Debug.LogError("--- MenuLayerManager [Start] : no layers defined. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            previousSceneName = "Splash";
            for (int i=0; i<layers.Length; i++)
            {
                layers[i].savedVerticalPos = layers[i].layerObj.transform.localPosition.y;
            }
            // configure fade to white
            fadeType = CinematicFade.Default;
            fadingDownFromColor = false;
        }
    }

    void Update()
    {
        // run pause timer
        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.deltaTime;
            if (pauseTimer < 0f)
                pauseTimer = 0f;
        }
        // run fade timer
        if (pauseTimer == 0f && fadeTimer > 0f)
        {
            fadeTimer -= Time.deltaTime;
            if (fadeTimer < 0f)
            {
                fadeTimer = 0f;
                if (fadingDownFromColor)
                    fadeType = CinematicFade.Default; // reset
            }
        }

        // run animation
        if ( targetKey > currentAnimProgress * (TOTALANIMKEYS-1) )
        {
            currentAnimProgress += (1f/ANIMATIONINTERPDURATION * Time.deltaTime) / (TOTALANIMKEYS - 1);
            verticalMovement = animCurve.Evaluate(currentAnimProgress) * -60f;
        }

        // apply vert(ical movement to layers
        for (int i = 0; i < layers.Length; i++)
        {
            Vector3 pos = layers[i].layerObj.transform.localPosition;

            pos.y = layers[i].savedVerticalPos + (verticalMovement * layers[i].layerMoveMultiplier);

            layers[i].layerObj.transform.localPosition = pos;
        }

        // apply color to layers
    }

    void SetAnimProgress( float progress )
    {
        currentAnimProgress = progress * 0.5f;
        verticalMovement = animCurve.Evaluate(currentAnimProgress) * -60f;
        targetKey = Mathf.RoundToInt(progress);
    }

    public void LaunchGameAnimation()
    {
        targetKey = 2;
        // fade to white
        fadeType = CinematicFade.FadeWhite;
        fadingDownFromColor = false;
        fadeTimer = FADETIME;
        pauseTimer = LAUNCHPAUSETIME;
    }

    void OnSceneLoaded(Scene s, LoadSceneMode mode)
    {
        // hide all layers if in game
        if (s.name == "GreenerGame" && previousSceneName == "Menu")
        {
            for (int i=0; i<layers.Length; i++)
            {
                layers[i].layerObj.SetActive(false);
            }
            // fade from white
            if (currentSceneName == "Menu")
            {
                fadeType = CinematicFade.FadeWhite;
                fadingDownFromColor = true;
                fadeTimer = FADETIME;
            }
        }
        else if (previousSceneName == "GreenerGame")
        {
            for (int i = 0; i < layers.Length; i++)
            {
                layers[i].layerObj.SetActive(true);
            }
        }
        // reconfigure layers for this scene instantly, unless animating splash->menu->game
        if (s.name == "Menu" && previousSceneName != "Splash" && previousSceneName != "Credits")
            SetAnimProgress(1);
        else if (s.name == "Splash")
            SetAnimProgress(0);

        previousSceneName = s.name;
        currentSceneName = s.name;
    }

    void OnGUI()
    {
        if (fadeTimer == 0f && fadeType == CinematicFade.Default)
            return;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;

        r.x = -0.1f * w;
        r.y = -0.1f * h;
        r.width = 1.2f * w;
        r.height = 1.2f * h;
        Texture2D t = Texture2D.whiteTexture;
        Color c = Color.white;
        if (fadeType == CinematicFade.FadeBlack)
            c = Color.black;
        if (fadingDownFromColor)
            c.a = (fadeTimer / FADETIME);
        else
            c.a = 1f - (fadeTimer / FADETIME);
        GUI.color = c;
        GUI.DrawTexture(r, t);
    }
}
