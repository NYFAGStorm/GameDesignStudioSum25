// REVIEW: necessary namespaces

// as we develop, continue to add statistics to keep regarding magic
[System.Serializable]
public class MagicStats
{
    public float totalMagicTime;
    public float totalManaEarned;
    public float totalManaSpent;
}

// spells can hold the details of a spell one might cast
// REVIEW:
public class SpellData
{
    public string spellName;
    public float prepareTime;
    public float castTime;
    public float duration;
}

// casts can hold the details of a single spell a player casts
// REVIEW:
public class CastData
{
    public SpellData spell;
    public float prepTime;
    public float castTime;
    public float lifeTime;
}

// magic is a single player's current magic, including casts in effect
[System.Serializable]
public class MagicData
{
    public MagicStats stats;
    public float mana;
    public CastData[] casts;
}
