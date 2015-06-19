using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using HeadBobber;

// Use this for initialization
public enum MoveState {
	none,
	movingDownToCrouch,
	crouching,
	movingUpFromCrouch,
	slideCrouchTransition,
	movingDownToSlide,
	earlySlide,
	lateSlide,
	movingUpFromSlide,
	startClimbing,
	climbing,
	hoisting,
    vaultTakeOff,
    vaultPushOff,
    vaultPlant,
    vaultLand
};

public enum MoverState {
	none   = 0,
	normal = 1,
	slide  = 2,
	climb  = 3,
	ledge  = 4
};
[System.Serializable]
public class SlideVariables {
	public float minEnterSpeed = 3.0f;
	public float heightScale   = 0.25f;
	public float minTime       = 1.0f;
	public float maxTime       = 2.0f;
	public float downTime      = 0.1f;
	public float upTime        = 0.1f;
	public float speed         = 0.7f;
	public float crouchTransitionTime  = 0.4f;
	public float crouchTransitionSpeed = 0.2f;
	public float height        = 1.0f;
};
[System.Serializable]
public class CrouchVariables {
	public float downTime    = 0.1f;
	public float upTime      = 0.1f;
	public float heightScale = 0.5f;
	public float speed       = 0.5f;
	public float height      = 1.0f;
};
[System.Serializable]
public class ClimbVariables {
	public int zone       = 0;
	public int topZone    = 0;
	public int groundZone = 0;
	public int keyDown    = 0;
};
[System.Serializable]
public class VaultVariables {
	public bool can         = false;
	public float scoot      = 1.0f;
	public float slowDown   = 0.1f;
	public float boost      = 1.0f;
	public float maxHeight  = 3.0f;
    public bool plant       = false;
};
public class ParkourController : MonoBehaviour {
    //public Vector3 start_position;
	public SlideVariables  Slide    = new SlideVariables();
	public CrouchVariables Crouch   = new CrouchVariables();
	public ClimbVariables  Climb    = new ClimbVariables();
	public VaultVariables  Vault    = new VaultVariables();
    //public AnimationVariables Anim  = new AnimationVariables();
    //public AnimationState Anim;
	private int crouchKey  = 0;
	public int crouchKeep = 0;
	private HashSet<Collider> crouchZones = new HashSet<Collider>();
	// move vars
	private float speed = 1.0f;
	private float dt = 0.0f;  // for use in the movement FSM
	public MoveState state = MoveState.none;
	public MoverState moverState = MoverState.none;

    // check if you're grounded
    public bool grounded = true;
	
	private float gravity;
    //private int vaulting = 0;
    private Vector3 vaultEndVelocity = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 velocity;
	private float oldHeight;
	private float height = 2.0f;
	private float startHeight = 2.0f;
	
	public float distFromGround = 1.0f;
	public float distFromWall;
	public Vector3 climbPlane = (new Vector3(1,0,1)).normalized;
	public Vector3 slideDirection = new Vector3(0.0f,0.0f,0.0f);
	
	private CharacterMotor motor;
	private CharacterController controller;
	private CameraBobber bobber;
    private Transform camera, axe;
	private float camHeight;
    
    void Awake() {
        // ANIMATION INIT
        // we MUST have the axe parented to this game object, and the axe needs
        // to have the animator AND animation components
        //Anim._animation = transform.Find("Axe").GetComponent<Animation>();
        //Debug.Log(transform.Find("Axe") == null);
        //if (!Anim._animation) {
        //    Debug.Log("The character the controller is attached to doesn't have animations.  Moving them might look weird.");
        //}
        // ANIMATION INIT
    }

	void Start () {
        CameraFade.StartAlphaFade(Color.white, true, 1.0f, 0.0f);
		motor = GetComponent<CharacterMotor>();
		gravity = motor.movement.gravity;
		controller = GetComponent<CharacterController>();
		height = controller.height;
		bobber = GetComponentInChildren<CameraBobber>();
		dt = 1.0f;
        //start_position = GetComponent<Transform>().position;
        camera = transform.FindChild("Axe/Root/Neck/Head/POVCamera/Main Camera").transform;//GetComponent<Camera>().transform;
		camHeight = camera.transform.localPosition.y;
		axe = transform.FindChild("Axe");
		//Physics.IgnoreCollision(collider,GetComponent<CapsuleCollider>());
	}
	
