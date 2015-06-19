using UnityEngine;
using System.Collections;

public enum BobState {
	none,
	walk
};

public class CameraBobber : MonoBehaviour {
	// mouse look
	public float sensitivityY = 15F;

	public float minimumY = -60F;
	public float maximumY = 60F;

	float rotationY = 0F;
	
	// camera bob
	private float timer        = 0.0f;
	public float bobbingSpeed  = 0.18f;
	public float bobbingAmount = 0.2f;
	private float midpoint     = 0.0f;
	public float speed         = 1.0f;
	public BobState state      = BobState.none;
	public float dist;
	public float multiplier = 1.0f;
	
	public float totalRotationY = 0.0f;
	public float rotateChange = 0.0f;
	// fixate
	public bool fixateOnObject = true;
	public float fixateWeight = 1.0f;
	public float fixateMinTaperDist = 0.5f;
	public float fixateMaxTaperDist = 2.0f;
	private float dt;
	
	// Update is called once per frame
	void Update() {}
	void LateUpdate() {
		rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
		rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
		//transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
		totalRotationY = rotationY;
		switch (state) {
		case BobState.walk:
			if (fixateOnObject)
				rayWalk();
			else
				bobWalk();
			break;
		};
		transform.localEulerAngles = new Vector3(-totalRotationY, transform.localEulerAngles.y, 0);
	}
	void Start() {
		// Make the rigid body not change rotation
		if (rigidbody)
			rigidbody.freezeRotation = true;
		
		midpoint = transform.localPosition.y;
	}
	
	float fslerp(float a, float b, float t) {
		return Vector3.Slerp(new Vector3(0.0f,a,0.0f), new Vector3(0.0f, b, 0.0f), t)[1];
	}
	void rayWalk() {
		var waveslice = 0.0f; 
		var horizontal = Input.GetAxis("Horizontal"); 
		var vertical = Input.GetAxis("Vertical");
		if (Mathf.Abs(horizontal) == 0 && Mathf.Abs(vertical) == 0) { 
			timer = 0.0f; 
		} else { 
			waveslice = Mathf.Sin(timer); 
			timer = timer + bobbingSpeed; 
			if (timer > Mathf.PI * 2) { 
				timer = timer - (Mathf.PI * 2); 
			}
		}
		float translateChange = 0.0f;
		if (waveslice != 0) { 
			translateChange = waveslice * bobbingAmount; 
			var totalAxes = Mathf.Abs(horizontal) + Mathf.Abs(vertical); 
			totalAxes = Mathf.Clamp (totalAxes, 0.0f, 1.0f); 
			translateChange = totalAxes * translateChange; 
			
		} else { 
			transform.localPosition = new Vector3(transform.localPosition[0], midpoint*multiplier, transform.localPosition[2]); 
		}
		transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
		transform.localPosition = new Vector3(transform.localPosition[0], midpoint*multiplier, transform.localPosition[2]);
		
		var ray = Camera.main.ScreenPointToRay (new Vector2(Screen.width/2,Screen.height/2));
		RaycastHit hitinfo = new RaycastHit();
		Physics.Raycast(ray, out hitinfo);
		dist = hitinfo.distance;
		if (dist > 0) {
			float weight = 1.0f;
			float angle = Mathf.Acos(Vector3.Dot(hitinfo.normal,ray.direction));
			angle /= Mathf.PI;
			weight = fslerp(0.0f,1.0f,angle);
			rotateChange = Mathf.Rad2Deg*Mathf.Atan2(Mathf.Sqrt(dist*dist+translateChange*translateChange),translateChange)-90;
			rotateChange*=weight;
			
			if (dist < fixateMaxTaperDist) {
				weight = fslerp(0.0f, 1.0f, (dist - fixateMinTaperDist)/(fixateMaxTaperDist-fixateMinTaperDist));
				if (dist < fixateMinTaperDist) {
					weight = 0.0f;
				}
				rotateChange = 0.0f;
			}
		} else {
			rotateChange = 0.0f;
		}
		totalRotationY = rotationY + rotateChange*fixateWeight;
		transform.localPosition = new Vector3(transform.localPosition[0], midpoint*multiplier+translateChange, transform.localPosition[2]);
	}
	
	void bobWalk() {
		var waveslice = 0.0f; 
		var horizontal = Input.GetAxis("Horizontal"); 
		var vertical = Input.GetAxis("Vertical"); 
		if (Mathf.Abs(horizontal) == 0 && Mathf.Abs(vertical) == 0) { 
			timer = 0.0f; 
		} else { 
			waveslice = Mathf.Sin(timer); 
			timer = timer + bobbingSpeed; 
			if (timer > Mathf.PI * 2) { 
				timer = timer - (Mathf.PI * 2); 
			} 
		} 
		if (waveslice != 0) { 
			var translateChange = waveslice * bobbingAmount; 
			var totalAxes = Mathf.Abs(horizontal) + Mathf.Abs(vertical); 
			totalAxes = Mathf.Clamp (totalAxes, 0.0f, 1.0f); 
			translateChange = totalAxes * translateChange; 
			transform.localPosition = new Vector3(midpoint + translateChange, transform.localPosition[1], transform.localPosition[2]); 
		} else { 
			transform.localPosition = new Vector3(midpoint, transform.localPosition[1], transform.localPosition[2]); 
		}
	}
}
