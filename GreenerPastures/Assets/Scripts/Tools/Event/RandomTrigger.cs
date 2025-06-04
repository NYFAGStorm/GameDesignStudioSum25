using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NYFA Studio/Event/RandomTrigger")]
public class RandomTrigger : MonoBehaviour
{
    // Author: Glenn Storm
    // This randomly activates one of a series of objects

    [Tooltip("This is the list of objects to be activated, one will be chosen at random when this tool is turned on.")]
    public GameObject[] objectsToActivate;
    [Tooltip("If true, this tool will deactivate all objects to activate before selecting one to turn on.")]
    public bool deactivateOthers;
    [Tooltip("If true, the previous selection will not be selected.")]
    public bool noRepeat;

    private bool valid;
    private int prevSelection = -1;


    void OnEnable()
    {
        if ( valid )
        {
            DoTrigger();
        }
    }

    void Start()
    {
        // validate
        if ( objectsToActivate == null || objectsToActivate.Length == 0 )
        {
            Debug.LogError("--- RandomTrigger [Start] : "+gameObject.name+" Objects To Activate is empty. Aborting.");
            enabled = false;
        }
        else if ( noRepeat && objectsToActivate.Length == 1 )
        {
            Debug.LogWarning("--- RandomTrigger [Start] : " + gameObject.name + " is set to No Repeat, but there is only one Object To Activate. Will set No Repeat to false.");
            noRepeat = false;
        }
        // initialize
        if ( enabled )
        {
            valid = true;
            DoTrigger();
        }
    }

    void DoTrigger()
    {
        if ( deactivateOthers )
        {
            for (int i = 0; i < objectsToActivate.Length; i++)
            {
                if ( objectsToActivate[i].activeInHierarchy )
                    objectsToActivate[i].SetActive(false);
            }
        }
        int randomPick = Random.Range(0, objectsToActivate.Length);
        while (noRepeat && randomPick == prevSelection)
        {
            randomPick = Random.Range(0, objectsToActivate.Length);
        }
        prevSelection = randomPick;
        objectsToActivate[randomPick].SetActive(true);
        gameObject.SetActive(false);
    }
}
