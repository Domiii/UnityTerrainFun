using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(IslandTerrainGenerator))]
public class IslandTerrainGeneratorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		
		var script = (IslandTerrainGenerator)target;
		
		if(GUILayout.Button("Randomize"))
		{
			script.Randomize();
		}
		if(GUILayout.Button("Build"))
		{
			script.Generate();
		}
		if(GUILayout.Button("Clear"))
		{
			script.Clear();
		}
	}
}
