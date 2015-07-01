using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 
/// </summary>
/// <see cref="http://scrawkblog.com/2013/05/15/simple-procedural-terrain-in-unity/">Based on</see>
public class TerrainGenerator : MonoBehaviour 
{
	public TerrainSettings m_settings;
	
	//Private
	IntVector2 m_tilePos;
	Terrain m_terrain;
	PerlinNoise m_groundNoise, m_mountainNoise, m_treeNoise, m_detailNoise;
	
	public TerrainGenerator() {
		m_settings = new TerrainSettings ();
		m_tilePos = new IntVector2 (0, 0);
	}
	
	public TerrainGenerator(TerrainSettings settings, IntVector2 tilePos) {
		m_settings = settings;
		m_tilePos = tilePos;
	}
	
	void Start() {
		m_terrain = GetComponent<Terrain>();
		if (!IsGenerated()) {
			Generate ();
		}
	}
	
	public bool IsGenerated() {
		return m_terrain && m_terrain.terrainData;
	}
	
	/// <summary>
	/// Randomize the seeds, so the next terrain will actually look different
	/// </summary>
	public void Randomize() {
		m_settings.m_terrainSeeds.Randomize ();
	}
	
	public void Clear() {
		m_terrain = GetComponent<Terrain>();
		
		// destroy existing terrain data
		if (m_terrain.terrainData) {
			DestroyImmediate(m_terrain.terrainData);
			m_terrain.terrainData = null;
		}
	}
	
	public void Generate() {
		m_terrain = GetComponent<Terrain>();
		Clear ();

		// re-create prototypes (just to be safe)
		m_settings.CreateProtoTypes ();

		// Create noise generators
		m_groundNoise = new PerlinNoise(m_settings.m_terrainSeeds.m_groundSeed);
		m_mountainNoise = new PerlinNoise(m_settings.m_terrainSeeds.m_mountainSeed);
		m_treeNoise = new PerlinNoise(m_settings.m_terrainSeeds.m_treeSeed);
		m_detailNoise = new PerlinNoise(m_settings.m_terrainSeeds.m_detailSeed);

		// sanity checks!
		if(!Mathf.IsPowerOfTwo(m_settings.m_terrainSize.HeightMapSize-1))
		{
			Debug.Log("TerrianGenerator::Start - height map size must be pow2+1 number");
			m_settings.m_terrainSize.TileWidth = Mathf.ClosestPowerOfTwo(m_settings.m_terrainSize.HeightMapSize)+1;
		}
		
		if(!Mathf.IsPowerOfTwo(m_settings.m_terrainSize.DetailMapSize))
		{
			Debug.Log("TerrianGenerator::Start - Detail map size must be pow2 number");
			m_settings.m_terrainSize.DetailMapSize = Mathf.ClosestPowerOfTwo(m_settings.m_terrainSize.DetailMapSize);
		}
		
		if(m_settings.m_renderSettings.m_detailResolutionPerPatch < 8)
		{
			Debug.Log("TerrianGenerator::Start - Detail resolution per patch must be >= 8, changing to 8");
			m_settings.m_renderSettings.m_detailResolutionPerPatch = 8;
		}
		
		float[,] htmap = new float[m_settings.m_terrainSize.HeightMapSize, m_settings.m_terrainSize.HeightMapSize];
		
		// compute heigh maps
		FillHeights(htmap);
		
		
		// start generating terrain data
		TerrainData terrainData = m_terrain.terrainData = new TerrainData();
		GetComponent<TerrainCollider> ().terrainData = m_terrain.terrainData;	// also set collider's data
		
		terrainData.heightmapResolution = m_settings.m_terrainSize.TileWidth;
		terrainData.SetHeights(0, 0, htmap);
		terrainData.size = new Vector3(m_settings.m_terrainSize.TileWidth, m_settings.m_terrainSize.TerrainHeight, m_settings.m_terrainSize.TileWidth);
		terrainData.splatPrototypes = m_settings.m_splatPrototypes;
		terrainData.treePrototypes = m_settings.m_treeProtoTypes;
		terrainData.detailPrototypes = m_settings.m_detailProtoTypes;
		
		FillAlphaMap(terrainData);
		
		m_terrain.transform.position = new Vector3(-m_settings.m_terrainSize.TileWidth*0.5f, 0, -m_settings.m_terrainSize.TileWidth*0.5f);
		m_terrain.heightmapPixelError = m_settings.m_renderSettings.m_pixelMapError;
		m_terrain.basemapDistance = m_settings.m_renderSettings.m_baseMapDist;
		
		//disable this for better frame rate
		m_terrain.castShadows = false;
		
		FillTreeInstances(m_terrain);
		FillDetailMap(m_terrain);
	}
	
