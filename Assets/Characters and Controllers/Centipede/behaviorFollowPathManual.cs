using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class behaviorFollowPathManual : MonoBehaviour {
    
    public List<Transform> path = new List<Transform>();
    public int startIndex = 0;
    public float closeEnough = 2f;
    public float rotationSpeed = 4.5f;
    public float speed = 12f;
    public bool looping = false;

    private enum MovingState { moving = 0, pausing = 1, stopped = 2 }
    private MovingState currentState;
    private float pauseTimeout;
    private Transform waypoint;
    private int index;
    
    private Animator animator;
    
    /// Use this for initialization
	public void Start () {
        animator = GetComponent<Animator>();
        currentState = MovingState.moving;
        pauseTimeout = 0f;
        
        if (startIndex > 0 && startIndex < path.Count) 
            index = startIndex;
        else index = 0;
        waypoint = path[index];
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
        ///If currently stopped
        if (currentState == MovingState.stopped){
            animator.SetBool("StartMoving", false);
            rigidbody.velocity = Vector3.zero;
            return;
        }
        
        ///If currently pausing
        if (currentState == MovingState.pausing){
            animator.SetBool("StartMoving", false);
            pauseTimeout -= Time.deltaTime;
            if (pauseTimeout <= 0f){                   
                currentState = MovingState.moving;                
            }
            return;
        }
        
        ///If currently moving
        else if (currentState == MovingState.moving){
            animator.SetBool("StartMoving", true);
            ///update target
            Vector3 target = waypoint.position;
            Vector3 moveDirection = target - transform.position;
            
            ///check if reached target, if so
            if (moveDirection.magnitude < closeEnough){
                if (index < path.Count - 1){ ///If not at end of path
                    index++;
                    waypoint = path[index];
                }
                else{ ///If at end of path
                    if (looping == true){ ///If agent loops on path
                        index = 0;
                        waypoint = path[index];
                    }
                    else ///If agent doesn't loop
                        currentState = MovingState.stopped;
                }
            }
            else{
                ///rotate to face waypoint
                Quaternion rot = Quaternion.LookRotation((moveDirection).normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.smoothDeltaTime * rotationSpeed);
                
                ///move towards waypoint
                rigidbody.velocity = transform.forward * speed;
            }
        }
	}
}
