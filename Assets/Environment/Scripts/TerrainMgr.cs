using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 
/// </summary>
public class TerrainMgr : MonoBehaviour {
	#region Singleton management
	public static TerrainMgr Instance {
		get;
		private set;
	}
		
	TerrainMgr() {
		//if (Instance != null) throw new UnityException ("Tried to instantiate singleton more than once: "  + this);
		Instance = this;
	}
	#endregion

	/// <summary>
	/// Default terrain size
	/// </summary>
	public TerrainSize TerrainSize = new TerrainSize();
}