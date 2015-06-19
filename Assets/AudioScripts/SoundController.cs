using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundController : MonoBehaviour {

    public List<string> prefix;    
    public AudioSource source;
    public AudioSource continuous;
    
    private List<Object[]> audioList = new List<Object[]>();
    public List<Object[]> AudioList {
        get { return audioList; }
    }
    
    private AudioClip defaultClip;
        
	/// Use this for initialization
	void Start () {
        source = (AudioSource)gameObject.AddComponent("AudioSource");
        continuous = (AudioSource)gameObject.AddComponent("AudioSource");
        defaultClip = (AudioClip)Resources.Load("Sounds/defaultClip");
        source.clip = defaultClip;
        continuous.clip = defaultClip;        
        for (int i = 0; i < prefix.Count; i++){
            string path = "Sounds/" + prefix[i];
            Object[] sounds = Resources.LoadAll(path, typeof(AudioClip));
            audioList.Add(sounds);            
        }        
	}
	
    public void PlaySound(string key, float vol = 1.0f){
        int index = prefix.IndexOf(key);
        //print("Playing key = " + key + " and index is " + index.ToString());
        if (index != -1)
            if (audioList[index].Length > 0)
                source.PlayOneShot((AudioClip)audioList[index][Random.Range(0, audioList[index].Length)], Mathf.Clamp01(vol));
    }
    
    public void StartSound(string key, float vol = 1.0f){
        int index = prefix.IndexOf(key);
        //print("Starting key = " + key + " and index is " + index.ToString());
        if (index != -1)
            if (audioList[index].Length > 0 && !continuous.isPlaying){
                continuous.loop = true;
                continuous.volume = Mathf.Clamp01(vol);
                continuous.clip = ((AudioClip)audioList[index][Random.Range(0, audioList[index].Length)]);
                continuous.Play();
            }
    }
    
    public void StopSound(){
        //print("Stopping Sound");
        continuous.Stop();
        continuous.volume = 1.0f;
    }
    
    public void PauseSound(){
        //print("Pausing Sound");
        continuous.Pause();        
    }
    
    
}