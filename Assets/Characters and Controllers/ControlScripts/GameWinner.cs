using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameWinner : MonoBehaviour {

    public float fadeDuration = 3;
    public Texture whiteTexture;
    
    private float alphaFadeValue = 1f;
    private bool winning = false;

	/// Use this for initialization
	/*void Start () {

	}*/
	/*
	/// Update is called once per frame
	void Update () {
        if (winning){
            Input.ResetInputAxes(); ///Disables keyboard and mouse input
            alphaFadeValue -= Mathf.Clamp01(Time.deltaTime / fadeDuration);
            GUI.color = new Color(1, 1, 1, alphaFadeValue);
            GUI.DrawTexture( new Rect(0, 0, Screen.width, Screen.height ), whiteTexture );
        }
	}*/
    
	private void OnTriggerEnter(Collider other) {
        winning = true;
        CameraFade.StartAlphaFade(Color.white, false, fadeDuration, 0.0f);
        Invoke ("EndGame", fadeDuration);
    }
    
    private void EndGame(){
        winning = false;
        Application.LoadLevel(0);
    }
}
