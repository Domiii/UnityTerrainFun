/*
 * The author of this software is Steven Fortune.  Copyright (c) 1994 by AT&T
 * Bell Laboratories.
 * Permission to use, copy, modify, and distribute this software for any
 * purpose without fee is hereby granted, provided that this entire notice
 * is included in all copies of any software which is or includes a copy
 * or modification of this software and in all copies of the supporting
 * documentation for such software.
 * THIS SOFTWARE IS BEING PROVIDED "AS IS", WITHOUT ANY EXPRESS OR IMPLIED
 * WARRANTY.  IN PARTICULAR, NEITHER THE AUTHORS NOR AT&T MAKE ANY
 * REPRESENTATION OR WARRANTY OF ANY KIND CONCERNING THE MERCHANTABILITY
 * OF THIS SOFTWARE OR ITS FITNESS FOR ANY PARTICULAR PURPOSE.
 */

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Delaunay.Geo;
using Delaunay.Utils;
using Delaunay.LR;

namespace Delaunay
{
	public sealed class Voronoi: Utils.IDisposable
	{
		private SiteList _sites;
		private Dictionary <Vector2,Site> _sitesIndexedByLocation;
		private Site[] _sitesIndexedById;
		private List<Triangle> _triangles;
		private List<Edge> _edges;

		
		// TODO generalize this so it doesn't have to be a rectangle;
		// then we can make the fractal voronois-within-voronois
		private Rect _plotBounds;
		public Rect plotBounds {
			get { return _plotBounds;}
		}

		#region Construction + Initialization
		public Voronoi (List<Vector2> points, Rect plotBounds)
		{
			Reset (points, plotBounds);
		}

		private void Reset(List<Vector2> points, Rect plotBounds)
		{
			_sites = new SiteList ();
			_sitesIndexedByLocation = new Dictionary <Vector2,Site> (); // XXX: Used to be Dictionary(true) -- weak refs. 
			_sitesIndexedById = new Site[points.Count];
			AddSites (points);
			_plotBounds = plotBounds;
			_triangles = new List<Triangle> ();
			_edges = new List<Edge> ();
			FortunesAlgorithm ();
		}
		
		private void AddSites (List<Vector2> points)
		{
			int length = points.Count;
			for (int i = 0; i < length; ++i) {
				AddSite (points [i], (uint)i, (uint)i);		// for now, index and id happen to be the same
			}
		}
		
		private void AddSite (Vector2 p, uint index, uint id)
		{
			if (_sitesIndexedByLocation.ContainsKey (p))
				return; // Prevent duplicate site! (Adapted from https://github.com/nodename/as3delaunay/issues/1)

			float weight = UnityEngine.Random.value * 100f;

			Site site = Site.Create (p, (uint)index, weight, id);
			_sites.Add (site);
			_sitesIndexedByLocation [p] = site;
			_sitesIndexedById[(int)id] = site;
		}
		#endregion


		public List<Edge> Edges
		{
			get { return _edges; }
		}
		
		public SiteList Sites
		{
			get { return _sites; }
		}
		
		public Site[] SitesById
		{
			get { return _sitesIndexedById; }
		}


		public T[] CreateSiteArrayById<T>() {
			return new T[_sites.Count];
		}
          
		/// <summary>
		/// Get vertices of the site containing the given point.
		/// </summary>
		/// <param name="p"></param>
		public List<Vector2> GetRegion (uint id)
		{
			if (_sitesIndexedById.Length <= id)
				return new List<Vector2> ();

			Site site = _sitesIndexedById[(int)id];
			return site.GetRegion (_plotBounds);
		}

		// TODO: bug: if you call this before you call region(), something goes wrong :(
		public List<Site> GetNeighborSitesForSite (uint id)
		{
			if (_sitesIndexedById.Length <= id)
				return new List<Site> ();

			Site site = _sitesIndexedById [(int)id];
			return site.GetNeighborSites ();
		}

