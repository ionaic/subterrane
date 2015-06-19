using UnityEngine;
using System.Collections;

public class MATTGUI : MonoBehaviour {
	private Camera cam;
	public int fontSize = 20;
	// Use this for initialization
	void Start () {
		cam = transform.GetComponent<Camera>();
	}
	void OnGUI () {
		GUI.skin.label.fontSize = GUI.skin.box.fontSize = GUI.skin.button.fontSize = fontSize;
		
		int width = 320, height = 240;
        GUI.BeginGroup(new Rect(cam.pixelWidth / 2 - width/2, cam.pixelHeight / 2 - height/2, width, height));
		// Make a background box
		GUI.Box(new Rect(0,0,width,height), "Main Menu");

		// Make the first button. If it is pressed, Application.Loadlevel (1) will be executed
		if(GUI.Button(new Rect(20,height/3-40,width-40,height/3), "Start Game")) {
			Application.LoadLevel(1);
		}

		// Make the second button.
		if(GUI.Button(new Rect(20,2*height/3-20,width-40,height/3), "Quit")) {
			Application.Quit();
		}
        GUI.EndGroup();
	}
	// Update is called once per frame
	void Update () {
	
	}
}
