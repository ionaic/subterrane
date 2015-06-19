using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class splinePointsDebug : MonoBehaviour {

    public GameObject centipede;
    private List<Transform> splinePoint = new List<Transform>();

	// Use this for initialization
	void Start () {
        splinePoint.Add(GameObject.Find("/SplinePoints/s0").transform);
        splinePoint.Add(GameObject.Find("/SplinePoints/s1").transform);
        splinePoint.Add(GameObject.Find("/SplinePoints/s2").transform);
        splinePoint.Add(GameObject.Find("/SplinePoints/s3").transform);
        splinePoint.Add(GameObject.Find("/SplinePoints/s4").transform);
        splinePoint.Add(GameObject.Find("/SplinePoints/s5").transform);
        splinePoint.Add(GameObject.Find("/SplinePoints/s6").transform);
        splinePoint.Add(GameObject.Find("/SplinePoints/s7").transform);
        splinePoint.Add(GameObject.Find("/SplinePoints/s8").transform);
        splinePoint.Add(GameObject.Find("/SplinePoints/s9").transform);
        splinePoint.Add(GameObject.Find("/SplinePoints/s10").transform);
        splinePoint.Add(GameObject.Find("/SplinePoints/s11").transform);
        splinePoint.Add(GameObject.Find("/SplinePoints/s12").transform);
        splinePoint.Add(GameObject.Find("/SplinePoints/s13").transform);
        splinePoint.Add(GameObject.Find("/SplinePoints/s14").transform);
        splinePoint.Add(GameObject.Find("/SplinePoints/s15").transform);
        splinePoint.Add(GameObject.Find("/SplinePoints/s16").transform);        
	}
	
	// Update is called once per frame
	void Update () {
        for (int i = 0; i < splinePoint.Count; i++){
            splinePoint[i].position = centipede.GetComponent<behaviorCentipedeCCD>().Spline()[i] + new Vector3(0f,1.5f,0f);            
        }
	}
}
