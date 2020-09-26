using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(TriggerItem))]
public class TriggerItemEditor : Editor {

    TriggerItem targetObj;

    public override void OnInspectorGUI()
    {
        targetObj = target as TriggerItem;

        targetObj.systemType = (TriggerItem.SystemType) EditorGUILayout.EnumPopup("System Type: ", targetObj.systemType);

        if(targetObj.systemType == TriggerItem.SystemType.normalType)
        {
            SerializedProperty onTrigger = serializedObject.FindProperty("onTriggerEvents");

            EditorGUILayout.PropertyField(onTrigger);

        }else if (targetObj.systemType == TriggerItem.SystemType.openCloseType)
        {
            SerializedProperty openEvents = serializedObject.FindProperty("openEvents");
            EditorGUILayout.PropertyField(openEvents);

            SerializedProperty closeEvents = serializedObject.FindProperty("closeEvents");
            EditorGUILayout.PropertyField(closeEvents);

            targetObj.closed = EditorGUILayout.Toggle("Is closed?", targetObj.closed);

        }

        serializedObject.ApplyModifiedProperties();

        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(target);
            EditorSceneManager.MarkAllScenesDirty();
        }
    }

}
