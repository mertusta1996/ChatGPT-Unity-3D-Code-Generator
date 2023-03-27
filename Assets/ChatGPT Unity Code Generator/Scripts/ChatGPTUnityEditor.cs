using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ChatGPTUnityCodeGenerator))]
public class ChatGPTUnityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.LabelField(" ");
        
        if(GUILayout.Button("Generate Unity3D Code"))
        {
            ChatGPTUnityCodeGenerator.Instance.GenerateUnity3DCode();
        }
        if(GUILayout.Button("Add \"Generated Code\" to Scene"))
        {
            ChatGPTUnityCodeGenerator.Instance.AddGeneratedCodeToScene();
        }
    }
}