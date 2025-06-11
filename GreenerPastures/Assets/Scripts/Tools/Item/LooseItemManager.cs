using UnityEngine;

public class LooseItemManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles an item in the game world, as a loose item

    // NOTE: using Texture2D to emlpoy shaders and ease import process in art pipeline

    public LooseItemData looseItem;
    public float frameTime; // if animating, set positive seconds
    public bool animOnce; // do not repeat loop, if animating (returns to frame 0)
    public Texture2D[] frames = new Texture2D[1];

    private float frameTimer;
    private Renderer itemRenderer;
    private bool pulseActive;
    private float pulseTimer;

    const float MINIMUMFRAMETIME = 0.05f; // max animation fps is 20
    const float ITEMPULSEDURATION = 0.5f;


    void Start()
    {
        // validate
        itemRenderer = GetComponent<Renderer>();
        if (itemRenderer == null)
        {
            Debug.LogError("--- LooseItemManager [Start] : "+gameObject.name+" no renderer component found. aborting.");
            enabled = false;
        }
        if ( frames == null || frames.Length == 0 )
        {
            Debug.LogError("--- LooseItemManager [Start] : " + gameObject.name + " no art frames configured. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            itemRenderer.material.mainTexture = frames[0];
        }
    }

    void Update()
    {
        // validate item renderer
        if (itemRenderer == null)
        {
            Debug.LogError("--- LooseItemManager [Start] : " + gameObject.name + " no renderer component found. aborting.");
            enabled = false;
            return;
        }

        CheckItemPulse();

        // configure horizontal flip
        Vector2 flip = new Vector2(1f, 1f);
        if (looseItem.flipped)
            flip.x = -1f;
        itemRenderer.material.SetTextureScale("_MainTex", flip);
        // TEST: if we have multi-layer composite shader, flip all layers
        if (itemRenderer.material.shader.name == "Unlit/Two Layer Composite" ||
            itemRenderer.material.shader.name == "Unlit/Three Layer Composite")
            itemRenderer.material.SetTextureScale("_AltTex", flip);
        if (itemRenderer.material.shader.name == "Unlit/Three Layer Composite")
            itemRenderer.material.SetTextureScale("_AccentTex", flip);

        // run sprite timer
        if (frameTimer > 0f)
        {
            frameTimer -= Time.deltaTime;
            if (frameTimer < 0f)
            {
                frameTimer = frameTime;
                // add slight timing variation
                frameTimer += (Random.Range(-MINIMUMFRAMETIME/2f,MINIMUMFRAMETIME/2f));
                looseItem.artFrame++;
                if (looseItem.artFrame > frames.Length)
                    looseItem.artFrame = 0;
                if (animOnce)
                {
                    animOnce = false;
                    frameTimer = 0f;
                }
                // configure current sprite, based on sprite array
                itemRenderer.material.mainTexture = frames[ looseItem.artFrame ];
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
    /// Configures a new item object data and art, by item type
    /// </summary>
    /// <param name="itemType">item type</param>
    public void ConfigureItem( ItemType itemType )
    {
        looseItem = InventorySystem.CreateItem(itemType);
        // refer to art library to configure image list and anim properties
        ArtLibraryManager alm = GameObject.FindAnyObjectByType<ArtLibraryManager>();
        ArtData artData = alm.GetArtData(itemType);
        frames = alm.GetImageList(artData);
        itemRenderer.material.mainTexture = frames[0];
        frameTime = artData.animFrameTime;
        animOnce = !artData.animLoop;
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
    /// Sets the image on this item horizontal orientation
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
        SetItemFlip(!looseItem.flipped);
    }

    /// <summary>
    /// Begins animating this item, if animation frames configured
    /// </summary>
    public void AnimateItem()
    {
        looseItem.artFrame = 0;
        frameTimer = frameTime; // begin animating immediately
    }

    /// <summary>
    /// Stops animating this item
    /// </summary>
    public void StopAnimation()
    {
        frameTimer = 0f;
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
        if (itemRenderer == null)
            return;
        // base fill color
        Color c = itemRenderer.material.GetColor("_Color");
        c.r = value;
        c.g = value;
        c.b = 0f;
        c.a = (value/2f) + 0.5f;
        itemRenderer.material.SetColor("_Color", c);
    }
}
