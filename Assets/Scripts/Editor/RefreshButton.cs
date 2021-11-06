using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(World))]
public class RefreshButton : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        World gen = (World)target;
        if(GUILayout.Button("Refresh terrain"))
        {
            /*gen.SetOffset();*/
        }
    }
}
