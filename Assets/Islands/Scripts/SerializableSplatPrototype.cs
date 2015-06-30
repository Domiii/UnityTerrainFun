using UnityEngine;


[System.Serializable]
public class SerializableSplatPrototype {
	public Texture2D texture;
	public Vector2 tileSize;
	public Vector2 tileOffset;

	public SerializableSplatPrototype() {
		tileSize = new Vector2 (1, 1);
		tileOffset = new Vector2 (0, 0);
	}
}