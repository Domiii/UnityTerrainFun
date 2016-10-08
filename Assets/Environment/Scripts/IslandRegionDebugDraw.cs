using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using Delaunay.Geo;
using Delaunay;
using System.Linq;


[System.Serializable]
public class IslandRegionDebugDraw {

	IslandRegionGenerator m_islandRegionGenerator;

	/// Defines drawing options for one site
	class SiteDraw {
		public Site site;
		public Vector3 siteCoord;
		public Vector3[] vertices;
	}

	//float m_sitePointSize;
	Color[] m_colors;
	SiteDraw[] m_siteDrawInfo;
	//Vector3[] m_hullVertices;
	static GUIStyle guiStyle;

	private List<LineSegment> m_delaunayTriangulation;

	public IslandRegionDebugDraw(IslandRegionGenerator islandRegionGenerator) {
		m_islandRegionGenerator = islandRegionGenerator;

		var voronoi = islandRegionGenerator.m_voronoi;

		//m_sitePointSize = voronoi.plotBounds.width * .005f;

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

		//var delta = .1f;
		//		m_hullVertices = new Vector3[] {
		//			RelativeVertex(0-delta, 0, 0-delta),
		//			RelativeVertex(0-delta, 0, voronoi.plotBounds.height+delta),
		//			RelativeVertex(voronoi.plotBounds.width+delta, 0, voronoi.plotBounds.height+delta),
		//			RelativeVertex(voronoi.plotBounds.width+delta, 0, 0-delta)
		//		};

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
		if (view != null) {
			Vector3 screenPos = view.camera.WorldToScreenPoint (worldPos);
			Vector2 size = GUI.skin.label.CalcSize (new GUIContent (text));	// TODO: This ignores style
			GUI.Label (new Rect (screenPos.x - (size.x / 2), -screenPos.y + view.position.height - 28, size.x, size.y), text, guiStyle);
		}
		UnityEditor.Handles.EndGUI ();
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
			//var siteData = m_islandRegionGenerator.m_siteData[site.id];

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