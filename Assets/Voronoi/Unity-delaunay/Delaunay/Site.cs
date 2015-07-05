using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Delaunay.Geo;
using Delaunay.LR;

namespace Delaunay
{
		
	public sealed class Site: ICoord, IComparable
	{
		private static Stack<Site> _pool = new Stack<Site> ();
		public static Site Create (Vector2 p, uint index, float weight, uint id)
		{
			if (_pool.Count > 0) {
				return _pool.Pop ().Init (p, index, weight, id);
			} else {
				return new Site (p, index, weight, id);
			}
		}
		
		internal static void SortSites (List<Site> sites)
		{
//			sites.sort(Site.compare);
			sites.Sort (); // XXX: Check if this works
		}

		/**
		 * sort sites on y, then x, coord
		 * also change each site's _siteIndex to match its new position in the list
		 * so the _siteIndex can be used to identify the site for nearest-neighbor queries
		 * 
		 * haha "also" - means more than one responsibility...
		 * 
		 */
		public int CompareTo (System.Object obj) // XXX: Really, really worried about this because it depends on how sorting works in AS3 impl - Julian
		{
			Site s2 = (Site)obj;

			int returnValue = Voronoi.CompareByYThenX (this, s2);
			
			// swap _siteIndex values if necessary to match new ordering:
			uint tempIndex;
			if (returnValue == -1) {
				if (this._siteIndex > s2._siteIndex) {
					tempIndex = this._siteIndex;
					this._siteIndex = s2._siteIndex;
					s2._siteIndex = tempIndex;
				}
			} else if (returnValue == 1) {
				if (s2._siteIndex > this._siteIndex) {
					tempIndex = s2._siteIndex;
					s2._siteIndex = this._siteIndex;
					this._siteIndex = tempIndex;
				}
				
			}
			
			return returnValue;
		}


		private static readonly float EPSILON = .005f;
		private static bool CloseEnough (Vector2 p0, Vector2 p1)
		{
			return Vector2.Distance (p0, p1) < EPSILON;
		}
				
		private Vector2 _coord;
		public Vector2 Coord {
			get { return _coord;}
		}

		public uint id { get; private set;}
		public float weight;
		
		private uint _siteIndex;
		
		// the edges that define this Site's Voronoi region:
		private List<Edge> _edges;
		internal List<Edge> edges {
			get { return _edges;}
		}
		// which end of each edge hooks up with the previous edge in _edges:
		private List<Side> _edgeOrientations;
		// ordered list of points that define the region clipped to bounds:
		private List<Vector2> _region;
		private bool _touchesHull;

		private Site (Vector2 p, uint index, float weight, uint id)
		{
//			if (lock != PrivateConstructorEnforcer)
//			{
//				throw new Error("Site constructor is private");
//			}
			Init (p, index, weight, id);
		}
		
		private Site Init (Vector2 p, uint index, float weight, uint id)
		{
			_coord = p;
			_siteIndex = index;
			this.id = id;
			this.weight = weight;
			_edges = new List<Edge> ();
			_region = null;
			return this;
		}

		/// <summary>
		/// Vertex set, as previously computed
		/// </summary>
		public List<Vector2> RegionCached {
			get { return _region; }
		}

		public bool TouchesHull {
			get { return _touchesHull; }
		}
		
		public override string ToString ()
		{
			return "Site " + _siteIndex.ToString () + ": " + Coord.ToString ();
		}
		
		private void Move (Vector2 p)
		{
			Clear ();
			_coord = p;
		}
		
		public void Dispose ()
		{
//			_coord = null;
			Clear ();
			_pool.Push (this);
		}
		
		private void Clear ()
		{
			if (_edges != null) {
				_edges.Clear ();
				_edges = null;
			}
			if (_edgeOrientations != null) {
				_edgeOrientations.Clear ();
				_edgeOrientations = null;
			}
			if (_region != null) {
				_region.Clear ();
				_region = null;
			}
		}
		
		public void AddEdge (Edge edge)
		{
			_edges.Add (edge);
		}
		
		public Edge NearestEdge ()
		{
			_edges.Sort (delegate (Edge a, Edge b) {
				return Edge.CompareSitesDistances (a, b);
			});
			return _edges [0];
		}
		
