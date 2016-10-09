using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Delaunay;
using Delaunay.Geo;

[System.Serializable]
public class MapCellSettings {
	public int CellTypeId;


}

/// <summary>
/// Represents a cell of the Voronoi partitioning, spanning over the terrain
/// </summary>
[RequireComponent(typeof(PolygonCollider2D))]
public class MapCell : MonoBehaviour {
	//public IntVector2 Coordinates;

	public Site site;

	[HideInInspector]
	public int distanceFromBoundary;

	[HideInInspector]
	public Site[] neighborSites;

	PolygonCollider2D polygon;


	public MapGenerator Map {
		get {
			return transform.parent.GetComponent<MapGenerator> ();
		}
	}

	void Start() {
		transform.localRotation = Quaternion.identity;
	}

	internal void ResetCell (Site site) {
		this.site = site;

		// add polygon
		polygon = gameObject.GetComponent<PolygonCollider2D>();
		polygon.SetPath (0, site.Vertices.ToArray ());

		gameObject.name = ToString ();
	}

	public override string ToString ()
	{
		return "MapCell #" + site.id;
	}
}
