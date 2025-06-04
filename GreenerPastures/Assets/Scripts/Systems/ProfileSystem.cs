// REVIEW: necessary namespaces

public static class ProfileSystem
{
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
        retProfile.state = ProfileState.Creating;
        retProfile.options = new ProfileOptionsData();

        return retProfile;
    }
}
