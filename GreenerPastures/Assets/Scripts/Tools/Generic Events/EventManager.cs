using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NYFA Studio/Event/EventManager")]
public class EventManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This turns objects on and off, as an event

    [System.Serializable]
    public struct Event
    {
        public string eventName;
        public GameObject[] objectsToActivate;
        public GameObject[] objectsToDeactivate;
        public GameObject[] objectsToToggle;
        //public bool affectScriptsOnly; // enable/disable MonoBehavior scripts, instead of activating/deactivating objects
    }
    public Event[] events;


    void Start()
    {
        // validate
        if ( events == null || events.Length == 0 )
        {
            Debug.LogError("--- EventManager [Start] : " + gameObject.name + " events configured. Aborting.");
            enabled = false;
        }
        for ( int i=0; i<events.Length; i++ )
        {
            if ( events[i].eventName == "" )
            {
                Debug.LogError("--- EventManager [Start] : " + gameObject.name + " events #"+i+" has no name configured. Aborting.");
                enabled = false;
            }
            if ( events[i].objectsToActivate != null && events[i].objectsToActivate.Length > 0 )
            {
                for ( int n=0; n<events[i].objectsToActivate.Length; n++ )
                {
                    if ( events[i].objectsToActivate[n] == null )
                    {
                        Debug.LogError("--- EventManager [Start] : " + gameObject.name + " event '" + events[i].eventName + "' has a missing Object To Activate at #" + n + ". Aborting.");
                        enabled = false;
                    }
                }
            }
            if (events[i].objectsToDeactivate != null && events[i].objectsToDeactivate.Length > 0)
            {
                for (int n = 0; n < events[i].objectsToDeactivate.Length; n++)
                {
                    if (events[i].objectsToDeactivate[n] == null)
                    {
                        Debug.LogError("--- EventManager [Start] : " + gameObject.name + " event '" + events[i].eventName + "' has a missing Object To Deactivate at #" + n + ". Aborting.");
                        enabled = false;
                    }
                }
            }
            if (events[i].objectsToToggle != null && events[i].objectsToToggle.Length > 0)
            {
                for (int n = 0; n < events[i].objectsToToggle.Length; n++)
                {
                    if (events[i].objectsToToggle[n] == null)
                    {
                        Debug.LogError("--- EventManager [Start] : " + gameObject.name + " event '" + events[i].eventName + "' has a missing Object To Toggle at #" + n + ". Aborting.");
                        enabled = false;
                    }
                }
            }
        }
        // initialize
    }

    public void SignalTrigger( string eName )
    {
        int eventIdx = -1;
        for ( int n = 0; n<events.Length; n++ )
        {
            if ( events[n].eventName == eName )
            {
                eventIdx = n;
                break;
            }
        }
        if (eventIdx > -1)
        {
            if (events[eventIdx].objectsToActivate != null)
            {
                // activate
                for (int i = 0; i < events[eventIdx].objectsToActivate.Length; i++)
                {
                    events[eventIdx].objectsToActivate[i].SetActive(true);
                }
            }
            if (events[eventIdx].objectsToDeactivate != null)
            {
                // deactivate
                for (int i = 0; i < events[eventIdx].objectsToDeactivate.Length; i++)
                {
                    events[eventIdx].objectsToDeactivate[i].SetActive(false);
                }
            }
            if (events[eventIdx].objectsToToggle != null)
            {
                // toggle
                for (int i = 0; i < events[eventIdx].objectsToToggle.Length; i++)
                {
                    events[eventIdx].objectsToToggle[i].SetActive( !events[eventIdx].objectsToToggle[i].activeInHierarchy );
                }
            }
        }
        else
            Debug.LogWarning("--- EventManager [SignalTrigger] : "+gameObject.name+" no event with name '"+eName+"' found. Will ignore.");
    }
}
