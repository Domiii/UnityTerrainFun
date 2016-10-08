using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(HeightMapGenerator))]
public class TerrainGeneratorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		
		var script = (HeightMapGenerator)target;
		
		if(GUILayout.Button("Randomize"))
		{
			script.Randomize();
		}
		if(GUILayout.Button("Build"))
		{
			script.GenAll();
		}
		if(GUILayout.Button("Clear"))
		{
			script.Clear();
		}
	}
}
