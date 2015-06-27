using UnityEngine;

public enum Direction4 {
	North,
	East,
	South,
	West
}

/// <summary>
/// Utility class for working with directions
/// </summary>
public static class Directions {
	public const int Direction4Count = 4;

	private static IntVector2[] vectors4 = {
		new IntVector2(0, 1),
		new IntVector2(1, 0),
		new IntVector2(0, -1),
		new IntVector2(-1, 0)
	};

	private static Direction4[] opposites4 = {
		Direction4.South,
		Direction4.West,
		Direction4.North,
		Direction4.East
	};

	private static Quaternion[] rotations4 = {
		Quaternion.identity,
		Quaternion.Euler(0f, 90f, 0f),
		Quaternion.Euler(0f, 180f, 0f),
		Quaternion.Euler(0f, 270f, 0f)
	};

	public static Direction4 RandomValue {
		get {
			return (Direction4)Random.Range(0, Direction4Count);
		}
	}

	public static IntVector2 ToIntVector2 (this Direction4 direction) {
		return vectors4[(int)direction];
	}
	
	public static Direction4 GetOpposite (this Direction4 direction) {
		return opposites4[(int)direction];
	}
	
	public static Quaternion ToRotation (this Direction4 direction) {
		return rotations4[(int)direction];
	}
}