using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NYFA Studio/Utility/FolderTool")]
public class FolderTool : MonoBehaviour
{
    // Author: Glenn Storm
    // This locks game objects at world origin, allowing them to act like folders in the Hierarchy


    void Start()
    {
        gameObject.transform.position = Vector3.zero;
        gameObject.transform.rotation = Quaternion.identity;
        gameObject.transform.localScale = Vector3.one;
    }

    void Update()
    {
        gameObject.transform.position = Vector3.zero;
        gameObject.transform.rotation = Quaternion.identity;
        gameObject.transform.localScale = Vector3.one;
    }

    void OnDrawGizmos()
    {
        gameObject.transform.position = Vector3.zero;
        gameObject.transform.rotation = Quaternion.identity;
        gameObject.transform.localScale = Vector3.one;
    }
}
