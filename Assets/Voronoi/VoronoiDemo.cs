using UnityEngine;
using System.Collections.Generic;
using Delaunay;
using Delaunay.Geo;

public class VoronoiDemo : MonoBehaviour
{
	[SerializeField]
	private int
		m_pointCount = 300;

	private List<Vector2> m_points;
	private float m_mapWidth = 100;
	private float m_mapHeight = 50;
	private Voronoi m_voronoi = null;
	private List<LineSegment> m_edges = null;
	private List<LineSegment> m_spanningTree;
	private List<LineSegment> m_delaunayTriangulation;

	void Awake ()
	{
		Gen ();
	}

	void Update ()
	{
		if (Input.anyKeyDown) {
			Gen ();
		}
	}

	public void Gen ()
	{
		if (m_voronoi != null) {
			m_voronoi.Dispose();
		}

		List<uint> colors = new List<uint> ();
		m_points = new List<Vector2> ();
			
		for (int i = 0; i < m_pointCount; i++) {
			colors.Add (0);
			m_points.Add (new Vector2 (
					UnityEngine.Random.Range (0, m_mapWidth),
					UnityEngine.Random.Range (0, m_mapHeight))
			);
		}

		m_voronoi = new Delaunay.Voronoi (m_points, colors, new Rect (0, 0, m_mapWidth, m_mapHeight));
		m_edges = m_voronoi.VoronoiDiagram ();
			
		//m_spanningTree = v.SpanningTree (KruskalType.MINIMUM);
		//m_delaunayTriangulation = v.DelaunayTriangulation ();
		
		Debug.Log ("Gen done!");
	}

	public void Relax(int nIterations) {
		if (m_voronoi == null) {
			Gen ();
		}
		m_voronoi = m_voronoi.Relax(nIterations);
		m_edges = m_voronoi.VoronoiDiagram ();
		Debug.Log ("Relax done!");
	}

	void OnDrawGizmos ()
	{
		// draw initial points (red)
		Gizmos.color = Color.red;
		if (m_points != null) {
			for (int i = 0; i < m_points.Count; i++) {
				Gizmos.DrawSphere (m_points [i], 0.2f);
			}
		}

		// draw voronoi diagram (white)
		if (m_voronoi != null) {
			Gizmos.color = Color.white;
			for (int i = 0; i< m_edges.Count; i++) {
				Vector2 left = (Vector2)m_edges [i].p0;
				Vector2 right = (Vector2)m_edges [i].p1;
				Gizmos.DrawLine ((Vector3)left, (Vector3)right);
			}

//			// draw one of the voronoi sites (red)
//			Gizmos.color = Color.red;
//			var vertices = m_voronoi.Region(m_points[12]);
//			for (var i = 1; i < vertices.Count; ++i) {
//				Gizmos.DrawLine ((Vector3)vertices[i-1], (Vector3)vertices[i]);
//			}
//			Gizmos.DrawLine ((Vector3)vertices[vertices.Count-1], (Vector3)vertices[0]);
		}
		     
	     
	    // draw Delaunay (magenta)
		if (m_delaunayTriangulation != null) {
			Gizmos.color = Color.magenta;
			for (int i = 0; i< m_delaunayTriangulation.Count; i++) {
				Vector2 left = (Vector2)m_delaunayTriangulation [i].p0;
				Vector2 right = (Vector2)m_delaunayTriangulation [i].p1;
				Gizmos.DrawLine ((Vector3)left, (Vector3)right);
			}
		}
		
		// draw spanning tree (green)
		if (m_spanningTree != null) {
			Gizmos.color = Color.green;
			for (int i = 0; i< m_spanningTree.Count; i++) {
				LineSegment seg = m_spanningTree [i];				
				Vector2 left = (Vector2)seg.p0;
				Vector2 right = (Vector2)seg.p1;
				Gizmos.DrawLine ((Vector3)left, (Vector3)right);
			}
		}

		Gizmos.color = Color.yellow;
		Gizmos.DrawLine (new Vector2 (0, 0), new Vector2 (0, m_mapHeight));
		Gizmos.DrawLine (new Vector2 (0, 0), new Vector2 (m_mapWidth, 0));
		Gizmos.DrawLine (new Vector2 (m_mapWidth, 0), new Vector2 (m_mapWidth, m_mapHeight));
		Gizmos.DrawLine (new Vector2 (0, m_mapHeight), new Vector2 (m_mapWidth, m_mapHeight));
	}
}