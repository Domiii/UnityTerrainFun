using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(MapPartitioning))]
public class MapPartitioningEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		
		var script = (MapPartitioning)target;
		
		if(GUILayout.Button("Gen"))
		{
			script.GenerateAll();
			
			// re-draw scene
			//SceneView.RepaintAll();
			SceneView.RepaintAll();
			SceneView.RepaintAll();
		}
	}
}
