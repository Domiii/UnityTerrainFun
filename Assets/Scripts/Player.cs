using UnityEngine;
using System.Collections;

/// <summary>
/// Singleton player instance.
/// NOTE: Singleton classes only ever have a single instance.
/// </summary>
public class Player : Unit {
	static Player instance;
	public static Player Instance { get { return instance; } }

	protected Player() {
		if (instance) {
			throw new System.Exception("Tried to create more than one player at the same time");
		}
		instance = this;
	}

	// Use this for initialization
	protected override void Start () {
		base.Start ();
	}
	
	// Update is called once per frame
	protected override void Update () {
		base.Update ();
	}
}
