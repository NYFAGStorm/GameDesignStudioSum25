using UnityEngine;

public class ShapeLibraryManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This holds data for item shapes, each as an array of booleans

    public struct ItemTypeShape
    {
        public bool[] pieces;
    }

    public ItemTypeShape[] itemShapes;


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
    /// Provide the full shape library for each item type
    /// </summary>
    /// <returns>an array of 3x3 booleans as item shapes</returns>
    public ItemTypeShape[] GetShapeLibrary()
    {
        return itemShapes;
    }
}
