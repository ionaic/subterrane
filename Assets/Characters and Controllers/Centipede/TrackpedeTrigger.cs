using UnityEngine;
using System.Collections;

public class TrackpedeTrigger : MonoBehaviour {

    public GameObject player;
    public GameObject trackerpede;
    public bool playMusic = false;
    public bool typePathFollower = false;
    public float delay = 0f;    
    
    private Animator animator;
    
    void Start(){
        animator = trackerpede.GetComponent<Animator>();
    }
    
    void OnTriggerEnter(Collider other) {
        if (other.tag == "Player")
            Invoke("StartMoving", delay);
    }
    
    void StartMoving(){
        animator.SetBool("StartMoving", true);
        if (playMusic){
            foreach( AudioSource source in trackerpede.GetComponents<AudioSource>() )
                if (source.bypassEffects == true)
                    source.volume = 1f;
        }   
        if (typePathFollower){
            trackerpede.GetComponent<behaviorFollowPathManual>().enabled = true;
            trackerpede.GetComponent<behaviorFollowPathManual>().Start();
        }
        else{
            player.GetComponent<Tracker>().enabled = true;
            trackerpede.GetComponent<behaviorFollowPathTracker>().enabled = true;
        }    
    }
}
