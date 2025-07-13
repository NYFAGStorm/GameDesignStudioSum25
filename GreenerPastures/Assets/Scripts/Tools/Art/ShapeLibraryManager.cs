using UnityEngine;

public class ShapeLibraryManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This holds data for ingredient shapes, each as an array of booleans

    [System.Serializable]
    public struct IngredientShapeType
    {
        public ItemType item;
        public PlantType plant;
        public bool[] pieces;
    }
    [Tooltip("Each entry represents the shape of one ingredient. The nine booleans represent a 3x3 grid; 0-2 on top row, 3-5 middle row, 6-8 bottom row. True means this shape includes this square.")]
    public IngredientShapeType[] ingredientShapes;


    void Start()
    {
        // validate
        // TODO: validate only one shape per combination of item and plant
        // initialize
        if (enabled)
        {
            if ( ingredientShapes == null || ingredientShapes.Length == 0 )
            {
                // REVIEW:
                // temp - create a shape library entry for every
                // default item type and every plant type
                int numOfTypes = System.Enum.GetNames(typeof(ItemType)).Length +
                    System.Enum.GetNames(typeof(PlantType)).Length;
                ingredientShapes = new IngredientShapeType[numOfTypes];
                for (int i = 0; i < numOfTypes; i++)
                {
                    int itemCount = System.Enum.GetNames(typeof(ItemType)).Length;
                    if (i < itemCount)
                        ingredientShapes[i].item = ((ItemType)i);
                    else
                        ingredientShapes[i].plant = ((PlantType)i-itemCount);
                    ingredientShapes[i].pieces = new bool[9];
                    ingredientShapes[i].pieces[4] = true; // center square on
                }
            }
        }
    }

    void Update()
    {
        
    }

    /// <summary>
    /// Provide the full shape library for each ungredient type
    /// </summary>
    /// <returns>an array of 3x3 booleans as ingredient shapes</returns>
    public IngredientShapeType[] GetShapeLibrary()
    {
        return ingredientShapes;
    }

    /// <summary>
    /// Returns the 3x3 grid shape of an ingredient with given item type and plant type
    /// </summary>
    /// <param name="iType">item type</param>
    /// <param name="pType">plant type</param>
    /// <returns>3x3 grid represented as an array of 9 booleans, where true means that piece is part of the shape</returns>
    public bool[] GetIngredientShape( ItemType iType, PlantType pType )
    {
        bool[] retShape = new bool[9];

        bool found = false;

        for (int i = 0; i < ingredientShapes.Length; i++)
        {
            if (ingredientShapes[i].item == iType && ingredientShapes[i].plant == pType)
            {
                retShape = ingredientShapes[i].pieces;
                found = true;
                break;
            }
        }

        if (!found)
        {
            Debug.LogWarning("--- ShapeLibraryManager [GetIngredientShape] : no shape found for item '' and plant ''. will use default seed shape.");
            retShape[4] = true;
        }

        return retShape;
    }
}