		public List<Circle> GetCircles ()
		{
			return _sites.GetCircles ();
		}

		public List<LineSegment> GetVoronoiBoundaryForSite (uint id)
		{
			return DelaunayHelpers.VisibleLineSegments (DelaunayHelpers.SelectEdgesForSitePoint (id, _edges));
		}

		public List<LineSegment> GetDelaunayLinesForSite (uint id)
		{
			return DelaunayHelpers.DelaunayLinesForEdges (DelaunayHelpers.SelectEdgesForSitePoint (id, _edges));
		}
		
		public List<LineSegment> ComputeVoronoiDiagram ()
		{
			return DelaunayHelpers.VisibleLineSegments (_edges);
		}
		
		public List<LineSegment> ComputeDelaunayTriangulation (/*BitmapData keepOutMask = null*/)
		{
			return DelaunayHelpers.DelaunayLinesForEdges (DelaunayHelpers.SelectNonIntersectingEdges (/*keepOutMask,*/_edges));
		}
		
		public List<LineSegment> GetHullLineSegments ()
		{
			return DelaunayHelpers.DelaunayLinesForEdges (GetHullEdges ());
		}
		
		private List<Edge> GetHullEdges ()
		{
			return _edges.FindAll (delegate (Edge edge) {
				return (edge.IsPartOfConvexHull ());
			});
		}
		
		/// <summary>
		/// Returns the set of all sites touching the convex hull of the delaunay graph, in order.
		/// NOTE: This does not include all sites touching the boundary.
		/// </summary>
		public List<Site> GetConvexHullSitesInOrder ()
		{
			List<Edge> hullEdges = GetHullEdges ();
			
			List<Site> sites = new List<Site> ();
			if (hullEdges.Count == 0) {
				return sites;
			}
			
			EdgeReorderer reorderer = new EdgeReorderer (hullEdges, VertexOrSite.SITE);
			hullEdges = reorderer.edges;
			List<Side> orientations = reorderer.edgeOrientations;
			reorderer.Dispose ();
			
			Side orientation;

			int n = hullEdges.Count;
			for (int i = 0; i < n; ++i) {
				Edge edge = hullEdges [i];
				orientation = orientations [i];
				sites.Add (edge.Site (orientation));
			}
			return sites;
		}
		
		/// <summary>
		/// Returns the set of all sites touching the boundary of the entire region, in no particular order.
		/// </summary>
		public Site[] GetHullSites ()
		{
			return _sites.Where (site => {
				GetRegion(site.id); // make sure, vertices have been computed
				return site.TouchesHull;
			}).ToArray();
		}
		
		public List<LineSegment> GetSpanningTree (KruskalType type = KruskalType.MINIMUM/*, BitmapData keepOutMask = null*/)
		{
			List<Edge> edges = DelaunayHelpers.SelectNonIntersectingEdges (/*keepOutMask,*/_edges);
			List<LineSegment> segments = DelaunayHelpers.DelaunayLinesForEdges (edges);
			return DelaunayHelpers.Kruskal (segments, type);
		}

		public List<List<Vector2>> GetRegions ()
		{
			return _sites.GetRegions (_plotBounds);
		}
		
		/**
		 * 
		 * @param proximityMap a BitmapData whose regions are filled with the site index values; see PlanePointsCanvas::fillRegions()
		 * @param x
		 * @param y
		 * @return coordinates of nearest Site to (x, y)
		 * 
		 */
		public Nullable<Vector2> GetNearestSitePoint (/*BitmapData proximityMap,*/float x, float y)
		{
			return _sites.NearestSitePoint (/*proximityMap,*/x, y);
		}
		
		public IEnumerable<Vector2> GetSiteCoords ()
		{
			return _sites.GetSiteCoords ();
		}


