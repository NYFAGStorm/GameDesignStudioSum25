using UnityEngine;
using UnityEngine.SceneManagement;

[AddComponentMenu("NYFA Studio/Interface/MainMenu")]
public class MainMenu : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the main menu

    public enum ButtonAction
    {
        Default,
        SceneSwitch,
        Quit,
        PopupLaunch,
    }

    public string titleText;
    [Tooltip("These values represent a proportion of screen space, as percentages of screen space. Like, less than 1")]
    public Rect title;

    public Font titleFont;
    public FontStyle titleFontStyle;
    public Color titleFontColor = Color.white;
    [Tooltip("Font will scale appropriately based on active screen width, using this 1024 size")]
    public int titleFontSizeAt1024 = 60;

    [System.Serializable]
    public struct ButtonDef
    {
        public string buttonText;
        public Rect buttonPos;
        public ButtonAction buttonAction;
        public string sceneName;
        public bool buttonEnabled;
        public bool buttonVisible;
    }
    public ButtonDef[] buttons;

    public Font buttonFont;
    public FontStyle buttonFontStyle;
    public Color buttonFontColor = Color.white;
    [Tooltip("Font will scale appropriately based on active screen width, using this 1024 size")]
    public int buttonFontSizeAt1024 = 48;

    private float sceneSwitchTimer;
    private string sceneSwitchName;

    private SaveLoadManager saveMgr;
    private bool profileActive; // from data, is profile logged in
    private string profileName;
    private bool gameLoaded; // from data, is game data loaded
    private string gameName;

    private MultiGamepad padMgr;
    private int padButtonSelection = -1;
    private int padMaxButton = 5;

    private bool profilePopup; // is displaying popup
    private bool popupEnabled; // is popping up or down
    private float popupTimer;
    private float popupProgress;
    private AnimationCurve popupCurve;
    private string popupName;
    private string popupPass;
    private bool popupConfirm;
    private string confirmPass;
    private string popupFeedback;

    const float POPUPTIME = 1f;
    const int PLAYERPROFILEBUTTON = 1;
    const int GAMESELECTIONBUTTON = 2;


    void Start()
    {
        // validate
        saveMgr = GameObject.FindFirstObjectByType<SaveLoadManager>();
        if (saveMgr == null)
        {
            Debug.LogError("--- MainMenu [Start] : no save load manager found in scene. aborting.");
            enabled = false;
        }
        padMgr = GameObject.FindFirstObjectByType<MultiGamepad>();
        if (padMgr == null)
        {
            Debug.LogError("--- MainMenu [Start] : no pad manager found in scene. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            // set profile active based on data
            ConfigureMenuButtonsEnabled();
            popupCurve = AnimationCurve.EaseInOut(0f,0f,1f,1f);
        }
    }

    void Update()
    {
        // run popup timer
        if ( popupTimer > 0f )
        {
            popupTimer -= Time.deltaTime;
            if (popupTimer < 0f)
            {
                popupTimer = 0f;
                popupEnabled = !popupEnabled;
                if (!popupEnabled)
                {
                    profilePopup = false;
                    for (int i = 0; i < buttons.Length; i++)
                    {
                        buttons[i].buttonVisible = true;
                    }
                }
            }
            else
            {
                if (popupEnabled)
                    popupProgress = 1f - popupCurve.Evaluate(popupTimer / POPUPTIME);
                else
                    popupProgress = popupCurve.Evaluate(popupTimer / POPUPTIME);
            }
            popupProgress = Mathf.Clamp01(popupProgress);
        }

        // run switch timer
        if ( sceneSwitchTimer > 0f )
        {
            sceneSwitchTimer -= Time.deltaTime;
            if ( sceneSwitchTimer < .1f )
                SceneManager.LoadScene(sceneSwitchName);
        }

        if ( profilePopup )
        {
            padMaxButton = 1;
            // determine ui selection from game pad input
            if (padMgr.gPadDown[0].XaxisL < 0f)
            {
                padButtonSelection--;
                if (padButtonSelection < 0)
                    padButtonSelection = padMaxButton;
            }
            else if (padMgr.gPadDown[0].XaxisL > 0f)
            {
                padButtonSelection++;
                if (padButtonSelection > padMaxButton)
                    padButtonSelection = 0;
            }
        }
        else
        {
            padMaxButton = 5;
            // determine ui selection from game pad input
            if (padMgr.gPadDown[0].YaxisL > 0f)
            {
                padButtonSelection--;
                if (padButtonSelection < 0)
                    padButtonSelection = padMaxButton;
            }
            else if (padMgr.gPadDown[0].YaxisL < 0f)
            {
                padButtonSelection++;
                if (padButtonSelection > padMaxButton)
                    padButtonSelection = 0;
            }
        }
    }

    void ConfigureMenuButtonsEnabled()
    {
        profileActive = saveMgr.IsProfileLoggedIn();
        if (profileActive)
            profileName = saveMgr.GetCurrentProfile().loginName;
        gameLoaded = saveMgr.IsGameCurrentlyLoaded();
        if (gameLoaded)
            gameName = saveMgr.GetCurrentGameData().gameName;
        buttons[0].buttonEnabled = (profileActive && gameLoaded);
        buttons[2].buttonEnabled = profileActive;
        buttons[3].buttonEnabled = profileActive;
    }

    void OnGUI()
    {
        if (sceneSwitchTimer > 0f)
            return;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;
        GUIStyle g = new GUIStyle(GUI.skin.label);
        g.font = titleFont;
        g.fontStyle = titleFontStyle;
        g.fontSize = Mathf.RoundToInt(titleFontSizeAt1024 * (w / 1024f));
        g.alignment = TextAnchor.MiddleCenter;
        g.normal.textColor = titleFontColor;
        g.hover.textColor = titleFontColor;
        g.active.textColor = titleFontColor;
        string s = titleText;

        r = title;
        r.x *= w;
        r.y *= h;
        r.width *= w;
        r.height *= h;

        GUI.Label(r, s, g);

        // PLAYER PROFILE CREATION / LOGIN
        if (profilePopup)
        {
            r = new Rect();
            r.x = 0.2f * w;
            r.y = (0.3f * h) + (popupProgress * 0.85f * h);
            r.width = 0.6f * w;
            r.height = 0.5f * h;
            g = new GUIStyle(GUI.skin.box);
            g.normal.textColor = buttonFontColor;
            g.hover.textColor = buttonFontColor;
            g.active.textColor = buttonFontColor;
            g.fontStyle = FontStyle.Bold;
            g.fontSize = Mathf.RoundToInt(24 * (w / 1024f));
            g.padding = new RectOffset(0, 0, 20, 0);
            s = "Player Profile";
            GUI.color = Color.white;
            GUI.Box(r, s, g);

            // Popup Entry Labels
            r.x += 0.05f * w;
            r.y += 0.1f * h;
            r.width = 0.25f * w;
            r.height = 0.05f * h;
            g = new GUIStyle(GUI.skin.label);
            g.normal.textColor = buttonFontColor;
            g.hover.textColor = buttonFontColor;
            g.active.textColor = buttonFontColor;
            g.alignment = TextAnchor.MiddleRight;
            g.padding = new RectOffset(0, 20, 0, 0);
            g.fontSize = Mathf.RoundToInt(20 * (w/1024f));
            g.fontStyle = FontStyle.Normal;
            s = "Player Profile Name";
            GUI.Label(r, s, g);
            r.y += 0.075f * h;
            s = "Profile Password";
            GUI.Label(r, s, g);
            if (!profileActive && popupConfirm)
            {
                r.y += 0.075f * h;
                s = "Confirm Password";
                GUI.Label(r, s, g);
            }

            // Popup Text Entry
            r.x += 0.25f * w;
            r.y -= 0.075f * h;
            if (popupConfirm)
                r.y -= 0.075f * h;
            g = new GUIStyle(GUI.skin.textField);
            g.normal.textColor = buttonFontColor;
            g.hover.textColor = buttonFontColor;
            g.active.textColor = buttonFontColor;
            g.alignment = TextAnchor.MiddleLeft;
            g.padding = new RectOffset(20, 0, 0, 0);
            g.fontSize = Mathf.RoundToInt(20 * (w / 1024f));
            if (profileActive)
                GUI.Label(r, popupName, g);
            else
                popupName = GUI.TextField(r, popupName, g);
            r.y += 0.075f * h;
            if (profileActive)
                GUI.enabled = false;
            popupPass = GUI.PasswordField(r, popupPass, char.Parse("*"), g);
            GUI.enabled = true;
            if (!profileActive && popupConfirm)
            {
                r.y += 0.075f * h;
                confirmPass = GUI.PasswordField(r, confirmPass, char.Parse("*"), g);
            }

            // Popup Feedback
            r.x = 0.25f * w;
            r.y += 0.135f * h;
            if (!profileActive && popupConfirm)
                r.y -= 0.075f * h;

            r.width = 0.5f * w;
            r.height = 0.05f * h;
            g = new GUIStyle(GUI.skin.label);
            Color c = Color.gray;
            g.normal.textColor = c;
            g.hover.textColor = c;
            g.active.textColor = c;
            g.alignment = TextAnchor.MiddleCenter;
            g.padding = new RectOffset(0, 20, 0, 0);
            g.fontSize = Mathf.RoundToInt(16 * (w / 1024f));
            g.fontStyle = FontStyle.BoldAndItalic;
            s = popupFeedback;
            GUI.Label(r, s, g);

            // Popup Buttons
            r.x = 0.25f * w;
            r.y += 0.0625f * h;
            // FIXME: minor formatting issue upon creation button click
            // REVIEW: use popup timer anim?
            r.width = 0.2f * w;
            r.height = 0.1f * h;
            g = new GUIStyle(GUI.skin.button);
            g.normal.textColor = buttonFontColor;
            g.active.textColor = buttonFontColor;
            // Create / Login / Logout
            if (padButtonSelection == 0)
                g.normal.textColor = Color.white;
            g.fontSize = Mathf.RoundToInt(20 * (w / 1024f));
            s = "LOGIN";
            if (profileActive)
                s = "LOGOUT";
            else if (popupConfirm && popupPass == confirmPass)
                s = "CREATE";
            if (GUI.Button(r, s, g) || (padMgr != null &&
                padMgr.gamepads[0].isActive && padButtonSelection == 0 &&
                padMgr.gPadDown[0].aButton))
            {
                if (popupConfirm)
                {
                    // confirming password
                    if (popupPass != confirmPass)
                    {
                        // mismatch confirm
                        popupFeedback = "password and confirmation do not match";
                    }
                    else if (ProfileSystem.ProfileExistsInRoster(saveMgr.GetRosterData(), popupName))
                    {
                        // existing user name
                        popupFeedback = "this user name already exists";
                    }
                    else
                    {
                        // create new
                        ProfileData newProfile = ProfileSystem.InitializeProfile(popupName, popupPass);
                        saveMgr.CreateNewRosterEntry(newProfile);
                        saveMgr.LoginProfile(newProfile);
                        ConfigureMenuButtonsEnabled();
                        popupTimer = POPUPTIME;
                        if (padMgr != null && padMgr.gamepads[0].isActive)
                        {
                            padButtonSelection = -1;
                            padMaxButton = 5;
                        }
                        popupFeedback = "new profile created and logged in";
                    }
                }
                else
                {
                    if (profileActive)
                    {
                        // logout active profile
                        saveMgr.LogoutProfile(saveMgr.GetCurrentProfile());
                        ConfigureMenuButtonsEnabled();
                        // NOTE: remain here, clear fields for login
                        popupName = "";
                        popupPass = "";
                        confirmPass = "";
                        popupConfirm = false;
                        popupFeedback = "profile logged OUT";
                    }
                    else if (ProfileSystem.ProfileExistsInRoster(saveMgr.GetRosterData(), popupName))
                    {
                        // profile name exists in roster
                        if (ProfileSystem.PasswordMatchToProfile(saveMgr.GetRosterData(), popupName, popupPass))
                        {
                            // login profile
                            saveMgr.LoginProfile(ProfileSystem.GetProfile(saveMgr.GetRosterData(), popupName, popupPass));
                            ConfigureMenuButtonsEnabled();
                            popupTimer = POPUPTIME;
                            if (padMgr != null && padMgr.gamepads[0].isActive)
                            {
                                padButtonSelection = -1;
                                padMaxButton = 5;
                            }
                            popupFeedback = "profile logged IN";
                        }
                        else
                        {
                            // password mismatch
                            popupFeedback = "password invalid";
                        }
                    }
                    else
                    {
                        // no user by that name in roster
                        popupConfirm = true;
                        popupFeedback = "new user confirm or [CANCEL]";
                    }
                }
            }

            // Cancel
            r.x += 0.3f * w;
            if (padButtonSelection == 1)
                g.normal.textColor = Color.white;
            g.fontSize = Mathf.RoundToInt(20 * (w / 1024f));
            s = "CANCEL";
            if (GUI.Button(r, s, g) || (padMgr != null && 
                padMgr.gamepads[0].isActive && padButtonSelection == 1 &&
                padMgr.gPadDown[0].aButton))
            {
                popupConfirm = false;
                popupFeedback = "";
                popupTimer = POPUPTIME;
                if (padMgr != null && padMgr.gamepads[0].isActive)
                {
                    padButtonSelection = -1;
                    padMaxButton = 5;
                }
            }

            return;
        }

        if (buttons == null || buttons.Length == 0)
            return;

        // buttons array
        g = new GUIStyle(GUI.skin.button);
        g.font = buttonFont;
        g.fontStyle = buttonFontStyle;

        for ( int i=0; i<buttons.Length; i++)
        {
            r = buttons[i].buttonPos;
            r.x *= w;
            r.y *= h;
            r.width *= w;
            r.height *= h;

            g.normal.textColor = buttonFontColor;
            if (padButtonSelection == i)
                g.normal.textColor = Color.white;
            g.active.textColor = buttonFontColor;
            g.fontSize = Mathf.RoundToInt(buttonFontSizeAt1024 * (w / 1024f));

            s = buttons[i].buttonText;
            if (i == PLAYERPROFILEBUTTON && profileActive)
            {
                s = "Profile : " + profileName;
                g.fontSize = Mathf.RoundToInt((buttonFontSizeAt1024 - 8f) * (w / 1024f));
            }
            if (i == GAMESELECTIONBUTTON && profileActive && gameLoaded)
            {
                s = "Game : " + gameName;
                g.fontSize = Mathf.RoundToInt((buttonFontSizeAt1024 - 8f) * (w / 1024f));
            }

            GUI.enabled = (buttons[i].buttonEnabled);

            if (buttons[i].buttonVisible && ( GUI.Button(r,s,g) ||
            GUI.enabled && padButtonSelection == i && padMgr.gPadDown[0].aButton) )
            {
                if (buttons[i].sceneName != "" && buttons[i].buttonAction == ButtonAction.SceneSwitch)
                {
                    if ((buttons[i].sceneName == "GreenerGame"))
                    {
                        // little cinematic menu fun
                        GameObject.FindAnyObjectByType<MenuLayerManager>().targetKey = 2;
                        sceneSwitchName = buttons[i].sceneName;
                        sceneSwitchTimer = 4f;
                    }
                    else
                        SceneManager.LoadScene(buttons[i].sceneName);
                }
                else if (buttons[i].buttonAction == ButtonAction.PopupLaunch)
                {
                    // profile popup
                    popupName = "";
                    popupPass = "";
                    confirmPass = "";
                    if (i == PLAYERPROFILEBUTTON && profileActive)
                        popupName = profileName;
                    for (int n = 0; n < buttons.Length; n++)
                    {
                        buttons[n].buttonVisible = false;
                    }
                    profilePopup = true;
                    popupTimer = POPUPTIME;
                    if (padMgr != null && padMgr.gamepads[0].isActive)
                    {
                        padButtonSelection = -1;
                        padMaxButton = 1;
                    }
                }
                else if (buttons[i].buttonAction == ButtonAction.Quit)
                    Application.Quit();
            }
        }

        GUI.enabled = true;
    }
}
