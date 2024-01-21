using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder;

//Use the character class to tie together the Inputs, the movement, the body, and then the game logic
public class Character : MonoBehaviour
{

    new public Camera camera;

    public Rigidbody body;
    new public BoxCollider collider;

    //Force from player driven actions applied and reset each frame by the character, links force and velocity for consistency
    public Vector3 velocity {get => body.IsUnityNull() ? Vector3.zero : body.velocity; set => body.AddRelativeForce(value - velocity, ForceMode.VelocityChange);}


    //Handles interpretting raw input into the world space interpretation
    public HumanoidPlayerControls actions;
    //Handles movement of the controller rigidbody as well as the manipulation of the IK targets
    public HumanoidMovementController movement;
    //Creates the bones attached to the rest of the rigid body (as well as the RB and collider itself)
    public HumanoidBodyGenerator bodygen;
    //The Class containing the rigidbody as well as functions to create different key framed stances probably?
    public HumanoidBody humanoidBody;

    void Awake(){
        bodygen = gameObject.GetOrAddComponent<HumanoidBodyGenerator>();
        humanoidBody = bodygen.Generate(gameObject);
        body = humanoidBody.GetComponent<Rigidbody>();
        collider = humanoidBody.GetComponent<BoxCollider>();
        movement = gameObject.GetOrAddComponent<HumanoidMovementController>();
        //Instantiate our actions map
        actions =  new HumanoidPlayerControls();
        //Adds callback actions for events
        actions.movement.look.performed += movement.look.OnLook;
        actions.movement.zoom.performed += movement.look.OnZoom;
        //actions.combat.Thrust.performed += slash.Try;
        collider.size -= new Vector3(0, movement.ground.floatHeight, 0);
        collider.center += new Vector3(0, movement.ground.floatHeight * 0.5f, 0);

    }
    // Start is called before the first frame update
    void Start()
    {   
        
        

    }

    void OnEnable(){
        actions.Enable();
    }
    void OnDisable(){
        actions.Disable();
    }

    // Update is called once per frame
    void Update(){
        ReadMovementInput();
    }
    void FixedUpdate(){
        
        Debug.DrawLine(transform.position,transform.position + (velocity / 10f), Color.blue, 0.02f);
        Debug.DrawLine(transform.position,transform.position +  (body.GetAccumulatedForce() / 50f), Color.green, 0.02f);
    }
    void ReadMovementInput(){
        Vector3 InputDirection = actions.movement.move.ReadValue<Vector3>();
        movement.move.MoveVertical(InputDirection.y);
        InputDirection.y = 0;
        //Now we want to apply the camera angle to our raw inputs by going from local to the camera to the world and then from world to local to player
        InputDirection = movement.look.camera.TransformDirection(InputDirection);
        //InputDirection = transform.InverseTransformDirection(InputDirection);

        movement.move.MoveHorizontal(InputDirection);

    }

}

