using UnityEngine;
using System.Collections;

public class AudioEngine : MonoBehaviour {

    private enum SoundState {
        none,
        movingDownToCrouch,
        movingDownToSlide,
        movingUpFromSlide,        
        hoisting,
        vaultTakeOff,
        vaultPlant,
        vaultPushOff,
        vaultLand,
        startClimbing,            
        climbing,
        crawling,
        running       
    
    /**  ignoreMovingDownToCrouch,
            ignoreMovingDownToSlide,
            ignoreMovingUpFromSlide,        
            ignoreHoisting,
            ignoreVaultTakeOff,
            ignoreVaultPlant,
            ignoreVaultPushOff,
            ignoreVaultLand,
            ignoreStartClimbing,
    **/
    };
    
    public enum PantState {
        none,
        noneToLight,
        light,
        lightToHeavy,
        heavy,
        heavyToLight,
        lightToNone
    };
    
    public float minPantDistance = 25.0f; /// play heavy pant
    public float maxPantDistance = 144.0f; /// play light pant
    
    private SoundState state = SoundState.none;
    private SoundState lastState = SoundState.none;
    private bool lastFrameJump = true;
    private bool playJump = false;
    private bool playLand = false;
    private PantState pantState = PantState.none;
    private PantState lastPantState = PantState.none;
    private GameObject[] trackerpedes;
    
    public PantState GetPantState { get { return pantState; } }
    public GameObject[] Trackerpedes { get { return trackerpedes; } }
    
    private ParkourController controller;
    private CharacterMotor motor;
    private SoundController sound;
    private PantQueue pant;
    private Vector3 directionVector;
    
	/// Use this for initialization
	void Start () {
        controller = GetComponent<ParkourController>();
        motor = GetComponent<CharacterMotor>();
        sound = GetComponent<SoundController>();
        pant = GetComponent<PantQueue>();
        trackerpedes = GameObject.FindGameObjectsWithTag("Trackerpede");
	}
	
	/// Update is called once per frame
	void Update () {
        if (!lastFrameJump && motor.IsGrounded() && Input.GetButton("Jump")){
            lastFrameJump = true;
            playJump = true;
        }
        else if (lastFrameJump && motor.IsGrounded() && !Input.GetButton("Jump")){
            lastFrameJump = false;
            playLand = true;
        }   
        
        lastState = state;        
        if (controller.state == MoveState.movingDownToCrouch)
            state = SoundState.movingDownToCrouch;
        else if (controller.state == MoveState.movingDownToSlide)
            state = SoundState.movingDownToSlide;
        else if (controller.state == MoveState.movingUpFromSlide)
            state = SoundState.movingUpFromSlide;
        else if (controller.state == MoveState.hoisting)
            state = SoundState.hoisting;
        else if (controller.state == MoveState.vaultTakeOff)
            state = SoundState.vaultTakeOff;
        else if (controller.state == MoveState.vaultPlant)
            state = SoundState.vaultPlant;
        else if (controller.state == MoveState.vaultPushOff)
            state = SoundState.vaultPushOff;
        else if (controller.state == MoveState.vaultLand)
            state = SoundState.vaultLand;
        else if (controller.state == MoveState.startClimbing)
            state = SoundState.startClimbing;
        else if (controller.state == MoveState.climbing)
            state = SoundState.climbing;
        else{
            directionVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            if (directionVector != Vector3.zero){
                if (controller.state == MoveState.crouching)
                    state = SoundState.crawling;
                else if (controller.state == MoveState.none)
                    state = SoundState.running;
            }
            else state = SoundState.none;
        }
        
        UpdateSoundState();
        UpdatePantQueue();
	}
    
