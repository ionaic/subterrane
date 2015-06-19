using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlashlightController : MonoBehaviour {

    public Light flashlight0;    
    public Light flashlight1;    

    public float minLightIntensity = 0.25f;
    public float maxLightIntensity0 = 2.3f;
    public float maxLightIntensity1 = 1f;
    public float minLightFlickerSpeed = 0.05f;
    public float maxLightFlickerSpeed = 0.25f;
    
    public float minHeavyIntensity = 0f;
    public float maxHeavyIntensity0 = 2.3f;
    public float maxHeavyIntensity1 = 1f; 
    public float minHeavyFlickerSpeed = 0.01f;
    public float maxHeavyFlickerSpeed = 0.1f;
    
    public bool turnOffLight = false;
    
    public Color heavyColor0;
    public Color heavyColor1;

    private float flickerCounter = 0f;
    private AudioEngine audioEngine;
    private Color startColor0;
    private Color startColor1;

	// Use this for initialization
	void Start () {        
        audioEngine = gameObject.GetComponent<AudioEngine>();
        startColor0 = flashlight0.color;
        startColor1 = flashlight1.color;
	}
	
	// Update is called once per frame
	void Update () {    
        switch (audioEngine.GetPantState)
        {
            case AudioEngine.PantState.light:
                flashlight0.enabled = true;
                flashlight1.enabled = true;
                flashlight0.color = startColor0;
                flashlight1.color = startColor1;
                FlickerFlashlightLight();
                break;
            case AudioEngine.PantState.heavy:
                flashlight0.color = heavyColor0;
                flashlight1.color = heavyColor1;
                
                if (turnOffLight) FlickerOffFlashlightHeavy();
                else FlickerFlashlightHeavy();                    
                break;            
            default:                
                flashlight0.enabled = true;
                flashlight1.enabled = true;
                flashlight0.intensity = 2.3f;
                flashlight1.intensity = 1f;
                flashlight0.color = startColor0;
                flashlight1.color = startColor1;
                break;
        }
        //print (audioEngine.GetPantState);
	}
    
    private void FlickerFlashlightLight(){
        if (flickerCounter < Time.time){               
            flashlight0.intensity = Random.Range( minLightIntensity, maxLightIntensity0 );                         
            flashlight1.intensity = Mathf.Clamp( flashlight0.intensity, minLightIntensity, maxLightIntensity1 );                         
            flickerCounter = Time.time + Random.Range( minLightFlickerSpeed, maxLightFlickerSpeed );
        }
    }
    
    private void FlickerFlashlightHeavy(){
        if (flickerCounter < Time.time){           
            flashlight0.intensity = Random.Range( minHeavyIntensity, maxHeavyIntensity0 );
            flashlight1.intensity = Mathf.Clamp( flashlight0.intensity, minHeavyIntensity, maxHeavyIntensity1 );               
            flickerCounter = Time.time + Random.Range( minHeavyFlickerSpeed, maxHeavyFlickerSpeed );
        }
    }
    
    private void FlickerOffFlashlightHeavy(){
        if (flickerCounter < Time.time){
            ///flashlight 0
            if (flashlight0.enabled)
                flashlight0.enabled = false;
            else{
                flashlight0.enabled = true;             
                flashlight0.intensity = Random.Range( minHeavyIntensity, maxHeavyIntensity0 );
            }
            ///flashlight 1
            if (flashlight1.enabled)
                flashlight1.enabled = false;
            else{
                flashlight1.enabled = true;             
                flashlight1.intensity = Mathf.Clamp( flashlight0.intensity, minHeavyIntensity, maxHeavyIntensity1 );
            }     
            flickerCounter = Time.time + Random.Range( minHeavyFlickerSpeed, maxHeavyFlickerSpeed );
        }
    }
}
