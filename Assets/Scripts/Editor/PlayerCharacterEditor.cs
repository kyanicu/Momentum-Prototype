//C# Example (LookAtPointEditor.cs)
using UnityEngine;
using UnityEditor;

/*
[CustomEditor(typeof(PlayerCharacter))]
[CanEditMultipleObjects]
public class PlayerCharacterEditor : Editor 
{
/*
    SerializedProperty player;
    
    void OnEnable()
    {
        player = serializedObject.FindProperty("lookAtPoint");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(player);
        serializedObject.ApplyModifiedProperties();
    }
}
*/