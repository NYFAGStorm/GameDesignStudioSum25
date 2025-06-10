using UnityEngine;

public class LooseItemManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles an item in the game world, as a loose item

    // REVIEW: if using Sprites or Texture2Ds, and SpriteRenderer or MeshRenderer
    // need Texture2Ds for shader work?

    public LooseItemData looseItem;
    public float spriteTime; // if animating, set positive seconds
    public bool animOnce; // do not repeat loop, if animating (returns to frame 0)
    public Sprite[] sprites = new Sprite[1];

    private float spriteTimer;
    private SpriteRenderer spriteRenderer;
    private bool pulseActive;
    private float pulseTimer;

    const float MINIMUMFRAMETIME = 0.05f; // max animation fps is 20
    const float ITEMPULSEDURATION = 0.5f;


    void Start()
    {
        // validate
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("--- LooseItemManager [Start] : "+gameObject.name+" no sprite renderer component found. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {

        }
    }

    void Update()
    {
        // validate sprite renderer
        if (spriteRenderer == null)
        {
            Debug.LogError("--- LooseItemManager [Start] : " + gameObject.name + " no sprite renderer component found. aborting.");
            enabled = false;
            return;
        }

        CheckItemPulse();

        // configure horizontal flip
        spriteRenderer.flipX = looseItem.flipped;

        // run sprite timer
        if (spriteTimer > 0f)
        {
            spriteTimer -= Time.deltaTime;
            if (spriteTimer < 0f)
            {
                spriteTimer = spriteTime;
                // add slight timing variation
                spriteTimer += (Random.Range(-MINIMUMFRAMETIME/2f,MINIMUMFRAMETIME/2f));
                looseItem.spriteFrame++;
                if (looseItem.spriteFrame > sprites.Length)
                    looseItem.spriteFrame = 0;
                if (animOnce)
                {
                    animOnce = false;
                    spriteTimer = 0f;
                }
                // configure current sprite, based on sprite array
                spriteRenderer.sprite = sprites[ looseItem.spriteFrame ];
            }
        }

        // detect durability failure or empty inventory, flag for deletion
        if (looseItem != null && (looseItem.inv.items.Length == 0 || 
            looseItem.inv.items[0].health <= 0f) )
        {
            looseItem.deleteMe = true;
        }

        // detect deletion flag
        if ( looseItem != null && looseItem.deleteMe )
        {
            // delete
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Configures a new item object data and sprites, by item type
    /// </summary>
    /// <param name="itemType">item type</param>
    public void ConfigureItem( ItemType itemType )
    {
        looseItem = InventorySystem.CreateItem(itemType);
        // refer to sprite library to configure sprite list and anim properties
        SpriteLibraryManager slm = GameObject.FindAnyObjectByType<SpriteLibraryManager>();
        SpriteData spriteData = slm.GetSpriteData(itemType);
        sprites = slm.GetSpriteList(spriteData);
        spriteRenderer.sprite = sprites[0];
        spriteTime = spriteData.animFrameTime;
        animOnce = !spriteData.animLoop;
    }

    /// <summary>
    /// Configures an item based on an item instance that existed in inventory (dropped)
    /// </summary>
    /// <param name="itemInstance">item data</param>
    public void ConfigureItem( ItemData itemInstance )
    {
        ConfigureItem(itemInstance.type);
        // use item instance
        looseItem.inv.items[0] = itemInstance;
    }

    /// <summary>
    /// Sets the sprite on this item horizontal orientation
    /// </summary>
    /// <param name="flipped">flipped horizontally from default</param>
    public void SetItemFlip( bool flipped )
    {
        looseItem.flipped = flipped;
    }

    /// <summary>
    /// Toggles this item sprite horizontal flip
    /// </summary>
    public void ToggleItemFlip()
    {
        looseItem.flipped = !looseItem.flipped;
    }

    /// <summary>
    /// Begins animating this item, if animation frames configured
    /// </summary>
    public void AnimateItem()
    {
        looseItem.spriteFrame = 0;
        spriteTimer = spriteTime; // begin animating immediately
    }

    /// <summary>
    /// Stops animating this item
    /// </summary>
    public void StopAnimation()
    {
        spriteTimer = 0f;
    }

    /// <summary>
    /// Gets the health / durability of this item
    /// </summary>
    /// <returns>health percentage (0-1)</returns>
    public float GetItemHealth()
    {
        return looseItem.inv.items[0].health;
    }

    /// <summary>
    /// Sets the health / durability of this item
    /// </summary>
    /// <param name="value">health percentage (0-1)</param>
    public void SetItemHealth( float value )
    {
        looseItem.inv.items[0].health = value;
    }

    /// <summary>
    /// Adjusts the health / durability of this item
    /// </summary>
    /// <param name="value">health percentage (0-1), to be added, result clamped</param>
    public void AdjustItemHealth( float value )
    {
        looseItem.inv.items[0].health = Mathf.Clamp01(looseItem.inv.items[0].health + value);
    }

    void CheckItemPulse()
    {
        if (!pulseActive)
            return;
        pulseTimer += Time.deltaTime;
        float pulse = 1f - ((pulseTimer % ITEMPULSEDURATION) * 0.9f);
        SetItemHighlight(pulse);
    }

    /// <summary>
    /// Sets the item pulse
    /// </summary>
    /// <param name="active">pulse if true</param>
    public void SetItemPulse(bool active)
    {
        pulseActive = active;
        SetItemHighlight(1f);
        pulseTimer = 0f;
    }

    void SetItemHighlight(float value)
    {
        if (spriteRenderer == null)
            return;
        // REVIEW: need another way to highlight, for any color sprite
        Color c = spriteRenderer.material.color;
        c.r = 1f;
        c.g = value;
        c.b = 1f;
        c.a = 1f;
        spriteRenderer.material.color = c;
    }
}
