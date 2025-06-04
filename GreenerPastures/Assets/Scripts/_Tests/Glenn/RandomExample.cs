using UnityEngine;

public class RandomExample : MonoBehaviour
{
    // Author: Glenn Storm
    // This demonstrates the random utility functions available

    public enum RandomType
    {
        FlatRandom,
        WeightedRandom,
        GaussianRandom
    }

    public RandomType typeOfRandom;
    public float randomResult;
    public float multiplier = 5f;
    public float additional = 1f;
    public bool roundResult = true;
    public float finalResult;
    public bool rollRandom;


    void Start()
    {
        // validate
        // initialize
    }

    void Update()
    {
        // wait until 'rollRandom' is clicked
        if (!rollRandom)
            return;

        rollRandom = false;

        if (typeOfRandom == RandomType.FlatRandom)
            randomResult = RandomSystem.FlatRandom01();
        else if (typeOfRandom == RandomType.WeightedRandom)
            randomResult = RandomSystem.WeightedRandom01();
        else if (typeOfRandom == RandomType.GaussianRandom)
            randomResult = RandomSystem.GaussianRandom01();

        randomResult = (randomResult * multiplier) + additional;

        if (roundResult)
            finalResult = Mathf.RoundToInt(randomResult);
        else 
            finalResult = randomResult;
    }
}
