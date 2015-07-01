using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		
		var script = (TerrainGenerator)target;
		
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
