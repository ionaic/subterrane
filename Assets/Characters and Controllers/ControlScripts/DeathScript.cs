using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DeathScript : MonoBehaviour {
    private AudioEngine audioEng;
    public Vector3 start_position;
    public Quaternion start_rotation;
    public float fadeToBlack = 1.0f;
    public float darkTime = 1.0f;
    public float fadeToWhite = 1.0f;
    public float fadeFromWhite = 1.0f;
    //public List<Transform> trackerpedes = new List<Transform>();
    //private Camera cam;
    //private CameraFadeExt camFade;
    // for possible feature that allows you to set which checkpoint from public variable...
    //public int checkpoint = 0;
    //public Level current_level;

	// Use this for initialization
	void Start () {
        // initialize checkpoint to wherever chell started just in case
        start_position = GetComponent<Transform>().position;
        audioEng = GetComponent<AudioEngine>();
	}
    
	void OnTriggerEnter(Collider other) {
        if (other.tag == "Death") {            
            // cut to black and fade in from white
            //CameraFade.StartAlphaFade(Color.black, false, fadeToBlack, darkTime);
            //CameraFade.StartAlphaFade(Color.white, false, fadeToWhite, fadeToBlack + darkTime);
            //CameraFade.StartAlphaFade(Color.black, false, fadeToBlack, 0f, CameraFade.StartAlphaFade(Color.white, true, fadeFromWhite, darkTime, Die()));
            //CameraFade.StartAlphaFade(Color.white, true, fadeFromWhite, darkTime, () => {Die();});
            Die();
            //CameraFade.StartAlphaFade(Color.black, true, fadeFromWhite, fadeToBlack + fadeToWhite + darkTime);
            //CameraFade.StartAlphaFade(Color.black, true, fadeFromWhite, fadeToBlack + fadeToWhite + darkTime);
        }
        else if (other.tag == "Checkpoint") {
            start_position = other.transform.position;
            start_rotation = other.transform.rotation;
        }
    }
    
    public void Die() {
        gameObject.SetActive(false);
        gameObject.SetActive(true);
        CameraFade.StartAlphaFade(Color.black, true, fadeFromWhite, darkTime);
        gameObject.GetComponent<SoundController>().StopSound();
        transform.position = start_position;
        transform.rotation = start_rotation;
        foreach (GameObject trackerpede in gameObject.GetComponent<AudioEngine>().Trackerpedes)
            trackerpede.GetComponent<KillPlayer>().ResetTrackerpede();
    }
}
