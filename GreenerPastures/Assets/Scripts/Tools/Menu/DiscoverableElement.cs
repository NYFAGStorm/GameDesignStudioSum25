using UnityEngine;

public class DiscoverableElement : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles a static screen clickable element

    // REVIEW: how gamepad can discover element without point and click?

    public enum RevealTransition
    {
        Default,
        SildeOff,
        FadeOut,
        ScaleDown,
        ScaleUpFade
    }

    public enum RewardType
    {
        Default,
        Gold,
        XP,
        Arcana,
        Item
    }

    public Texture2D elementTexture;
    [Tooltip("The viewport space this element exists. (percentage of screen space")]
    public Rect elementSpace;
    public RevealTransition revealMode;
    public float revealTime = 0.618f;
    [Tooltip("This animation curve describes the transition timing between start and end. (if not defined, will be linear)")]
    public AnimationCurve revealAnimation;
    [Tooltip("The viewport space this element will move toward during reveal. (percentage of screen space)")]
    public Vector2 elementTarget;
    public RewardType reward;
    public int rewardAmount;
    [Tooltip("If reward type is 'Item', define the item type here.")]
    public ItemType rewardItemType;

    private bool elementDiscovered;
    private float revealTimer;
    private float revealProgress;
    // TODO: set as client player once logged in
    // REVIEW: hold onto rewards and deliver to next logged in player?
    // NOTE: consider each of these properties in this player data element an addition
    private PlayerData playerData = new PlayerData();


    void Start()
    {
        // validate
        if (elementTexture == null)
        {
            Debug.LogError("--- DiscoverableElement [Start] : " + gameObject.name + " no element texture defined. aborting.");
            enabled = false;
        }
        if (elementSpace == Rect.zero)
        {
            Debug.LogError("--- DiscoverableElement [Start] : " + gameObject.name + " no element space defined. aborting.");
            enabled = false;
        }
        if (revealMode == RevealTransition.Default)
        {
            Debug.LogError("--- DiscoverableElement [Start] : " + gameObject.name + " no reveal mode defined. aborting.");
            enabled = false;
        }
        if (revealTime <= 0f)
        {
            Debug.LogError("--- DiscoverableElement [Start] : " + gameObject.name + " invalid reveal time. aborting.");
            enabled = false;
        }
        if (elementTarget == Vector2.zero)
        {
            Debug.LogError("--- DiscoverableElement [Start] : " + gameObject.name + " no element target defined. aborting.");
            enabled = false;
        }
        if (reward == RewardType.Default)
        {
            Debug.LogError("--- DiscoverableElement [Start] : " + gameObject.name + " no reward defined. aborting.");
            enabled = false;
        }
        if (rewardAmount <= 0)
        {
            Debug.LogError("--- DiscoverableElement [Start] : " + gameObject.name + " no reward amount defined. aborting.");
            enabled = false;
        }
        if (reward == RewardType.Item && rewardItemType == ItemType.Default)
        {
            Debug.LogError("--- DiscoverableElement [Start] : " + gameObject.name + " reward type 'Item' defined but no reward item type defined. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            if (revealAnimation.keys == null || revealAnimation.keys.Length == 0)
                revealAnimation = AnimationCurve.Linear(0f,0f,1f,1f);
        }
    }

    void Update()
    {
        if (!elementDiscovered)
            return;

        // run reveal timer
        if (revealTimer > 0f)
        {
            revealTimer -= Time.deltaTime;
            if (revealTimer < 0f)
                revealTimer = 0f;
            revealProgress = Mathf.Clamp01(1f - (revealTimer/revealTime));

            revealProgress = revealAnimation.Evaluate(revealProgress);

            if (revealProgress == 1f)
                ProvideReward();
        }
    }

    void RevealElement()
    {
        if (elementDiscovered)
            return;
        revealTimer = revealTime;
        elementDiscovered = true;
    }

    void ProvideReward()
    {
        switch (reward)
        {
            case RewardType.Default:
                // we should never be here
                break;
            case RewardType.Gold:
                playerData.gold += rewardAmount;
                break;
            case RewardType.XP:
                playerData.xp += rewardAmount;
                // PlayerControlManager.AwardXP( PlayerData.XP_FINDCLICKABLE );
                break;
            case RewardType.Arcana:
                playerData.arcana += rewardAmount;
                break;
            case RewardType.Item:
                // TODO: provide item as either inventory (if open slot) or loose item
                break;
            default:
                Debug.LogWarning("--- DiscoverableElement [ProvideReward] : " + gameObject.name + " reward type undefined. will ignore.");
                break;
        }
    }

    void OnGUI()
    {
        if (elementDiscovered && revealProgress == 1f)
            return;

        Rect r = elementSpace;
        float w = Screen.width;
        float h = Screen.height;

        GUIStyle g = new GUIStyle();
        g.normal.background = null;
        g.hover.background = null;
        g.active.background = null;
        Color c = Color.white;
        Texture2D t = elementTexture;

        if (!elementDiscovered)
        {
            r.x *= w;
            r.y *= h;
            r.width *= w;
            r.height *= h;

            GUI.DrawTexture(r, t);
            // REVIEW: clickable area smaller than image space?
            if (GUI.Button(r, "", g))
                RevealElement();
            return;
        }

        // adjust for lerp to target
        r.x = Mathf.Lerp(elementSpace.x, elementTarget.x, revealProgress);
        r.y = Mathf.Lerp(elementSpace.y, elementTarget.y, revealProgress);

        // REVIEW: consider calculating aspect ratio
        Vector2 elementCenter = Vector2.zero;
        elementCenter.x = elementSpace.x + (elementSpace.width * 0.5f);
        elementCenter.y = elementSpace.y + (elementSpace.height * 0.5f);

        switch (revealMode)
        {
            case RevealTransition.Default:
                // we should never be here
                break;
            case RevealTransition.SildeOff:
                break;
            case RevealTransition.FadeOut:
                c.a = 1f - revealProgress;
                break;
            case RevealTransition.ScaleDown:
                r.x += (elementCenter.x - elementSpace.x) * revealProgress;
                r.y += (elementCenter.y - elementSpace.y) * revealProgress;
                r.width -= revealProgress * elementSpace.width;
                r.height -= revealProgress * elementSpace.height;
                break;
            case RevealTransition.ScaleUpFade:
                r.x -= (elementCenter.x - elementSpace.x) * .381f * revealProgress;
                r.y -= (elementCenter.y - elementSpace.y) * .381f * revealProgress;
                r.width += revealProgress * 0.381f * elementSpace.width;
                r.height += revealProgress * 0.381f * elementSpace.height;
                c.a = 1f - revealProgress;
                break;
            default:
                break;
        }

        GUI.color = c;

        r.x *= w;
        r.y *= h;
        r.width *= w;
        r.height *= h;

        GUI.DrawTexture(r, t);
    }
}
