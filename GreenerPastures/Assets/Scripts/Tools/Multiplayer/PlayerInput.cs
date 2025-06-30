using UnityEngine;
using Fusion;

public struct PlayerInput : INetworkInput
{
    public bool up;
    public bool down;
    public bool left;
    public bool right;
    public bool actionA;
    public bool actionADown; // 'first press' frame signal only (must un-press)
    public bool actionB;
    public bool actionBDown;
    public bool actionC;
    public bool actionCDown;
    public bool actionD;
    public bool actionDDown;
    public bool lBump;
    public bool lBumpDown;
    public bool rBump;
    public bool rBumpDown;
}
