using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 
/// </summary>
public class MapGenerator : MonoBehaviour {
	#region Singleton management
	public static MapGenerator Instance {
		get;
		private set;
	}
		
	MapGenerator() {
		//if (Instance != null) throw new UnityException ("Tried to instantiate singleton more than once: "  + this);
		Instance = this;
	}
	#endregion

	/// <summary>
	/// Default terrain size
	/// </summary>
	public Transform pivot;
	public TerrainTile tilePrefab;
	public TerrainSize terrainSize = new TerrainSize();
	public TerrainSeeds terrainSeeds = new TerrainSeeds();
	public float baseHeight = 0;

	IntVector2 currentCenterIndex;

	TerrainData[,] terrainData = new TerrainData[3,3];
	TerrainTile[,] tiles = new TerrainTile[3,3];

	public IntVector2 GetTileIndex(float x, float z) {
		var i = (int)((x / terrainSize.tileSize) + 0.5f);
		var j = (int)((z / terrainSize.tileSize) + 0.5f);
		return new IntVector2 (i, j);
	}

	void Start() {
		CreateTerrainData ();
		CreateTiles ();
	}

	public void ClearTiles() {
		// TODO
	}

	void CreateTerrainData() {
		for (var j = 0; j < 2; ++j) {
			for (var i = 0; i < 2; ++i) {
				terrainData[i, j] = new TerrainData();
			}
		}
	}

	void CreateTiles() {
		var center = currentCenterIndex;
		for (var dy = - 1; dy <= 1; ++dy) {
			var y = center.y + dy;
			for (var dx = - 1; dx <= 1; ++dx) {
				var x = center.x + dx;

				var tile = tiles[dx+1, dy+1];
				var data = terrainData [dx + 1, dy + 1];
				if (tile == null) {
					tile = tiles [dx + 1, dy + 1] = CreateTile (x, y, data);
				}
			}
		}
	}

	void ResetTile(TerrainTile tile, IntVector2 tileIndex, TerrainData data) {
		tile.ResetTile (tileIndex);

		// TODO: Recompute TerrainData
	}

	/// <summary>
	/// Updates all tiles to make sure we see the tiles currently surrounding the pivot
	/// </summary>
	void CenterTilesAroundPivot() {
		var p = pivot.position;
		var centerIndex = GetTileIndex(p.x, p.z);
		var pivotTile = GetTile (centerIndex);
		if (pivotTile != tiles [1, 1]) {
			// pivot not standing on center tile -> Shift the whole thing!
			var dx = centerIndex.x - currentCenterIndex.x;
			var dy = centerIndex.y - currentCenterIndex.y;
			ShiftTiles(dx, dy);
		}
	}

	void ShiftTiles(int dx, int dy) {
		if (Mathf.Abs (dx) > 2 || Mathf.Abs (dx) > 2) {
			// Jumped more than 2 tiles -> reset everything!

			// TODO: Reset everything
		} else {
			// We can re-use at least one tile -> just move everything correspondingly

			// TODO: Move all tiles
		}
	}

	public TerrainTile GetTile(float x, float z) {
		var idx = GetTileIndex (x, z);
		return GetTile(idx);
	}

	public TerrainTile GetTile(IntVector2 idx) {
		var i = idx.x - currentCenterIndex.x;
		var j = idx.y - currentCenterIndex.y;
		if (Mathf.Abs(i) <= 1 && Mathf.Abs(j) <= 1) {
			return tiles[i+1, j+1];
		}
		return null;
	}

	TerrainTile CreateTile(int x, int y, TerrainData data) {
		var xPos = (x-0.5f) * terrainSize.tileSize;
		var zPos = (y-0.5f) * terrainSize.tileSize;
		var tile = Instantiate (tilePrefab, new Vector3(xPos, baseHeight, zPos), Quaternion.identity, transform) as TerrainTile;

		// set TerrainData
		var terrain = tile.GetComponent<Terrain> ();
		var collider = tile.GetComponent<TerrainCollider> ();
		terrain.terrainData = collider.terrainData = data;

		// reset tile and data
		ResetTile (tile, new IntVector2(x, y), data);

		return tile;
	}
}