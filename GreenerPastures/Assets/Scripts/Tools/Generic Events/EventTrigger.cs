using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NYFA Studio/Event/EventTrigger")]
public class EventTrigger : MonoBehaviour
{
    // Author: Glenn Storm
    // This triggers events on an Event Manager to fire.

    [Tooltip("The Event Manager tool this trigger refers to. If blank, will try all Events Managers in scene for matching event name.")]
    public EventManager eventMgr;
    [Tooltip("The name of the event to trigger.")]
    public string eventName;
    [Tooltip("If more than zero, will wait this many seconds after activation to trigger the event.")]
    public float eventDelay;
    [Tooltip("If true, a collider component on this object (set to isTrigger) will be used to activate the trigger. If false, the Event Delay will be used.")]
    public bool useCollider;
    [Tooltip("This is an optional reference to the only collider this trigger will respond to. NOTE: Use Collider must be true.")]
    public Collider validCollider;
    [Tooltip("If true, this object will deactivate upon trigger, and be ready to re-activate. If false, this component will disable instead.")]
    public bool resetOnTrigger;
    [Tooltip("If true, will use the Collider2D objects for collision detection. If false, will use Collider objects in 3-D.")]
    public bool configureFor2D;

    private EventManager em;
    private float timer;
    private bool valid;


    void OnEnable()
    {
        if ( valid && resetOnTrigger )
        {
            if (!useCollider)
            {
                if (eventDelay > 0f)
                    timer = eventDelay;
                else
                    DoTrigger();
            }
        }
    }

    void Start()
    {
        // validate
        if ( eventMgr == null )
        {
            // search for event manager in scene
            EventManager[] ems = GameObject.FindObjectsByType<EventManager>(FindObjectsSortMode.None);
            for ( int i=0; i<ems.Length; i++ )
            {
                // take first with matching event name
                for ( int n=0; n<ems[i].events.Length; n++ )
                {
                    if ( ems[i].events[n].eventName == eventName )
                    {
                        em = ems[i];
                        break;
                    }
                }
            }
        }
        else
        {
            // ensure event name exists on named event mgr
            for (int n = 0; n < eventMgr.events.Length; n++)
            {
                if (eventMgr.events[n].eventName == eventName)
                {
                    em = eventMgr;
                    break;
                }
            }
        }
        if (em == null)
        {
            Debug.LogError("--- EventTrigger [Start] : " + gameObject.name + " no Event Manager with matching event name '" + eventName + "' found. Aborting.");
            enabled = false;
        }
        else
            eventMgr = em;
        if ( eventName == "" )
        {
            Debug.LogError("--- EventTrigger [Start] : " + gameObject.name + " no Event Name found. Aborting.");
            enabled = false;
        }
        if (eventDelay < 0f)
            Debug.LogWarning("--- EventTrigger [Start] : " + gameObject.name + " event delay is invalid. Will ignore.");
        if (useCollider)
        {
            if ( configureFor2D )
            {
                Collider2D c = gameObject.GetComponent<Collider2D>();
                if (c == null)
                {
                    Debug.LogError("--- EventTrigger [Start] : " + gameObject.name + " is set to Use Collider, but no collider2D component found. Aborting.");
                    enabled = false;
                }
                else if (!c.isTrigger)
                {
                    Debug.LogError("--- EventTrigger [Start] : " + gameObject.name + " is set to Use Collider, but collider2D is not set to isTrigger. Aborting.");
                    enabled = false;
                }
            }
            else
            {
                Collider c = gameObject.GetComponent<Collider>();
                if (c == null)
                {
                    Debug.LogError("--- EventTrigger [Start] : " + gameObject.name + " is set to Use Collider, but no collider component found. Aborting.");
                    enabled = false;
                }
                else if (!c.isTrigger)
                {
                    Debug.LogError("--- EventTrigger [Start] : " + gameObject.name + " is set to Use Collider, but collider is not set to isTrigger. Aborting.");
                    enabled = false;
                }
            }
        }
        // initialize
        if ( enabled )
        {
            valid = true;
            if ( !useCollider )
            {
                if (eventDelay > 0f)
                {
                    timer = eventDelay;
                }
                else
                    DoTrigger();
            }
        }
    }

    void Update()
    {
        if ( timer > 0f )
        {
            timer -= Time.deltaTime;
            if ( timer <= 0f )
            {
                timer = 0f;
                DoTrigger();
            }
        }
    }

    void DoTrigger()
    {
        em.SignalTrigger( eventName );
        // reset or disable
        if (resetOnTrigger)
        {
            gameObject.SetActive(false);
        }
        else
        {
            enabled = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!enabled || !configureFor2D)
            return;

        if ( validCollider == null || other == validCollider )
        {
            if (eventDelay > 0f)
                timer = eventDelay;
            else
                DoTrigger();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!enabled || configureFor2D)
            return;

        if (validCollider == null || other == validCollider)
        {
            if (eventDelay > 0f)
                timer = eventDelay;
            else
                DoTrigger();
        }
    }
}
