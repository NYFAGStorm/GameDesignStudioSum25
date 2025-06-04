using UnityEngine;

public static class RandomSystem
{
    /// <summary>
    /// Provides a random result between 0 and 1, with a flat distribution
    /// </summary>
    /// <returns>result float</returns>
    public static float FlatRandom01()
    {
        return Random.Range(0f, 1f);
    }
    
    /// <summary>
    /// Provides a random result between 0 and 1, with a weighted distribution (weighted lower)
    /// </summary>
    /// <returns>result float</returns>
    public static float WeightedRandom01()
    {
        float max = Random.Range(0f, 1f);
        return Random.Range(0f, max);
    }

    /// <summary>
    /// Provides a random result between 0 and 1, with a gaussian distribution (weighted to the middle, like a bell curve)
    /// </summary>
    /// <returns>result float</returns>
    public static float GaussianRandom01()
    {
        float min = Random.Range(0f, 1f);
        float max = Random.Range(0f, 1f);
        if ( min > max )
        {
            float tmp = min;
            min = max;
            max = tmp;
        }
        return Random.Range(min, max);
    }
}
