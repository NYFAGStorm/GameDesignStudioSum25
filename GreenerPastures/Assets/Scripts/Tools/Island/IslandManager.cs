using UnityEngine;

public class IslandManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles all floating islands and the structures on them

    public IslandData[] islands;


    void Start()
    {
        // validate
        // initialize
        if (enabled)
        {

        }
    }

    void Update()
    {
        
    }

    /// <summary>
    /// Returns the island data array
    /// </summary>
    /// <returns>island data array</returns>
    public IslandData[] GetIslandData()
    {
        return islands;
    }

    /// <summary>
    /// Sets the island data array
    /// </summary>
    /// <param name="islandData">island data array</param>
    public void SetIslandData( IslandData[] islandData )
    {
        islands = islandData;
        ConfigureIslands();
    }

    void ConfigureIslands()
    {
        // TODO:
    }
}
