using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(IslandRegionGenerator))]
public class IslandRegionGeneratorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		
		var script = (IslandRegionGenerator)target;
		
		if(GUILayout.Button("Gen"))
		{
			script.GenerateAll();
			
			// re-draw scene
			SceneView.RepaintAll();
			SceneView.RepaintAll();
		}
	}
}
