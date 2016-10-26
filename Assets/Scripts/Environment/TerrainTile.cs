using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Terrain))]
public class TerrainTile : MonoBehaviour {
	public IntVector2 tileIndex;

//	Vector2 minPos;
//	Vector2 maxPos;


	public IntVector2 TileIndex {
		get {
			return tileIndex;
		}
	}

	public MapGenerator Map {
		get {
			return transform.parent.GetComponent<MapGenerator> ();
		}
	}

	/// <summary>
	/// The Tile is a square where this size is the side-length
	/// </summary>
	public float Size {
		get {
			return Map.terrainSize.tileSize;
		}
	}

	internal void ResetTile(IntVector2 tileIndex) {
		this.tileIndex = tileIndex;

		var size = Size;
		var xPos = (tileIndex.x-0.5f) * size;
		var zPos = (tileIndex.y-0.5f) * size;
		transform.position = new Vector3 (xPos, Map.baseHeight, zPos);
			
//		var p = transform.position;
//		minPos = new Vector2 (p.x, p.z);
//		maxPos = new Vector2 (p.x + size, p.z + size);

		gameObject.name = ToString ();

		// re-generate things
		//SendMessage("Gen", SendMessageOptions.DontRequireReceiver);
		GetComponent<HeightMapGenerator>().Gen();
	}
	
	void Start () {
	}


	void Update () {
		
	}

	public override string ToString ()
	{
		return string.Format ("[TerrainTile ({0}, {1})]", TileIndex.x, TileIndex.y);
	}
}
