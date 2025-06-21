// REVIEW: necessary namespaces

public static class ProfileSystem
{
    /// <summary>
    /// Creates a user roster to hold all profile data
    /// </summary>
    /// <param name="version">application version number string</param>
    /// <returns>iniitalized roster data</returns>
    public static RosterData InitializeUserRoster( string version )
    {
        RosterData retRoster = new RosterData();

        retRoster.versionNumber = version;
        retRoster.profiles = new ProfileData[0];

        return retRoster;
    }

    /// <summary>
    /// Returns true if given username exists as a profile in the given roster
    /// </summary>
    /// <param name="roster">roster data</param>
    /// <param name="username">profile login name</param>
    /// <returns>true if exists in roster, false if does not</returns>
    public static bool ProfileExistsInRoster( RosterData roster, string username )
    {
        bool retBool = false;

        for ( int i = 0; i < roster.profiles.Length; i++ )
        {
            if (roster.profiles[i].loginName == username)
            {
                retBool = true;
                break;
            }
        }

        return retBool;
    }

    /// <summary>
    /// Returns true if given roster holds a profile with matching name and password
    /// </summary>
    /// <param name="roster">roster data</param>
    /// <param name="username">profile login name</param>
    /// <param name="password">profile login password</param>
    /// <returns>true if password matches profile, false if not or if does not exist</returns>
    public static bool PasswordMatchToProfile( RosterData roster, string username, string password )
    {
        bool retBool = false;

        for (int i = 0; i < roster.profiles.Length; i++)
        {
            if (roster.profiles[i].loginName == username &&
                roster.profiles[i].loginPass == password)
            {
                retBool = true;
                break;
            }
        }

        return retBool;
    }

    /// <summary>
    /// Returns the profile data from given profile login name and password
    /// </summary>
    /// <param name="roster">roster data</param>
    /// <param name="username">profile login name</param>
    /// <param name="password">profile login password</param>
    /// <returns>profile data login name and login password, or blank if does not exist</returns>
    public static ProfileData GetProfile( RosterData roster, string username, string password )
    {
        ProfileData retProfile = new ProfileData();

        for (int i = 0; i < roster.profiles.Length; i++)
        {
            if (roster.profiles[i].loginName == username &&
                roster.profiles[i].loginPass == password)
            {
                retProfile = roster.profiles[i];
                break;
            }
        }

        return retProfile;
    }

    /// <summary>
    /// Adds the given profile to the given roster, if not already in
    /// </summary>
    /// <param name="roster">roster data</param>
    /// <param name="profile">profile data</param>
    /// <returns>roster data with profile added, if not already in</returns>
    public static RosterData AddProfile( RosterData roster, ProfileData profile )
    {
        RosterData retRoster = roster;

        // validate does not already exist in roster
        bool found = false;
        for (int i = 0; i < retRoster.profiles.Length; i++)
        {
            if (retRoster.profiles[i] == profile)
            {
                found = true;
                break;
            }
        }
        if (found)
            return retRoster;
        // add profile
        ProfileData[] tmp = new ProfileData[retRoster.profiles.Length + 1];
        for (int i = 0; i < retRoster.profiles.Length; i++)
        {
            tmp[i] = retRoster.profiles[i];
        }
        tmp[retRoster.profiles.Length] = profile;
        retRoster.profiles = tmp;

        return retRoster;
    }

    /// <summary>
    /// Removes a given profile from a given roster, if the profile existed
    /// </summary>
    /// <param name="roster">roster data</param>
    /// <param name="profile">profile data</param>
    /// <returns>roster data with profile removed, if it existed</returns>
    static public RosterData RemoveProfile( RosterData roster, ProfileData profile )
    {
        RosterData retRoster = roster;

        // validate already exists in roster
        bool found = false;
        for (int i = 0; i < retRoster.profiles.Length; i++)
        {
            if (retRoster.profiles[i] == profile)
            {
                found = true;
                break;
            }
        }
        if (!found)
            return retRoster;
        // remove profile
        ProfileData[] tmp = new ProfileData[retRoster.profiles.Length - 1];
        int count = 0;
        for (int i = 0; i < retRoster.profiles.Length; i++)
        {
            if (retRoster.profiles[i] != profile)
            {
                tmp[count] = retRoster.profiles[i];
                count++;
            }
        }
        retRoster.profiles = tmp;

        return retRoster;
    }

    /// <summary>
    /// Creates a new profile
    /// </summary>
    /// <param name="userName">profile user name</param>
    /// <param name="password">profile password</param>
    /// <returns>initialized profile data</returns>
    public static ProfileData InitializeProfile( string userName, string password )
    {
        ProfileData retProfile = new ProfileData();

        // initialize
        retProfile.loginName = userName;
        retProfile.loginPass = password;
        retProfile.profileID = "[" + System.DateTime.Now.Millisecond.ToString() + "]:" + userName + "::" + System.DateTime.Now.ToString();
        retProfile.state = ProfileState.Creating;
        retProfile.options = new ProfileOptionsData();
        retProfile.gameKeys = new string[0];

        return retProfile;
    }

    /// <summary>
    /// Adds a unique game key to the given user profile, if not already there
    /// </summary>
    /// <param name="profile">profile data</param>
    /// <param name="key">game key</param>
    /// <returns>the profile data with key added, if it wasn't already there</returns>
    public static ProfileData AddGameKey( ProfileData profile, string key )
    {
        ProfileData retProfile = profile;

        // validate key does not exist
        bool found = false;
        for (int i = 0; i < retProfile.gameKeys.Length; i++)
        {
            if (retProfile.gameKeys[i] == key)
            {
                found = true;
                break;
            }
        }
        if (found)
            return retProfile;
        // add key
        string[] tmp = new string[retProfile.gameKeys.Length + 1];
        for (int i = 0; i < retProfile.gameKeys.Length; i++)
        {
            tmp[i] = retProfile.gameKeys[i];
        }
        tmp[retProfile.gameKeys.Length] = key;
        retProfile.gameKeys = tmp;

        return retProfile;
    }

    /// <summary>
    /// Removes a unique game key from a given profile, if it existed
    /// </summary>
    /// <param name="profile">profile data</param>
    /// <param name="key">game key</param>
    /// <returns>profile data with game key removed, if it existed</returns>
    public static ProfileData RemoveGameKey( ProfileData profile, string key )
    {
        ProfileData retProfile = profile;

        // validate key does exist
        bool found = false;
        for (int i = 0; i < retProfile.gameKeys.Length; i++)
        {
            if (retProfile.gameKeys[i] == key)
            {
                found = true;
                break;
            }
        }
        if (!found)
            return retProfile;
        // remove key
        string[] tmp = new string[retProfile.gameKeys.Length - 1];
        int count = 0;
        for (int i = 0; i < retProfile.gameKeys.Length; i++)
        {
            if (retProfile.gameKeys[i] != key)
            {
                tmp[count] = retProfile.gameKeys[i];
                count++;
            }
        }
        retProfile.gameKeys = tmp;

        return retProfile;
    }
}