	float fslerp(float a, float b, float t) {
		return Vector3.Slerp(new Vector3(0.0f,a,0.0f), new Vector3(0.0f, b, 0.0f), t)[1];
	}
	void setHeight(float height) {
		controller.height = height;
		controller.center = new Vector3(0.0f,height*0.5f,0.0f);
		axe.localPosition = new Vector3(axe.localPosition.x,height*0.75f-0.5f,axe.localPosition.z);
	}
	void UpdateMoveState() {
		switch (state) {
		case MoveState.none:
			if (crouchKey>0) {
				Vector2 xzv = new Vector2(motor.movement.velocity.x,motor.movement.velocity.z);
				if (xzv.magnitude >= Slide.minEnterSpeed) {
					state = MoveState.movingDownToSlide;
					slideDirection = motor.movement.velocity.normalized;
					moverState = MoverState.slide;
				} else {
					state = MoveState.movingDownToCrouch;
					//controller.height = 0.8f;
					//controller.center = new Vector3(0.0f,0.4f,0.0f);
				}
				dt = 0.0f;
			} else if (Climb.zone>0 && Climb.groundZone>0 && Climb.keyDown > 0) {
				state = MoveState.startClimbing;
				moverState = MoverState.climb;
				Climb.keyDown = 0;
				motor.movement.gravity = 0;
				bobber.state = BobState.none;
			} else if (Climb.topZone>0) {
				state = MoveState.hoisting;
				moverState = MoverState.climb;
				Climb.keyDown = 0;
				motor.movement.gravity = 0;
				bobber.state = BobState.none;
			} else if (Vault.can && Input.GetButton("Jump")) {
                state = MoveState.vaultTakeOff;
                vaultEndVelocity = velocity - motor.inputMoveDirection * Vault.slowDown;
				oldHeight = transform.position.y;
            } else {
				speed = 1.0f;
				bobber.state = BobState.walk;
				bobber.multiplier = 1.0f;
				moverState = MoverState.normal;
			}
			break;
		case MoveState.startClimbing:
			if (Climb.zone == 0 || Climb.keyDown > 0) {
				state = MoveState.none;
				Climb.keyDown = 0;
				motor.movement.gravity = gravity;
			} else if (Climb.groundZone == 0) {
				state = MoveState.climbing;
			} else if (Climb.topZone > 0) {
				state = MoveState.hoisting;
			}
			bobber.state = BobState.none;
			break;
		case MoveState.climbing:
			if (Climb.zone == 0 || Climb.keyDown > 0) {
				state = MoveState.none;
				Climb.keyDown = 0;
				motor.movement.gravity = gravity;
			} else if (Climb.groundZone > 0) {
				state = MoveState.none;
				Climb.keyDown = 0;
				motor.movement.gravity = gravity;
			} else if (Climb.topZone > 0) {
				state = MoveState.hoisting;
			}
			break;
		case MoveState.hoisting:
			if (Climb.topZone == 0) {
				state = MoveState.none;
				motor.movement.gravity = gravity;
			} else if (Climb.keyDown > 0) {
				state = MoveState.none;
				Climb.keyDown = 0;
				motor.movement.gravity = gravity;
			}
			moverState = MoverState.ledge;
			break;
		case MoveState.movingDownToSlide:
			// state transitions
			if (dt>=Slide.downTime) {
				state = MoveState.earlySlide;
				// make sure the exit values are correct
				speed = fslerp(1.0f,Slide.speed,dt/Slide.maxTime);
				setHeight(startHeight * Slide.heightScale);
				bobber.multiplier = fslerp(1.0f,Slide.heightScale, dt/Slide.downTime);
			} else {
			// actions for this state
				speed = fslerp(1.0f,Slide.speed,dt/Slide.maxTime);
				//controller.height = height * fslerp(1.0f,Slide.heightScale,dt/Slide.downTime);
                setHeight(startHeight * fslerp(1.0f,Slide.heightScale,dt/Slide.downTime));
				bobber.multiplier = fslerp(1.0f,Slide.heightScale, dt/Slide.downTime);
			}
			bobber.state = BobState.none;
			break;
		case MoveState.earlySlide:
			// state transitions
			if (dt>=Slide.minTime) {
				if (crouchKey > 0) {
					state = MoveState.lateSlide;
				} else if (crouchKeep == 0) {
					state = MoveState.movingUpFromSlide;
				} else {
					state = MoveState.slideCrouchTransition;
				}
				// make sure the exit values are correct
				speed = fslerp(1.0f,Slide.speed,dt/Slide.maxTime);
			} else {
			// actions for this state
				speed = fslerp(1.0f,Slide.speed,dt/Slide.maxTime);
			}
			break;
		case MoveState.lateSlide:
			// state transitions
			if (dt >= Slide.maxTime) {
				if (crouchKey > 0) {
					state = MoveState.slideCrouchTransition;
				} else if (crouchKeep == 0) {
					state = MoveState.movingUpFromSlide;
				} else {
					state = MoveState.slideCrouchTransition;
				}
				// make sure the exit values are correct
				speed = Slide.speed;
			} else {
			// actions for this state
				speed = fslerp(1.0f,Slide.speed,dt/Slide.maxTime);
			}
			break;
		case MoveState.movingUpFromSlide:
			// state transitions
			if (crouchKeep > 0) {
				state = MoveState.slideCrouchTransition;
				dt = 0.0f;
			} else if (dt >= Slide.upTime) {
				state = MoveState.none;
				dt = 0.0f;
				var oldHeight = controller.height;
				setHeight(startHeight);
				//transform.localPosition -= new Vector3(0.0f, (oldHeight-controller.height)*0.55f, 0.0f);
                bobber.multiplier = fslerp( Slide.heightScale,1.0f,dt/Slide.upTime);
			} else {
			// actions for this state
				var oldHeight = controller.height;
				//controller.height = height * fslerp(Slide.heightScale, 1.0f, (dt-Slide.maxTime)/Slide.upTime);
                controller.height = startHeight * fslerp(Slide.heightScale,1.0f,dt/Slide.downTime);
				controller.center = new Vector3(0.0f,controller.height*0.5f,0.0f);
				axe.localPosition = new Vector3(axe.localPosition.x,controller.height*0.5f,axe.localPosition.z);
				//transform.localPosition -= new Vector3(0.0f, (oldHeight-controller.height)*0.55f, 0.0f);
				moverState = MoverState.normal;
                bobber.multiplier = fslerp( Slide.heightScale,1.0f,dt/Slide.upTime);
			}
			break;
		case MoveState.slideCrouchTransition:
			// state transitions
			if (dt >= Slide.crouchTransitionTime) {
				state = MoveState.crouching;
				dt = 0.0f;
			// actions for this state
			} else if(dt <= Slide.crouchTransitionTime*0.25f) {
				speed = fslerp(Slide.speed, Slide.crouchTransitionSpeed, dt/(Slide.crouchTransitionTime*0.25f));
			} else if(dt <= Slide.crouchTransitionTime*0.75f) {
				speed = Slide.crouchTransitionSpeed;
				setHeight(startHeight*fslerp(Slide.heightScale,Crouch.heightScale,(dt-Slide.crouchTransitionTime*0.25f)/(Slide.crouchTransitionTime*0.5f)));
				moverState = MoverState.normal;
			} else {
				speed = fslerp(Slide.crouchTransitionSpeed,Crouch.speed,(dt-Slide.crouchTransitionTime*0.75f)/(Slide.crouchTransitionTime*0.25f));
				setHeight(startHeight * Crouch.heightScale);
				bobber.state = BobState.walk;
				moverState = MoverState.normal;
			}
			break;
		case MoveState.movingDownToCrouch:
			// state transitions
			if (dt>=Crouch.downTime) {
				state = MoveState.crouching;
				setHeight(startHeight * Crouch.heightScale);
				dt = 0.0f;
			} else if (crouchKey == 0 && crouchKeep == 0) {
				state = MoveState.movingUpFromCrouch;
                bobber.multiplier = fslerp(1.0f,Crouch.heightScale, dt/Crouch.downTime);
				dt = 0.0f;
			} else {
			// actions for this state
				speed = Crouch.speed;
                //var oldHeight = controller.height;
				//controller.height = height * fslerp(1.0f,Crouch.heightScale,dt/Crouch.downTime);
				setHeight (startHeight * fslerp(1.0f,Crouch.heightScale,dt/Crouch.downTime));
				//camera.transform.y -= (controller.height - old_height);
                bobber.multiplier = fslerp(1.0f,Crouch.heightScale, dt/Crouch.downTime);
				bobber.state = BobState.walk;
			}
			break;
		case MoveState.crouching:
			// state transitions
			if (crouchKey == 0 && crouchKeep == 0) {
				state = MoveState.movingUpFromCrouch;
				dt = 0.0f;
				//controller.height = 2.0f;
				//controller.center = new Vector3(0.0f,1.0f,0.0f);
			} else {
			// actions for this state
				speed = Crouch.speed;
				bobber.state = BobState.walk;
				moverState = MoverState.normal;
			}
			break;
		case MoveState.movingUpFromCrouch:
			if (crouchKeep > 0 || crouchKey > 0) {
				state = MoveState.movingDownToCrouch;
				dt = 0.0f;
			} else if(dt>=Crouch.upTime) {
				state = MoveState.none;
                var oldHeight = controller.height;
				//controller.height = height;
                //camera.transform.y -= (controller.height - old_height);
                setHeight(startHeight);
                bobber.multiplier = fslerp( Crouch.heightScale,1.0f,dt/Crouch.upTime);
				//transform.localPosition += new Vector3(0.0f, (oldHeight-controller.height)*0.55f+0.001f, 0.0f);
			} else {
			// actions for this state
				speed = Slide.crouchTransitionSpeed;
				//float old_height = controller.height;
                var oldHeight = controller.height;
				setHeight (startHeight * fslerp(Crouch.heightScale,1.0f,dt/Crouch.downTime));
				//controller.height = height*fslerp(Crouch.heightScale,1.0f,dt/Crouch.upTime);
                //camera.transform.y -= (controller.height - old_height);
                bobber.multiplier = fslerp(Crouch.heightScale, 1.0f,dt/Crouch.upTime);
				//transform.localPosition += new Vector3(0.0f, (oldHeight-controller.height)*0.55f+0.001f, 0.0f);
				//transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + (oldHeight-controller.height)*0.45f+100.0f, transform.localPosition.z);
			}
			break;
        case MoveState.vaultTakeOff:
            bobber.state = BobState.none;
			//motor.movement.velocity += new Vector3(0,Time.deltaTime*4,0);
            motor.SetVelocity(velocity + new Vector3(0, Time.deltaTime * 4, 0));
            if (transform.position.y > oldHeight+Vault.boost && Vault.plant) {//distFromGround > Vault.maxHeight
                state = MoveState.vaultPlant;
            }
            break;
        case MoveState.vaultPlant:
            Vector3 dir = transform.forward;
            dir.y = 0;
            dir.Normalize();
            if (velocity.sqrMagnitude >= (vaultEndVelocity + dir * Vault.scoot).sqrMagnitude) {
                state = MoveState.vaultPushOff;
                //motor.SetVelocity(velocity + motor.inputMoveDirection * Vault.scoot);
                //motor.SetVelocity(velocity + Vector3.forward * Vault.scoot);
            }
            motor.movement.velocity = Vector3.Slerp(motor.movement.velocity, vaultEndVelocity + dir * Vault.scoot, Time.deltaTime);
            //motor.SetVelocity(Vector3.Slerp(velocity, vaultEndVelocity + dir * Vault.scoot, Time.time));
            break;
        case MoveState.vaultPushOff:
            motor.SetVelocity(Vector3.Lerp(velocity, vaultEndVelocity, Time.time));
            if (velocity.sqrMagnitude == vaultEndVelocity.sqrMagnitude) {
                state = MoveState.none;
                bobber.state = BobState.walk;
            }
            break;
		};
	}
	// Update is called once per frame (sort of)
	void Update () {
		RaycastHit hitinfo = new RaycastHit();
		//RaycastHit TOP = new RaycastHit();
		//RaycastHit BOT = new RaycastHit();
        //RaycastHit vaultHit = new RaycastHit();
		//int layermask = 1+2+4+8+16; // Player
		float height = controller.height*transform.localScale.y;
		Vector3 bottom = transform.position-new Vector3(0.0f,height*0.5f,0.0f);
		//Vector3 top = transform.position+new Vector3(0.0f,height*0.49f,0.0f);
		
		/*Physics.Raycast(bottom, Vector3.up, out BOT);//, Mathf.Infinity, layermask);
		Physics.Raycast(top, Vector3.down, out TOP);//, Mathf.Infinity, layermask);
		float diff = Mathf.Abs(TOP.point.y-BOT.point.y);
		if (TOP.point != new Vector3(0,0,0) && BOT.point != new Vector3(0,0,0) &&
			moverState != MoverState.climb && moverState!=MoverState.ledge) {
			float dtop = Mathf.Abs(top.y - TOP.point.y);
			float dbot = Mathf.Abs(BOT.point.y - bottom.y);
			if (diff < height && dbot >= dtop) {// && BOT.point.y <= top.y && TOP.point.y >= bottom.y) {
				transform.localPosition += new Vector3(0,BOT.distance*1.01f,0);
				Debug.Log("CATCHING: " + (TOP.distance+BOT.distance) + ", " + height*transform.localScale.y);
				//Debug.Log(diff);
			}
		}*/

        // ANIMATION sector
        // check first to make sure the animations exist
        //if (Anim._animation) {
        //    // if the character is jumping
        //    if (motor.IsJumping()) {//state == MoveState.Jumping) {
        //        Anim._animation[Anim.jumpAnimation.name].speed = Anim.jumpAnimationSpeed;
        //        Anim._animation[Anim.jumpAnimation.name].wrapMode = WrapMode.Once;
        //        Anim._animation.CrossFade(Anim.jumpAnimation.name);
        //        // next animation
        //        Anim._animation[Anim.fallAnimation.name].speed = Anim.fallAnimationSpeed;
        //        Anim._animation[Anim.fallAnimation.name].wrapMode = WrapMode.Once;
        //        Anim._animation.CrossFadeQueued(Anim.fallAnimation.name, Anim.crossFadeTime, QueueMode.CompleteOthers);
        //    }
        //    else if (isGrounded()) {
        //        Anim._animation[Anim.hitGroundAnimation.name].speed = Anim.hitGroundAnimationSpeed;
        //        Anim._animation[Anim.hitGroundAnimation.name].wrapMode = WrapMode.Once;
        //        Anim._animation.CrossFadeQueued(Anim.hitGroundAnimation.name, Anim.crossFadeTime, QueueMode.CompleteOthers);
        //    }
        //    //else if (jumping to grounded) {
        //    //    Anim._animation[land
        //    //}
        //    else {
        //        if (state == MoveState.startClimbing) {
        //            Anim.climbAnim += 1;
        //            Anim._animation[Anim.digWallAnimation.name].speed = Anim.digWallAnimationSpeed;
        //            Anim._animation[Anim.digWallAnimation.name].wrapMode = WrapMode.Once;
        //            Anim._animation.CrossFade(Anim.digWallAnimation.name);

        //            Anim.climbAnim += 1;
        //            Anim._animation[Anim.toNeutralAnimation.name].speed = Anim.toNeutralAnimationSpeed;
        //            Anim._animation[Anim.toNeutralAnimation.name].wrapMode = WrapMode.Once;
        //            Anim._animation.CrossFadeQueued(Anim.liftRightAnimation.name, Anim.crossFadeTime, QueueMode.CompleteOthers);
        //        }
        //        else if (state == MoveState.climbing) {
        //            if (Anim.climbAnim % 2 == 0) { // test if even or odd
        //                Anim.climbAnim += 1;
        //                Anim._animation[Anim.liftLeftAnimation.name].speed = Anim.liftLeftAnimationSpeed;
        //                Anim._animation[Anim.liftLeftAnimation.name].wrapMode = WrapMode.Once;
        //                Anim._animation.CrossFadeQueued(Anim.liftLeftAnimation.name, Anim.crossFadeTime, QueueMode.CompleteOthers);
        //            }
        //            else {
        //                Anim.climbAnim += 1;
        //                Anim._animation[Anim.liftRightAnimation.name].speed = Anim.liftRightAnimationSpeed;
        //                Anim._animation[Anim.liftRightAnimation.name].wrapMode = WrapMode.Once;
        //                Anim._animation.CrossFadeQueued(Anim.liftRightAnimation.name, Anim.crossFadeTime, QueueMode.CompleteOthers);
        //            }
        //            if (Climb.topZone > 0) {
        //                Anim.climbAnim = 0;
        //            }
        //        }
        //        //else if (state == ) {
        //        //}
        //        else { // fell through to idle
        //            Anim._animation[Anim._animation.clip.name].speed = Anim.idleAnimationSpeed;
        //            Anim._animation[Anim._animation.clip.name].wrapMode = WrapMode.Once;
        //            Anim._animation.CrossFade(Anim._animation.clip.name);
        //        }
        //    }
        //}

        // ANIMATION sector

        // FOR DEBUGGING/TESTING
        if (Input.GetKeyDown(KeyCode.K)) {
            GetComponent<DeathScript>().Die();
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.LoadLevel(0);
        }

		Physics.Raycast(bottom, Vector3.down, out hitinfo);//, Mathf.Infinity, layermask);
		distFromGround = hitinfo.distance;
		
		distFromWall = 9999999;
		for (uint i = 0; i<8; ++i) {
			Vector3 dir = Vector3.forward;
			dir[1] = 0;
			dir.Normalize();
			dir = Quaternion.AngleAxis(45*i, Vector3.up) * dir;
			Physics.Raycast(bottom, dir, out hitinfo);
			if (hitinfo.distance < distFromWall) {
				distFromWall = hitinfo.distance;
				climbPlane = Quaternion.AngleAxis(90, Vector3.up) * dir;
			}
		}
		
		dt += Time.deltaTime;
		bobber.speed = speed;
		
        // current velocity from the character motor
        velocity = motor.movement.velocity;
		
		if (Input.GetKeyDown(KeyCode.Space)) {
			Climb.keyDown++;
		} else if (Input.GetKeyDown(KeyCode.Space)) {
			if (Climb.keyDown > 0) {
				Climb.keyDown--;
			}
        }
        // vault
        //Physics.Raycast(bottom, Vector3.forward, out vaultHit);
		//Vault.can = Vault.can && 
        //if (state == MoveState.vaultLand) {
        //    motor.SetVelocity(Vector3.Lerp(velocity + motor.inputMoveDirection * Vault.scoot, vaultEndVelocity, Time.time));
        //}

		if (Input.GetKeyDown(KeyCode.LeftShift)) {
			crouchKey++;
		} else if (Input.GetKeyUp(KeyCode.LeftShift)) {
			crouchKey--;
		}
		UpdateMoveState();
		
		Vector3 directionVector;
		switch (moverState) {
		case MoverState.ledge:
			directionVector = new Vector3(0.0f,1.5f,0.0f)+climbPlane*Input.GetAxis("Horizontal")+3.5f*camera.transform.forward;
			motor.SetVelocity(directionVector);
			Debug.Log(directionVector.magnitude);
			motor.inputJump = false;
			motor.inputMoveDirection = transform.rotation * directionVector;
			break;
		case MoverState.climb:
			directionVector = new Vector3(0,Input.GetAxis("Vertical"),0)+climbPlane*Input.GetAxis("Horizontal");
			motor.SetVelocity(directionVector);
			motor.inputJump = false;
			motor.inputMoveDirection = transform.rotation * directionVector;
			break;
		case MoverState.slide:
			directionVector = slideDirection;
			motor.inputMoveDirection = directionVector * speed;
			break;
		case MoverState.normal:
			//motor.grounded = true;
			directionVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
			
			if (directionVector != Vector3.zero) {
				// Get the length of the directon vector and then normalize it
				// Dividing by the length is cheaper than normalizing when we already have the length anyway
				var directionLength = directionVector.magnitude;
				directionVector = directionVector / directionLength;
				
				// Make sure the length is no bigger than 1
				directionLength = Mathf.Min(1, directionLength);
				
				// Make the input vector more sensitive towards the extremes and less sensitive in the middle
				// This makes it easier to control slow speeds when using analog sticks
				directionLength = directionLength * directionLength;
				directionLength*=speed;
				
				// Multiply the normalized direction vector by the modified length
				directionVector = directionVector * directionLength;
			}
			motor.inputJump = Input.GetButton("Jump");
			motor.inputMoveDirection = camera.transform.rotation * directionVector;
			break;
		};
	}
    void LateUpdate() {
		// pickaxes
		Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width/2,Screen.height/2));
		Vector3 point = ray.GetPoint(0.04f);
		point = transform.InverseTransformPoint(point);
		//transform.Find("Axe").localPosition = point * 0.01f;
		//transform.Find("Axe").localRotation = Camera.main.transform.localRotation;
	}
	
	// Use this for initialization
	void OnTriggerEnter(Collider other) {
		if (other.tag == "Climbable Zone")   { Climb.zone++;       }
		if (other.tag == "Climbable Top")    { Climb.topZone++;    }
		if (other.tag == "Climbable Ground") { Climb.groundZone++; }
		if (other.tag == "Crouch Keep")      {
			crouchZones.Add(other);
			crouchKeep = crouchZones.Count;
			//crouchKeep++;
		}
        if (other.tag == "Vault Ground")     { 
            Vault.can = true;
            GameObject obj = other.gameObject.transform.parent.Find("Model").gameObject;
            Vault.maxHeight = Vault.boost + obj.collider.bounds.max.y;
        }
        if (other.tag == "Vault Top") { Vault.plant = true; }
        //if (other.tag == "Death") {            
        //    gameObject.SetActive(false);
        //    gameObject.SetActive(true);
        //    gameObject.GetComponent<SoundController>().StopSound();
        //    transform.position = start_position;
        //}
	}
	void OnTriggerExit(Collider other) {
		if (other.tag == "Climbable Zone")   { Climb.zone--;        }
		if (other.tag == "Climbable Top")    { Climb.topZone--;     }
		if (other.tag == "Climbable Ground") { Climb.groundZone--;  }
		if (other.tag == "Crouch Keep")      {
			crouchZones.Remove(other);
			crouchKeep = crouchZones.Count;
			//crouchKeep--;
		}
        if (other.tag == "Vault Ground")     { Vault.can = false;   }
        if (other.tag == "Vault Top")        { Vault.plant = false; }
	}
    
    public bool isJumping() {
        return !isGrounded() && motor.IsJumping();
    }

    public bool jumpReachedApex() {
        return isJumping() && motor.movement.velocity.y <= 0.0f;
    }
    
    public bool isGrounded() {
        return (controller.collisionFlags & CollisionFlags.Below) != 0;
    }

    public bool climbTopZone() {
        return Climb.topZone > 0;
    }
    
    public bool isClimbing() {
        return moverState == MoverState.ledge || moverState == MoverState.climb;
    }

    public bool startClimbing() {
        return state == MoveState.startClimbing || moverState == MoverState.ledge;
    }
}
