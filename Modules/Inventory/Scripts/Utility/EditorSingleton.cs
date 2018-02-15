using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[ExecuteInEditMode]
public class EditorSingleton : MonoBehaviour
{
    public Texture test;
    public Texture test2;
    public GUIStyle style;
    public static EditorSingleton instance;
    [ExecuteInEditMode]
    void Update()
    {
        if (Application.isEditor == false)
        {
            Destroy(this);
        }
        if (instance == null)
        {
            instance = this;
        }
    }
}
