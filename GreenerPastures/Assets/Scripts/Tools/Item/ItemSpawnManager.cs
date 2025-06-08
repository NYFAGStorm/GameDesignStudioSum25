using System.Runtime.InteropServices;
using UnityEngine;

public class ItemSpawnManager : MonoBehaviour
{
    // Author: Glenn Storm
    // Handles loose item spawning for dropped items

    public AnimationCurve dropCurve;

    private GameObject dropItem;
    private Vector3 spawnPoint;
    private Vector3 dropTarget;
    private float dropTimer;

    const float DROPTIME = 1f;
    const float VERTICALORIGIN = 0.125f;


    void Start()
    {
        // validate
        if (dropCurve == null || dropCurve.length < 2)
        {
            Debug.LogError("--- ItemSpawnManager [Start] : drop curve undefined. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {

        }
    }

    void Update()
    {
        // run drop timer
        if ( dropTimer > 0f )
        {
            dropTimer -= Time.deltaTime;
            if ( dropTimer < 0f )
            {
                dropTimer = 0f;
                Vector3 land = dropTarget + (Vector3.up * VERTICALORIGIN);
                dropItem.transform.position = land;
                dropItem = null;
                spawnPoint = Vector3.zero;
                dropTarget = Vector3.zero;
                return;
            }
            if (dropItem == null)
            {
                Debug.LogWarning("--- ItemSpawnManager [Update] : drop item lost. will ignore.");
                dropTimer = 0f;
                return;
            }
            else
            {
                Vector3 pos = Vector3.Lerp(spawnPoint,dropTarget, 1f-(dropTimer/DROPTIME));
                pos.y = dropCurve.Evaluate(1f - (dropTimer / DROPTIME)) + VERTICALORIGIN;
                dropItem.transform.position = pos;
            }
        }
    }

    /// <summary>
    /// Spawns a loose item to live in the game world
    /// </summary>
    /// <param name="item">loose item data</param>
    /// <param name="location">position in world to put item</param>
    public void SpawnItem( LooseItemData item, Vector3 spawnLocation, Vector3 dropLocation )
    {
        GameObject looseItem = new GameObject();
        looseItem.transform.position = spawnLocation + (Vector3.up * VERTICALORIGIN);
        looseItem.name = "Item "+item.inv.items[0].type.ToString();
        LooseItemManager lim = looseItem.AddComponent<LooseItemManager>();
        lim.looseItem = item;
        lim.looseItem.flipped = (dropLocation.x < spawnLocation.x);

        lim.sprites = new Sprite[1];
        SpriteData d = GameObject.FindAnyObjectByType<SpriteLibraryManager>().GetSpriteData(item.inv.items[0].type);
        lim.sprites[0] = GameObject.FindAnyObjectByType<SpriteLibraryManager>().itemSprites[d.spriteIndexBase];
        SpriteRenderer sr = looseItem.AddComponent<SpriteRenderer>();
        sr.sprite = lim.sprites[0];
        sr.flipX = lim.looseItem.flipped;

        // hand drop animation
        dropItem = looseItem.gameObject;
        spawnPoint = spawnLocation;
        dropTarget = dropLocation;
        dropTimer = DROPTIME;
    }
}
