using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(GameMgr))]
public class IslandGameMgrEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		
		
		var script = (GameMgr)target;
		
		if (GUILayout.Button ("Menu")) {
			EditorWindow.GetWindow<GameMgrGui>().Toggle();
		}
	}
}
