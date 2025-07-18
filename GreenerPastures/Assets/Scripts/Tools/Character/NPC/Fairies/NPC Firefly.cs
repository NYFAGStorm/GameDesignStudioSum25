using UnityEngine;

public class NPCFirefly : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the firefly npc in menu mini game

    public float wingFrameTime = 0.02f;
    public Texture2D[] wingFrames;
    public float fireflyFrameTime = 0.1f;
    public Texture2D[] fireflyFrames;

    private Vector3 moveVector;
    private bool faceLeft;
    private Renderer render;
    private float wingFrameTimer;
    private float fireflyFrameTimer;
    private int wingFrame;
    private int fireflyFrame;

    private float flyTime;
    private Vector2 noiseVector;
    private float agitation;
    private float agitationWeight = 0.1f;

    private Vector3 moveTarget;
    private float targetTimer;
    private Vector2 fireflyRange;

    const float MAXMOVESPEED = 6.18f;
    const float ZIPFACTOR = 0.618f;


    void Start()
    {
        // validate
        render = gameObject.GetComponent<Renderer>();
        if (render == null)
        {
            Debug.LogError("--- NPC Firefly [Start] : no renderer found on this object. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            wingFrameTimer = wingFrameTime;
            // seed flytime
            flyTime = RandomSystem.GaussianRandom01() * 61.8f;
            noiseVector = new Vector2( RandomSystem.GaussianRandom01(), RandomSystem.GaussianRandom01() );
            targetTimer = 1f + RandomSystem.GaussianRandom01() * 3.81f;
            fireflyRange = new Vector2(18f, 10f);
        }
    }

    void Update()
    {
        // run wing frame timer
        if (wingFrameTimer > 0f)
        {
            wingFrameTimer -= Time.deltaTime;
            if (wingFrameTimer < 0f)
            {
                wingFrameTimer = wingFrameTime;
                // wing frame increment
                wingFrame++;
                if (wingFrame >= wingFrames.Length)
                    wingFrame = 0;
                render.material.mainTexture = wingFrames[wingFrame];
            }
        }
        
        // run firefly frame time
        if (fireflyFrameTimer > 0f)
        {
            fireflyFrameTimer -= Time.deltaTime;
            if (fireflyFrameTimer < 0f)
            {
                // increment firefly frame
                fireflyFrame++;
                if (fireflyFrame >= fireflyFrames.Length)
                {
                    fireflyFrame = 0;
                    fireflyFrameTimer = 0f;
                    Vector2 facing = Vector2.one;
                    if (faceLeft)
                        facing.x = -1f;
                    render.material.SetTextureScale("_LineArt", facing);
                    render.material.SetTextureScale("_MainTex", facing);
                }
                else
                    fireflyFrameTimer = fireflyFrameTime;
                render.material.SetTexture("_LineArt", fireflyFrames[fireflyFrame]);
            }
        }

        // run target time
        if (targetTimer > 0f)
        {
            targetTimer -= Time.deltaTime;
            if (targetTimer < 0f)
            {
                targetTimer = 1f + RandomSystem.GaussianRandom01() * 3.81f;
                Vector3 newTarget = Vector3.zero;
                newTarget.x = RandomSystem.GaussianRandom01() * fireflyRange.x;
                newTarget.x -= fireflyRange.x * 0.5f;
                newTarget.y = RandomSystem.GaussianRandom01() * fireflyRange.y;
                newTarget.y -= fireflyRange.y * 0.5f;
                moveTarget = newTarget;
            }
        }

        // detect direction change
        if (moveVector.x < 0f && !faceLeft)
        {
            faceLeft = true;
            fireflyFrameTimer = fireflyFrameTime;
        }
        else if (moveVector.x > 0f && faceLeft)
        {
            faceLeft = false;
            fireflyFrameTimer = fireflyFrameTime;
        }

        // handle movement
        Vector3 pos = gameObject.transform.position;
        pos.x += moveVector.x * MAXMOVESPEED * Time.deltaTime;
        pos.y += moveVector.y * MAXMOVESPEED * Time.deltaTime;
        gameObject.transform.position = pos;

        // flytime increase
        flyTime += Time.deltaTime;

        // set move vector
        Vector3 m = Vector3.zero;
        m.x = Mathf.PerlinNoise(flyTime, noiseVector.y) - 0.5f;
        m.y = Mathf.PerlinNoise(noiseVector.x, flyTime) - 0.5f;
        // center gravity based on agitation
        m.x -= ZIPFACTOR * (gameObject.transform.position.x-moveTarget.x) * (1f - agitation);
        m.y -= ZIPFACTOR * (gameObject.transform.position.y-moveTarget.y) * (1f - agitation);
        moveVector = m;

        // weigh down agitation
        agitation = Mathf.Clamp01(agitation - (agitationWeight * Time.deltaTime));
    }
}
