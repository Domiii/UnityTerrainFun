using UnityEngine;
using System.Collections;

public class Sun : MonoBehaviour {
	public float RealSecondsPerDay = 10;
	//public Vector3 AxisOfRotation = new Vector3(0, -1, 1);
	public Vector3 AxisOfRotation = new Vector3(1, 0, 0);
	public float TimeOfDayOverride = 0;

	/// <summary>
	/// Virtual time of day from 0 = 00:00 to 1 = 00:00 on the next day
	/// </summary>
	public float virtualTimeOfDay;

	// Use this for initialization
	void Start () {
		// get current time of day
		var systemTimeOfDay = System.DateTime.Now.TimeOfDay;
		virtualTimeOfDay = (float)systemTimeOfDay.TotalDays;
		
		AxisOfRotation.Normalize(); // normalize axis
	}
	
	// Update is called once per frame
	void Update () {
		// advance time of day
		var timeDiff = Time.deltaTime;
		var virtualTimeDiff = timeDiff / RealSecondsPerDay;
		virtualTimeOfDay += virtualTimeDiff;
		virtualTimeOfDay = Mathf.Repeat(virtualTimeOfDay, 1);

		// update rotation
		//var angle = TimeOfDayOverride * 360;
		var angle = virtualTimeOfDay * 360;
		transform.localRotation = Quaternion.Euler(angle, 0, 0);
	}
}
