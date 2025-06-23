using UnityEngine;

[System.Serializable]
public struct Account
{
    public string username;
    public string password;
}

[CreateAssetMenu(fileName = "Accounts", menuName = "Scriptable Objects/Accounts")]
public class Accounts : ScriptableObject
{
    public Account[] accounts;
}
