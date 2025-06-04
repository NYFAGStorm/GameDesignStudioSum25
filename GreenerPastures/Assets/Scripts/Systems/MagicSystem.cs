// REVIEW: necessary namespaces

public static class MagicSystem
{
    const float STARTINGMANA = 100.0f; // REVIEW: what is the starting mana?

    public static MagicData IntializeMagic()
    {
        MagicData retMagic = new MagicData();

        // initialize
        retMagic.stats = new MagicStats();
        retMagic.mana = STARTINGMANA;
        retMagic.casts = new CastData[0];

        return retMagic;
    }
}
