using UnityEngine;

public class CastManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles individual spell charges that have been cast into the world

    public CastData[] casts = new CastData[0];

    private int singleCastToRemove; // will remove one per frame


    void Start()
    {
        // validate
        // initialize
        if (enabled)
        {
            singleCastToRemove = -1;
        }
    }

    void Update()
    {
        if (casts == null || casts.Length == 0)
            return;

        singleCastToRemove = -1;

        // run cast lifetimes
        for (int i = 0; i < casts.Length; i++)
        {
            casts[i].lifetime -= Time.deltaTime;
            if (casts[i].lifetime <= 0f)
            {
                casts[i].lifetime = 0f;
                if (singleCastToRemove == -1)
                    singleCastToRemove = i;
            }
        }

        // remove expired cast
        if (singleCastToRemove > -1)
            RemoveCast(singleCastToRemove);

        // handle cast effects in the game world
        for (int i = 0; i < casts.Length; i++)
        {
            HandleCastEffect(i);
        }
    }

    void RemoveCast( int index )
    {
        CastData[] tmp = new CastData[casts.Length-1];
        int count = 0;
        for ( int i = 0; i < casts.Length; i++ )
        {
            if (i != index)
            {
                tmp[count] = casts[i];
                count++;
            }
        }
        casts = tmp;
    }

    /// <summary>
    /// Acquires a new spell cast into the world, to be managed until expired
    /// </summary>
    /// <param name="cast">spell cast data</param>
    public void AcquireNewCast( CastData newCast )
    {
        CastData[] tmp = new CastData[casts.Length + 1];
        for (int i = 0; i < casts.Length; i++)
        {
            tmp[i] = casts[i];
        }
        tmp[casts.Length] = newCast;
        casts = tmp;
    }

    void HandleCastEffect( int index )
    {
        SpellType spellType = casts[index].type;
        Vector3 positionEffect = new Vector3(casts[index].posX, casts[index].posY, casts[index].posZ);
        float areaOfEffect = casts[index].rangeAOE;

        switch (spellType)
        {
            case SpellType.Default:
                // should never be here
                break;
            case SpellType.FastGrowI:
                break;
            case SpellType.SummonWaterI:
                break;
            case SpellType.BlessI:
                break;
            case SpellType.MalnutritionI:
                break;
            case SpellType.ProsperousI:
                break;
            case SpellType.LesionI:
                break;
            case SpellType.EclipseI:
                break;
            case SpellType.GoldenThumbI:
                break;
            default:
                Debug.LogWarning("--- CastManager [HandleCastEffect] : spell type effect not found for cast index "+index+". will ignore.");
                break;
        }
    }
}
