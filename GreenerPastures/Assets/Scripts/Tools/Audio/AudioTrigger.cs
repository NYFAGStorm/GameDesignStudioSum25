using UnityEngine;

[AddComponentMenu("NYFA Studio/Audio/AudioTrigger")]
public class AudioTrigger : MonoBehaviour
{
    // Author: Glenn Storm
    // This triggers sounds on the Audio Manager

    [Tooltip("The name of the audio manager tool used by this trigger. This allows multiple audio managers to be used.")]
    public string audioMgrName;
    [Tooltip("The name of the sound as configured in the AudioManager tool.")]
    public string soundName;
    [Tooltip("An optional delay in seconds between when this tool is activated and the sound is triggered.")]
    public float triggerDelay;
    [Tooltip("An optional game object to use as the source for this sound in 3D.")]
    public GameObject soundObject;
    [Tooltip("If soundObject used, this is the distance the volume rolloff begins.")]
    public float minDistance = 1f;
    [Tooltip("If soundObject used, this is the distance the volume rolloff ends.")]
    public float maxDistance = 10f;
    [Tooltip("If true, this tool will reset by deactivating, to be reused. If false, this tool will disable when triggered.")]
    public bool resetOnTrigger;

    private AudioManager am;
    private float timer;
    private bool triggered;
    private bool valid;

    void OnEnable()
    {
        if ( resetOnTrigger && valid && triggered )
        {
            if (triggerDelay > 0f)
                timer = triggerDelay;
            else
                DoTrigger();
        }
    }

    void Start()
    {
        // validate
        if (audioMgrName == "")
            Debug.LogWarning("---AudioTrigger[Start] : "+gameObject.name+" is missing the Audio Mgr Name property. Match this to the game object name of your Audio Manager tool.");
        AudioManager[] ams = GameObject.FindObjectsByType<AudioManager>(FindObjectsSortMode.None);
        for (int i = 0; i < ams.Length; i++)
        {
            if ( ams[i].gameObject.name == audioMgrName )
            {
                am = ams[i];
                break;
            }
        }
        if ( am == null )
        {
            Debug.LogError("--- AudioTrigger [Start] : no AudioManager tool named '"+audioMgrName+"' found in scene. Add _one_ and configure it with all your game sounds. Aborting.");
            enabled = false;
        }
        if ( soundName == "" )
        {
            Debug.LogError("--- AudioTrigger [Start] : " + gameObject.name + " no sound name configured. Aborting.");
            enabled = false;
        }
        else if ( am != null )
        {
            if ( !am.SoundExists(soundName) )
            {
                Debug.LogError("--- AudioTrigger [Start] : " + gameObject.name + " no sound with name '"+soundName+"' exists in the list of game sounds in the Audio Manager tool. Aborting.");
                enabled = false;
            }
        }
        if ( triggerDelay < 0f )
            Debug.LogWarning("---AudioTrigger[Start] : "+gameObject.name+" has a negative trigger delay configured. Will ignore.");
        if ( soundObject != null )
        {
            if ( minDistance < 0f )
            {
                Debug.LogWarning("---AudioTrigger[Start] : " + gameObject.name + " has a negative min distance configured. Will set to zero.");
                minDistance = 0f;
            }
            if ( maxDistance <= minDistance )
            {
                Debug.LogWarning("---AudioTrigger[Start] : " + gameObject.name + " has an invalid max distance configured, as compared to min distance. (min="+minDistance+", max="+maxDistance+") Will set max to min + 1f.");
                maxDistance = minDistance + 1f;
            }
        }
        if ( enabled )
        {
            // initialize
            valid = true;
            if (triggerDelay > 0f)
                timer = triggerDelay;
            else
                DoTrigger();
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
        // validate
        if (am == null)
        {
            // audio managers can be 'lost' in result of singleton pattern
            //Debug.LogWarning("--- AudioTrigger [DoTrigger] : audio manager lost. will ignore.");
            return;
        }

        triggered = true;

        // handle 3D sound
        if (soundObject == null)
            am.StartSound(soundName);
        else
            am.StartSound(soundName, soundObject, minDistance, maxDistance);

        if (resetOnTrigger)
            gameObject.SetActive(false);
        else
        {
            enabled = false;
            triggered = false; // not meant to be restarted
        }
    }
}
