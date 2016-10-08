using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Terrain))]
public class TerrainTile : MonoBehaviour {
	IntVector2 tileIndex;

	Vector2 minPos;
	Vector2 maxPos;

	public MapGenerator Generator {
		get {
			return transform.parent.GetComponent<MapGenerator> ();
		}
	}

	public float Size {
		get {
			return Generator.terrainSize.tileSize;
		}
	}

	internal void ResetTile(IntVector2 tileIndex) {
		this.tileIndex = tileIndex;

		var p = transform.position;
		var size = Size;
		minPos = new Vector2 (p.x, p.z);
		maxPos = new Vector2 (p.x + size, p.z + size);
	}
	
	void Start () {
	}


	void Update () {
		
	}
}
