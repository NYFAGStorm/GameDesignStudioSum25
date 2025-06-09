using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuLayerManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles bg layer movement for menus
    // (NOTE: while not in menus, layers hidden)

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

    private float currentAnimProgress;
    private string previousSceneName;

    const int TOTALANIMKEYS = 3;
    const float ANIMATIONINTERPDURATION = 3f;


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
        }
    }

    void Update()
    {
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

    void OnSceneLoaded(Scene s, LoadSceneMode mode)
    {
        // hide all layers if in game
        if (s.name == "Proto_GreenerStuff" && previousSceneName == "Menu")
        {
            for (int i=0; i<layers.Length; i++)
            {
                layers[i].layerObj.SetActive(false);
            }
        }
        else if (previousSceneName == "Proto_GreenerStuff")
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
    }
}
