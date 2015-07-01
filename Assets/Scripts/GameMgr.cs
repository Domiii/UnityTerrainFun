using UnityEngine;
using System.Collections;

using UnityStandardAssets.Characters.FirstPerson;

[System.Serializable]
public class GameMenu {
	public Texture m_backgroundTexture;
	
	private bool m_isOpen;
	private GUIContent m_backgroundContent;

	public GameMenu() {
		IsOpen = false;
	}

	public bool IsOpen {
		get { return m_isOpen; }
		set {
			m_isOpen = value;
			if (!value) return;
			
			// re-create cached assets	
			m_backgroundContent = new GUIContent (m_backgroundTexture);
		}
	}
	
	public void OnGUI() {
		if (!IsOpen) return;

		var dimensions = new Rect (Screen.width / 4, Screen.height / 4, Screen.width / 2, Screen.height / 2);

		GUI.Label(dimensions, m_backgroundContent);
		GUILayout.BeginArea(dimensions);

		GUILayout.Label ("HI");
		GUILayout.FlexibleSpace();
		GUILayout.FlexibleSpace();
		IsOpen = !GUILayout.Button("OK");
		//GUILayout.Toggle(IsOpen, "Close");

		GUILayout.EndArea();
	}
}

public class GameMgr : MonoBehaviour {
	#region Singleton
	public static GameMgr Instance {
		get;
		private set;
	}
	#endregion
	
	[SerializeField] private GameMenu menu;

	GameMgr() {
		if (Instance != null) throw new UnityException ("Tried to instantiate singleton more than once: "  + this);
		Instance = this;

		menu = new GameMenu ();
	}

	public GameMenu Menu { get { return menu; } }

	// Use this for initialization
	void Start () {
		QualitySettings.antiAliasing = 8;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			// toggle menu
			Menu.IsOpen = !Menu.IsOpen;

			// toggle mouse control
			FirstPersonController.Instance.IsMouseControlOn = !Menu.IsOpen;
		}
	}

	void OnGUI() {
		Menu.OnGUI ();
	}
}
