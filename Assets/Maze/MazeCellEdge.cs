using UnityEngine;

public abstract class MazeCellEdge : MonoBehaviour {
	
	public MazeCell cell, otherCell;
	
	public Direction4 direction;

	public virtual void Initialize (MazeCell cell, MazeCell otherCell, Direction4 direction) {
		this.cell = cell;
		this.otherCell = otherCell;
		this.direction = direction;
		cell.SetEdge(direction, this);
		transform.parent = cell.transform;
		transform.localPosition = Vector3.zero;
		transform.localRotation = direction.ToRotation();
	}
}
