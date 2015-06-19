using UnityEngine;
using System.Collections;

public class IndividualAudioEngine : MonoBehaviour {

    public string type = "rubble";
    public bool oneShot = false;
    public bool alreadyPlayed = false;
    //public float vol = 1.0f;

    private SoundController sound;
    private Vector3 directionVector;
    
	/// Use this for initialization
	void Start () {        
        sound = GetComponent<SoundController>();
	}
	
	/// Update is called once per frame
	void Update () {        
        if (sound.continuous.isPlaying){
            //print("volume is " + sound.continuous.volume);
            sound.continuous.volume = 1f - (sound.continuous.time / sound.continuous.clip.length);            
        }
	}
    
    void OnTriggerEnter(Collider other) {
        //print ("Collided with " + other.name + ", playing " + type);
        if (!alreadyPlayed){
            sound.StopSound();
            if (oneShot){
                sound.PlaySound(type);
                alreadyPlayed = true;
            }    
            else
                sound.StartSound(type);        
            sound.continuous.loop = false;
        }
    }
}