		#region Construction Algorithm
		private Site fortunesAlgorithm_bottomMostSite;
		private void FortunesAlgorithm ()
		{
			Site newSite, bottomSite, topSite, tempSite;
			Vertex v, vertex;
			Vector2 newintstar = Vector2.zero; //Because the compiler doesn't know that it will have a value - Julian
			Side leftRight;
			Halfedge lbnd, rbnd, llbnd, rrbnd, bisector;
			Edge edge;
			
			Rect dataBounds = _sites.GetSitesBounds ();
			
			int sqrt_nsites = (int)(Mathf.Sqrt (_sites.Count + 4));
			HalfedgePriorityQueue heap = new HalfedgePriorityQueue (dataBounds.y, dataBounds.height, sqrt_nsites);
			EdgeList edgeList = new EdgeList (dataBounds.x, dataBounds.width, sqrt_nsites);
			List<Halfedge> halfEdges = new List<Halfedge> ();
			List<Vertex> vertices = new List<Vertex> ();
			
			fortunesAlgorithm_bottomMostSite = _sites.Next ();
			newSite = _sites.Next ();
			
			for (;;) {
				if (heap.Empty () == false) {
					newintstar = heap.Min ();
				}
			
				if (newSite != null 
					&& (heap.Empty () || CompareByYThenX (newSite, newintstar) < 0)) {
					/* new site is smallest */
					//trace("smallest: new site " + newSite);
					
					// Step 8:
					lbnd = edgeList.EdgeListLeftNeighbor (newSite.Coord);	// the Halfedge just to the left of newSite
					//trace("lbnd: " + lbnd);
					rbnd = lbnd.edgeListRightNeighbor;		// the Halfedge just to the right
					//trace("rbnd: " + rbnd);
					bottomSite = FortunesAlgorithm_rightRegion (lbnd);		// this is the same as leftRegion(rbnd)
					// this Site determines the region containing the new site
					//trace("new Site is in region of existing site: " + bottomSite);
					
					// Step 9:
					edge = Edge.CreateBisectingEdge (bottomSite, newSite);
					//trace("new edge: " + edge);
					_edges.Add (edge);
					
					bisector = Halfedge.Create (edge, Side.LEFT);
					halfEdges.Add (bisector);
					// inserting two Halfedges into edgeList constitutes Step 10:
					// insert bisector to the right of lbnd:
					edgeList.Insert (lbnd, bisector);
					
					// first half of Step 11:
					if ((vertex = Vertex.Intersect (lbnd, bisector)) != null) {
						vertices.Add (vertex);
						heap.Remove (lbnd);
						lbnd.vertex = vertex;
						lbnd.ystar = vertex.y + newSite.Dist (vertex);
						heap.Insert (lbnd);
					}
					
					lbnd = bisector;
					bisector = Halfedge.Create (edge, Side.RIGHT);
					halfEdges.Add (bisector);
					// second Halfedge for Step 10:
					// insert bisector to the right of lbnd:
					edgeList.Insert (lbnd, bisector);
					
					// second half of Step 11:
					if ((vertex = Vertex.Intersect (bisector, rbnd)) != null) {
						vertices.Add (vertex);
						bisector.vertex = vertex;
						bisector.ystar = vertex.y + newSite.Dist (vertex);
						heap.Insert (bisector);	
					}
					
					newSite = _sites.Next ();	
				} else if (heap.Empty () == false) {
					/* intersection is smallest */
					lbnd = heap.ExtractMin ();
					llbnd = lbnd.edgeListLeftNeighbor;
					rbnd = lbnd.edgeListRightNeighbor;
					rrbnd = rbnd.edgeListRightNeighbor;
					bottomSite = FortunesAlgorithm_leftRegion (lbnd);
					topSite = FortunesAlgorithm_rightRegion (rbnd);
					// these three sites define a Delaunay triangle
					// (not actually using these for anything...)
					//_triangles.Add(new Triangle(bottomSite, topSite, rightRegion(lbnd)));
					
					v = lbnd.vertex;
					v.SetIndex ();
					lbnd.edge.SetVertex ((Side)lbnd.leftRight, v);
					rbnd.edge.SetVertex ((Side)rbnd.leftRight, v);
					edgeList.Remove (lbnd); 
					heap.Remove (rbnd);
					edgeList.Remove (rbnd); 
					leftRight = Side.LEFT;
					if (bottomSite.y > topSite.y) {
						tempSite = bottomSite;
						bottomSite = topSite;
						topSite = tempSite;
						leftRight = Side.RIGHT;
					}
					edge = Edge.CreateBisectingEdge (bottomSite, topSite);
					_edges.Add (edge);
					bisector = Halfedge.Create (edge, leftRight);
					halfEdges.Add (bisector);
					edgeList.Insert (llbnd, bisector);
					edge.SetVertex (SideHelper.Other (leftRight), v);
					if ((vertex = Vertex.Intersect (llbnd, bisector)) != null) {
						vertices.Add (vertex);
						heap.Remove (llbnd);
						llbnd.vertex = vertex;
						llbnd.ystar = vertex.y + bottomSite.Dist (vertex);
						heap.Insert (llbnd);
					}
					if ((vertex = Vertex.Intersect (bisector, rrbnd)) != null) {
						vertices.Add (vertex);
						bisector.vertex = vertex;
						bisector.ystar = vertex.y + bottomSite.Dist (vertex);
						heap.Insert (bisector);
					}
				} else {
					break;
				}
			}
			
			// heap should be empty now
			heap.Dispose ();
			edgeList.Dispose ();
			
			for (int hIndex = 0; hIndex<halfEdges.Count; hIndex++) {
				Halfedge halfEdge = halfEdges [hIndex];
				halfEdge.ReallyDispose ();
			}
			halfEdges.Clear ();
			
			// we need the vertices to clip the edges
			for (int eIndex = 0; eIndex<_edges.Count; eIndex++) {
				edge = _edges [eIndex];
				edge.ClipVertices (_plotBounds);
			}
			// but we don't actually ever use them again!
			for (int vIndex = 0; vIndex<vertices.Count; vIndex++) {
				vertex = vertices [vIndex];
				vertex.Dispose ();
			}
			vertices.Clear ();
		}

