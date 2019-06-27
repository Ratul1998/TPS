﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(Weapons))]
public class WeaponEditor : Editor {

    Weapons weapon;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        weapon = (Weapons)target;

        if (GUILayout.Button("Save gun equip location."))
        {
            Transform weaponT = weapon.transform;
            Vector3 weaponPos = weaponT.localPosition;
            Vector3 weaponRot = weaponT.localEulerAngles;
            weapon.weaponSettings.equipPosition = weaponPos;
            weapon.weaponSettings.equipRotation = weaponRot;
        }

        if (GUILayout.Button("Save gun unequip location."))
        {
            Transform weaponT = weapon.transform;
            Vector3 weaponPos = weaponT.localPosition;
            Vector3 weaponRot = weaponT.localEulerAngles;
            weapon.weaponSettings.unequipPosition = weaponPos;
            weapon.weaponSettings.unequipRotation = weaponRot;
        }

        EditorGUILayout.LabelField("Debug Positioning");

        if (GUILayout.Button("Move gun to equip location"))
        {
            Transform weaponT = weapon.transform;
            weaponT.localPosition = weapon.weaponSettings.equipPosition;
            Quaternion eulerAngles = Quaternion.Euler(weapon.weaponSettings.equipRotation);
            weaponT.localRotation = eulerAngles;
        }

        if (GUILayout.Button("Move gun to unequip location"))
        {
            Transform weaponT = weapon.transform;
            weaponT.localPosition = weapon.weaponSettings.unequipPosition;
            Quaternion eulerAngles = Quaternion.Euler(weapon.weaponSettings.unequipRotation);
            weaponT.localRotation = eulerAngles;
        }
    }

}
