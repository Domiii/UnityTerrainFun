using UnityEngine;
using System.Collections;

public class Sun : MonoBehaviour {
	[HideInInspector]
	public float VirtualTimeOfDay;
	public float RealSecondsPerDay = 10;

	// Use this for initialization
	void Start () {
		// get current time of day
		var systemTimeOfDay = System.DateTime.Now.TimeOfDay;
		VirtualTimeOfDay = (float)systemTimeOfDay.TotalDays;
	}
	
	// Update is called once per frame
	void Update () {
		// advance time of day
		var timeDiff = Time.deltaTime;
		var virtualTimeDiff = timeDiff / RealSecondsPerDay;
		VirtualTimeOfDay += virtualTimeDiff;
		VirtualTimeOfDay = Mathf.Repeat(VirtualTimeOfDay, 1);

		// update rotation
		//transform.localRotation = 
		transform.localRotation = Quaternion.Euler(new Vector3(360 * VirtualTimeOfDay,-240,170));
	}
}
