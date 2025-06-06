// REVIEW: necessary namespaces

public static class FarmSystem
{
    const int TOTALFARMPLOTS = 25; // REVIEW: how big are farms?

    public static PlotData InitializePlot()
    {
        PlotData retPlot = new PlotData();

        retPlot.condition = PlotCondition.Wild;
        retPlot.plant = PlantSystem.InitializePlant();
        retPlot.plotEffects = new PlotEffect[0];

        return retPlot;
    }

    public static FarmData InitializeFarm()
    {
        FarmData retFarm = new FarmData();

        // initialize
        retFarm.plots = new PlotData[TOTALFARMPLOTS];
        for (int i=0; i<TOTALFARMPLOTS; i++)
        {
            retFarm.plots[i] = InitializePlot();
        }
        retFarm.farmEffects = new FarmEffects[0];

        return retFarm;
    }
}
