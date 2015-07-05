using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Delaunay;
using Delaunay.Geo;


[System.Serializable]
public class IslandRegionDebugDraw {
	
	IslandRegionGenerator m_islandRegionGenerator;

	float m_sitePointSize;
	List<Vector3> m_sitePoints;
	/// <summary>
	/// Set of vertices defining the region boundary hull.
	/// </summary>
	Vector3[] m_hullVertices;
	/// <summary>
	/// Set of all vertices touching the region boundary hull.
	/// </summary>
	Vector3[][] m_hullSiteVertices;
	Vector3[][] m_siteVertices;

	public IslandRegionDebugDraw(IslandRegionGenerator islandRegionGenerator) {
		m_islandRegionGenerator = islandRegionGenerator;

		var voronoi = islandRegionGenerator.m_voronoi;

		m_sitePointSize = voronoi.plotBounds.width * .005f;

		m_sitePoints = new List<Vector3> ();
		m_siteVertices = new Vector3[voronoi.Sites.Count][];
		for (int i = 0; i < voronoi.Sites.Count; i++) {
			var site = voronoi.Sites[i];
			var position = site.Coord;
			m_sitePoints.Add(RelativeVertex(position.x, 0, position.y));

			var vertices = voronoi.GetRegion(site.id).Select (v => RelativeVertex(v.x, 0, v.y));
			m_siteVertices[i] = vertices.ToArray();
		}

		m_hullSiteVertices = islandRegionGenerator.m_hullSites.Select (site => {
			var vertices = voronoi.GetRegion(site.id).Select (v => RelativeVertex(v.x, 0, v.y));
			return vertices.ToArray();
		}).ToArray();
//		m_hullSiteVertices = islandRegionGenerator.m_delaunayHullSegments.Select(line => {
//			return new [] {
//				RelativeVertex(line.p0.Value.x, 0, line.p0.Value.y),
//				RelativeVertex(line.p1.Value.x, 0, line.p1.Value.y)};
//		}).ToArray();

		var delta = .1f;
		m_hullVertices = new Vector3[] {
			RelativeVertex(0-delta, 0, 0-delta),
			RelativeVertex(0-delta, 0, voronoi.plotBounds.height+delta),
			RelativeVertex(voronoi.plotBounds.width+delta, 0, voronoi.plotBounds.height+delta),
			RelativeVertex(voronoi.plotBounds.width+delta, 0, 0-delta)
		};
	}

	private Vector3 RelativeVertex(float x, float y, float z) {
		return new Vector3(x, y, z) + m_islandRegionGenerator.RegionOffset + Vector3.up * 400;
	}

	public void Draw() {
		if (m_sitePoints == null) return;

		// draw initial points (white)
		Gizmos.color = Color.white;
		for (int i = 0; i < m_sitePoints.Count; i++) {
			Gizmos.DrawSphere (m_sitePoints[i], m_sitePointSize);
		}
		
		// draw voronoi diagram (white)
		Gizmos.color = Color.white;
		for (int i = 0; i< m_siteVertices.Length; i++) {
			var vertices = m_siteVertices[i];
			for (var j = 1; j < vertices.Length; ++j) {
				Gizmos.DrawLine (vertices[j-1], vertices[j]);
			}
			Gizmos.DrawLine (vertices[vertices.Length-1], vertices[0]);
		}

		// draw hull sites (blue)
		Gizmos.color = Color.blue;
		for (int i = 0; i< m_hullSiteVertices.Length; i++) {
			var vertices = m_hullSiteVertices[i];
			for (var j = 1; j < vertices.Length; ++j) {
				Gizmos.DrawLine (vertices[j-1], vertices[j]);
			}
			Gizmos.DrawLine (vertices[vertices.Length-1], vertices[0]);
		}
		
		// draw bounding box (yellow)
//		Gizmos.color = Color.yellow;
//		Gizmos.DrawLine (m_boundaryVertices[0], m_boundaryVertices[1]);
//		Gizmos.DrawLine (m_boundaryVertices[1], m_boundaryVertices[2]);
//		Gizmos.DrawLine (m_boundaryVertices[2], m_boundaryVertices[3]);
//		Gizmos.DrawLine (m_boundaryVertices[3], m_boundaryVertices[0]);
	}
}

/// <summary>
/// Data associated with each Voronoi Site.
/// </summary>
[System.Serializable]
public class SiteData {
	public readonly Site Site;
	[HideInInspector] public int DistanceFromBoundary;

	public SiteData(Site site) {
		Site = site;
	}
}


/// <see cref="http://www-cs-students.stanford.edu/~amitp/game-programming/polygon-map-generation/">For inspiration</see>
[RequireComponent(typeof(Terrain), typeof(TerrainGenerator))]
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

	public Vector3 RegionOffset {
		get {
			return transform.position;
		}
	}

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
		m_voronoi = m_voronoi.Relax (1);

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

	void InitSiteData() {
		// create SiteData set
		m_siteData = new SiteData[m_voronoi.Sites.Count];
		foreach (var site in m_voronoi.Sites) {
			m_siteData[site.id] = new SiteData(site);
		}

		// compute every site's distance to the boundary, using Dynamic Programming, starting from boundary

		foreach (var site in m_hullSites) {
			m_siteData[site.id].DistanceFromBoundary = 0;
		}

	}
	
	void OnDrawGizmos () {
		if (m_voronoi == null)
			return;

		if (m_debugDraw == null) {
			m_debugDraw = new IslandRegionDebugDraw(this);
		}
		m_debugDraw.Draw ();
	}
}