		public List<Site> GetNeighborSites ()
		{
			if (_edges == null || _edges.Count == 0) {
				return new List<Site> ();
			}
			if (_edgeOrientations == null) { 
				// initialize additional region information
				ReorderEdges ();
			}

			List<Site> list = new List<Site> ();
			Edge edge;
			for (int i = 0; i < _edges.Count; i++) {
				edge = _edges [i];
				list.Add (GetNeighborSite (edge));
			}
			return list;
		}
			
		private Site GetNeighborSite (Edge edge)
		{
			if (this == edge.leftSite) {
				return edge.rightSite;
			}
			if (this == edge.rightSite) {
				return edge.leftSite;
			}
			return null;
		}
		
		internal List<Vector2> GetRegion (Rect clippingBounds)
		{
			if (_edges == null || _edges.Count == 0) {
				return new List<Vector2> ();
			}
			if (_edgeOrientations == null) { 
				ReorderEdges ();
				_region = ComputeVertexSet (clippingBounds);
				// TODO: Why not store vertex set as polygon?
				if ((new Polygon (_region)).Winding () == Winding.CLOCKWISE) {
					_region.Reverse ();
				}
			}
			return _region;
		}
		
		private void ReorderEdges ()
		{
			//trace("_edges:", _edges);
			EdgeReorderer reorderer = new EdgeReorderer (_edges, VertexOrSite.VERTEX);
			_edges = reorderer.edges;
			//trace("reordered:", _edges);
			_edgeOrientations = reorderer.edgeOrientations;
			reorderer.Dispose ();
		}
		
		private List<Vector2> ComputeVertexSet (Rect bounds)
		{
			_region = new List<Vector2> ();
			int n = _edges.Count;
			int i = 0;
			Edge edge;
			while (i < n && ((_edges[i] as Edge).visible == false)) {
				++i;
			}
			
			if (i == n) {
				// no edges visible
				return new List<Vector2> ();
			}
			edge = _edges [i];
			Side orientation = _edgeOrientations [i];

			if (edge.clippedEnds [orientation] == null) {
				Debug.LogError ("XXX: Null detected when there should be a Vector2!");
			}
			if (edge.clippedEnds [SideHelper.Other (orientation)] == null) {
				Debug.LogError ("XXX: Null detected when there should be a Vector2!");
			}
			_region.Add ((Vector2)edge.clippedEnds [orientation]);
			_region.Add ((Vector2)edge.clippedEnds [SideHelper.Other (orientation)]);

			for (int j = i + 1; j < n; ++j) {
				edge = _edges [j];
				if (edge.visible == false) {
					continue;
				}
				Connect (_region, j, bounds);
			}
			// close up the polygon by adding another corner point of the bounds if needed:
			Connect (_region, i, bounds, true);

			// done computing vertex set, now check if any vertex touches the hull boundary
			_touchesHull = _region.Any(v => BoundsCheck.Check(v, bounds) != 0);
			return _region;
		}
		
