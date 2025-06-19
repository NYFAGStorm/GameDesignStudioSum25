using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles creating, reading and writing save data file


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

    // TODO: auto-load on enable
    // TODO: auto-save on destroy

    // TODO: validate save data
    // TODO: separate version file
    // TODO: handle mismatch version data

    // TODO: once data loaded, find all timestamp data and handle tracked timer data
    //  (that is, subtract global time progress and _likely_ zero out timer values
    // use TimeManager.GetTimestampDifference( long ) to set float timer values
    //  > Magic
    //    spell book cooldowns
    //    cast lifetimes
    // ...

    void LoadGameData()
    {

    }

    void SaveGameData()
    {

    }
}
