using UnityEngine;
using System.Collections;
using UnityEditor;


/// <summary>
/// TODO: The global map editor should eventually allow editing of the terrain partitioning and individual partitions
/// </summary>
[CustomEditor(typeof(GameMgr))]
public class MapEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		if (GUILayout.Button ("Re-generate partitions")) {
			
		}
		
		//var script = (GameMgr)target;
		
//		if (GUILayout.Button ("Menu")) {
//			EditorWindow.GetWindow<GameMgrEditor>().Toggle();
//		}
	}
}
