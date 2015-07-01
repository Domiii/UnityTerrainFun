// C# example:
using UnityEngine;
using UnityEditor;


/// <summary>
/// This is a wrapper to also show the GameMgr's menu in the Scene view.
/// </summary>
public class GameMgrGui : EditorWindow {
	public bool IsOpen {
		get { return GameMgr.Instance.Menu.IsOpen; }
		private set {
			GameMgr.Instance.Menu.IsOpen = value;
		}
	}

	public void Toggle() {
		IsOpen = !IsOpen;
		if (IsOpen) {
			Show ();
		} 
		else {
			Close();
		}
	}
	
	void OnGUI () {
		GameMgr.Instance.Menu.OnGUI ();
	}
}
