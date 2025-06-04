using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NYFA Studio/Utility/MoveObject")]
public class MoveObject : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles simple postiion movement of objects in a sequence of targets

    [System.Serializable]
    public struct KeyFrame
    {
        public float delay;
        public Vector3 targetPos;
        // TODO: rot and scale
        public float duration;
        public bool hFlip;
        public bool vFlip;
    }
    public KeyFrame[] keys;
    public bool loop;
    public bool resetOnComplete;

    private bool valid;
    private Vector3 initPos;
    private Vector3 prevPos;
    private float delayTimer;
    private float moveTimer;
    private int currentTarget;


    void OnEnable()
    {
       if (valid)
        {
            currentTarget = 0;
            delayTimer = keys[0].delay;
            if (delayTimer == 0f)
                delayTimer = Time.deltaTime;
            moveTimer = keys[0].duration;
            gameObject.transform.position = initPos;
            prevPos = gameObject.transform.position;
        }
    }

    void Start()
    {
        // validate
        if ( keys == null || keys.Length == 0 )
        {
            Debug.LogError("--- MoveObject [Start] : no keys configured. aborting.");
            enabled = false;
        }
        // initialize
        if ( enabled )
        {
            valid = true;
            delayTimer = keys[0].delay;
            if (delayTimer == 0f)
                delayTimer = Time.deltaTime;
            moveTimer = keys[0].duration;
            initPos = gameObject.transform.position;
            prevPos = gameObject.transform.position;
        }
    }

    void Update()
    {
        if ( delayTimer > 0f )
        {
            delayTimer -= Time.deltaTime;
            if ( delayTimer < 0f )
            {
                delayTimer = 0f;
            }
        }
        else if ( moveTimer > 0f )
        {
            moveTimer -= Time.deltaTime;
            if ( moveTimer < 0f )
            {
                moveTimer = 0f;
                // target done
                MoveToTarget(currentTarget, 1f);
                prevPos = gameObject.transform.position;
                currentTarget++;
                if ( currentTarget >= keys.Length )
                {
                    // move done
                    if ( loop )
                    { 
                        currentTarget = 0;
                        delayTimer = keys[0].delay;
                        if (delayTimer == 0f)
                            delayTimer = Time.deltaTime;
                        moveTimer = keys[0].duration;
                        Vector3 s = Vector3.one;
                        if (keys[0].hFlip)
                            s.x *= -1f;
                        if (keys[0].vFlip)
                            s.y *= -1f;
                        gameObject.transform.localScale = s;
                    }
                    else
                    {
                        if (resetOnComplete)
                            gameObject.SetActive(false);
                        else
                            enabled = false;
                    }
                }
                else
                {
                    delayTimer = keys[currentTarget].delay;
                    if (delayTimer == 0f)
                        delayTimer = Time.deltaTime;
                    moveTimer = keys[currentTarget].duration;
                    Vector3 s = Vector3.one;
                    if (keys[currentTarget].hFlip)
                        s.x *= -1f;
                    if (keys[currentTarget].vFlip)
                        s.y *= -1f;
                    gameObject.transform.localScale = s;
                }
            }
            else
            {
                float progress = 1f - (moveTimer / keys[currentTarget].duration);
                MoveToTarget(currentTarget, progress);
            }
        }
    }

    void MoveToTarget( int keyTarget, float progress )
    {
        gameObject.transform.position = Vector3.Lerp(prevPos, keys[keyTarget].targetPos, progress);
    }
}
