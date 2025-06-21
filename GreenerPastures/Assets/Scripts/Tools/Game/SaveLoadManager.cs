using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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

    private string persistentPath;

    const string COMPANYNAME = "NYFA Game Design";
    const string PRODUCTNAME = "Greener Pastures";
    const string ROSTERFILE = "/GreenerRoster.dat";
    const string GAMESPATH = "/Games/";
    const string GAMEFILEPREFIX = "GreenerGame-";
    const string GAMEFILESUFFIX = ".dat";
    const string VERSIONNUMBERSTRING = "06.20.0127.a";


    void Awake()
    {
        persistentPath = Application.persistentDataPath;
    }

    void OnEnable()
    {
        // auto-load roster data
        if (LoadRosterData())
        {
            Debug.Log("--- SaveLoadManager [OnEnable] : GreenerRoster.dat auto-load success. [" + roster.profiles.Length + " profiles]");
        }
        else
        {
            Debug.LogWarning("--- SaveLoadManager [OnEnable] : GreenerRoster.dat auto-load unsuccessful. will establish new roster.");
            HandleFirstRunRoster();
            SaveRosterData();
        }
        // detect version mismatch and handle
        HandleVersionMismatch();
    }

    void OnDisable()
    {
        // auto-save roster data
        if (SaveRosterData())
        {
            Debug.Log("--- SaveLoadManager [OnDisable] : GreenerRoster.dat auto-save success. [" + roster.profiles.Length + " profiles]");
        }
        else
            Debug.LogWarning("--- SaveLoadManager [OnDisable] : GreenerRoster.dat auto-save unsuccessful. will ignore.");
    }

    void Start()
    {
        // validate
        // initialize
        if (enabled)
        {
            profile = null;
            game = null;
        }
    }

    void Update()
    {

    }

    public string GetVersionNumber()
    {
        return VERSIONNUMBERSTRING;
    }

    string GetGameDataPath()
    {
        return persistentPath;
    }

    string GetGameFileName(string key)
    {
        return GAMESPATH + GAMEFILEPREFIX + key + GAMEFILESUFFIX;
    }

    // TODO: 
    // . (upon selection of game...)
    // . load game data .dat

    // TODO: handle mismatch version data

    // TODO: once data loaded, find all timestamp data and handle tracked timer data
    //  (that is, subtract global time progress and _likely_ zero out timer values
    // use TimeManager.GetTimestampDifference( long ) to set float timer values
    //  > Magic
    //    spell book cooldowns
    //    cast lifetimes
    // ...

    bool LoadRosterData()
    {
        bool retBool = false;
        // load roster data file
        string p = GetGameDataPath() + ROSTERFILE;

        if (File.Exists(p))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = File.Open(p, FileMode.Open);
            roster = bf.Deserialize(fs) as RosterData;
            fs.Close();
            retBool = true;
        }

        return retBool;
    }

    bool SaveRosterData()
    {
        bool retBool = false;

        if (profile != null)
            LogoutProfile(profile);
        if (game != null)
        {
            SaveGameData(game.gameKey);
            game = null;
        }
        // save roster data file
        string p = GetGameDataPath() + ROSTERFILE;

        if (!File.Exists(p))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = File.Create(p);
            bf.Serialize(fs, roster);
            fs.Close();
            retBool = true;
        }
        else
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = File.Open(p, FileMode.Open);
            // write (overwrite)
            bf.Serialize(fs, roster);
            fs.Close();
            retBool = true;
        }

        return retBool;
    }

    public RosterData GetRosterData()
    {
        return roster;
    }

    void HandleFirstRunRoster()
    {
        roster = ProfileSystem.InitializeUserRoster(VERSIONNUMBERSTRING);
    }

    void HandleVersionMismatch()
    {
        if (roster.versionNumber == VERSIONNUMBERSTRING)
            return;

        Debug.LogWarning("--- SaveLoadManager [HandleVersionMismatch] : version mismatch. game data unrealiable.");

        // TODO:
    }

    public void CreateNewRosterEntry( ProfileData profile )
    {
        roster = ProfileSystem.AddProfile(roster, profile);
    }

    public void LoginProfile( ProfileData pData )
    {
        if (pData == null)
        {
            Debug.LogError("--- SaveLoadManager [LoginProfile] : invalid profile data. aborting.");
            return;
        }
        else if (profile != null && pData != profile)
        {
            Debug.LogError("--- SaveLoadManager [LoginProfile] : another profile is currently logged in. aborting.");
            return;
        }
        else if (profile != null && pData == profile)
        {
            Debug.LogError("--- SaveLoadManager [LoginProfile] : profile data is currently logged in. aborting.");
            return;
        }
        profile = pData;
        profile.state = ProfileState.Connecting;
    }

    public bool IsProfileLoggedIn()
    {
        return (profile != null);
    }

    public ProfileData GetCurrentProfile()
    {
        if (profile == null)
            Debug.LogWarning("--- SaveLoadManager [GetCurrentProfile] : no profile logged in. will return null.");

        return profile;
    }

    public void LogoutProfile( ProfileData pData )
    {
        if (profile == null)
        {
            Debug.LogError("--- SaveLoadManager [LogoutProfile] : no current profile logged in. aborting.");
            return;
        }
        else if (pData != profile)
        {
            Debug.LogError("--- SaveLoadManager [LogoutProfile] : given profile does not match current. aborting.");
            return;
        }
        else if (pData == null)
        {
            Debug.LogError("--- SaveLoadManager [LogoutProfile] : given profile data invalid. aborting.");
            return;
        }
        profile.state = ProfileState.Offline;
        profile = null;
    }

    public string[] GetGameKeysForCurrentProfile()
    {
        if (profile == null)
        {
            Debug.LogWarning("--- SaveLoadManager [GetGameKeysForCurrentProfile] : no profile logged in. will return empty list.");
            return new string[0];
        }
        return profile.gameKeys;
    }

    public bool LoadGameData( string gameKey )
    {
        bool retBool = false;
        // load game data file
        string p = GetGameDataPath() + GetGameFileName(gameKey);

        if (File.Exists(p))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = File.Open(p, FileMode.Open);
            game = bf.Deserialize(fs) as GameData;
            fs.Close();
            retBool = true;
        }

        return retBool;
    }

    public bool SaveGameData( string gameKey )
    {
        bool retBool = false;
        // save game data file
        string p = GetGameDataPath() + GetGameFileName(gameKey);

        if (!File.Exists(p))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = File.Create(p);
            bf.Serialize(fs, game);
            fs.Close();
            retBool = true;
        }
        else
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = File.Open(p, FileMode.Open);
            // write (overwrite)
            bf.Serialize(fs, game);
            fs.Close();
            retBool = true;
        }

        return retBool;
    }

    public GameData GetCurrentGameData()
    {
        if (game == null)
            Debug.LogWarning("--- SaveLoadManager [GetCurrentGameData] : no game loaded. will return null.");

        return game;
    }

    public void SetCurrentGameData( GameData gData )
    {
        if (gData == null)
        {
            Debug.LogError("--- SaveLoadManager [SetCurrentGameData] : empty game data. aborting.");
            return;
        }
        if (game != null)
            Debug.LogWarning("--- SaveLoadManager [SetCurrentGameData] : current game exists. will overwrite.");

        game = gData;
    }
}
