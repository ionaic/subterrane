using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class behaviorSnakeMove : MonoBehaviour {

    public bool allowVerticalMovement = false;
    public float moveSpeed = 4.0f;
    public float rotationSpeed = 1.0f;
    public Transform piece;
    private Transform lastPiece;
    
    // Use this for initialization
    void Start () {
        lastPiece = transform;        
    }
    
    // Update is called once per frame
    void Update () {
        transform.Translate(0,0,moveSpeed*Time.smoothDeltaTime);
        if(Input.GetKeyUp(KeyCode.Space)){
            addPiece();
        }        
        float Yrotation = Input.GetAxis ("Horizontal") * rotationSpeed;
        transform.Rotate(0, Yrotation, 0);
        if (allowVerticalMovement){
            float Xrotation = Input.GetAxis ("Vertical") * rotationSpeed;
            transform.Rotate(Xrotation, 0, 0);
        }
        
    }
    
    void addPiece(){
        Transform newPiece = (Transform)Instantiate(piece, transform.position-(rigidbody.velocity*100), Quaternion.identity);
        newPiece.name = "Piece";
        newPiece.GetComponent<SmoothFollow>().target = lastPiece;
        lastPiece = newPiece;
    }
    
}