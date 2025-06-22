using UnityEngine;

public class ShapeLibraryManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This holds data for item shapes, each as an array of booleans

    [System.Serializable]
    public struct ItemTypeShape
    {
        public string item;
        public bool[] pieces;
    }
    [Tooltip("Each entry represents the shape of one item. The nine booleans represent a 3x3 grid; 0-2 on top row, 3-5 middle row, 6-8 bottom row. True means this shape includes this square.")]
    public ItemTypeShape[] itemShapes;


    void Start()
    {
        // validate
        // initialize
        if (enabled)
        {
            if ( itemShapes == null || itemShapes.Length == 0 )
            {
                // temp - create a shape library entry for every
                // default item type and every plant type
                int numOfTypes = System.Enum.GetNames(typeof(ItemType)).Length +
                    System.Enum.GetNames(typeof(PlantType)).Length;
                itemShapes = new ItemTypeShape[numOfTypes];
                for (int i = 0; i < numOfTypes; i++)
                {
                    int itemCount = System.Enum.GetNames(typeof(ItemType)).Length;
                    if (i < itemCount)
                        itemShapes[i].item = ((ItemType)i).ToString();
                    else
                        itemShapes[i].item = ((PlantType)i-itemCount).ToString();
                    itemShapes[i].pieces = new bool[9];
                    itemShapes[i].pieces[4] = true; // center square on
                }
            }
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
