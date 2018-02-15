using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;

[CustomPropertyDrawer (typeof (DamageableProperties))]
public class DmgPropertiesDrawer : PropertyDrawer
{
    public GUIStyle style;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        SerializedProperty dmgAmount = property.FindPropertyRelative("damageAmmount");
        SerializedProperty dmgType = property.FindPropertyRelative("typeOfDamage");

        int indentLevel = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

            //EditorGUI.LabelField(new Rect(position.x, position.y, 50, 20), "Damage");
            EditorGUI.PropertyField(new Rect(position.x, position.y, 50, 20), dmgAmount, GUIContent.none);

            //EditorGUI.LabelField(new Rect(position.x, position.y + 25, 50, 20), "Type");
            EditorGUI.PropertyField(new Rect(position.x + 50, position.y, 50, 20), dmgType, GUIContent.none);

       // EditorGUI.PropertyField(new Rect(position.x, position.y, 100, position.height), id, GUIContent.none);

        EditorGUI.indentLevel = indentLevel;

        EditorGUI.EndProperty();


    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 30f;
    }
}


