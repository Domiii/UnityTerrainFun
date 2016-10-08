using UnityEngine;
using System.Collections;

//[RequireComponent(typeof(MapPartitioning))]
//public class PartitionPainter : MonoBehaviour {
//	MapPartitioning terrainPartitioning;
//
//	// Use this for initialization
//	void Start () {
//		terrainPartitioning = GetComponent<MapPartitioning> ();
//	}
//
////	public void Paint(Terrain terrain) {
////		var terrainSize = TerrainMgr.Instance.TerrainSize;
////
////		if(!Mathf.IsPowerOfTwo(terrainSize.DetailMapSize))
////		{
////			Debug.Log("TerrianGenerator::Start - Detail map size must be power of 2 (e.g. 128, 256, 512 etc.)");
////			terrainSize.DetailMapSize = Mathf.ClosestPowerOfTwo(terrainSize.DetailMapSize);
////		}
////
////		if(m_settings.m_renderSettings.m_detailResolutionPerPatch < 8)
////		{
////			Debug.Log("TerrianGenerator::Start - Detail resolution per patch must be >= 8, changing to 8");
////			m_settings.m_renderSettings.m_detailResolutionPerPatch = 8;
////		}
////
////		// start generating terrain data
////		TerrainData terrainData = terrain.terrainData;
////		if (terrainData == null) {
////			terrain.terrainData = new TerrainData ();
////			GetComponent<TerrainCollider> ().terrainData = terrain.terrainData;	// also set collider's data
////		}
////
////		terrainData.size = new Vector3(terrainSize.TileWidth, terrainSize.TerrainHeight, terrainSize.TileWidth);
////		terrainData.splatPrototypes = m_settings.m_splatPrototypes;
////		terrainData.treePrototypes = m_settings.m_treeProtoTypes;
////		terrainData.detailPrototypes = m_settings.m_detailProtoTypes;
////
////		GenAlphaMap(terrainData);
////
////		m_terrain.transform.position = new Vector3(-terrainSize.TileWidth*0.5f, 0, -terrainSize.TileWidth*0.5f);
////		m_terrain.heightmapPixelError = m_settings.m_renderSettings.m_pixelMapError;
////		m_terrain.basemapDistance = m_settings.m_renderSettings.m_baseMapDist;
////		GenDetailMap(m_terrain);
////	}
////
////	/// <summary>
////	/// This method decides where how much of each texture can be seen.
////	/// </summary>
////	/// <see cref="https://alastaira.wordpress.com/2013/11/14/procedural-terrain-splatmapping/"/>
////	/// <param name="terrainData">Terrain data.</param>
////	void GenAlphaMap(TerrainData terrainData) 
////	{
////		float[,,] map  = new float[terrainSize.AlphaMapSize, terrainSize.AlphaMapSize, 2];
////
////		for(int x = 0; x < terrainSize.AlphaMapSize; x++) 
////		{
////			for (int z = 0; z < terrainSize.AlphaMapSize; z++) 
////			{
////				// Get the normalized terrain coordinate that
////				// corresponds to the the point.
////				float normX = x * 1.0f / (terrainSize.AlphaMapSize - 1);
////				float normZ = z * 1.0f / (terrainSize.AlphaMapSize - 1);
////
////				// Get the steepness value at the normalized coordinate.
////				float angle = terrainData.GetSteepness(normX, normZ);
////
////				// Steepness is given as an angle, 0..90 degrees. Divide
////				// by 90 to get an alpha blending value in the range 0..1.
////				float frac = angle / 90.0f;
////				if (angle < 30) {
////					frac = 0;
////				}
////				map[z, x, 0] = 1.0f - frac;
////				map[z, x, 1] = frac;
////			}
////		}
////
////		terrainData.alphamapResolution = terrainSize.AlphaMapSize;
////		terrainData.SetAlphamaps(0, 0, map);
////	}
////
////	void GenDetailMap(Terrain terrain)
////	{
////		//each layer is drawn separately so if you have a lot of layers your draw calls will increase 
////
////		// initialize detail maps
////		int[][,] detailMaps = new int[m_settings.m_detailTextures.Length][,];
////		for (var i = 0; i < detailMaps.Length; ++i) {
////			detailMaps[i] = new int[terrainSize.DetailMapSize,terrainSize.DetailMapSize];
////		}
////
////		float ratio = (float)terrainSize.TileWidth/(float)terrainSize.DetailMapSize;
////		float unit = 1.0f / (terrainSize.DetailMapSize - 1);
////
////		for (int x = 0; x < terrainSize.DetailMapSize; x++) {
////			for (int z = 0; z < terrainSize.DetailMapSize; z++) {
////				for (var i = 0; i < detailMaps.Length; ++i) {
////					detailMaps[i][z, x] = 0;
////				}
////
////				float normX = x * unit;
////				float normZ = z * unit;
////
////				// Get the steepness value at the normalized coordinate.
////				float angle = terrain.terrainData.GetSteepness (normX, normZ);
////
////				// Steepness is given as an angle, 0..90 degrees. Divide
////				// by 90 to get an alpha blending value in the range 0..1.
////				float frac = angle / 90.0f;
////
////				if (frac < 0.5f) {
////					float worldPosX = (x) * ratio;
////					float worldPosZ = (z) * ratio;
////
////					float noise = m_detailNoise.FractalNoise2D (worldPosX, worldPosZ, 3, m_settings.m_detailFrq, 1.0f);
////
////					if (noise > 0.0f && detailMaps.Length > 0) {
////						//Randomly select what layer to use
////						var layer = detailMaps[Random.Range(0, detailMaps.Length-1)];
////						layer[z, x] = 1;
////					}
////				}
////			}
////		}
////
////		terrain.terrainData.wavingGrassStrength = m_settings.m_wavingGrassStrength;
////		terrain.terrainData.wavingGrassAmount = m_settings.m_wavingGrassAmount;
////		terrain.terrainData.wavingGrassSpeed = m_settings.m_wavingGrassSpeed;
////		terrain.terrainData.wavingGrassTint = m_settings.m_wavingGrassTint;
////		terrain.detailObjectDensity = m_settings.m_detailObjectDensity;
////		terrain.detailObjectDistance = m_settings.m_renderSettings.m_detailObjectDistance;
////		terrain.terrainData.SetDetailResolution(terrainSize.DetailMapSize, m_settings.m_renderSettings.m_detailResolutionPerPatch);
////
////		// assign detail layers to terrain
////		for (var i = 0; i < detailMaps.Length; ++i) {
////			terrain.terrainData.SetDetailLayer (0, 0, i, detailMaps [i]);
////		}
////	}
//}
