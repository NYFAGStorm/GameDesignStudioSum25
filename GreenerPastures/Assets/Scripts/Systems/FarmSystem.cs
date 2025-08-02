// REVIEW: necessary namespaces

public static class FarmSystem
{
    const int TOTALFARMPLOTS = 25; // REVIEW: how big are farms?

    /// <summary>
    /// Creates new plot data properties and an array of plot effects
    /// </summary>
    /// <returns>initialized plot data</returns>
    public static PlotData InitializePlot()
    {
        PlotData retPlot = new PlotData();

        retPlot.condition = PlotCondition.Wild;
        retPlot.soil = 0.5f;
        retPlot.plant = PlantSystem.InitializePlant( PlantType.Default );
        retPlot.plotEffects = new PlotEffect[0];

        return retPlot;
    }

    /// <summary>
    /// Creates new farm data with an array of plots and an array of farm effects
    /// </summary>
    /// <returns>initialized farm data</returns>
    public static FarmData InitializeFarm()
    {
        FarmData retFarm = new FarmData();

        // initialize
        retFarm.plots = new PlotData[TOTALFARMPLOTS];
        for (int i=0; i<TOTALFARMPLOTS; i++)
        {
            retFarm.plots[i] = InitializePlot();
        }
        retFarm.farmEffects = new FarmEffect[0];

        return retFarm;
    }

    /// <summary>
    /// Adds an instance of a plot effect on given plot data
    /// </summary>
    /// <param name="plot">plot data</param>
    /// <param name="effect">plot effect type</param>
    /// <returns>plot data with plot effect added</returns>
    public static PlotData AddPlotEffect( PlotData plot, PlotEffect effect )
    {
        PlotData retPlot = plot;

        // validate does not exist
        bool found = false;
        for (int i = 0; i < retPlot.plotEffects.Length; i++)
        {
            if (retPlot.plotEffects[i] == effect)
            {
                found = true;
                break;
            }
        }
        if (found)
            return retPlot;
        // add
        PlotEffect[] tmp = new PlotEffect[retPlot.plotEffects.Length+1];
        for ( int i = 0; i < retPlot.plotEffects.Length; i++ )
        {
            tmp[i] = retPlot.plotEffects[i];
        }
        tmp[retPlot.plotEffects.Length] = effect;
        retPlot.plotEffects = tmp;

        return retPlot;
    }

    /// <summary>
    /// Removes the first found instance of a plot effect type on given plot data
    /// </summary>
    /// <param name="plot">plot data</param>
    /// <param name="effect">plot effect type</param>
    /// <returns>plot data with plot effect removed, if found</returns>
    public static PlotData RemovePlotEffect(PlotData plot, PlotEffect effect)
    {
        PlotData retPlot = plot;

        // validate exists
        bool found = false;
        for (int i = 0; i < retPlot.plotEffects.Length; i++)
        {
            if (retPlot.plotEffects[i] == effect)
            {
                found = true;
                break;
            }
        }
        if (!found)
            return retPlot;
        // remove
        PlotEffect[] tmp = new PlotEffect[retPlot.plotEffects.Length - 1];
        int count = 0;
        for (int i = 0; i < retPlot.plotEffects.Length; i++)
        {
            if (retPlot.plotEffects[i] != effect)
            {
                tmp[count] = retPlot.plotEffects[i];
                count++;
            }
        }
        retPlot.plotEffects = tmp;

        return retPlot;
    }

    /// <summary>
    /// Removes all plot effects from given plot data
    /// </summary>
    /// <param name="plot">plot data</param>
    /// <returns>plot data with no plot effects</returns>
    public static PlotData ClearAllPlotEffects(PlotData plot)
    {
        PlotData retPlot = plot;

        plot.plotEffects = new PlotEffect[0];

        return retPlot;
    }

    /// <summary>
    /// Returns true if given plot data includes a given effect
    /// </summary>
    /// <param name="plot">plot data</param>
    /// <param name="effect">plot effect type</param>
    /// <returns>true if plot effect exists, false if not</returns>
    public static bool PlotHasEffect(PlotData plot, PlotEffect effect)
    {
        bool retBool = false;

        for (int i = 0; i < plot.plotEffects.Length; i++)
        {
            if (plot.plotEffects[i] == effect)
            {
                retBool = true;
                break;
            }
        }

        return retBool;
    }
}
