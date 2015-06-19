using UnityEngine;
using System.Collections;

public class ParkourAnimationState : MonoBehaviour {
    // animator
    public Animator animation;
    // animation info state
    public AnimatorStateInfo state;
    // controller
    public ParkourController controller;

	//void Awake () {
    //    // we MUST have the axe parented to this game object, and the axe needs
    //    // to have the animator AND animation components
    //    if (!animation) {
    //        animation = transform.Find("Axe").GetComponent<Animator>();
    //        Debug.Log(transform.Find("Axe") == null);
    //        if (!animation) {
    //            Debug.Log("The character the controller is attached to doesn't have animations.  Moving them might look weird.");
    //        }
    //    }
	//}

    void Start() {
        // we MUST have the axe parented to this game object, and the axe needs
        // to have the animator AND animation components
        if (!animation) {
            animation = transform.Find("Axe").GetComponent<Animator>();
            Debug.Log(transform.Find("Axe") == null);
            if (!animation) {
                Debug.Log("The character the controller is attached to doesn't have animations.  Moving them might look weird.");
            }
        }

        controller = GetComponent<ParkourController>();
    }
    
    void UpdateState() {
        // update the current upward velocity
        animation.SetFloat("UpwardVel", controller.velocity.y);
        // set the current velocity square magnitude
        animation.SetFloat("VelSqrMag", controller.velocity.sqrMagnitude);

        // are we falling still? did we land?
        if (controller.isGrounded()) {
            animation.SetBool("Landed", true);
        }
        else {
            animation.SetBool("Landed", false);
        }

        // are we jumping
        if (controller.isJumping()) {
            animation.SetBool("Jump", true);
        }
        else {
            animation.SetBool("Jump", false);
        }

        // are we climbing?
        if (controller.isClimbing()) {
            animation.SetBool("Climbing", true);
        }
        else {
            animation.SetBool("Climbing", false);
        }
        
        // did we just hit a wall?
        if (controller.startClimbing()) {
            animation.SetBool("HitClimbAble", true);
        }
        else {
            animation.SetBool("HitClimbAble", true);
        }
    }
	
	// Update is called once per frame
	void Update () {
        state = animation.GetCurrentAnimatorStateInfo(0);
        UpdateState();
	}
};
