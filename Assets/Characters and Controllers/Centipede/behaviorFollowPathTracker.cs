using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class behaviorFollowPathTracker : MonoBehaviour {
    
    public GameObject player;
    public float closeEnough = 2f;
    public float rotationSpeed = 4.5f;
    public float speed = 4f;
    
    private enum MovingState { moving = 0, pausing = 1, stopped = 2, sleeping = 3 }
    private MovingState currentState;
    private float pauseTimeout;
    private Vector3 target;
    private int index;    
    private Tracker tracker;
    
    /// Use this for initialization
	void Start () {
        tracker = player.GetComponent<Tracker>();
        pauseTimeout = 0f;
        currentState = MovingState.sleeping;
        Invoke("WakeUp", tracker.startTime);
	}
    
    public void WakeUp(){
        currentState = MovingState.stopped;
    }
	
    public void pauseAgent (float pauseTime){
        if (currentState == MovingState.moving){
            pauseTimeout = pauseTime;
            currentState = MovingState.pausing;   
        }
        else {
            currentState = MovingState.moving;             
        }
    }
    
	/// Update is called once per frame
	void Update () { 
        switch (currentState)
        {
        ///If currently sleeping
        case MovingState.sleeping:        
            break;        
        ///If currently stopped
        case MovingState.stopped:
            rigidbody.velocity = Vector3.zero;
            if (tracker.Location.Count > 0){
                target = tracker.Pop();
                if (target != Vector3.zero)
                    currentState = MovingState.moving;
            }
            break;        
        ///If currently pausing
        case MovingState.pausing:
            pauseTimeout -= Time.deltaTime;
            if (pauseTimeout <= 0f){                   
                currentState = MovingState.moving;                
            }
            break;
        ///If currently moving
        case MovingState.moving:
            ///update moveDirection            
            Vector3 moveDirection = target - transform.position;
            
            ///check if reached target, if so
            if (moveDirection.magnitude < closeEnough){
                if (tracker.Location.Count > 0){ ///If not at end of path
                    target = tracker.Pop();
                }
                else{ ///If at end of path
                    currentState = MovingState.stopped;
                }
            }
            else{
                ///rotate to face target
                Quaternion rot = Quaternion.LookRotation((moveDirection).normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.smoothDeltaTime * rotationSpeed);
                
                ///move towards target
                rigidbody.velocity = transform.forward * speed;
            }
            break;
        default:
            break;
        }
	}
}
