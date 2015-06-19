using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CheckpointWarper : MonoBehaviour {

    public Transform player;  
    
    private List<GameObject> checkpoint = new List<GameObject>();

	/// Use this for initialization
	void Start () {
        foreach (Transform child in transform)
            checkpoint.Add(child.gameObject);
	}
	
	/// Update is called once per frame
	void Update () {
        if (Input.GetKeyUp(KeyCode.Alpha1) && checkpoint.Count >= 1)
			player.transform.position = checkpoint[0].transform.position;
		else if (Input.GetKeyUp(KeyCode.Alpha2) && checkpoint.Count >= 2)
			player.transform.position = checkpoint[1].transform.position;
		else if (Input.GetKeyUp(KeyCode.Alpha3) && checkpoint.Count >= 3)
			player.transform.position = checkpoint[2].transform.position;
		else if (Input.GetKeyUp(KeyCode.Alpha4) && checkpoint.Count >= 4)
			player.transform.position = checkpoint[3].transform.position;
		else if (Input.GetKeyUp(KeyCode.Alpha5) && checkpoint.Count >= 5)
			player.transform.position = checkpoint[4].transform.position;
		else if (Input.GetKeyUp(KeyCode.Alpha6) && checkpoint.Count >= 6)
			player.transform.position = checkpoint[5].transform.position;
        else if (Input.GetKeyUp(KeyCode.Alpha7) && checkpoint.Count >= 7)
			player.transform.position = checkpoint[6].transform.position;
        else if (Input.GetKeyUp(KeyCode.Alpha8) && checkpoint.Count >= 8)
			player.transform.position = checkpoint[7].transform.position;
        else if (Input.GetKeyUp(KeyCode.Alpha9) && checkpoint.Count >= 9)
			player.transform.position = checkpoint[8].transform.position;
	}
}
