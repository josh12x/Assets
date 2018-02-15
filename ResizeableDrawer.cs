using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomPropertyDrawer (typeof (ResizeableElement))]
public class ResizeableDrawer : PropertyDrawer
{
    public override void OnGUI(Rect posittion, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(posittion, label, property);

        posittion = EditorGUI.PrefixLabel(posittion, GUIUtility.GetControlID(FocusType.Passive), label);

        SerializedProperty sizeProp = property.FindPropertyRelative("sizePadding");
        SerializedProperty windowProp = property.FindPropertyRelative("windowToResize");

        int indentLevel = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        Rect sizePad = new Rect(posittion.x + 120, posittion.y, 100, posittion.height);

        Rect window = new Rect(posittion.x + 0, posittion.y, 100, posittion.height);


        EditorGUI.PropertyField(window, windowProp, GUIContent.none);
        EditorGUI.PropertyField(sizePad, sizeProp, GUIContent.none);

        EditorGUI.indentLevel = indentLevel;

        EditorGUI.EndProperty();
    }
}
