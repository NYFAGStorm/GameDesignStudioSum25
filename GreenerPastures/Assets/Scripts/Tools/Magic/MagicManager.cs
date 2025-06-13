using UnityEngine;

public class MagicManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles a player's use of their spell book and grimoire

    private PlayerControlManager pcm;
    private CastManager castMgr;


    void Start()
    {
        // validate
        pcm = GameObject.FindFirstObjectByType<PlayerControlManager>();
        if (pcm == null)
        {
            Debug.LogError("--- MagicManager [Start] : no player control manager found on this game object. aborting.");
            enabled = false;
        }
        castMgr = GameObject.FindFirstObjectByType<CastManager>();
        if (castMgr == null)
        {
            Debug.LogError("--- MagicManager [Start] : no cast manager found in scene. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            
        }
    }

    void Update()
    {
        
    }

    /// <summary>
    /// Casts a spell charge into the world given spell type and world position
    /// </summary>
    /// <param name="spell">spell type</param>
    /// <param name="pos">position in the game world to center the cast</param>
    /// <returns>true if successful, false if no charge or no spell book entry or invalid spell book entry</returns>
    public bool CastSpell( SpellType spell, Vector3 pos )
    {
        bool retBool = false;

        if (MagicSystem.SpellBookHasCharge(spell, pcm.playerData.magic.library))
        {
            if (MagicSystem.CastSpellFromBook(spell, pcm.playerData.magic.library, out pcm.playerData.magic.library))
            {
                SpellBookData spellData = MagicSystem.GetSpellBookEntry(spell, pcm.playerData.magic.library);
                if (spellData != null)
                {
                    castMgr.AcquireNewCast(MagicSystem.InitializeCast(spellData, pos));
                    retBool = true;
                }
                else
                    Debug.LogWarning("--- MagicManager [CastSpell] : unable to get spell book entry data for spell type "+spell.ToString()+". will ignore.");
            }
        }       

        return retBool;
    }
}