		private Site FortunesAlgorithm_leftRegion (Halfedge he)
		{
			Edge edge = he.edge;
			if (edge == null) {
				return fortunesAlgorithm_bottomMostSite;
			}
			return edge.Site ((Side)he.leftRight);
		}
		
		private Site FortunesAlgorithm_rightRegion (Halfedge he)
		{
			Edge edge = he.edge;
			if (edge == null) {
				return fortunesAlgorithm_bottomMostSite;
			}
			return edge.Site (SideHelper.Other ((Side)he.leftRight));
		}

		public static int CompareByYThenX (Site s1, Site s2)
		{
			if (s1.y < s2.y)
				return -1;
			if (s1.y > s2.y)
				return 1;
			if (s1.x < s2.x)
				return -1;
			if (s1.x > s2.x)
				return 1;
			return 0;
		}

		public static int CompareByYThenX (Site s1, Vector2 s2)
		{
			if (s1.y < s2.y)
				return -1;
			if (s1.y > s2.y)
				return 1;
			if (s1.x < s2.x)
				return -1;
			if (s1.x > s2.x)
				return 1;
			return 0;
		}
		#endregion


		#region Lloyd's Algorithm for relaxation
		/// <summary>
		/// Makes cell structure "more well-shaped" and "uniformly sized"
		/// by moving initial point clusters to cell centroids repeatedly.
		/// </summary>
		/// <see cref="https://en.wikipedia.org/wiki/Lloyd's_algorithm"/>
		/// <param name="nIterations">N iterations.</param>
		public Voronoi Relax(int nIterations = 1) {
			var voronoi = this;
			for (var i = 0; i < nIterations; ++i) {

				// compute new points
				var newPoints = new System.Collections.Generic.List<Vector2>();
				for (var iSite = 0; iSite < Sites.Count; ++iSite) {
					var site = voronoi.Sites[iSite];
					var vertices = site.GetRegion(_plotBounds);
					if (vertices.Count == 0) continue;

					var centroid = vertices[0];
					for (var iVertex = 1; iVertex < vertices.Count; ++iVertex) {
						centroid += vertices[iVertex];
					}
					centroid /= vertices.Count;

					newPoints.Add(centroid);
				}

				// re-compute the whole thing
				voronoi = new Voronoi(newPoints, _plotBounds);
			}
			return voronoi;
		}
		#endregion


