using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PantQueue : MonoBehaviour {

    public AudioSource panting;
    private AudioClip defaultClip;
    
    private bool continuePanting = false;
    public bool ContinuePanting {
        get { return continuePanting; }
        set { continuePanting = value; }
    }

    private List<AudioClip> clips = new List<AudioClip>();
    public List<AudioClip> Clips {
        get { return clips; }
        set { clips = value; }
    }
    
    ///Shortcut for Clips.Count
    public int Count {        
        get { return clips.Count; }
    }
    
    private SoundController sound;
    
    /// Use this for initialization
	void Start () {
        panting = (AudioSource)gameObject.AddComponent("AudioSource");
        panting.loop = false;
        panting.volume = 1.0f;
        defaultClip = (AudioClip)Resources.Load("Sounds/defaultClip");
        //clips.Add(defaultClip);
        sound = GetComponent<SoundController>();
    }
    
    ///Adds a new clip to back of list and returns size of list
    public int Enque(string key){
        int index = sound.prefix.IndexOf(key);        
        if (index != -1)
            if (sound.AudioList[index].Length > 0){
                //print("Queing " + key);
                clips.Add((AudioClip)sound.AudioList[index][Random.Range(0, sound.AudioList[index].Length)]);
            }
        else
            Debug.LogError("INDEX OF " + key + " NOT FOUND");
        return clips.Count;
    }
    
    ///Returns first clip in list and removes it if there is another
	public AudioClip Deque(){
        AudioClip first = (clips.Count < 1) ? defaultClip : clips[0];
        //if (clips.Count > 1)            
            clips.RemoveAt(0);
        return first;        
    }
    
    public void PantMachine(){        
        if (continuePanting || clips.Count > 0){
            //print ("clips.Count = "  + clips.Count);
            if (!panting.isPlaying){
                panting.loop = false; ///shouldn't have to do this
                panting.clip = Deque();
                panting.Play();
            }
        }
    }
    
    public void StopPanting(){
        //print("Stopping Panting");
        continuePanting = false;
        //panting.Stop(); ///Don't do this, let the queue empty
        panting.volume = 1.0f;
    }
    
    public void PausePanting(){
        //print("Pausing Panting");
        panting.Pause();        
    }

}