    void UpdateSoundState(){
        if (!motor.IsGrounded() && 
          state != SoundState.startClimbing &&
          state != SoundState.climbing &&
          state != SoundState.hoisting )
            sound.StopSound();
        if (state != SoundState.hoisting && lastState == SoundState.hoisting)
            sound.StopSound();
        if (state != SoundState.crawling && lastState == SoundState.crawling)
            sound.StopSound();
        if (state != SoundState.running && lastState == SoundState.running)
            sound.StopSound();    
    
        if (playJump){
            playJump = false;
            sound.PlaySound("rubble_light", 0.3f);            
        }
        if (playLand){
            playLand = false;
            sound.PlaySound("land");            
        }    
        //print ("Sound State = " + state);
		switch (state){
        case SoundState.none:            
            break;
        case SoundState.movingDownToCrouch:
            if (lastState != SoundState.movingDownToCrouch)
                sound.PlaySound("rubble_light", 0.1f);
            break;
        case SoundState.movingDownToSlide:            
            if (lastState != SoundState.movingDownToSlide){
                sound.PlaySound("rubble_light", 0.6f);
                sound.PlaySound("grunt_short", 0.5f);
            }    
            break;
        case SoundState.movingUpFromSlide:            
            if (lastState != SoundState.movingUpFromSlide)
                sound.PlaySound("rubble_light", 0.3f);
            break;
        case SoundState.hoisting:
            if (lastState != SoundState.hoisting)
                sound.PlaySound("grunt_long");
            break;
        case SoundState.vaultTakeOff:            
            if (lastState != SoundState.vaultTakeOff)
                sound.PlaySound("rubble_light");
            break;
        case SoundState.vaultPlant:            
            if (lastState != SoundState.vaultPlant)
                sound.PlaySound("hit");
            break;
        case SoundState.vaultPushOff:            
            if (lastState != SoundState.vaultPushOff)
                sound.PlaySound("rubble");
            break;
        case SoundState.vaultLand:            
            if (lastState != SoundState.vaultLand)
                sound.PlaySound("land_double");
            break;
        case SoundState.startClimbing:            
            if (lastState != SoundState.startClimbing){
                sound.PlaySound("rubble");
                sound.PlaySound("grunt_short");
                sound.StartSound("climbing");
            }    
            break;
        case SoundState.climbing:            
            break;
        case SoundState.crawling:
            if (motor.IsGrounded())
                sound.StartSound("crawling");
            break;
        case SoundState.running:
            if (motor.IsGrounded())
                sound.StartSound("running", 0.3f);
            break;
        };        
    }
    
    void UpdatePantQueue(){
        ///Start by finding the closest distance of all Trackerpedes
        float closestDistance = Mathf.Infinity;
        foreach (GameObject trackerpede in trackerpedes) {
            Vector3 diff = trackerpede.transform.position - transform.position;
            float distance = diff.sqrMagnitude;
            if (distance < closestDistance)
                closestDistance = distance;            
        }
        //print ("closestDistance is " + closestDistance + " and lastState is " + lastPantState);
        ///Next determine current state with min and max values and lastState
        /**
        none,
        noneToLight,
        light,
        lightToHeavy,
        heavy,
        HeavyTolight,
        lightToNone
        **/
        switch (lastPantState)
        {
            case PantState.none:
                if (closestDistance < maxPantDistance)
                    pantState = PantState.noneToLight;
                break;
            case PantState.light:
                if (closestDistance < minPantDistance)
                    pantState = PantState.lightToHeavy;
                else if (closestDistance > maxPantDistance)
                    pantState = PantState.lightToNone;
                break;
            case PantState.heavy:
                if (closestDistance > minPantDistance)
                    pantState = PantState.heavyToLight;
                break;
            case PantState.noneToLight:
                pantState = PantState.light;
                break;
            case PantState.lightToHeavy:
                pantState = PantState.heavy;
                break;
            case PantState.heavyToLight:
                pantState = PantState.light;
                break;
            case PantState.lightToNone:
                pantState = PantState.none;
                break;
            default:
                pantState = PantState.none;
                break;
        }
        lastPantState = pantState;
        
        ///Then que audioclips or stop playing clips based on state
        switch (pantState)
        {
            case PantState.light:
                if (pant.Count <= 1)
                    pant.Enque("pant_light");
                break;
            case PantState.heavy:
                if (pant.Count <= 1)
                    pant.Enque("pant_heavy");
                break;
            case PantState.noneToLight:
                pant.Enque("pant_light");
                pant.ContinuePanting = true;
                //print("Starting to  pant"); 
                break;
            case PantState.lightToHeavy:
                //pant.Enque("pant_light_in"); ///Clip too long
                break;
            case PantState.heavyToLight:
                pant.Enque("pant_heavy_out");
                break;
            case PantState.lightToNone:
                pant.StopPanting();
                break;
            default:
                break;
        }
        ///Then run the PantMachine
        pant.PantMachine();
    }    
}