	void FillHeights(float[,] htmap)
	{
		float ratio = (float)m_settings.m_terrainSize.TileWidth/(float)m_settings.m_terrainSize.HeightMapSize;
		
		for(int x = 0; x < m_settings.m_terrainSize.HeightMapSize; x++)
		{
			for(int z = 0; z < m_settings.m_terrainSize.HeightMapSize; z++)
			{
				float worldPosX = (x)*ratio;
				float worldPosZ = (z)*ratio;
				
				float mountains = Mathf.Max(0.0f, m_mountainNoise.FractalNoise2D(worldPosX, worldPosZ, 6, m_settings.m_mountainFrq, 0.8f));
				
				float plain = m_groundNoise.FractalNoise2D(worldPosX, worldPosZ, 4, m_settings.m_groundFrq, 0.1f) + 0.1f;
				
				htmap[z,x] = plain+mountains;
			}
		}
	}

	/// <summary>
	/// This method decides where how much of each texture can be seen.
	/// </summary>
	/// <see cref="https://alastaira.wordpress.com/2013/11/14/procedural-terrain-splatmapping/"/>
	/// <param name="terrainData">Terrain data.</param>
	void FillAlphaMap(TerrainData terrainData) 
	{
		float[,,] map  = new float[m_settings.m_terrainSize.AlphaMapSize, m_settings.m_terrainSize.AlphaMapSize, 2];
		
		for(int x = 0; x < m_settings.m_terrainSize.AlphaMapSize; x++) 
		{
			for (int z = 0; z < m_settings.m_terrainSize.AlphaMapSize; z++) 
			{
				// Get the normalized terrain coordinate that
				// corresponds to the the point.
				float normX = x * 1.0f / (m_settings.m_terrainSize.AlphaMapSize - 1);
				float normZ = z * 1.0f / (m_settings.m_terrainSize.AlphaMapSize - 1);
				
				// Get the steepness value at the normalized coordinate.
				float angle = terrainData.GetSteepness(normX, normZ);
				
				// Steepness is given as an angle, 0..90 degrees. Divide
				// by 90 to get an alpha blending value in the range 0..1.
				float frac = angle / 90.0f;
				if (angle < 30) {
					frac = 0;
				}
				map[z, x, 0] = 1.0f - frac;
				map[z, x, 1] = frac;
			}
		}

		terrainData.alphamapResolution = m_settings.m_terrainSize.AlphaMapSize;
		terrainData.SetAlphamaps(0, 0, map);
	}
	