		#region Dispose
		public void Dispose ()
		{
			int i, n;
			if (_sites != null) {
				_sites.Dispose ();
				_sites = null;
			}
			if (_triangles != null) {
				n = _triangles.Count;
				for (i = 0; i < n; ++i) {
					_triangles [i].Dispose ();
				}
				_triangles.Clear ();
				_triangles = null;
			}
			if (_edges != null) {
				n = _edges.Count;
				for (i = 0; i < n; ++i) {
					_edges [i].Dispose ();
				}
				_edges.Clear ();
				_edges = null;
			}
			//			_plotBounds = null;
			_sitesIndexedByLocation = null;
		}
		#endregion


		#region Graph Traversal
		public VoronoiBFS<TNodeData> TraverseVoronoiBFS<TNodeData>(VoronoiBFS<TNodeData>.TraversalAction cb) {
			// start with a random root
			return new VoronoiBFS<TNodeData>(this, new [] {Sites[0]}, cb);
		}

		public VoronoiBFS<TNodeData> TraverseVoronoiBFS<TNodeData>(IEnumerable<Site> roots, VoronoiBFS<TNodeData>.TraversalAction cb) {
			// start with a given set of roots
			return new VoronoiBFS<TNodeData>(this, roots, cb);
		}
		
		public class SiteTraversalNode<TData> {
			public readonly Site site;
			public SiteTraversalNode<TData>[] neighbors {
				get;
				internal set;
			}
			public TData data;

			internal SiteTraversalNode(Site site) {
				this.site = site;
			}
		}

		public class VoronoiBFS<TNodeData> {
			public delegate void TraversalAction(SiteTraversalNode<TNodeData> node, SiteTraversalNode<TNodeData> previous);


			Voronoi voronoi;
			Queue<SiteTraversalNode<TNodeData>> queue;
			VoronoiBFS<TNodeData>.TraversalAction cb;

			public SiteTraversalNode<TNodeData>[] nodesById {
				get;
				private set;
			}

			public VoronoiBFS(Voronoi voronoi, IEnumerable<Site> roots, VoronoiBFS<TNodeData>.TraversalAction cb) {
				this.voronoi = voronoi;
				queue = new Queue<SiteTraversalNode<TNodeData>>(roots.Select(site => new SiteTraversalNode<TNodeData>(site)));
				this.cb = cb;
				nodesById = voronoi.CreateSiteArrayById<SiteTraversalNode<TNodeData>>();
				Traverse ();
			}

			void Traverse() {
				SiteTraversalNode<TNodeData> previous = null;
				while (queue.Count > 0) {
					var current = queue.Dequeue();
					if (nodesById[current.site.id] != null) continue; // already visited

					// find all children
					nodesById[current.site.id] = current;
					var neighborSites = voronoi.GetNeighborSitesForSite(current.site.id);
					current.neighbors = neighborSites.Select(site => new SiteTraversalNode<TNodeData>(site)).ToArray();

					// run cb
					cb(current, previous);

					// add next batch of nodes to queue
					foreach (var child in current.neighbors) {
						queue.Enqueue(child);
					}
					previous = current;
				}
			}
		}
		#endregion
	}
}