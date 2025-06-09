using UnityEngine;

public class MenuLayerManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles bg layer movement for menus

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
}
