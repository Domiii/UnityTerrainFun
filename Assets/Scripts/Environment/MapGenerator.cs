using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 
/// </summary>
public class MapGenerator : MonoBehaviour {
	const int NBorderTiles = 1;
	const int NTilesPerRow = 1 + 2 * NBorderTiles;

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

	TerrainData[,] terrainData = new TerrainData[NTilesPerRow,NTilesPerRow];
	TerrainTile[,] tileBuffer = new TerrainTile[NTilesPerRow,NTilesPerRow];
	TerrainTile[,] tiles = new TerrainTile[NTilesPerRow,NTilesPerRow];


	void Start() {
		CreateTerrainData ();
		CreateTiles ();
	}

	void Update() {
		CenterTilesAroundPivot ();
	}

	#region Tile Queries
	public TerrainTile CenterTile {
		get {
			return tiles [NBorderTiles, NBorderTiles];
		}
	}

	public IntVector2 GetTileIndex(float x, float z) {
		var i = Mathf.RoundToInt((x / terrainSize.tileSize));
		var j = Mathf.RoundToInt((z / terrainSize.tileSize));
		return new IntVector2 (i, j);
	}

	public TerrainTile GetTile(float x, float z) {
		var idx = GetTileIndex (x, z);
		return GetTile(idx);
	}

	public TerrainTile GetTile(IntVector2 idx) {
		var di = idx.x - currentCenterIndex.x;
		var dj = idx.y - currentCenterIndex.y;
		return GetTileFromArray (di, dj);
	}

	TerrainTile GetTileFromArray(int di, int dj) {
		if (Mathf.Abs(di) <= NBorderTiles && Mathf.Abs(dj) <= NBorderTiles) {
			return tiles[di+NBorderTiles, dj+NBorderTiles];
		}
		return null;
	}

	TerrainTile GetTileFromBuffer(int di, int dj) {
		if (Mathf.Abs(di) <= NBorderTiles && Mathf.Abs(dj) <= NBorderTiles) {
			return tileBuffer[di+NBorderTiles, dj+NBorderTiles];
		}
		return null;
	}

	void PrintTiles() {
		for (int j = -NBorderTiles; j <= NBorderTiles; j++) {
			var row = new string[NTilesPerRow];
			for (int i = -NBorderTiles; i <= NBorderTiles; i++) {
				var tile = GetTileFromArray (i, j);
				row [i + NBorderTiles] = tile.ToString();
			}
			print (string.Join(", ", row));
		}
	}
	#endregion

	#region Tile Bookkeeping
	public void ClearTiles() {
		// TODO
	}

	void CreateTerrainData() {
		for (var j = 0; j < NTilesPerRow; ++j) {
			for (var i = 0; i < NTilesPerRow; ++i) {
				if (terrainData [i, j] == null) {
					terrainData [i, j] = new TerrainData ();
				}
			}
		}
	}

	void CreateTiles() {
		var center = currentCenterIndex;
		for (var dy = - NBorderTiles; dy <= NBorderTiles; ++dy) {
			var y = center.y + dy;
			for (var dx = - NBorderTiles; dx <= NBorderTiles; ++dx) {
				var x = center.x + dx;

				var i = dx + NBorderTiles;
				var j = dy + NBorderTiles;
				var tile = tiles[i, j];
				var data = terrainData [i, j];
				if (tile == null) {
					tile = tileBuffer[i, j] = tiles [i, j] = CreateTile (x, y, data);
				}
			}
		}
	}

	TerrainTile CreateTile(int i, int j, TerrainData data) {
		var tile = Instantiate (tilePrefab, transform) as TerrainTile;

		// set TerrainData
		var terrain = tile.GetComponent<Terrain> ();
		var collider = tile.GetComponent<TerrainCollider> ();
		terrain.terrainData = collider.terrainData = data;

		// set size
		data.heightmapResolution = terrainSize.tileSize;
		data.size = new Vector3(terrainSize.tileSize, 1, terrainSize.tileSize);

		// reset tile
		ResetTile (tile, new IntVector2(i, j));

		return tile;
	}

	void ResetTile(TerrainTile tile, IntVector2 tileIndex) {
		tile.ResetTile (tileIndex);

		// TODO: Recompute TerrainData and everything that is on the Terrain!
	}

	/// <summary>
	/// Updates all tiles to make sure we see the tiles currently surrounding the pivot
	/// </summary>
	void CenterTilesAroundPivot() {
		var p = pivot.position;
		var pivotTileIndex = GetTileIndex(p.x, p.z);
		var pivotTile = GetTile (pivotTileIndex);
		if (pivotTile != CenterTile) {
			// pivot not on center tile -> Shift the whole thing!
			var di = pivotTileIndex.x - currentCenterIndex.x;
			var dj = pivotTileIndex.y - currentCenterIndex.y;

			// Step #1: set new center
			currentCenterIndex = pivotTileIndex;

			// Step #2: shift all tiles
			ShiftTiles(di, dj);
		}
	}

	void ShiftTiles(int di, int dj) {
		// Step #1: Move all surviving tiles
		for (int j = -NBorderTiles; j <= NBorderTiles; j++) {
			var fromJ = WrapCircularIndex(j, dj);
			for (int i = -NBorderTiles; i <= NBorderTiles; i++) {
				// TODO: Compute circular index correctly
				// TODO: Move all tiles
				var fromI = WrapCircularIndex(i, di);
				var tile = GetTileFromBuffer (fromI, fromJ);
				Debug.AssertFormat(tile != null, "Invalid tile index: ({0}, {1})", fromI, fromJ);

				MoveTile (tile, i, j);
			}
		}

		// Step #2: Replace vanished tiles
		for (int j = -NBorderTiles; j <= NBorderTiles; j++) {
			var fromJ = j + dj;
			for (int i = -NBorderTiles; i <= NBorderTiles; i++) {
				var fromI = i + di;
				var newTile = GetTileFromArray (fromI, fromJ);
				if (newTile == null) {
					var replaceI = WrapCircularIndex(i, di);
					var replaceJ = WrapCircularIndex(j, dj);
					var tile = GetTileFromBuffer(replaceI, replaceJ);	// get tile form buffer (the tiles array has already been changed!)
					//print (string.Format("Tile Reset ({0}, {1}) -> ({2}, {3})", tile.TileIndex.x, tile.TileIndex.y, currentCenterIndex.x + i, currentCenterIndex.y + j));
					ResetTile(tile, new IntVector2(currentCenterIndex.x + i, currentCenterIndex.y + j));
				}
			}
		}

		// copy the new configuration to buffer
		UpdateTileBuffer ();
	}

	void MoveTile(TerrainTile tile, int di, int dj) {
		tiles[di + NBorderTiles, dj + NBorderTiles] = tile;
	}

	void UpdateTileBuffer () {
		for (var j = 0; j < NTilesPerRow; ++j) {
			for (var i = 0; i < NTilesPerRow; ++i) {
				tileBuffer [i, j] = tiles[i,j];
			}
		}
	}

	int WrapCircularIndex(int ri, int di) {
		var i = (ri + di + NBorderTiles) % NTilesPerRow;
		if (i < 0) {
			i = NTilesPerRow + i;
		}
		return i - NBorderTiles;
	}
	#endregion
}