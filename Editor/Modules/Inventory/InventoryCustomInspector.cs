using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Inventory))]
public class InventoryCustomInspector : Editor
{
    private Inventory _target = null;

    void OnEnable()
    {
        _target = (Inventory)target;
    }
    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
        _target.autoPopulate = EditorGUILayout.Toggle("Autopopulate?", _target.autoPopulate);
        if (_target.autoPopulate)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("AutoPopulate Variables", EditorStyles.boldLabel);
            _target.inventorySize = EditorGUILayout.IntField("Inventory size", _target.inventorySize);

            EditorGUILayout.Space();


        }
        _target.hasHotbar = EditorGUILayout.Toggle("Has hotbar?", _target.hasHotbar);

        if (_target.hasHotbar)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("HotBar Variables", EditorStyles.boldLabel);
            _target.hotBarMaster = EditorGUILayout.ObjectField("Hotbar Transform", _target.hotBarMaster, typeof(Transform), true) as Transform;
            EditorGUILayout.Space();
        }

        _target.usingController = EditorGUILayout.Toggle("Using Controller?", _target.usingController);

        if (_target.usingController)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Controller Variables", EditorStyles.boldLabel);
            _target.controllerItemSelectionMover = EditorGUILayout.ObjectField("Controller Mover", _target.controllerItemSelectionMover, typeof(Transform), true) as Transform;

            _target.PixlesPerControllermovement = EditorGUILayout.IntField("Pixles Per Movement", _target.PixlesPerControllermovement);

            _target.currentActiveWindowCategory = (UIinteractionCategory)EditorGUILayout.EnumPopup("Current Active Window", _target.currentActiveWindowCategory);

            _target.grabItemButton = (KeyCode)EditorGUILayout.EnumPopup("Grab Item Button", _target.grabItemButton);
            EditorGUILayout.Space();
        }

        _target.debugMode = EditorGUILayout.Toggle("Debug mode?", _target.debugMode);

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
        DrawDefaultInspector();
    }
}
