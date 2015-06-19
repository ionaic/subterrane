using UnityEngine;
using System.Collections;

public class KillPlayer : MonoBehaviour {

    public float deathDuration = 3.0f;
    public bool useFadePercentage = false;
    public float fadePercentage = 0.75f;
    public float fadeDuration = 1.0f;
    public float tensionVolume = 0.3333f;
    public LayerMask maskEverything;
    public LayerMask maskNothing;
    
    private Vector3 startPosition;
    private Quaternion startRotation;
    private GameObject player;
    private bool currentlyDying = false;
    private float startTime;
    private float soundFadeDelay;
    private SoundController sound;
    private Animator animator;
    
	/// Use this for initialization
	void Start () {        
        startPosition = transform.position;
        startRotation = transform.rotation;
        sound = GetComponent<SoundController>();
        animator = GetComponent<Animator>();
	}	
	
    void OnTriggerEnter(Collider other) {
        if (other.tag == "Player" && !currentlyDying){
            currentlyDying = true;
            player = other.gameObject;
            sound.PlaySound("death_crunch", 1f);
            other.GetComponent<SoundController>().PlaySound("death_gasp", 0.85f);
            ///Camera Fade Magic goes Here:
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.cullingMask = maskNothing;
            
            sound.StartSound("centipede_eating", 0.75f);
            startTime = Random.Range(0,sound.continuous.clip.length*0.75f);
            sound.continuous.time = startTime;
            ///calculate sound fade out
            soundFadeDelay = useFadePercentage ? deathDuration*fadePercentage : deathDuration - Mathf.Min(deathDuration, fadeDuration);
            Invoke("Murder", deathDuration);
        }
    }
    
    ///Used for soundFadeDelay
    void Update() {
        ///Disable all inputs while screen is blacked out, until player respawns
        if (currentlyDying)
            Input.ResetInputAxes();
    
        if (sound.continuous.isPlaying){
            float elapsedTime = sound.continuous.time - startTime;
            if (elapsedTime >= soundFadeDelay)
                sound.continuous.volume = 1f - (deathDuration - elapsedTime / deathDuration);
        }
    }
    
    ///Resets everything to normal death sequence
    private void Murder(){
        currentlyDying = false;
        sound.StopSound();
        Camera.main.clearFlags = CameraClearFlags.Skybox;
        Camera.main.cullingMask = maskEverything;
        ///The following were moved to ResetTrackerpede(), and are now called in DeathScript.Die()
        //transform.position = startPosition;
        //GetComponent<behaviorFollowPathTracker>().enabled = false;
        //Invoke("ReEnableTrackerpede", 5f);
        player.GetComponent<DeathScript>().Die();
    }    
    
    public void ResetTrackerpede(){
        transform.position = startPosition;
        transform.rotation = startRotation;
        GetComponent<behaviorFollowPathManual>().enabled = false;
        //GetComponent<behaviorFollowPathManual>().Start();
        foreach( AudioSource source in gameObject.GetComponents<AudioSource>() )
            if (source.bypassEffects == true)
                source.volume = tensionVolume;
        animator.SetBool("StartMoving", false);
        //Invoke("ReEnableTrackerpede", 5f);
    }    
    
    private void ReEnableTrackerpede(){
        GetComponent<behaviorFollowPathTracker>().enabled = true;
    }
}
