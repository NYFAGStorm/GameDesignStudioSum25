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

    const int TOTALINGREDIENTSHAPETYPES = 170;


    void Start()
    {
        // validate
        // initialize
        if (enabled)
        {
            InitializeShapeLibrary();
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

    void InitializeShapeLibrary()
    {
        ingredientShapes = new ShapeLibraryManager.IngredientShapeType[TOTALINGREDIENTSHAPETYPES + 2];

        // DEFAULT LEGACY SHAPE LIBRARY
        // FIXME: only need first 6 for default plant types
        for (int i = 0; i < ingredientShapes.Length; i++)
        {
            ingredientShapes[i].pieces = new bool[9];
            if (i < 6)
            {
                switch ((ItemType)i)
                {
                    case ItemType.Default:
                        ingredientShapes[i].item = ItemType.Default;
                        ingredientShapes[i].plant = PlantType.Default;
                        ingredientShapes[i].pieces[4] = true;
                        break;
                    case ItemType.Fertilizer:
                        ingredientShapes[i].item = ItemType.Fertilizer;
                        ingredientShapes[i].plant = PlantType.Default;
                        ingredientShapes[i].pieces[4] = true;
                        //shapeLibrary[i].pieces[6] = true;
                        //shapeLibrary[i].pieces[7] = true;
                        //shapeLibrary[i].pieces[8] = true;
                        break;
                    case ItemType.Seed:
                        ingredientShapes[i].item = ItemType.Seed;
                        ingredientShapes[i].plant = PlantType.Default;
                        ingredientShapes[i].pieces[4] = true;
                        break;
                    case ItemType.Plant:
                        ingredientShapes[i].item = ItemType.Plant;
                        ingredientShapes[i].plant = PlantType.Default;
                        //shapeLibrary[i].pieces[0] = true;
                        //shapeLibrary[i].pieces[1] = true;
                        //shapeLibrary[i].pieces[2] = true;
                        ingredientShapes[i].pieces[4] = true;
                        //shapeLibrary[i].pieces[7] = true;
                        break;
                    case ItemType.Stalk:
                        ingredientShapes[i].item = ItemType.Stalk;
                        ingredientShapes[i].plant = PlantType.Default;
                        ingredientShapes[i].pieces[4] = true;
                        //shapeLibrary[i].pieces[7] = true;
                        break;
                    case ItemType.Fruit:
                        ingredientShapes[i].item = ItemType.Fruit;
                        ingredientShapes[i].plant = PlantType.Default;
                        //shapeLibrary[i].pieces[1] = true;
                        //shapeLibrary[i].pieces[3] = true;
                        ingredientShapes[i].pieces[4] = true;
                        //shapeLibrary[i].pieces[5] = true;
                        //shapeLibrary[i].pieces[7] = true;
                        break;
                    case ItemType.Rock:
                        ingredientShapes[i].item = ItemType.Rock;
                        ingredientShapes[i].plant = PlantType.Default;
                        //shapeLibrary[i].pieces[0] = true;
                        //shapeLibrary[i].pieces[1] = true;
                        //shapeLibrary[i].pieces[3] = true;
                        ingredientShapes[i].pieces[4] = true;
                        break;
                    default:
                        ingredientShapes[i].item = ItemType.Default;
                        ingredientShapes[i].plant = PlantType.Default;
                        ingredientShapes[i].pieces[4] = true;
                        break;
                }
            }
        }
        // 
        for (int i = 0; i < 40; i++)
        {
            int idx = 6 + (i * 4);
            PlantType pt = (PlantType)i;
            ingredientShapes[idx].item = ItemType.Seed;
            ingredientShapes[idx].plant = pt;
            ingredientShapes[idx].pieces[4] = true;

            ingredientShapes[idx + 1].item = ItemType.Stalk;
            ingredientShapes[idx + 1].plant = pt;
            ingredientShapes[idx + 1].pieces = GetCustomShape(ItemType.Stalk, pt);
            ingredientShapes[idx + 2].item = ItemType.Fruit;
            ingredientShapes[idx + 2].plant = pt;
            ingredientShapes[idx + 2].pieces = GetCustomShape(ItemType.Fruit, pt);
            ingredientShapes[idx + 3].item = ItemType.Plant;
            ingredientShapes[idx + 3].plant = pt;
            ingredientShapes[idx + 3].pieces = GetCustomShape(ItemType.Plant, pt);
        }
    }

    bool[] GetCustomShape( ItemType iType, PlantType pType )
    {
        bool[] retBool = new bool[9];
        retBool[4] = true;
        // seed shape by default, otherwise provided here

        // COMMON
        if (pType == PlantType.Corn)
        {
            switch (iType)
            {
                case ItemType.Fruit:
                    retBool[1] = true;
                    retBool[4] = true;
                    break;
                case ItemType.Stalk:
                    retBool[0] = true;
                    retBool[3] = true;
                    retBool[4] = true;
                    break;
            }
        }
        if (pType == PlantType.Tomato)
        {
            switch (iType)
            {
                case ItemType.Fruit:
                    retBool[0] = true;
                    retBool[1] = true;
                    retBool[3] = true;
                    retBool[4] = true;
                    break;
                case ItemType.Stalk:
                    retBool[1] = true;
                    retBool[3] = true;
                    retBool[4] = true;
                    break;
            }
        }
        if (pType == PlantType.Carrot)
        {
            switch (iType)
            {
                case ItemType.Fruit:
                    retBool[3] = true;
                    retBool[4] = true;
                    break;
                case ItemType.Stalk:
                    retBool[4] = true;
                    break;
            }
        }
        if (pType == PlantType.Poppy)
        {
            switch (iType)
            {
                case ItemType.Fruit:
                    retBool[0] = true;
                    retBool[1] = true;
                    retBool[4] = true;
                    break;
                case ItemType.Stalk:
                    retBool[4] = true;
                    break;
            }
        }
        if (pType == PlantType.Rose)
        {
            switch (iType)
            {
                case ItemType.Fruit:
                    retBool[4] = true;
                    break;
                case ItemType.Stalk:
                    retBool[1] = true;
                    retBool[4] = true;
                    break;
            }
        }
        if (pType == PlantType.Sunflower)
        {
            switch (iType)
            {
                case ItemType.Fruit:
                    retBool[0] = true;
                    retBool[1] = true;
                    retBool[3] = true;
                    retBool[4] = true;
                    break;
                case ItemType.Stalk:
                    retBool[1] = true;
                    retBool[4] = true;
                    break;
            }
        }
        if (pType == PlantType.Moonflower)
        {
            switch (iType)
            {
                case ItemType.Fruit:
                    retBool[0] = true;
                    retBool[3] = true;
                    retBool[4] = true;
                    break;
                case ItemType.Stalk:
                    retBool[3] = true;
                    retBool[4] = true;
                    break;
            }
        }
        if (pType == PlantType.Apple)
        {
            switch (iType)
            {
                case ItemType.Fruit:
                    retBool[0] = true;
                    retBool[1] = true;
                    retBool[3] = true;
                    retBool[4] = true;
                    break;
                case ItemType.Stalk:
                    retBool[3] = true;
                    retBool[4] = true;
                    retBool[6] = true;
                    break;
            }
        }
        if (pType == PlantType.Orange)
        {
            switch (iType)
            {
                case ItemType.Fruit:
                    retBool[0] = true;
                    retBool[3] = true;
                    retBool[4] = true;
                    break;
                case ItemType.Stalk:
                    retBool[0] = true;
                    retBool[1] = true;
                    retBool[4] = true;
                    break;
            }
        }
        if (pType == PlantType.Lemon)
        {
            switch (iType)
            {
                case ItemType.Fruit:
                    retBool[3] = true;
                    retBool[4] = true;
                    break;
                case ItemType.Stalk:
                    retBool[1] = true;
                    retBool[3] = true;
                    retBool[4] = true;
                    break;
            }
        }
        // UNCOMMON
        if (pType == PlantType.Lotus)
        {
            switch (iType)
            {
                case ItemType.Fruit:
                    retBool[1] = true;
                    retBool[4] = true;
                    break;
                case ItemType.Stalk:
                    retBool[1] = true;
                    retBool[3] = true;
                    retBool[4] = true;
                    break;
            }
        }
        if (pType == PlantType.Marigold)
        {
            switch (iType)
            {
                case ItemType.Fruit:
                    retBool[0] = true;
                    retBool[3] = true;
                    retBool[4] = true;
                    break;
                case ItemType.Stalk:
                    retBool[4] = true;
                    break;
            }
        }
        if (pType == PlantType.Magnolia)
        {
            switch (iType)
            {
                case ItemType.Fruit:
                    retBool[1] = true;
                    retBool[3] = true;
                    retBool[4] = true;
                    break;
                case ItemType.Stalk:
                    retBool[1] = true;
                    retBool[4] = true;
                    break;
            }
        }
        if (pType == PlantType.Myosotis)
        {
            switch (iType)
            {
                case ItemType.Fruit:
                    retBool[4] = true;
                    break;
                case ItemType.Stalk:
                    retBool[3] = true;
                    retBool[4] = true;
                    retBool[6] = true;
                    break;
            }
        }
        if (pType == PlantType.Chrystalia)
        {
            switch (iType)
            {
                case ItemType.Fruit:
                    retBool[0] = true;
                    retBool[3] = true;
                    retBool[4] = true;
                    break;
                case ItemType.Stalk:
                    retBool[3] = true;
                    retBool[4] = true;
                    break;
            }
        }
        if (pType == PlantType.Pumpkin)
        {
            switch (iType)
            {
                case ItemType.Fruit:
                    retBool[0] = true;
                    retBool[1] = true;
                    retBool[3] = true;
                    retBool[4] = true;
                    break;
                case ItemType.Stalk:
                    retBool[3] = true;
                    retBool[4] = true;
                    retBool[6] = true;
                    break;
            }
        }
        if (pType == PlantType.Underbloom)
        {
            switch (iType)
            {
                case ItemType.Fruit:
                    retBool[1] = true;
                    retBool[4] = true;
                    break;
                case ItemType.Stalk:
                    retBool[0] = true;
                    retBool[1] = true;
                    retBool[4] = true;
                    break;
            }
        }
        if (pType == PlantType.WaterLily)
        {
            switch (iType)
            {
                case ItemType.Fruit:
                    retBool[0] = true;
                    retBool[3] = true;
                    retBool[4] = true;
                    break;
                case ItemType.Stalk:
                    retBool[3] = true;
                    retBool[4] = true;
                    break;
            }
        }
        if (pType == PlantType.Snowgrace)
        {
            switch (iType)
            {
                case ItemType.Fruit:
                    retBool[4] = true;
                    break;
                case ItemType.Stalk:
                    retBool[0] = true;
                    retBool[1] = true;
                    retBool[4] = true;
                    break;
            }
        }
        if (pType == PlantType.Popcorn)
        {
            switch (iType)
            {
                case ItemType.Fruit:
                    retBool[0] = true;
                    retBool[1] = true;
                    retBool[3] = true;
                    retBool[4] = true;
                    break;
                case ItemType.Stalk:
                    retBool[3] = true;
                    retBool[4] = true;
                    break;
            }
        }
        if (pType == PlantType.EclipseFlower)
        {
            switch (iType)
            {
                case ItemType.Fruit:
                    retBool[3] = true;
                    retBool[4] = true;
                    break;
                case ItemType.Stalk:
                    retBool[0] = true;
                    retBool[1] = true;
                    retBool[4] = true;
                    break;
            }
        }
        // RARE
        // SPECIAL
        // UNIQUE

        return retBool;
    }
}
