using UnityEngine;

public class RemotePlayerManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles un-packing and manipulation of a remote player's character in this net game

    // TODO: declare main reference to multiplayer data coming in

    public string profileID; // remote player profile ID
    public string playerName; // remote player name

    public PositionData playerPosition; // x,y,z + 'w' which = character facing pose

    private PositionData previousPosition; // we use this to help smooth, drive character anim mgr
    private Vector3 moveVector; // used for character anim mgr
    private CharacterAnimManager pam;

    private PlayerData playerData; // access for color and player effects (read-only)

    private bool remotePlayerIntialized;

    const float LERPDISTANCETHRESHOLD = 1f;


    void Start()
    {
        // validate
        pam = gameObject.GetComponentInChildren<CharacterAnimManager>();
        if (pam == null)
        {
            Debug.LogError("--- RemotePlayerManager [Start] : " + gameObject.name + " no character anim manager found on this object. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            previousPosition = playerPosition;
        }
    }

    void Update()
    {
        if (!remotePlayerIntialized)
            return;

        // update player position
        UpdatePlayerPosition();
    }

    public void InitializeRemotePlayer( string profID, string pName )
    {
        profileID = profID;
        playerName = pName;
        // get player options from game data, configure appearance
        SaveLoadManager saveMgr = GameObject.FindFirstObjectByType<SaveLoadManager>();
        if (saveMgr != null)
        {
            GameData gData = saveMgr.GetCurrentGameData();
            if (gData != null)
            {
                PlayerData pData = GameSystem.GetProfilePlayer(gData, profileID);
                if (pData != null)
                {
                    ConfigureAppearance(pData.options);
                    playerData = pData;
                }
            }
        }
        remotePlayerIntialized = true;
    }

    /// <summary>
    /// Use SetRemovePlayerPosition(pos) instead
    /// </summary>
    public void SetRemotePlayerPosition( Vector3 pos, bool artFlipped )
    {
        previousPosition = playerPosition;
        playerPosition.x = pos.x;
        playerPosition.y = pos.y;
        playerPosition.z = pos.z;
        // this will cause anim problems (needs to be based on move vector)
        playerPosition.w = artFlipped ? -1 : 1;
    }

    public void SetRemotePlayerPosition( PositionData pos )
    {
        moveVector = GameSystem.GetDeltaVector(pos, previousPosition);
        previousPosition = playerPosition;
        playerPosition = pos;
    }

    void DoColorTrail(Vector3 pos)
    {
        // REVIEW: cleanup

        // SPELL COLOR TRAIL I, II, II
        if (Time.time % 1f < 0.05f)
        {
            GameObject lightingFolderObject = GameObject.Find("Lighting");
            if (PlayerSystem.PlayerHasEffect(playerData, PlayerEffect.SpellColorTrailI))
            {
                Color c = PlayerSystem.GetPlayerColor(playerData.options.mainColor);
                GameObject vfx = GameObject.Instantiate((GameObject)Resources.Load("Spells/VFX Spell Color Trail"));
                vfx.transform.position = pos;
                Vector3 offset = Vector3.zero;
                offset.x = RandomSystem.GaussianRandom01() - 0.5f;
                offset.z = RandomSystem.GaussianRandom01() - 0.5f;
                offset *= 0.381f;
                vfx.transform.position += offset;
                Vector3 scl = Vector3.one;
                scl *= RandomSystem.GaussianRandom01();
                vfx.transform.localScale = scl;
                vfx.name = "VFX Color Trail I";
                vfx.transform.parent = lightingFolderObject.transform;
                vfx.GetComponent<Renderer>().material.color = c;
                Destroy(vfx, 3.81f);
            }
            if (PlayerSystem.PlayerHasEffect(playerData, PlayerEffect.SpellColorTrailII))
            {
                Color c = PlayerSystem.GetPlayerColor(playerData.options.secondaryColor);
                GameObject vfx = GameObject.Instantiate((GameObject)Resources.Load("Spells/VFX Spell Color Trail"));
                vfx.transform.position = pos;
                Vector3 offset = Vector3.zero;
                offset.x = RandomSystem.GaussianRandom01() - 0.5f;
                offset.z = RandomSystem.GaussianRandom01() - 0.5f;
                offset *= 0.381f;
                vfx.transform.position += offset;
                Vector3 scl = Vector3.one;
                scl *= RandomSystem.GaussianRandom01();
                vfx.transform.localScale = scl;
                vfx.name = "VFX Color Trail II";
                vfx.transform.parent = lightingFolderObject.transform;
                vfx.GetComponent<Renderer>().material.color = c;
                Destroy(vfx, 3.81f);
            }
            if (PlayerSystem.PlayerHasEffect(playerData, PlayerEffect.SpellColorTrailIII))
            {
                Color c = PlayerSystem.GetPlayerColor(playerData.options.accentColor);
                GameObject vfx = GameObject.Instantiate((GameObject)Resources.Load("Spells/VFX Spell Color Trail"));
                vfx.transform.position = pos;
                Vector3 offset = Vector3.zero;
                offset.x = RandomSystem.GaussianRandom01() - 0.5f;
                offset.z = RandomSystem.GaussianRandom01() - 0.5f;
                offset *= 0.381f;
                vfx.transform.position += offset;
                Vector3 scl = Vector3.one;
                scl *= RandomSystem.GaussianRandom01();
                vfx.transform.localScale = scl;
                vfx.name = "VFX Color Trail III";
                vfx.transform.parent = lightingFolderObject.transform;
                vfx.GetComponent<Renderer>().material.color = c;
                Destroy(vfx, 3.81f);
            }
        }
    }

    void UpdatePlayerPosition()
    {
        // REVIEW: perform some lerp if distance is great?
        if (GameSystem.PositionDistance(previousPosition, playerPosition) > LERPDISTANCETHRESHOLD)
        {
            // instead of set, do fancy stuff like lerp position before next tick?
            //GameSystem.Lerp(previousPosition, playerPosition, 0.1f); // enter some progress value of (tick/Time.deltaTime)
            // REVIEW: could do 'art flipped' from here, based change from previous
        }
        Vector3 pos = Vector3.zero;
        pos.x = playerPosition.x;
        pos.y = playerPosition.y;
        pos.z = playerPosition.z;
        DoColorTrail(pos);
        pam.characterMoveVector = moveVector;
        gameObject.transform.position = pos;
    }

    public void ConfigureAppearance( PlayerOptions options )
    {
        if (pam != null)
            pam.ConfigureAppearance(options);
        else
            Debug.LogWarning("--- RemotePlayerManager [ConfigureAppearance] : no character anim manager found. will ignore.");
    }
}
