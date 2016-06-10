using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(VoronoiDemo))]
public class VoronoiDemoEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		
		var script = (VoronoiDemo)target;
		
		if(GUILayout.Button("Gen"))
		{
			script.Gen();
		}
		
		if(GUILayout.Button("Relax"))
		{
			script.Relax(3);
		}
	}
}
