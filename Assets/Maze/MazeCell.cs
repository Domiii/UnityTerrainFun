using UnityEngine;

public class MazeCell : MonoBehaviour {
	public IntVector2 position;
	public MazeRoom room;

	private int initializedEdgeCount;
	private MazeCellEdge[] edges = new MazeCellEdge[Directions.Direction4Count];
	
	public bool IsFullyInitialized {
		get {
			return initializedEdgeCount == Directions.Direction4Count;
		}
	}
	
	public void Initialize (MazeRoom room) {
		room.Add(this);
		transform.GetChild(0).GetComponent<Renderer>().material = room.settings.floorMaterial;
	}

	public MazeCellEdge GetEdge (Direction4 direction) {
		return edges[(int)direction];
	}
	
	public void SetEdge (Direction4 direction, MazeCellEdge edge) {
		edges[(int)direction] = edge;
		initializedEdgeCount += 1;
	}

	public Direction4 RandomUninitializedDirection {
		get {
			int skips = Random.Range(0, Directions.Direction4Count - initializedEdgeCount);
			for (int i = 0; i < Directions.Direction4Count; i++) {
				if (edges[i] == null) {
					if (skips == 0) {
						return (Direction4)i;
					}
					skips -= 1;
				}
			}

			throw new System.InvalidOperationException("MazeCell has no uninitialized directions left.");
		}
	}
}