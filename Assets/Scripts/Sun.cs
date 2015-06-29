using UnityEngine;
using System.Collections;

public class Sun : MonoBehaviour {
	/// <summary>
	/// Virtual time of day from 0 = 00:00 to 1 = 00:00 on the next day
	/// </summary>
	[HideInInspector]
	public float VirtualTimeOfDay;
	public float RealSecondsPerDay = 10;
	public Vector3 StartingRotation = new Vector3 (20, 0, 0);
	public Vector3 AxisOfRotation = new Vector3(0, -1, 1);
	public float TimeOfDayOverride = 0;

	// Use this for initialization
	void Start () {
		// get current time of day
		var systemTimeOfDay = System.DateTime.Now.TimeOfDay;
		VirtualTimeOfDay = (float)systemTimeOfDay.TotalDays;
		
		AxisOfRotation.Normalize(); // normalize axis
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
		//transform.localRotation = Quaternion.Euler(new Vector3(360 * VirtualTimeOfDay,-240,170));

		//transform.localRotation = Quaternion.AngleAxis(360 * VirtualTimeOfDay, AxisOfRotation);
		//transform.localRotation = Quaternion.Euler(AxisOfRotation)
		transform.localRotation = Quaternion.Euler (StartingRotation) * Quaternion.AngleAxis(TimeOfDayOverride, AxisOfRotation);
	}
}
