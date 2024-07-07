using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Core.PathToWall))]
public class PathToWallEditor : Editor
{
    
    SerializedProperty pathProperty;
    SerializedProperty useCustomHeightSerializedProperty;
    SerializedProperty customHeightSerializedProperty;
    private SerializedProperty generatedMeshSerializedProperty;
    
    private bool showExtrasFoldout = false;
    
    void OnEnable()
    {
        pathProperty = serializedObject.FindProperty("path");
        useCustomHeightSerializedProperty = serializedObject.FindProperty("useCustomHeight");
        customHeightSerializedProperty = serializedObject.FindProperty("customHeight");
        generatedMeshSerializedProperty = serializedObject.FindProperty("generatedMesh");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        Core.PathToWall pathToWall = (Core.PathToWall)target;
        Vector3[] path = pathToWall.GetPath();
        
        EditorGUILayout.Space(10);
        
        showExtrasFoldout = EditorGUILayout.Foldout(showExtrasFoldout, "Extras", true);
        
        if (showExtrasFoldout)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(pathProperty);

            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.BeginHorizontal(new GUIStyle() { margin = new RectOffset(0, 0, 0, 0) });

        EditorGUIUtility.labelWidth = 130;
        
        EditorGUILayout.PrefixLabel("Use Custom Height");
        useCustomHeightSerializedProperty.boolValue = EditorGUILayout.Toggle(useCustomHeightSerializedProperty.boolValue);
        
        //BuildCustomHeightUIWithDisableBehaviour();
        BuildCustomHeightUIWithHideBehaviour();
        
        EditorGUIUtility.labelWidth = 0;
        
        EditorGUILayout.EndHorizontal();
        
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Generate New Path"))
        {
            pathToWall.GenerateNewPath();
        }
        
        EditorGUILayout.Space(10);
        
        EditorGUI.BeginDisabledGroup(path == null || path.Length == 0);
        
        if (GUILayout.Button("Generate Wall Mesh"))
        {
            pathToWall.GenerateWallMesh();
        }
        
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.Space(10);
        
        EditorGUI.BeginDisabledGroup(generatedMeshSerializedProperty.objectReferenceValue == null);
        
        if (GUILayout.Button("Destroy Wall Mesh"))
        {
            pathToWall.DestroyWallMesh();
        }
        
        EditorGUI.EndDisabledGroup();
        
        serializedObject.ApplyModifiedProperties();
    }

    private void BuildCustomHeightUIWithDisableBehaviour()
    {
        EditorGUI.BeginDisabledGroup(!useCustomHeightSerializedProperty.boolValue);
        
        EditorGUILayout.Space(50);
        customHeightSerializedProperty.floatValue = EditorGUILayout.FloatField(customHeightSerializedProperty.floatValue,GUILayout.Width(9999));
        
        EditorGUI.EndDisabledGroup();
    }

    private void BuildCustomHeightUIWithHideBehaviour()
    {
        EditorGUILayout.Space(50);

        if (useCustomHeightSerializedProperty.boolValue)
        {
            customHeightSerializedProperty.floatValue = EditorGUILayout.FloatField(customHeightSerializedProperty.floatValue, GUILayout.Width(9999));
        }
    }
}
