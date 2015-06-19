using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {
    public GUISkin MenuSkin;
	
    void FirstMenu() {
		Debug.Log("Hello world");
        // layout start
        GUI.BeginGroup(new Rect(Screen.width / 2 - 150, 50, 300, 200));

        // menu background
        GUI.Box(new Rect(0, 0, 300, 200), "");

        // buttons
        if (GUI.Button(new Rect(55, 100, 180, 40), "Start game")) {
            //Component mainMenuScript = GetComponent("MainMenu");
            //mainMenuScript.enabled = false;
            //GetComponent<MainMenu>().enabled = false;
            Application.LoadLevel("Level1Organic");
            //GameObject mapMenuScript = GetComponent("MapMenuScript");
            //mapMenuScript.enabled = true;
        }
        if (GUI.Button(new Rect(55, 150, 180, 40), "Quit")) {
            Application.Quit();
        }
        
        // end layout
        GUI.EndGroup();
    }

    void OnGui() {
		Debug.Log("Hello world");
        GUI.BeginGroup(new Rect(Screen.width / 2 - 400, Screen.height / 2 - 300, 800, 600));
        GUI.Box(new Rect(0, 0, 800, 600), "This box is now centered! - here you would put your main menu");
        GUI.EndGroup();
        
        //GUI.skin = MenuSkin;
        //
        //FirstMenu();
        //
        //Debug.Log("On Gui!");
    }
}
