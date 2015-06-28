using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class Unit : MonoBehaviour {
	public Color HighlighColor = Color.yellow;

	private NavMeshAgent agent;
	private new Renderer renderer;

	private bool isSelected;
	private Color originalColor;

	Unit() {
	}

	public NavMeshAgent Agent {
		get { return agent; }
		set { agent = value; }
	}
	
	protected void Reset() {
		Debug.Log("Reset: " + name);
	}

	protected void Start() {
		agent = GetComponent<NavMeshAgent>();
		renderer = GetComponent<Renderer> ();
	}

	protected void Update() {
		//IsSelected = !IsSelected;
		if (Input.GetButton("Fire1")) {
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit)) {
				agent.SetDestination(hit.point);
			}
		}
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