	void FillDetailMap(Terrain terrain)
	{
		//each layer is drawn separately so if you have a lot of layers your draw calls will increase 

		// initialize detail maps
		int[][,] detailMaps = new int[m_settings.m_detailTextures.Length][,];
		for (var i = 0; i < detailMaps.Length; ++i) {
			detailMaps[i] = new int[m_settings.m_terrainSize.DetailMapSize,m_settings.m_terrainSize.DetailMapSize];
		}

		float ratio = (float)m_settings.m_terrainSize.TileWidth/(float)m_settings.m_terrainSize.DetailMapSize;

		for (int x = 0; x < m_settings.m_terrainSize.DetailMapSize; x++) {
			for (int z = 0; z < m_settings.m_terrainSize.DetailMapSize; z++) {
				for (var i = 0; i < detailMaps.Length; ++i) {
					detailMaps[i][z, x] = 0;
				}
			
				float unit = 1.0f / (m_settings.m_terrainSize.DetailMapSize - 1);
			
				float normX = x * unit;
				float normZ = z * unit;
			
				// Get the steepness value at the normalized coordinate.
				float angle = terrain.terrainData.GetSteepness (normX, normZ);
			
				// Steepness is given as an angle, 0..90 degrees. Divide
				// by 90 to get an alpha blending value in the range 0..1.
				float frac = angle / 90.0f;
			
				if (frac < 0.5f) {
					float worldPosX = (x) * ratio;
					float worldPosZ = (z) * ratio;
				
					float noise = m_detailNoise.FractalNoise2D (worldPosX, worldPosZ, 3, m_settings.m_detailFrq, 1.0f);
				
					if (noise > 0.0f) {
						float rnd = Random.value;
						//Randomly select what layer to use
						if (rnd < 0.33f)
							detailMaps[0][z, x] = 1;
						else if (rnd < 0.66f)
							detailMaps[1][z, x] = 1;
						else
							detailMaps[2][z, x] = 1;
					}
				}
			}
		}
		
		terrain.terrainData.wavingGrassStrength = m_settings.m_wavingGrassStrength;
		terrain.terrainData.wavingGrassAmount = m_settings.m_wavingGrassAmount;
		terrain.terrainData.wavingGrassSpeed = m_settings.m_wavingGrassSpeed;
		terrain.terrainData.wavingGrassTint = m_settings.m_wavingGrassTint;
		terrain.detailObjectDensity = m_settings.m_detailObjectDensity;
		terrain.detailObjectDistance = m_settings.m_renderSettings.m_detailObjectDistance;
		terrain.terrainData.SetDetailResolution(m_settings.m_terrainSize.DetailMapSize, m_settings.m_renderSettings.m_detailResolutionPerPatch);

		// assign detail layers to terrain
		for (var i = 0; i < detailMaps.Length; ++i) {
			terrain.terrainData.SetDetailLayer (0, 0, 0, detailMaps [i]);
		}
	}
	
	
	
	void FillTreeInstances(Terrain terrain)
	{
		if (m_settings.m_treeProtoTypes.Length == 0) return; 	// no trees available
		
		for(int x = 0; x < m_settings.m_terrainSize.TileWidth; x += m_settings.m_treeSpacing) 
		{
			for (int z = 0; z < m_settings.m_terrainSize.TileWidth; z += m_settings.m_treeSpacing) 
			{
				
				float unit = 1.0f / (m_settings.m_terrainSize.TileWidth - 1);
				
				float offsetX = Random.value * unit * m_settings.m_treeSpacing;
				float offsetZ = Random.value * unit * m_settings.m_treeSpacing;
				
				float normX = x * unit + offsetX;
				float normZ = z * unit + offsetZ;
				
				// Get the steepness value at the normalized coordinate.
				float angle = terrain.terrainData.GetSteepness(normX, normZ);
				
				// Steepness is given as an angle, 0..90 degrees. Divide
				// by 90 to get an alpha blending value in the range 0..1.
				float frac = angle / 90.0f;
				
				if(frac < 0.5f) //make sure tree are not on steep slopes
				{
					float worldPosX = x;
					float worldPosZ = z;
					
					float noise = m_treeNoise.FractalNoise2D(worldPosX, worldPosZ, 3, m_settings.m_treeFrq, 1.0f);
					float ht = terrain.terrainData.GetInterpolatedHeight(normX, normZ);
					
					if(noise > 0.0f && ht < m_settings.m_terrainSize.TerrainHeight*0.4f)
					{
						
						TreeInstance temp = new TreeInstance();
						temp.position = new Vector3(normX,ht,normZ);
						temp.prototypeIndex = Random.Range(0, 3);
						temp.widthScale = 1;
						temp.heightScale = 1;
						temp.color = Color.white;
						temp.lightmapColor = Color.white;
						
						terrain.AddTreeInstance(temp);
					}
				}
				
			}
		}
		
		terrain.treeDistance = m_settings.m_renderSettings.m_treeDistance;
		terrain.treeBillboardDistance = m_settings.m_renderSettings.m_treeBillboardDistance;
		terrain.treeCrossFadeLength = m_settings.m_renderSettings.m_treeCrossFadeLength;
		terrain.treeMaximumFullLODCount = m_settings.m_renderSettings.m_treeMaximumFullLODCount;
	}
}


