using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(Throwables))]
public class ThrowableEditor : Editor{

    Throwables throwable;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        throwable = (Throwables)target;

        if (GUILayout.Button("Save Nade equip location."))
        {
            Transform weaponT = throwable.transform;
            Vector3 weaponPos = weaponT.localPosition;
            Vector3 weaponRot = weaponT.localEulerAngles;
            throwable.throwablesettings.equipPosition = weaponPos;
            throwable.throwablesettings.equipRotation = weaponRot;
        }

        if (GUILayout.Button("Save Nade unequip location."))
        {
            Transform weaponT = throwable.transform;
            Vector3 weaponPos = weaponT.localPosition;
            Vector3 weaponRot = weaponT.localEulerAngles;
            throwable.throwablesettings.unequipPosition = weaponPos;
            throwable.throwablesettings.unequipRotation = weaponRot;
        }

        EditorGUILayout.LabelField("Debug Positioning");

        if (GUILayout.Button("Move Nade to equip location"))
        {
            Transform weaponT = throwable.transform;
            weaponT.localPosition = throwable.throwablesettings.equipPosition;
            Quaternion eulerAngles = Quaternion.Euler(throwable.throwablesettings.equipRotation);
            weaponT.localRotation = eulerAngles;
        }

        if (GUILayout.Button("Move Made to unequip location"))
        {
            Transform weaponT = throwable.transform;
            weaponT.localPosition = throwable.throwablesettings.unequipPosition;
            Quaternion eulerAngles = Quaternion.Euler(throwable.throwablesettings.unequipRotation);
            weaponT.localRotation = eulerAngles;
        }
    }
}
