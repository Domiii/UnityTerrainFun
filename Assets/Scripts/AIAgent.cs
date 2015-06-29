using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class AIAgent : Unit {
	private NavMeshAgent navMeshAgent;
	
	protected AIAgent() {
	}
	
	public NavMeshAgent NavMeshAgent {
		get { return navMeshAgent; }
		set { navMeshAgent = value; }
	}
	
	protected override void Start() {
		navMeshAgent = GetComponent<NavMeshAgent>();
		base.Start ();
	}
	
	protected override void Update() {
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
