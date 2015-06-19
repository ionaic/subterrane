using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FollowHead : MonoBehaviour {
    public List<Transform> segments = new List<Transform>();
	
	public List<Vector3> positions = new List<Vector3>();
	public List<float> distances = new List<float>();
	public float mindist, maxdist;
    public bool freezeZ = false;
    private float frozenHead;
    private List<float> frozenZ = new List<float>();
	// Use this for initialization
	Vector3 nvec(Vector3 old) {
		return new Vector3(old.x, old.y, old.z);
	}
	void Start () {
		foreach(Transform segment in segments) {
			positions.Add(nvec(segment.position));
		}
		mindist = Vector3.Distance(positions[0],positions[1])/3;
		positions.Reverse();
		maxdist = 15;//Vector3.Distance(positions[0],positions[1])*2;
		
		distances.Add(0);
		for (int i=1; i<segments.Count; ++i) {
			distances.Add(Vector3.Distance(segments[i-1].position,segments[i].position));            
		}
		maxdist = mindist*2;
		for (int i=1; i<segments.Count; ++i) {
			maxdist+=distances[i];
		}
        frozenHead = transform.localEulerAngles.z;
        for (int i = 0; i < segments.Count; ++i){
            frozenZ.Add(segments[i].transform.localEulerAngles.z);
        }
	}
	
	// Update is called once per frame
	void Update () {
		// update the list of active positions
		Vector3 now = segments[0].position;
		float nowDist = Vector3.Distance(now,positions[positions.Count-1]);
		if (nowDist>=mindist) {
			positions.Add(nvec(now));
		}
		float lastDist = Vector3.Distance(positions[0],segments[0].position);
		Debug.Log(lastDist);
		Debug.Log(segments[6].position);
		if (lastDist>= maxdist) {
			positions.RemoveAt(0);
		}
		int idx = positions.Count-2;
		for (int i = 1; i<segments.Count; ++i) {
			// correct
			idx = locate(segments[i].position,distances[i],idx);
			Vector3 newPos = trigTime(positions[idx+1],positions[idx],segments[i].position,distances[i]);
			// in question
			/*Vector3 normal1 = newPos-segments[i-1].position;
			Vector3 normal2 = segments[i-1].position-segments[i].position;
			Vector3 axis = Vector3.Cross(normal1,normal2);
			axis.Normalize();
			float angle = Mathf.Acos(Vector3.Dot(normal1, normal2) / normal1.magnitude / normal2.magnitude);
			Quaternion newrot = Quaternion.AngleAxis(angle,axis);
			segments[i-1].localRotation = newrot;*/
			//segments[i-1].localRotation = Quaternion.AngleAxis(0.0f,new Vector3(0,0,0));
			Vector3 curdir = segments[i].localPosition;
			Vector3 newdir = segments[i-1].InverseTransformPoint(newPos);
			float theta = Mathf.Acos(Vector3.Dot(curdir,newdir)/(curdir.magnitude*newdir.magnitude));
			//Debug.Log(theta);
			Vector3 axis = Vector3.Cross(curdir,newdir);
			axis.Normalize();
			Quaternion newrot = Quaternion.AngleAxis(theta,axis);
			segments[i-1].localRotation *= newrot;
			// should be 0
			//Debug.Log((newPos - segments[i].position).magnitude);
		}
        /**
        if (freezeZ){
            for (int i = 0; i < segments.Count; ++i){
                segments[i].localEulerAngles = new Vector3(segments[i].localEulerAngles.x, segments[i].localEulerAngles.y, frozenZ[i]);
            }
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, frozenHead);
        }        
        **/
		/*	int idx = locate(segments[1].position,distances[1],positions.Count-2);
			Vector3 newPos = trigTime(positions[idx+1],positions[idx],segments[1].position,distances[1]);
			
			Vector3 normal1 = newPos-segments[0].position;
			Vector3 normal2 = segments[0].position-segments[1].position;
			Vector3 axis = Vector3.Cross(normal1,normal2);
			axis.Normalize();
			float angle = Mathf.Acos(Vector3.Dot(normal1, normal2) / normal1.magnitude / normal2.magnitude);
			Quaternion newrot = Quaternion.AngleAxis(angle,axis);
			segments[0].localRotation = newrot;*/
		//segments[1].localPosition = segments[0].InverseTransformPoint(newPos);
		//segments[0].LookAt(segments[0].InverseTransformPoint(newPos));
		//segments[1].localRotation = segments[0].InverseTransformDirection(newPos.normalized);
	}
    float solveAngle(Vector3 pointA, Vector3 pointB, Vector3 pointC) {
        //pointA is segment to be positioned
        //pointB is point on spline
        //pointC is rotating segment
        Vector3 diffA = pointB - pointA;
        Vector3 diffB = pointC - pointB;
        Vector3 diffC = pointA - pointC;
        
        float lineA = diffA.magnitude;
        float lineB = diffB.magnitude;
        float lineC = diffC.magnitude;
        
        //rearranged law of cosines
        //angleC = Acos((c*c-a*a-b*b)/2*a*b)
        float angleC = Mathf.Rad2Deg * Mathf.Acos(((lineC*lineC)-(lineA*lineA)-(lineB*lineB)) / (2*lineA*lineB));
        
        return angleC;
    }
	int locate(Vector3 pos, float dist, int start) {
		for (int i = start; i>0; i--) {
			if (Vector3.Distance(pos,positions[i])>dist) {
				//Debug.Log(start-i);
				return i;
			}
		}
		return 0;
	}
	Vector3 trigTime(Vector3 A, Vector3 B, Vector3 C, float d) {
		Vector3 CmA = C - A; float mCmA = Mathf.Abs(CmA.magnitude);
		Vector3 BmA = B - A; float mBmA = Mathf.Abs(BmA.magnitude);
		float a = Mathf.Acos(Vector3.Dot(BmA,CmA)/(mCmA*mBmA));
		if (a >= 3.14) {
			BmA.Normalize();
			return A + d*BmA;
		}
		//Debug.Log(a);
		float cont = mCmA*Mathf.Sin(a)/d;
		if (Mathf.Abs(cont)>1) {
			BmA.Normalize();
			return A + d*BmA;
		}
		float b = Mathf.Asin(cont);
		//Debug.Log(b);
		float c = Mathf.PI - a - b;
		//Debug.Log(c);
		float mDmA = d*Mathf.Sin(c)/Mathf.Sin(a);
		return ((mBmA-mDmA)/mBmA)*A + (mDmA/mBmA)*B;
	}
}
/*

		int idx = positions.Count-2;
		for (int i = 1; i<segments.Count; ++i) {
			idx = locate(segments[i].position,distances[i],idx);
			Vector3 newPos = trigTime(positions[idx+1],positions[idx],segments[i].position,distances[i]);
			
			Vector3 normal1 = newPos-segments[i-1].position;
			Vector3 normal2 = segments[i-1].position-segments[i].position;
			Vector3 axis = Vector3.Cross(normal1,normal2);
			axis.Normalize();
			float angle = Mathf.Acos(Vector3.Dot(normal1, normal2) / normal1.magnitude / normal2.magnitude);
			Quaternion newrot = Quaternion.AngleAxis(angle,axis);
			segments[i-1].localRotation = newrot;
		}
		*/