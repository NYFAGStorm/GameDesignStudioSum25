using UnityEngine;

public class TeleportManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles teleport pads, linked together in pairs
    // REVIEW: characters only? players only?

    public string teleporterTag;
    public GameObject teleportSubject;
    [Tooltip("if none defined, will ignore. otherwise reaching this node will trigger.")]
    public CameraTrigger associatedCamTrigger;
    public GameObject islandObj; // hold player to this center
    public float islandRadius = 7f;

    private bool teleportActive;
    private float teleportTimer;
    private float teleportCheckTimer;
    private TeleportManager pairedPad;

    const float TELEPORTDURATION = 0.5f;
    const float TELEPORTPADRADUIS = 0.25f;
    const float TELEPORTCHECKINTERVAL = 1f;


    void Start()
    {
        // validate
        if ( teleporterTag == "" )
        {
            Debug.LogError("--- TeleportManager [Start] : "+gameObject.name+" has no teleporter tag. aborting.");
            enabled = false;
        }
        else
        {
            int found = 0;
            TeleportManager[] tports = GameObject.FindObjectsByType<TeleportManager>(FindObjectsSortMode.None);
            for (int i=0; i<tports.Length; i++)
            {
                if (tports[i] != this && tports[i].teleporterTag == teleporterTag)
                {
                    if (tports[i] == this)
                        continue;
                    if (tports[i].teleporterTag == teleporterTag)
                    {
                        pairedPad = tports[i];
                        found++;
                    }
                }
            }
            if (found > 1)
            {
                Debug.LogError("--- TeleportManager [Start] : " + gameObject.name + " more than one teleport pad found with same tag '" + teleporterTag + "'. aborting.");
                enabled = false;
            }
        }
        if ( islandObj == null )
        {
            Debug.LogError("--- TeleportManager [Start] : " + gameObject.name + " no island object defined. aborting.");
            enabled = false;
        }
        if (enabled && pairedPad == null)
        {
            Debug.LogError("--- TeleportManager [Start] : " + gameObject.name + " no teleport pad found with tag '" + teleporterTag + "'. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            teleportCheckTimer = TELEPORTCHECKINTERVAL;
        }
    }

    void Update()
    {
        // run teleport timer
        if (teleportTimer > 0f)
        {
            teleportTimer -= Time.deltaTime;
            if (teleportTimer < 0f)
            {
                teleportTimer = 0f;
                DepositTeleported();
                teleportActive = false;
            }
        }

        // ignore if active
        if (teleportActive)
            return;

        if (teleportCheckTimer > 0f)
        {
            teleportCheckTimer -= Time.deltaTime;
            if (teleportCheckTimer > 0f)
                return;
            teleportCheckTimer = TELEPORTCHECKINTERVAL;
        }

        PlayerControlManager[] players = GameObject.FindObjectsByType<PlayerControlManager>(FindObjectsSortMode.None);
        for (int i=0; i<players.Length; i++)
        {
            if ( Vector3.Distance(players[i].gameObject.transform.position, gameObject.transform.position) < TELEPORTPADRADUIS )
            {
                AcquireTeleported(players[i].gameObject);
                break;
            }
        }
    }

    void AcquireTeleported( GameObject subject )
    {
        // validate
        if ( subject == null )
        {
            Debug.LogWarning("--- TeleportManager [AcquireTeleported] : " + gameObject.name + " teleport subject no foudn. will ignore.");
            return;
        }

        // acquire teleport subject
        LaunchTeleportEffects();
        teleportSubject = subject;
        // de-materialize teleport subject
        teleportSubject.GetComponent<PlayerControlManager>().characterFrozen = true;
        Renderer[] rends = subject.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rends)
        {
            r.enabled = false;
        }

        // engage teleport
        teleportActive = true;
        teleportTimer = TELEPORTDURATION;
    }

    void DepositTeleported()
    {
        // validate
        if (teleportSubject == null)
        {
            Debug.LogWarning("--- TeleportManager [DepositTeleported] : "+gameObject.name+" teleport subject lost. will ignore.");
            return;
        }

        // teleport location
        pairedPad.LaunchTeleportEffects();
        teleportSubject.transform.position = pairedPad.transform.position;
        // materialize teleport subject
        PlayerControlManager pcm = teleportSubject.GetComponent<PlayerControlManager>();
        pcm.characterFrozen = false;
        Renderer[] rends = teleportSubject.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rends)
        {
            r.enabled = true; // REVIEW: are there things that should not be visible?
        }

        // if associated camera trigger, trigger
        if (pairedPad.associatedCamTrigger != null)
            pairedPad.associatedCamTrigger.TriggerCameraMode();

        // hold player to new location
        pcm.playerData.island.w = pairedPad.islandRadius;
        pcm.playerData.island.x = pairedPad.islandObj.transform.position.x;
        pcm.playerData.island.y = pairedPad.islandObj.transform.position.y;
        pcm.playerData.island.z = pairedPad.islandObj.transform.position.z;
    }

    /// <summary>
    /// Launches effects of teleporting for this teleport node
    /// </summary>
    public void LaunchTeleportEffects()
    {
        teleportCheckTimer = 3f;
        // vfx
        GameObject vfx = GameObject.Instantiate((GameObject)Resources.Load("VFX Tport Flash"));
        vfx.transform.position = transform.position;
        vfx.transform.Find("VFX Sprite").GetComponent<SpriteRenderer>().material.color = Color.yellow;
        Destroy(vfx, 1f);
        // TODO: sfx
    }
}
