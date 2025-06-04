using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NYFA Studio/Event/IntervalTrigger")]
public class IntervalTrigger : MonoBehaviour
{
    // Author: Glenn Storm
    // This activates another object repeatedly at time intervals

    [Tooltip("This is the object to activate every time interval is complete.")]
    public GameObject objectToActivate;
    [Tooltip("This is the shortest time this tool will delay before activating the object.")]
    public float intervalBase;
    [Tooltip("This optional setting will define the longest time for the delay. If set, a random time between the two will be used each interval, as long as the Mode is not Regular.")]
    public float intervalMax;
    public enum IntervalMode
    {
        Regular,
        Random,
        Gaussian
    }
    [Tooltip("This mode determines how the interval will be set between each activation. Regular will happen at the intervalBase consistently. Random will happen between the intervalBase and intervalMax. Gaussian will happen in a random interval like Random, but more likely the middle.")]
    public IntervalMode mode;

    private float timer;


    void Start()
    {
        // validate
        if ( objectToActivate == null )
        {
            Debug.LogError("--- IntervalTrigger [Start] : "+gameObject.name+" Object To Activate is empty. Aborting.");
            enabled = false;
        }
        if (intervalBase < 0f)
        {
            Debug.LogWarning("--- IntervalTrigger [Start] : " + gameObject.name + " Interval Base is less than zero. Will set to zero.");
            intervalBase = 0f;
        }
        if ( intervalMax < intervalBase )
        {
            if ( intervalMax != 0f || mode != IntervalMode.Regular )
                Debug.LogWarning("--- IntervalTrigger [Start] : " + gameObject.name + " Interval Max is less than Interval Base. Will set to Interval Base.");
            intervalMax = intervalBase;
        }
        // initialize
        if ( enabled )
        {
            timer = SetTimer();
        }
    }

    void Update()
    {
        if ( timer > 0f )
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                objectToActivate.SetActive(true);
                timer = SetTimer();
            }
        }
    }

    float SetTimer()
    {
        float retFloat = intervalBase;
        if ( mode == IntervalMode.Random )
            retFloat += Random.Range(0f, (intervalMax-intervalBase));
        else if ( mode == IntervalMode.Gaussian )
        {
            float rand = Random.Range(0f, 1f);
            // adjusted to gaussian distributed random range
            float adj = Mathf.Sin(rand * Mathf.PI);
            adj = Mathf.Clamp(adj, 0f, 1f);
            adj -= 1f;
            if (rand > 0.5f)
                adj *= -1f;
            // from the middle of the random range, weighted result +/- half
            retFloat += (0.5f * (intervalMax - intervalBase)) + (adj * 0.5f * (intervalMax - intervalBase));
        }
        return retFloat;
    }
}
