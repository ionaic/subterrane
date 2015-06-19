using UnityEngine;
using System.Collections;

public class ReEnableLandTrigger : MonoBehaviour {

    public GameObject landTrigger;
	
    void OnTriggerEnter(Collider other) {
        landTrigger.GetComponent<IndividualAudioEngine>().alreadyPlayed = false;
    }
}
