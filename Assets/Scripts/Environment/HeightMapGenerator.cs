using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 
/// </summary>
/// <see cref="http://scrawkblog.com/2013/05/15/simple-procedural-terrain-in-unity/">Based on</see>
public class HeightMapGenerator : MonoBehaviour 
{
	// shared settings
	//public TerrainRenderSettings m_renderSettings;
	public TerrainSize terrainSize;

	// height-specific settings
	public TerrainHeightSettings heightSettings;
	
	//Private
	//IntVector2 m_tilePos;
	Terrain terrain;
	PerlinNoise groundNoise, mountainNoise; //treeNoise, detailNoise;
	
	public HeightMapGenerator() {
		//heightSettings = new TerrainSeeds ();
		heightSettings = new TerrainHeightSettings();
		//m_tilePos = new IntVector2 (0, 0);
	}
	
	void Start() {
		terrain = GetComponent<Terrain>();
		if (!IsGenerated()) {
			GenAll ();
		}
	}
	
	public bool IsGenerated() {
		return terrain && terrain.terrainData;
	}
	
	/// <summary>
	/// Randomize the seeds, so the next terrain will actually look different
	/// </summary>
	public void Randomize() {
		heightSettings.m_terrainSeeds.Randomize ();
	}
	
	public void Clear() {
		terrain = GetComponent<Terrain>();
		
		// destroy existing terrain data
		if (terrain.terrainData) {
			DestroyImmediate(terrain.terrainData);
			terrain.terrainData = null;
		}
	}
	
	public void GenAll() {
		terrain = GetComponent<Terrain>();
		Clear ();

		// Create noise generators
		groundNoise = new PerlinNoise(heightSettings.m_terrainSeeds.groundSeed);
		mountainNoise = new PerlinNoise(heightSettings.m_terrainSeeds.mountainSeed);

		// sanity checks!
		if(!Mathf.IsPowerOfTwo(terrainSize.HeightMapSize-1))
		{
			Debug.Log("TerrianGenerator::Start - height map size must be a power of 2 - 1 (e.g. 127, 255, 511 etc.)");
			terrainSize.tileSize = Mathf.ClosestPowerOfTwo(terrainSize.HeightMapSize)+1;
		}
		
		float[,] htmap = new float[terrainSize.HeightMapSize, terrainSize.HeightMapSize];
		
		// compute heigh maps
		GenHeights(htmap);

		
		// start generating terrain data
		TerrainData terrainData = terrain.terrainData = new TerrainData();
		GetComponent<TerrainCollider> ().terrainData = terrain.terrainData;	// also set collider's data

		terrainData.heightmapResolution = terrainSize.tileSize;
		terrainData.SetHeights(0, 0, htmap);
		terrainData.size = new Vector3(terrainSize.tileSize, 1, terrainSize.tileSize);

//		m_terrain.heightmapPixelError = m_settings.m_renderSettings.m_pixelMapError;
//		m_terrain.basemapDistance = m_settings.m_renderSettings.m_baseMapDist;
	}
	
