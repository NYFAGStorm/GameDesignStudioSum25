using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrayExample : MonoBehaviour
{
    // Author: Glenn Storm
    // Array example

    public string[] myArray;

    public string toAdd;
    public bool add;

    public int toRemove;
    public bool remove;

    public bool shuffleArray;

    [Tooltip("isDirty only indicates the potential the array is currently unsorted")]
    public bool isDirty;
    public bool sortAscending;
    public bool sortDescending;


    void Start()
    {
        // debug only
        //myArray = new string[] { "Abe", "Bob", "Chuck", "Dave", "Ed", "Frank" };
    }

    void Update()
    {
        if (add)
        {
            add = false;
            isDirty = AddToArray(toAdd);
        }
        if (remove)
        {
            remove = false;
            isDirty = RemoveFromArray(toRemove);
        }
        if (shuffleArray)
        {
            shuffleArray = false;
            isDirty = ShuffleArray();
        }
        if (sortAscending)
        {
            sortAscending = false;
            Sort(true);
            isDirty = false;
        }
        if (sortDescending)
        {
            sortDescending = false;
            Sort(false);
            isDirty = false;
        }
    }

    bool AddToArray(string newString)
    {
        string[] tmp = new string[myArray.Length + 1];
        for (int i = 0; i < myArray.Length; i++)
        {
            tmp[i] = myArray[i];
        }
        tmp[myArray.Length] = newString;
        myArray = tmp;
        toAdd = "";
        return (myArray.Length > 1);
    }

    bool RemoveFromArray(int index)
    {
        if (myArray.Length < 1)
            return false;
        string[] tmp = new string[myArray.Length - 1];
        int count = 0;
        for (int i = 0; i < myArray.Length; i++)
        {
            if (i != index)
            {
                tmp[count] = myArray[i];
                count++;
            }
        }
        myArray = tmp;
        toRemove = 0;
        return (myArray.Length > 1);
    }

    bool ShuffleArray()
    {
        if (myArray.Length < 2)
            return false;
        // NOTE: we avoid requiring unique array element values by shuffling index values
        int[] myArrayIndexes = new int[myArray.Length];
        int[] tmpIndexes = new int[myArray.Length];
        for (int i = 0;i<myArrayIndexes.Length; i++ ) { myArrayIndexes[i] = i; }
        for (int i=0; i<tmpIndexes.Length;i++) { tmpIndexes[i] = -1; }
        int count = 0;
        while (count < tmpIndexes.Length)
        {
            int rand = Random.Range(0, myArrayIndexes.Length);
            bool found = false;
            for (int i = 0;i<tmpIndexes.Length;i++)
            {
                if (tmpIndexes[i] == myArrayIndexes[rand])
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                tmpIndexes[count] = myArrayIndexes[rand];
                count++;
            }
        }
        string[] tmp = new string[myArray.Length];
        for (int i = 0;i<tmp.Length; i++)
        {
            tmp[i] = myArray[tmpIndexes[i]];
        }
        myArray = tmp;
        return true;
    }

    bool IsOrdered(bool ascendingOrder, string s1, string s2)
    {
        if (ascendingOrder)
            return (s1.CompareTo(s2) <= 0);
        else
            return (s1.CompareTo(s2) >= 0);
    }

    void Sort(bool ascending)
    {
        if (myArray.Length < 2)
            return;
        for (int i = 1; i < myArray.Length; i++)
        {
            int n = i;
            string tmp = myArray[n];
            while (n > 0 && !IsOrdered(ascending, myArray[n - 1], tmp))
            {
                myArray[n] = myArray[n-1];
                n--;
            }
            if (n != i)
                myArray[n] = tmp;
        }
    }
}
