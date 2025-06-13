// REVIEW: necessary namespaces

public static class MagicSystem
{
    public static MagicData IntializeMagic()
    {
        MagicData retMagic = new MagicData();

        // initialize
        retMagic.stats = new MagicStats();
        retMagic.library = new SpellLibrary();
        retMagic.library.grimiore = new GrimioreData[0];
        retMagic.library.spellBook = new SpellBookData[0];
        retMagic.casts = new CastData[0];

        return retMagic;
    }
}
