using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour {
	public Color HighlighColor = Color.yellow;

	protected new Renderer renderer;

	private bool isSelected;
	private Color originalColor;

	protected Unit() {
	}

	protected virtual void Start() {
		renderer = GetComponent<Renderer> ();
	}

	protected virtual void Update() {
//		//IsSelected = !IsSelected;
//		if (Input.GetButton("Fire1")) {
//			RaycastHit hit;
//			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//			if (Physics.Raycast(ray, out hit)) {
//				agent.SetDestination(hit.point);
//			}
//		}
	}

	public bool IsSelected {
		get {
			return isSelected;
		}
		set {
			if (value == isSelected) return;

			if (value) {
				// select
				originalColor = renderer.material.color;
				renderer.material.color = HighlighColor;
			}
			else {
				// de-select
				renderer.material.color = originalColor;
			}
			
			isSelected = value;
		}
	}
}
