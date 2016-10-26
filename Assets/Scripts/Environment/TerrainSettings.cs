using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Defines terrian height profile
/// </summary>
[System.Serializable]
public class TerrainHeightSettings {
	// same seeds = same terrain, different seeds = different terrain
	public TerrainSeeds terrainSeeds;
	public float pixelMapError = 6.0f; //A lower pixel error will draw terrain at a higher Level of detail but will be slower
	public float baseMapDist = 1000.0f; //The distance at which the low res base map will be drawn. Decrease to increase performance

	// noise parameters: A higher frq will create smaller, sharper features
	public float groundFrq = 1/800.0f;
	public float  mountainFrq = 1/1200.0f;
	public float  treeFrq = 1/400.0f;
	public float  detailFrq = 1/100.0f;


	public TerrainHeightSettings()
	{
		terrainSeeds = new TerrainSeeds ();
	}
}

/// <summary>
/// The parameters that define the look of a Terrain.
/// </summary>
[System.Serializable]
public class TerrainRenderSettings {
	// textures and decoration
	public SplatSettings[] m_splatMapSettings = new SplatSettings[0];
	public Texture2D[] m_detailTextures = new Texture2D[0];
	public GameObject[] m_treeObjects = new GameObject[0];
	public DetailRenderMode m_detailMode;
	
	// tree settings
	public int m_treeSpacing = 32; //spacing between trees
	
	// detail settings
	public float m_detailObjectDensity = 4.0f; //Creates more dense details within patch
	public float m_wavingGrassStrength = 0.4f;
	public float m_wavingGrassAmount = 0.2f;	
	public float m_wavingGrassSpeed = 0.4f;
	public Color m_wavingGrassTint = Color.white;
	public Color m_grassHealthyColor = Color.white;
	public Color m_grassDryColor = Color.white;

	// Unity's own representation of texture and decoration data
	public SplatPrototype[] m_splatPrototypes { get; private set; }
	public TreePrototype[] m_treeProtoTypes { get; private set; }
	public DetailPrototype[] m_detailProtoTypes { get; private set; }
	
	/// <summary>
	/// Prototypes are used by Unity internally.
	/// This method must be called explicitly prior to generation, if any related variables changed.
	/// </summary>
	public void CreateProtoTypes()
	{
		m_splatPrototypes = new SplatPrototype[m_splatMapSettings.Length];
		
		for (var i = 0; i < m_splatMapSettings.Length; ++i) {
			m_splatPrototypes[i] = new SplatPrototype{
				texture = m_splatMapSettings[i].texture,
				tileSize = m_splatMapSettings[i].tileSize,
				tileOffset = m_splatMapSettings[i].tileOffset
			};
		}
		
		if (m_treeObjects == null) {
			m_treeObjects = new GameObject[0];
		}
		m_treeProtoTypes = new TreePrototype[m_treeObjects.Length];
		
		for (var i = 0; i < m_treeObjects.Length; ++i) {
			m_treeProtoTypes[i] = new TreePrototype();
			m_treeProtoTypes[i].prefab = m_treeObjects[i];
		}
		
		m_detailProtoTypes = new DetailPrototype[m_detailTextures.Length];
		
		for (var i = 0; i < m_detailTextures.Length; ++i) {
			m_detailProtoTypes[i] = new DetailPrototype();
			m_detailProtoTypes[i].prototypeTexture = m_detailTextures[i];
			m_detailProtoTypes[i].renderMode = m_detailMode;
			m_detailProtoTypes[i].healthyColor = m_grassHealthyColor;
			m_detailProtoTypes[i].dryColor = m_grassDryColor;
		}
	}
}

[System.Serializable]
public class TerrainSize {
	// Terrain size specs
	public float maxHeight = 100;
	public int tileSize = 512;
	public int DetailMapSize = 128; //Resolutions of detail (Grass) layers

	public float m_treeBillboardDistance = 400.0f; //The distance at which trees meshes will turn into tree billboards
	public float m_treeCrossFadeLength = 20.0f; //As trees turn to billboards there transform is rotated to match the meshes, a higher number will make this transition smoother
	public float m_treeDistance = 2000.0f; //The distance at which trees will no longer be drawn
	public int m_treeMaximumFullLODCount = 400; //The maximum number of trees that will be drawn in a certain area.
	public int m_detailObjectDistance = 400; //The distance at which details will no longer be drawn
	public int m_detailResolutionPerPatch = 16; //The size of detail patches. A higher number may reduce draw calls as details will be batch in larger patches

	public TerrainSize() {
	}

	/// apparently, alpha map size must basically be the same as height map size (but +1 for the boundaries)
	public int HeightMapSize {
		get { return tileSize+1; }
	}

	/// apparently, alpha map size must basically be the same as height map size
	public int AlphaMapSize {
		get { return tileSize; }
	}
}

[System.Serializable]
public class TerrainSeeds {
	public int masterSeed;
	[HideInInspector] public int groundSeed;
	[HideInInspector] public int mountainSeed;
	[HideInInspector] public int treeSeed;
	[HideInInspector] public int detailSeed;

	public TerrainSeeds() {
		Randomize ();
	}
	
	/// <summary>
	/// Generates an Ok-ish random seed: Multiply tick count with some large prime numbers
	/// </summary>
	public void Randomize() {
		masterSeed = System.Environment.TickCount;
		detailSeed = (int)(masterSeed * 334214467);
		groundSeed = (int)(masterSeed * 413158523);
		mountainSeed = (int)(masterSeed * 613651369);
		treeSeed = (int)(masterSeed * 961748927);
	}
}