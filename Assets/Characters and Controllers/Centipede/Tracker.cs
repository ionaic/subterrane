using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tracker : MonoBehaviour {

    public float startTime = 15f;
    public float repeatRate = 0.5f;
    public bool useCameraPosition = true;

    private List<Vector3> location = new List<Vector3>();
    public List<Vector3> Location {
        get { return location; }
        set { location = value; }
    }

    /// Use this for initialization
	void Start () {        
        InvokeRepeating("TrackPlayer", 0f, repeatRate);
	}
    
    /// Called every repeatRate seconds after initial wait of startTime
	private void TrackPlayer () {
        Vector3 trackpoint;
        if (useCameraPosition)
            trackpoint = Camera.main.transform.position;
        else
            trackpoint = transform.position;
    
        if (location.Count <= 0 || (trackpoint != location[location.Count-1])) {
            location.Add(trackpoint);
            //print("Added new Trackpoint " + location[location.Count-1].ToString());
        }
        else{
            //Debug.LogWarning ("Skipped adding Trackpoint.");
        }
    }
    
    public Vector3 Pop(){
        if (location.Count > 0){
            Vector3 target = location[0];
            location.RemoveAt(0);
            return target;
        }
        else { 
            Debug.LogWarning("Tried to Pop when location was empty!");
            return Vector3.zero;
        }
    }
    
}