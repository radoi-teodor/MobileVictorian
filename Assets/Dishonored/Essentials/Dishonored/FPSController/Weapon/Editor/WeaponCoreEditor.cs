using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(WeaponCore))]
public class WeaponCoreEditor : Editor {

    WeaponCore core;

    private void OnEnable()
    {
        core = (WeaponCore)target;
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        core.weaponType = (WeaponCore.WeaponType) EditorGUILayout.EnumPopup("Weapon type: ", core.weaponType);

        if(core.weaponType == WeaponCore.WeaponType.ranged)
        {
            EditorGUILayout.Space();
            core.shootPoint = (GameObject)EditorGUILayout.ObjectField("Shoot point: ", core.shootPoint, typeof(GameObject), true);
            core.explosion = (GameObject)EditorGUILayout.ObjectField("Explosion: ", core.explosion, typeof(GameObject), true);
            core.bullet = (Bullet)EditorGUILayout.ObjectField("Bullet Object: ", core.bullet, typeof(Bullet), true);
            core.bullets = EditorGUILayout.IntField("Bullet count: ", core.bullets);

            EditorGUILayout.Space();

            core.damage = EditorGUILayout.FloatField("Damage: ", core.damage);

        }

        EditorGUILayout.Space();

        core.localPosition = EditorGUILayout.Vector3Field("Local Position: ", core.localPosition);
        core.localRotation = EditorGUILayout.Vector3Field("Local Rotation: ", core.localRotation);

        EditorGUILayout.Space();

        core.cursor = (Sprite)EditorGUILayout.ObjectField("Cursor image: ", core.cursor, typeof(Sprite), true);

        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(target);
            EditorSceneManager.MarkAllScenesDirty();
        }
    }

}
