using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Delaunay;
using Delaunay.Geo;

/**
 * TODO:
 * -> Generate points, using tile id as seed
 * -> Always partition current set of tiles
 * -> Create PolygonCollider2D and set points for each site
 * -> Use OverlapPoint for determining containment of individual places
 * 
 * @see https://docs.unity3d.com/ScriptReference/PolygonCollider2D.html
 * @see https://docs.unity3d.com/ScriptReference/Collider2D.OverlapPoint.html
 */



/// <see cref="http://www-cs-students.stanford.edu/~amitp/game-programming/polygon-map-generation/">For inspiration</see>
public class MapPartitioning : MonoBehaviour {
	public int partitionCount = 100;
	public MapCell CellPrefab;

	[HideInInspector] public Vector2 dimensions;
	[HideInInspector] public Voronoi voronoi;
	[HideInInspector] public Site[] hullSites;

	/// <summary>
	/// Additional data, stored by site id
	/// </summary>
	[HideInInspector] public MapCell[] cells;
	[HideInInspector] public List<LineSegment> voronoiEdges;
	[HideInInspector] public List<LineSegment> delaunayHullEdges;
	
	//[HideInInspector] public TerrainSettings terrainSettings;
	
	[HideInInspector] public MapDebugDraw debugDraw;


	public MapGenerator Map {
		get {
			return transform.parent.GetComponent<MapGenerator> ();
		}
	}

	void Start () {
		if (voronoi == null) {
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
		for (int i = 0; i < partitionCount; i++) {
			points.Add (new Vector2 (
				Random.Range (0, dimensions.x),
				Random.Range (0, dimensions.y))
			              );
		}
	}

	public void GenerateRegions() {
		List<Vector2> points = new List<Vector2> ();

		dimensions = new Vector2(Map.terrainSize.tileSize, Map.terrainSize.tileSize);

		// 1. generate points
		GeneratePoints (points);

		if (voronoi != null) {
			voronoi.Dispose();
		}

		// 2. generate voronoi regions
		voronoi = new Delaunay.Voronoi (points, new Rect (0, 0, dimensions.x, dimensions.y));

		// 3. relax, to make regions a bit more uniform
		//m_voronoi = m_voronoi.Relax (1);

		// 4a. get sites defining the island boundary
		hullSites = voronoi.GetHullSites ();
		
		// 4b. get dalaunay boundary
		//m_delaunayHullSegments = m_voronoi.GetHullLineSegments ();

		// 4c. get edges for drawing
		voronoiEdges = voronoi.ComputeVoronoiDiagram ();

		// 5. Initialize additional structure information
		InitSiteData ();

		// force a re-draw
		debugDraw = null;
	}

	MapCell CreateCell(Site site) {
		var cell = Instantiate (CellPrefab, transform) as MapCell;
		cell.ResetCell (site);
		return cell;
	}

	/// <summary>
	/// Upon every node visited during the traversal, increase boundary distance by 1!
	/// </summary>
	void _PreprocessSites(Voronoi.SiteTraversalNode<MapCell> current, Voronoi.SiteTraversalNode<MapCell> previous) {
		current.data = CreateCell(current.site);

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
				.Select (neighbor => neighbor.data.distanceFromBoundary + 1)
				.Min ();
		}

		current.data.distanceFromBoundary = distance;
		current.data.neighborSites = current.neighbors.Select (neighborNode => neighborNode.site).ToArray();

		if (current.site.id < 2) {
			//	Debug.Log(current.site.id + ": " + string.Join (", ", current.site.GetNeighborSites().Select (neighborSite => neighborSite.id.ToString()).ToArray()));
		}
	}

	void InitSiteData() {
		// compute every site's distance to the boundary, using Dynamic Programming, starting from boundary
		var roots = hullSites;
		cells = voronoi.TraverseVoronoiBFS<MapCell>(roots, _PreprocessSites)
			.nodesById
			.Select(node => node.data)
			.ToArray();
	}
	#endregion


	#region Debug + Drawing
	void OnDrawGizmos () {
		if (voronoi == null)
			return;

		if (debugDraw == null) {
			debugDraw = new MapDebugDraw(this);
		}
		//debugDraw.Draw ();
	}
	#endregion
}
