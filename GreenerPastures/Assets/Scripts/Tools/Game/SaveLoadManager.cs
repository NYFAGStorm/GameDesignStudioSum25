using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles creating, reading and writing save data files

    // LOCAL (CLIENT) FILE ARCHITECTURE
    //
    // <path>/GreenerRoster.dat
    // <path>/Games/GreenerGame-[gamekey].dat

    private RosterData roster; // roster of profiles available (local)

    private ProfileData profile; // the current profile logged in (local)
    private GameData game; // the currently loaded game data

    private string[] gamesAvailable; // list of game names this profile is in

    private string gameFile; // current game data file name

    const string ROSTERFILEPATH = "./GreenerRoster.dat";
    const string GAMESPATH = "./Games/";
    const string GAMEPREFIX = "GreenerGame-";
    const string GAMESUFFIX = ".dat";
    const string VERSIONNUMBERSTRING = "06.20.0126.a";


    void Start()
    {
        // validate
        // initialize
        if (enabled)
        {

        }
    }

    void Update()
    {
        
    }

    public string GetVersionNumber()
    {
        return VERSIONNUMBERSTRING;
    }

    // TODO: auto-load on enable
    // . load roster.dat
    // . (upon login , set profile to current)
    // . (upon selectin of game...)
    // . load game data .dat
    // TODO: auto-save on destroy
    // . save roster.dat
    // . save current game data .dat

    // TODO: handle mismatch version data

    // TODO: once data loaded, find all timestamp data and handle tracked timer data
    //  (that is, subtract global time progress and _likely_ zero out timer values
    // use TimeManager.GetTimestampDifference( long ) to set float timer values
    //  > Magic
    //    spell book cooldowns
    //    cast lifetimes
    // ...

    void LoadRosterData()
    {
        // TODO: 

        // TODO: detect no roster file
        // HandleFirstRunRoster();
        // SaveRosterData();

        // TODO: detect version mismatch
        // HandleVersionMismatch();
    }

    void SaveRosterData()
    {
        if (profile != null)
            LogoutProfile(profile);
        if (game != null)
        {
            SaveGameData();
            game = null;
        }
        // TODO:
    }

    public void LoginProfile( ProfileData pData )
    {
        if (pData != null && pData != profile)
        {
            Debug.LogError("--- SaveLoadManager [LogoutProfile] :another profile is currently logged in. aborting.");
            return;
        }
        else if (pData == profile)
            Debug.LogWarning("--- SaveLoadManager [LogoutProfile] : current profile already logged in. will ignore.");
        profile = pData;
    }

    public void LogoutProfile( ProfileData pData )
    {
        if (pData != profile)
        {
            Debug.LogError("--- SaveLoadManager [LogoutProfile] : given profile does not match current. aborting.");
            return;
        }
        else if (pData == null)
            Debug.LogWarning("--- SaveLoadManager [LogoutProfile] : no current profile. will ignore.");
        profile = null;
    }

    void LoadGameData()
    {
        // TODO:
    }

    void SaveGameData()
    {
        // TODO:
    }

    void HandleFirstRunRoster()
    {
        roster = ProfileSystem.InitializeUserRoster(VERSIONNUMBERSTRING);
    }

    void HandleVersionMismatch()
    {
        // TODO:
    }
}
