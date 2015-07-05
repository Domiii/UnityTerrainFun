using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Delaunay;
using Delaunay.Geo;


[System.Serializable]
public class IslandRegionDebugDraw {
	
	Voronoi m_voronoi;
	Vector3 m_regionOffset;
	List<Vector3> m_sitePoints;
	float m_sitePointSize;
	Vector3[] m_boundaryVertices;
	List<List<Vector3>> m_siteVertices;

	public IslandRegionDebugDraw(Voronoi voronoiRegions, List<LineSegment> voronoiEdges, Vector3 regionOffset) {
		if (voronoiRegions == null) return;

		m_voronoi = voronoiRegions;
		m_regionOffset = regionOffset;
		m_sitePointSize = m_voronoi.plotBounds.width * .002f;

		m_sitePoints = new List<Vector3> ();
		m_siteVertices = new List<List<Vector3>> ();
		for (int i = 0; i < voronoiRegions.Sites.Count; i++) {
			var site = voronoiRegions.Sites[i];
			var position = site.Coord;
			m_sitePoints.Add(RelativeVertex(position.x, 0, position.y));

			var vertices = voronoiRegions.GetRegion(site.id).Select (v => RelativeVertex(v.x, 0, v.y));
			m_siteVertices.Add(new List<Vector3>(vertices));
		}

		var delta = .1f;
		m_boundaryVertices = new Vector3[] {
			RelativeVertex(0-delta, 0, 0-delta),
			RelativeVertex(0-delta, 0, m_voronoi.plotBounds.height+delta),
			RelativeVertex(m_voronoi.plotBounds.width+delta, 0, m_voronoi.plotBounds.height+delta),
			RelativeVertex(m_voronoi.plotBounds.width+delta, 0, 0-delta)
		};
	}

	private Vector3 RelativeVertex(float x, float y, float z) {
		return new Vector3(x, y, z) + m_regionOffset;
	}

	public void Draw() {
		if (m_sitePoints == null) return;

		// draw initial points (red)
		Gizmos.color = Color.red;
		for (int i = 0; i < m_sitePoints.Count; i++) {
			Gizmos.DrawSphere (m_sitePoints[i], m_sitePointSize);
		}
		
		// draw voronoi diagram (white)
		Gizmos.color = Color.white;
		for (int i = 0; i< m_siteVertices.Count; i++) {
			var vertices = m_siteVertices[i];
			for (var j = 1; j < vertices.Count; ++j) {
				Gizmos.DrawLine (vertices[j-1], vertices[j]);
			}
		}
		
		//			// draw one of the voronoi sites (red)
		//			Gizmos.color = Color.red;
		//			var vertices = m_voronoi.Region(m_points[12]);
		//			for (var i = 1; i < vertices.Count; ++i) {
		//				Gizmos.DrawLine ((Vector3)vertices[i-1], (Vector3)vertices[i]);
		//			}
		//			Gizmos.DrawLine ((Vector3)vertices[vertices.Count-1], (Vector3)vertices[0]);
		
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
	public readonly int DistanceFromBoundary;


	public SiteData(Site site, int distanceFromBoundary) {
		Site = site;
		DistanceFromBoundary = distanceFromBoundary;
	}
}


/// <see cref="http://www-cs-students.stanford.edu/~amitp/game-programming/polygon-map-generation/">For inspiration</see>
[RequireComponent(typeof(Terrain))]
public class IslandRegionGenerator : MonoBehaviour {
	public int m_siteCount = 100;

	[HideInInspector] public Vector2 m_dimensions;
	[HideInInspector] public Voronoi m_voronoi;
	[HideInInspector] public SiteData[] m_siteData;
	[HideInInspector] public List<LineSegment> m_voronoiEdges;
	
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

		// 3b. Compute additional site data

		// 4. get edges for drawing
		m_voronoiEdges = m_voronoi.ComputeVoronoiDiagram ();

		// force a re-draw
		m_debugDraw = null;
	}

	void InitSiteData() {
		m_siteData = new SiteData[m_voronoi.Sites.Count];
// TODO: Init!
//		// compute every site's distance to the boundary, using Dynamic Programming, starting from boundary
//		foreach (var site in m_voronoi.bou
	}
	
	void OnDrawGizmos () {
		if (m_voronoi == null)
			return;

		if (m_debugDraw == null) {
			var terrain = GetComponent<TerrainGenerator>();
			Vector3 regionOffset = terrain.transform.position;
			m_debugDraw = new IslandRegionDebugDraw(m_voronoi, m_voronoiEdges, regionOffset);
		}
		m_debugDraw.Draw ();
	}
}
