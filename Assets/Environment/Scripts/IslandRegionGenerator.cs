using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Delaunay;
using Delaunay.Geo;


[System.Serializable]
public class IslandRegionDebugDraw {
	
	IslandRegionGenerator m_islandRegionGenerator;

	/// Defines drawing options for one site
	class SiteDraw {
		public Site site;
		public Vector3 siteCoord;
		public Vector3[] vertices;
	}

	float m_sitePointSize;
	Color[] m_colors;
	SiteDraw[] m_siteDrawInfo;
	Vector3[] m_hullVertices;
	static GUIStyle guiStyle;
	
	private List<LineSegment> m_delaunayTriangulation;

	public IslandRegionDebugDraw(IslandRegionGenerator islandRegionGenerator) {
		m_islandRegionGenerator = islandRegionGenerator;

		var voronoi = islandRegionGenerator.m_voronoi;

		m_sitePointSize = voronoi.plotBounds.width * .005f;

		m_siteDrawInfo = new SiteDraw[voronoi.Sites.Count];
		
		m_delaunayTriangulation = voronoi.ComputeDelaunayTriangulation ();

		for (int i = 0; i < voronoi.Sites.Count; i++) {
			var site = voronoi.Sites[i];

			var coord = site.Coord;
			var vertices = voronoi
				.GetRegion(site.id)
				.Select (v => RegionToWorldCoordinate(v));

			m_siteDrawInfo[i] = new SiteDraw() {
				site = site,
				siteCoord = RegionToWorldCoordinate(coord),
				vertices = vertices.ToArray()
			};
		}

//		m_hullSiteVertices = islandRegionGenerator.m_hullSites.Select (site => {
//			var vertices = voronoi.GetRegion(site.id).Select (v => RegionToWorldCoordinate(v));
//			return vertices.ToArray();
//		}).ToArray();
//		m_hullSiteVertices = islandRegionGenerator.m_delaunayHullSegments.Select(line => {
//			return new [] {
//				RegionToWorldCoordinate(line.p0.Value),
//				RegionToWorldCoordinate(line.p1.Value)};
//		}).ToArray();

		var delta = .1f;
		m_hullVertices = new Vector3[] {
			RelativeVertex(0-delta, 0, 0-delta),
			RelativeVertex(0-delta, 0, voronoi.plotBounds.height+delta),
			RelativeVertex(voronoi.plotBounds.width+delta, 0, voronoi.plotBounds.height+delta),
			RelativeVertex(voronoi.plotBounds.width+delta, 0, 0-delta)
		};

		InitGui ();
	}

	private void InitGui() {
		// set GUI style
		guiStyle = new GUIStyle ();
		guiStyle.fontSize = 30;

		// produce color array
		m_colors = new []{
			Color.gray,
			Color.green,
			Color.cyan,
			Color.yellow,
			Color.blue,
			Color.magenta,
			Color.red
		};
	}

	private Color GetColor(int index) {
		return m_colors[index % m_colors.Length];
	}
	
	private Vector3 RegionToWorldCoordinate(Vector2 v) {
		return m_islandRegionGenerator.RegionToWorldTransform (v);
	}

	/// <summary>
	/// Returns two world vectors to represent the given region edge
	/// </summary>
	private Vector3[] RegionToWorldCoordinate(Edge edge) {
		return new [] {
			RegionToWorldCoordinate(edge.leftSite.Coord),
			RegionToWorldCoordinate(edge.rightSite.Coord)
		};
	}

	private Vector3 RelativeVertex(float x, float y, float z) {
		return m_islandRegionGenerator.RegionToWorldTransform (x, y, z);
	}

	/// <summary>
	/// Draws the given string.
	/// TODO: Move this to editor-only code.
	/// </summary>
	/// <see cref="https://gist.github.com/Arakade/9dd844c2f9c10e97e3d0"/>
	static void DrawString(string text, Vector3 worldPos, Color? color = null) {
		UnityEditor.Handles.BeginGUI();
		if (color.HasValue) guiStyle.normal.textColor = color.Value;

		var view = UnityEditor.SceneView.currentDrawingSceneView;
		Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);
		Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));	// TODO: This ignores style
		GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height - 28, size.x, size.y), text, guiStyle);
		UnityEditor.Handles.EndGUI();
	}
	
	public void Draw() {
		if (m_islandRegionGenerator == null) return;
		
		// draw voronoi diagram (white)
		for (int i = 0; i< m_siteDrawInfo.Length; i++) {
			var drawInfo = m_siteDrawInfo[i];
			var site = drawInfo.site;
			var siteData = m_islandRegionGenerator.m_siteData[site.id];

			Gizmos.color = GetColor(siteData.DistanceFromBoundary);

			var vertices = drawInfo.vertices;
			for (var j = 1; j < vertices.Length; ++j) {
				Gizmos.DrawLine (vertices[j-1], vertices[j]);
			}
			Gizmos.DrawLine (vertices[vertices.Length-1], vertices[0]);
		}
		
		// draw local Delaunay graph of a few nodes
		{
			Gizmos.color = Color.magenta;
			var drawInfo = m_siteDrawInfo [0];
			var site = drawInfo.site;
			
			var edges = site.edges.Select(edge => RegionToWorldCoordinate(edge));
			foreach (var edge in edges) {
				Gizmos.DrawLine(edge[0], edge[1]);
			}
		}

		// draw Delaunay triangulation
		Gizmos.color = Color.cyan;
		{
			foreach (var edge in m_delaunayTriangulation) {
				Gizmos.DrawLine(RegionToWorldCoordinate(edge.p0.Value), RegionToWorldCoordinate(edge.p1.Value));
			}
		}
		
		// draw initial points (white)
		Gizmos.color = Color.white;
		for (int i = 0; i < m_siteDrawInfo.Length; i++) {
			var drawInfo = m_siteDrawInfo[i];
			var site = drawInfo.site;
			var siteData = m_islandRegionGenerator.m_siteData[site.id];
			
			//Gizmos.DrawSphere (drawInfo.siteCoord, m_sitePointSize);
			//DrawString(drawInfo.site.id.ToString(), drawInfo.siteCoord, Gizmos.color);
			DrawString(site.id.ToString(), drawInfo.siteCoord, Gizmos.color);
			//DrawString(siteData.NeighborSites.Length.ToString(), drawInfo.siteCoord, Gizmos.color);
		}

		// TODO: draw random neighbor set

		
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
	[HideInInspector] public Site[] NeighborSites;

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
			Debug.Log(current.site.id + ": " + string.Join (", ", current.site.GetNeighborSites().Select (neighborSite => neighborSite.id.ToString()).ToArray()));
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