	void GenHeights(float[,] htmap)
	{
		float ratio = (float)terrainSize.tileSize/(float)terrainSize.HeightMapSize;
		
		for(int x = 0; x < terrainSize.HeightMapSize; x++)
		{
			for(int z = 0; z < terrainSize.HeightMapSize; z++)
			{
				float worldPosX = (x)*ratio;
				float worldPosZ = (z)*ratio;
				
				float mountains = Mathf.Max(0.0f, mountainNoise.FractalNoise2D(worldPosX, worldPosZ, 6, heightSettings.mountainFrq, 0.8f));
				
				float plain = groundNoise.FractalNoise2D(worldPosX, worldPosZ, 4, heightSettings.groundFrq, 0.1f) + 0.1f;
				
				htmap[z,x] = plain+mountains;
			}
		}
	}


//	void GenDecorations() {
//		if(!Mathf.IsPowerOfTwo(m_terrainSize.DetailMapSize))
//		{
//			Debug.Log("TerrianGenerator::Start - Detail map size must be power of 2 (e.g. 128, 256, 512 etc.)");
//			m_terrainSize.DetailMapSize = Mathf.ClosestPowerOfTwo(m_terrainSize.DetailMapSize);
//		}
//		treeNoise = new PerlinNoise(heightSettings.m_terrainSeeds.m_treeSeed);
//		detailNoise = new PerlinNoise(heightSettings.m_terrainSeeds.m_detailSeed);
//
//		// re-create prototypes (just to be safe)
//		settings.CreateProtoTypes ();
//
//		terrainData.splatPrototypes = m_settings.m_splatPrototypes;
//		GenAlphaMap(terrainData);
//
//		terrainData.treePrototypes = m_settings.m_treeProtoTypes;
//		GenTreeInstances(m_terrain);
//
//
//		if(m_settings.m_renderSettings.m_detailResolutionPerPatch < 8)
//		{
//			Debug.Log("TerrianGenerator::Start - Detail resolution per patch must be >= 8, changing to 8");
//			m_settings.m_renderSettings.m_detailResolutionPerPatch = 8;
//		}
//		terrainData.detailPrototypes = m_settings.m_detailProtoTypes;
//		GenDetailMap(m_terrain);
//	}
//

//	/// <summary>
//	/// This method decides where how much of each texture can be seen.
//	/// </summary>
//	/// <see cref="https://alastaira.wordpress.com/2013/11/14/procedural-terrain-splatmapping/"/>
//	/// <param name="terrainData">Terrain data.</param>
//	void GenAlphaMap(TerrainData terrainData) 
//	{
//		float[,,] map  = new float[m_terrainSize.AlphaMapSize, m_terrainSize.AlphaMapSize, 2];
//		
//		for(int x = 0; x < m_terrainSize.AlphaMapSize; x++) 
//		{
//			for (int z = 0; z < m_terrainSize.AlphaMapSize; z++) 
//			{
//				// Get the normalized terrain coordinate that
//				// corresponds to the the point.
//				float normX = x * 1.0f / (m_terrainSize.AlphaMapSize - 1);
//				float normZ = z * 1.0f / (m_terrainSize.AlphaMapSize - 1);
//				
//				// Get the steepness value at the normalized coordinate.
//				float angle = terrainData.GetSteepness(normX, normZ);
//				
//				// Steepness is given as an angle, 0..90 degrees. Divide
//				// by 90 to get an alpha blending value in the range 0..1.
//				float frac = angle / 90.0f;
//				if (angle < 30) {
//					frac = 0;
//				}
//				map[z, x, 0] = 1.0f - frac;
//				map[z, x, 1] = frac;
//			}
//		}
//
//		terrainData.alphamapResolution = m_terrainSize.AlphaMapSize;
//		terrainData.SetAlphamaps(0, 0, map);
//	}
//	
//	void GenDetailMap(Terrain terrain)
//	{
//		//each layer is drawn separately so if you have a lot of layers your draw calls will increase 
//
//		// initialize detail maps
//		int[][,] detailMaps = new int[m_settings.m_detailTextures.Length][,];
//		for (var i = 0; i < detailMaps.Length; ++i) {
//			detailMaps[i] = new int[m_terrainSize.DetailMapSize,m_terrainSize.DetailMapSize];
//		}
//
//		float ratio = (float)m_terrainSize.TileSize/(float)m_terrainSize.DetailMapSize;
//		float unit = 1.0f / (m_terrainSize.DetailMapSize - 1);
//
//		for (int x = 0; x < m_terrainSize.DetailMapSize; x++) {
//			for (int z = 0; z < m_terrainSize.DetailMapSize; z++) {
//				for (var i = 0; i < detailMaps.Length; ++i) {
//					detailMaps[i][z, x] = 0;
//				}
//			
//				float normX = x * unit;
//				float normZ = z * unit;
//			
//				// Get the steepness value at the normalized coordinate.
//				float angle = terrain.terrainData.GetSteepness (normX, normZ);
//			
//				// Steepness is given as an angle, 0..90 degrees. Divide
//				// by 90 to get an alpha blending value in the range 0..1.
//				float frac = angle / 90.0f;
//			
//				if (frac < 0.5f) {
//					float worldPosX = (x) * ratio;
//					float worldPosZ = (z) * ratio;
//				
//					float noise = m_detailNoise.FractalNoise2D (worldPosX, worldPosZ, 3, m_settings.m_detailFrq, 1.0f);
//				
//					if (noise > 0.0f && detailMaps.Length > 0) {
//						//Randomly select what layer to use
//						var layer = detailMaps[Random.Range(0, detailMaps.Length-1)];
//						layer[z, x] = 1;
//					}
//				}
//			}
//		}
//		
//		terrain.terrainData.wavingGrassStrength = m_settings.m_wavingGrassStrength;
//		terrain.terrainData.wavingGrassAmount = m_settings.m_wavingGrassAmount;
//		terrain.terrainData.wavingGrassSpeed = m_settings.m_wavingGrassSpeed;
//		terrain.terrainData.wavingGrassTint = m_settings.m_wavingGrassTint;
//		terrain.detailObjectDensity = m_settings.m_detailObjectDensity;
//		terrain.detailObjectDistance = m_settings.m_renderSettings.m_detailObjectDistance;
//		terrain.terrainData.SetDetailResolution(m_terrainSize.DetailMapSize, m_settings.m_renderSettings.m_detailResolutionPerPatch);
//
//		// assign detail layers to terrain
//		for (var i = 0; i < detailMaps.Length; ++i) {
//			terrain.terrainData.SetDetailLayer (0, 0, i, detailMaps [i]);
//		}
//	}
//	
//	void GenTreeInstances(Terrain terrain)
//	{
//		if (m_settings.m_treeProtoTypes.Length == 0) return; 	// no trees available
//		
//		for(int x = 0; x < m_terrainSize.TileSize; x += m_settings.m_treeSpacing) 
//		{
//			for (int z = 0; z < m_terrainSize.TileSize; z += m_settings.m_treeSpacing) 
//			{
//				
//				float unit = 1.0f / (m_terrainSize.TileSize - 1);
//				
//				float offsetX = Random.value * unit * m_settings.m_treeSpacing;
//				float offsetZ = Random.value * unit * m_settings.m_treeSpacing;
//				
//				float normX = x * unit + offsetX;
//				float normZ = z * unit + offsetZ;
//				
//				// Get the steepness value at the normalized coordinate.
//				float angle = terrain.terrainData.GetSteepness(normX, normZ);
//				
//				// Steepness is given as an angle, 0..90 degrees. Divide
//				// by 90 to get an alpha blending value in the range 0..1.
//				float frac = angle / 90.0f;
//				
//				if(frac < 0.5f) //make sure tree are not on steep slopes
//				{
//					float worldPosX = x;
//					float worldPosZ = z;
//					
//					float noise = m_treeNoise.FractalNoise2D(worldPosX, worldPosZ, 3, m_settings.m_treeFrq, 1.0f);
//					float ht = terrain.terrainData.GetInterpolatedHeight(normX, normZ);
//					
//					if(noise > 0.0f && ht < m_terrainSize.tileSize*0.4f)
//					{
//						
//						TreeInstance temp = new TreeInstance();
//						temp.position = new Vector3(normX,ht,normZ);
//						temp.prototypeIndex = Random.Range(0, 3);
//						temp.widthScale = 1;
//						temp.heightScale = 1;
//						temp.color = Color.white;
//						temp.lightmapColor = Color.white;
//						
//						terrain.AddTreeInstance(temp);
//					}
//				}
//				
//			}
//		}
//		
//		terrain.treeDistance = m_settings.m_renderSettings.m_treeDistance;
//		terrain.treeBillboardDistance = m_settings.m_renderSettings.m_treeBillboardDistance;
//		terrain.treeCrossFadeLength = m_settings.m_renderSettings.m_treeCrossFadeLength;
//		terrain.treeMaximumFullLODCount = m_settings.m_renderSettings.m_treeMaximumFullLODCount;
//	}
}


