using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NodeFramework))]
public class NodeFrameworkEditor : Editor
{
    private NodeFramework scriptableObject;
    private SerializedProperty optionalJsonPathProp;

    private void OnEnable()
    {
        scriptableObject = (NodeFramework)target;
        optionalJsonPathProp = serializedObject.FindProperty("optionalJsonPath");
        
        scriptableObject.RecalculateDependencies();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.Space(20);
        
        if (GUILayout.Button("Open Node Framework Editor"))
        {
            NodeFrameworkWindowEditor.CreateWindow(scriptableObject).SetPanCenteredOnStarterNode();
        }
        
        EditorGUILayout.Space(20);
        
        EditorGUILayout.PropertyField(optionalJsonPathProp);
        
        if(GUILayout.Button("Import Json from optional path"))
        {
            scriptableObject.ImportJson(scriptableObject.optionalJsonPath);
        }
        
        if(GUILayout.Button("Export Json from optional path"))
        {
            scriptableObject.ExportJson(scriptableObject.optionalJsonPath);
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}
