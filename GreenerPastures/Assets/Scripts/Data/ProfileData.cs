// REVIEW: necessary namespaces

public enum ProfileState
{
    Default,
    Creating,
    Offline,
    Connecting,
    Playing,
    Disconnecting
}

[System.Serializable]
public class ProfileData
{
    public string loginName;
    public string loginPass;
    public string profileID; // a unique identifier (separate from name)
    public ProfileState state;
    public ProfileOptionsData options;
    public string[] gameKeys; // game file identifiers this profile is in
}

[System.Serializable]
public class RosterData
{
    public string versionNumber;
    public ProfileData[] profiles;
}