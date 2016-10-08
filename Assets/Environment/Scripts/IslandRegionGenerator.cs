using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Delaunay;
using Delaunay.Geo;

/// <summary>
/// Data associated with each Voronoi Site.
/// </summary>
[System.Serializable]
public class SiteData {
	public readonly Site Site;
	[HideInInspector] public int DistanceFromBoundary;
	[HideInInspector] public Site[] NeighborSites;

	public SiteData(Site site) {
		Site = site;
	}
}


/// <see cref="http://www-cs-students.stanford.edu/~amitp/game-programming/polygon-map-generation/">For inspiration</see>
[RequireComponent(typeof(Terrain), typeof(TerrainGenerator))]
[ExecuteInEditMode]
public class IslandRegionGenerator : MonoBehaviour {
	public int m_siteCount = 100;

	[HideInInspector] public Vector2 m_dimensions;
	[HideInInspector] public Voronoi m_voronoi;
	[HideInInspector] public Site[] m_hullSites;
	/// <summary>
	/// Additional data, stored by site id
	/// </summary>
	[HideInInspector] public SiteData[] m_siteData;
	[HideInInspector] public List<LineSegment> m_voronoiEdges;
	[HideInInspector] public List<LineSegment> m_delaunayHullSegments;
	
	[HideInInspector] public TerrainSettings m_terrainSettings;
	
	[HideInInspector] public IslandRegionDebugDraw m_debugDraw;
	
	IslandRegionGenerator() {
		m_terrainSettings = new TerrainSettings();
	}
	
	IslandRegionGenerator(TerrainSettings terrainSettings) {
		m_terrainSettings = terrainSettings;
	}

	void Start () {
		if (m_voronoi == null) {
			GenerateAll();
		}
	}

	void Update () {
		
	}


	#region Coordinates + Transformations
	public Vector3 RegionOffset {
		get {
			return transform.position;
		}
	}

	public Vector3 RegionToWorldTransform(Vector2 v) {
		return RegionToWorldTransform(v.x, 0, v.y);
	}
	
	public Vector3 RegionToWorldTransform(float x, float y, float z) {
		return new Vector3(x, y, z) + RegionOffset + Vector3.up * 400;
	}
	#endregion


	#region Region Generation
	public void GenerateAll() {
		GenerateRegions ();
	}
	
	public void GeneratePoints(List<Vector2> points) {
		for (int i = 0; i < m_siteCount; i++) {
			points.Add (new Vector2 (
				UnityEngine.Random.Range (0, m_dimensions.x),
				UnityEngine.Random.Range (0, m_dimensions.y))
			              );
		}
	}

	public void GenerateRegions() {
		List<Vector2> points = new List<Vector2> ();

		m_dimensions = new Vector2(TerrainMgr.Instance.TerrainSize.TileWidth, TerrainMgr.Instance.TerrainSize.TileWidth);

		// 1. generate points
		GeneratePoints (points);

		if (m_voronoi != null) {
			m_voronoi.Dispose();
		}

		// 2. generate voronoi regions
		m_voronoi = new Delaunay.Voronoi (points, new Rect (0, 0, m_dimensions.x, m_dimensions.y));

		// 3. relax, to make regions a bit more uniform
		//m_voronoi = m_voronoi.Relax (1);

		// 4a. get sites defining the island boundary
		m_hullSites = m_voronoi.GetHullSites ();
		
		// 4b. get dalaunay boundary
		//m_delaunayHullSegments = m_voronoi.GetHullLineSegments ();

		// 4c. get edges for drawing
		m_voronoiEdges = m_voronoi.ComputeVoronoiDiagram ();

		// 5. Initialize additional structure information
		InitSiteData ();

		// force a re-draw
		m_debugDraw = null;
	}

	/// <summary>
	/// Upon every node visited during the traversal, increase boundary distance by 1!
	/// </summary>
	void _PreprocessSite(Voronoi.SiteTraversalNode<SiteData> current, Voronoi.SiteTraversalNode<SiteData> previous) {
		current.data = new SiteData (current.site);

		// the next node's distance to the boundary is the min of all previous distances + 1
		var visitedNeighbors = current.neighbors
			.Where (neighbor => neighbor.data != null);

		int distance;
		if (!visitedNeighbors.Any ()) {
			// current node is a root
			distance = 1;
		}
		else {
			// current node is not a root
			distance = visitedNeighbors
				.Select (neighbor => neighbor.data.DistanceFromBoundary + 1)
				.Min ();
		}

		current.data.DistanceFromBoundary = distance;
		current.data.NeighborSites = current.neighbors.Select (neighborNode => neighborNode.site).ToArray();

		if (current.site.id < 2) {
			//	Debug.Log(current.site.id + ": " + string.Join (", ", current.site.GetNeighborSites().Select (neighborSite => neighborSite.id.ToString()).ToArray()));
		}
	}

	void InitSiteData() {
		// compute every site's distance to the boundary, using Dynamic Programming, starting from boundary
		var roots = m_hullSites;
		m_siteData = m_voronoi.TraverseVoronoiBFS<SiteData>(roots, _PreprocessSite)
			.nodesById
			.Select(node => node.data)
			.ToArray();
	}
	#endregion


	#region Debug + Drawing
	void OnDrawGizmos () {
		if (m_voronoi == null)
			return;

		if (m_debugDraw == null) {
			m_debugDraw = new IslandRegionDebugDraw(this);
		}
		m_debugDraw.Draw ();
	}
	#endregion
}
