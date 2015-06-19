using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using CyclicCoordinateDescentNS;

public class behaviorCentipedeCCD : MonoBehaviour {

    public Transform head;
    public Transform butt;
    public float deltaModifier = 4f;
    public int maxIterations = 1;
    public double acceptableRange = 0.1;
        
    //segments exclude the head and butt
    public List<Transform> segment = new List<Transform>();    
    
    private List<Transform> centipede = new List<Transform>();  
    private int segmentLength;
    private int centipedeLength;
    private List<Bone_2D_CCD> bones;
    private List<Vector3> joint = new List<Vector3>();
    private List<Vector3> spline = new List<Vector3>();
    private float deltaMin;
    private float deltaTrigger;
    private Vector3 lastPos;
        
	// Use this for initialization
	void Start () {
        segmentLength = segment.Count;
        //create full centipede list
        centipede.Add(head);
        for (int i = 0; i < segmentLength; i++)
            centipede.Add(segment[i]);
        centipede.Add(butt);
        
        centipedeLength = centipede.Count;
        //create CCD bones list
        bones = createBoneList(centipede);
        //calculate deltaMin
        deltaMin = Mathf.Abs((butt.position - head.position).magnitude / ((2 * centipedeLength)-1));
        /*
        float average = 0f;
        for (int i = 0; i < centipedeLength-1; i++){
            average = (centipede[i+1].position - centipede[i].position).magnitude / 2;
        }
        average = average / centipedeLength-1;
        deltaMin = Mathf.Abs(average);
        */
        
        deltaModifier = Mathf.Max(deltaModifier, 1f); //set minimum 1
        deltaTrigger = deltaMin / deltaModifier;
        print (deltaMin + " - " + deltaTrigger);
        lastPos = head.position;
        
        //set up spline starting positions and joint Vectors
        for (int i = 0; i < centipedeLength-1; i++){
            spline.Add(centipede[i].position);
            Vector3 newJoint = new Vector3(
                centipede[i].position.x //- (centipede[i+1].position.x - centipede[i].position.x)/2 
                ,centipede[i].position.y //- (centipede[i+1].position.y - centipede[i].position.y)/2 
                ,centipede[i].position.z - (centipede[i+1].position.z - centipede[i].position.z)/2 
            );
            spline.Add(newJoint);
            joint.Add(newJoint);            
        }
        spline.Add(butt.position);        
	}
	
	// Update is called once per frame
	void Update () {
        //if moved enough to update spline
        if ( (head.position - lastPos).magnitude >=  deltaTrigger ){
            lastPos = head.position;
            
            System.Text.StringBuilder sb = new System.Text.StringBuilder();     
                sb.Append("bones ");
                for (int i = 0; i < bones.Count; i++){
                        sb.Append(" ");
                        sb.Append(bones[i].angle);
                        sb.Append(" ");
                }
                Debug.Log(sb.ToString());
            
            updateBoneList();
            //bones.Reverse();            
            calculateCCD();
            //bones.Reverse();
            for (int i = 1; i < centipedeLength; i++)
                centipede[i].localRotation = Quaternion.Euler(0f, (float)bones[i].angle, 0f);
        }	    
	}
    
    public List<Vector3> Spline(){
        return spline;
    }
    
    void updateSpline() {
        //update spline list
        spline.Insert(0,head.position); // add head.position to front
        spline.RemoveAt(spline.Count-1); // pop last value
        
        //update rotation
        for (int i = 0; i < segmentLength; i++){
            float angle;
            Vector3 pointA;
            Vector3 pointB = segment[i].InverseTransformPoint(spline[(i+1)*2]);
            //TRY RELATIVE TO PREVIOUS SEGMENT OR TO CURRENT SEGMENT
            if (i == segmentLength-1) pointA = segment[i].InverseTransformPoint(butt.position);
            else pointA = segment[i].InverseTransformPoint(segment[i+1].position);
            
            angle = solveAngle(pointA, pointB, segment[i].InverseTransformPoint(segment[i].position));            
            if (segment[i].InverseTransformPoint(pointB).x <= 0) angle = angle * -1; //if spline to rightside
            //segment[i].rotation = Quaternion.Euler(0,angle,0);
            segment[i].Rotate(0,angle/10f,0);
        } 
        
    }
    
    void calculateCCD(){
        //update spline list
        spline.Insert(0,head.position); // add head.position to front
        spline.RemoveAt(spline.Count-1); // pop last value
        
        //calculate CCD until target reached or maxIterations reached
        int numIterations = 0;
        while (numIterations < maxIterations){
            CCDResult result = CCD.CalcIK_2D_CCD(ref bones, spline[spline.Count-2].x, spline[spline.Count-2].z, acceptableRange);
            if (result == CCDResult.Success){
                Debug.Log("CCD resulted in Success!!!");
                return;
            }
            else if (result == CCDResult.Failure){
                Debug.LogError("CCD resulted in Failure...");
                return;
            }
            numIterations++;
        }
        Debug.LogWarning("CCD timed out early.");
        return;        
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
    
    List<Bone_2D_CCD> createBoneList(List<Transform> centipede){
        List<Bone_2D_CCD> bones = new List<Bone_2D_CCD>();
        for (int i = 0; i < centipede.Count; i++){
            Bone_2D_CCD bone = new Bone_2D_CCD();
            bone.x = centipede[i].position.x;
            bone.y = centipede[i].position.z;
            bone.angle = head.InverseTransformPoint(centipede[i].eulerAngles).y;
            bones.Add(bone);
        }
        if (bones.Count != centipede.Count)
            Debug.LogWarning("bones List and centipede List lengths not equal.");
        return bones;
    }
    
    void updateBoneList(){        
        for (int i = 0; i < centipede.Count; i++){
            bones[i].x = centipede[i].position.x;
            bones[i].y = centipede[i].position.z;
            bones[i].angle = head.InverseTransformPoint(centipede[i].eulerAngles).y;       
        }        
        return;
    }

}    