		private void Connect (List<Vector2> points, int j, Rect bounds, bool closingUp = false)
		{
			Vector2 rightPoint = points [points.Count - 1];
			Edge newEdge = _edges [j] as Edge;
			Side newOrientation = _edgeOrientations [j];
			// the point that  must be connected to rightPoint:
			if (newEdge.clippedEnds [newOrientation] == null) {
				Debug.LogError ("XXX: Null detected when there should be a Vector2!");
			}
			Vector2 newPoint = (Vector2)newEdge.clippedEnds [newOrientation];
			if (!CloseEnough (rightPoint, newPoint)) {
				// The points do not coincide, so they must have been clipped at the bounds;
				// see if they are on the same border of the bounds:

				if (rightPoint.x != newPoint.x
					&& rightPoint.y != newPoint.y) {
					// They are on different borders of the bounds;
					// insert one or two corners of bounds as needed to hook them up:
					// (NOTE this will not be correct if the region should take up more than
					// half of the bounds rect, for then we will have gone the wrong way
					// around the bounds and included the smaller part rather than the larger)
					int rightCheck = BoundsCheck.Check (rightPoint, bounds);
					int newCheck = BoundsCheck.Check (newPoint, bounds);

					float px, py;
					if ((rightCheck & BoundsCheck.RIGHT) != 0) {
						px = bounds.xMax;
						if ((newCheck & BoundsCheck.BOTTOM) != 0) {
							py = bounds.yMax;
							points.Add (new Vector2 (px, py));
						} else if ((newCheck & BoundsCheck.TOP) != 0) {
							py = bounds.yMin;
							points.Add (new Vector2 (px, py));
						} else if ((newCheck & BoundsCheck.LEFT) != 0) {
							if (rightPoint.y - bounds.y + newPoint.y - bounds.y < bounds.height) {
								py = bounds.yMin;
							} else {
								py = bounds.yMax;
							}
							points.Add (new Vector2 (px, py));
							points.Add (new Vector2 (bounds.xMin, py));
						}
					} else if ((rightCheck & BoundsCheck.LEFT) != 0) {
						px = bounds.xMin;
						if ((newCheck & BoundsCheck.BOTTOM) != 0) {
							py = bounds.yMax;
							points.Add (new Vector2 (px, py));
						} else if ((newCheck & BoundsCheck.TOP) != 0) {
							py = bounds.yMin;
							points.Add (new Vector2 (px, py));
						} else if ((newCheck & BoundsCheck.RIGHT) != 0) {
							if (rightPoint.y - bounds.y + newPoint.y - bounds.y < bounds.height) {
								py = bounds.yMin;
							} else {
								py = bounds.yMax;
							}
							points.Add (new Vector2 (px, py));
							points.Add (new Vector2 (bounds.xMax, py));
						}
					} else if ((rightCheck & BoundsCheck.TOP) != 0) {
						py = bounds.yMin;
						if ((newCheck & BoundsCheck.RIGHT) != 0) {
							px = bounds.xMax;
							points.Add (new Vector2 (px, py));
						} else if ((newCheck & BoundsCheck.LEFT) != 0) {
							px = bounds.xMin;
							points.Add (new Vector2 (px, py));
						} else if ((newCheck & BoundsCheck.BOTTOM) != 0) {
							if (rightPoint.x - bounds.x + newPoint.x - bounds.x < bounds.width) {
								px = bounds.xMin;
							} else {
								px = bounds.xMax;
							}
							points.Add (new Vector2 (px, py));
							points.Add (new Vector2 (px, bounds.yMax));
						}
					} else if ((rightCheck & BoundsCheck.BOTTOM) != 0) {
						py = bounds.yMax;
						if ((newCheck & BoundsCheck.RIGHT) != 0) {
							px = bounds.xMax;
							points.Add (new Vector2 (px, py));
						} else if ((newCheck & BoundsCheck.LEFT) != 0) {
							px = bounds.xMin;
							points.Add (new Vector2 (px, py));
						} else if ((newCheck & BoundsCheck.TOP) != 0) {
							if (rightPoint.x - bounds.x + newPoint.x - bounds.x < bounds.width) {
								px = bounds.xMin;
							} else {
								px = bounds.xMax;
							}
							points.Add (new Vector2 (px, py));
							points.Add (new Vector2 (px, bounds.yMin));
						}
					}
				}
				if (closingUp) {
					// newEdge's ends have already been added
					return;
				}
				points.Add (newPoint);
			}
			if (newEdge.clippedEnds [SideHelper.Other (newOrientation)] == null) {
				Debug.LogError ("XXX: Null detected when there should be a Vector2!");
			}
			Vector2 newRightPoint = (Vector2)newEdge.clippedEnds [SideHelper.Other (newOrientation)];
			if (!CloseEnough (points [0], newRightPoint)) {
				points.Add (newRightPoint);
			}
		}
								
		public float x {
			get { return _coord.x;}
		}
		internal float y {
			get { return _coord.y;}
		}
		
		public float Dist (ICoord p)
		{
			return Vector2.Distance (p.Coord, this._coord);
		}

	}
}

//	class PrivateConstructorEnforcer {}

//	import flash.geom.Point;
//	import flash.geom.Rectangle;
	
static class BoundsCheck
{
	public static readonly int TOP = 1;
	public static readonly int BOTTOM = 2;
	public static readonly int LEFT = 4;
	public static readonly int RIGHT = 8;
		
	/**
		 * 
		 * @param point
		 * @param bounds
		 * @return an int with the appropriate bits set if the Point lies on the corresponding bounds lines
		 * 
		 */
	public static int Check (Vector2 point, Rect bounds)
	{
		int value = 0;
		if (point.x == bounds.xMin) {
			value |= LEFT;
		}
		if (point.x == bounds.xMax) {
			value |= RIGHT;
		}
		if (point.y == bounds.yMin) {
			value |= TOP;
		}
		if (point.y == bounds.yMax) {
			value |= BOTTOM;
		}
		return value;
	}
}