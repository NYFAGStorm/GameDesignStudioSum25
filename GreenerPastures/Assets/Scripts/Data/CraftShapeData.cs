using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Author: Gustavo Rojas Flores
// Allows easy editing of crafting shape data for crafting spells with plants

[CreateAssetMenu(fileName = "Crafting Shape", menuName = "Greener Pastures/Crafting Shape", order = 1)]
public class CraftShape : ScriptableObject
{
    public PlantType plantType;
    public Sprite craftingTileImage;
    public bool[] craftingGridShape = new bool[25];
}

// Custom inspector
[CustomEditor(typeof(CraftShape))]
public class CraftingShapeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        try
        {
            serializedObject.Update();

            // Create serialized properties
            SerializedProperty pType = serializedObject.FindProperty("plantType");
            SerializedProperty tileImage = serializedObject.FindProperty("craftingTileImage");
            SerializedProperty boolGrid = serializedObject.FindProperty("craftingGridShape");

            // Draw first two properties
            EditorGUILayout.PropertyField(pType);
            EditorGUILayout.ObjectField(tileImage);
            
            // Create boolean grid with header
            EditorGUILayout.LabelField("Crafting Grid Shape", EditorStyles.boldLabel);
            
            for (int y = 0; y < 5; y++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int x = 0; x < 5; x++)
                {
                    SerializedProperty craftTile = boolGrid.GetArrayElementAtIndex(y * 5 + x);
                    craftTile.boolValue = GUILayout.Toggle(craftTile.boolValue, GUIContent.none, GUILayout.Width(13));
                }
                EditorGUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
        }
        catch (Exception e) 
        {
            //Debug.Log(e);
        }
    }
}