using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class AIAgent : Unit {
	public Color HighlighColor = Color.yellow;
	
	private NavMeshAgent navMeshAgent;

	private Color originalColor;
	
	protected AIAgent() {
	}
	
	public NavMeshAgent NavMeshAgent {
		get { return navMeshAgent; }
		set { navMeshAgent = value; }
	}
	
	protected virtual void Reset() {

	}
	
	protected virtual void Start() {
		navMeshAgent = GetComponent<NavMeshAgent>();
		base.Start ();
	}
	
	protected virtual void Update() {
		//IsSelected = !IsSelected;
		if (Input.GetButton("Fire1")) {
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit)) {
				navMeshAgent.SetDestination(hit.point);
			}
		}

		base.Update ();
	}
}
