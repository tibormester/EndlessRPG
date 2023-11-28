using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder;

public class Character : MonoBehaviour
{

    new public Camera camera;

    //Velocity from things like explosions, gravity, being shoved etc... uncapped
    public Vector3 momentum;
    //Velocity from player driven actions
    public Vector3 locomotion;

    public Vector3 velocity {get => momentum + locomotion;}
    
    //A way to see the change in velocity if needed...
    public Vector3 pastMomentum;
    public Vector3 pastLocomotion;

    public Vector3 moveDirection;
    public Vector3 lookDirection;

    public CharacterController controller;
    public HumanoidPlayerControls actions;

    public Grounded grounded;
    public Walk walk;
    public Look look;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Awake(){
        //Instantiate our actions map
        actions =  new HumanoidPlayerControls();
        //Adds callback actions for events
        actions.movement.look.performed += look.OnLook;
        actions.movement.zoom.performed += look.OnZoom;

        
        
    }

    void OnEnable(){
        actions.Enable();
    }
    void OnDisable(){
        actions.Disable();
    }

    // Update is called once per frame
    void Update(){
        Vector3 temp = actions.movement.move.ReadValue<Vector3>();
        Vector3 horizontalDirection = temp;
        horizontalDirection.y = 0;
        Vector3 verticalDirection = temp - horizontalDirection;

        //Should probably abstract to the look action,
        horizontalDirection = look.camera.TransformDirection(horizontalDirection);
        horizontalDirection.y = 0;
        horizontalDirection = transform.InverseTransformDirection(horizontalDirection);

        moveDirection = horizontalDirection + verticalDirection;
        //moveDirection = transform.InverseTransformDirection(moveDirection);

        if (grounded.active){
            walk.Try();
        }
        //Placeholder to fix bug where sliding along a wall counts as continuous collisions launching the player super fast
        //collisionTimer = (collisionTimer < 0) ? 0f : collisionTimer - Time.deltaTime;
    }
    void FixedUpdate(){


        Debug.DrawLine(transform.position, transform.TransformPoint(10 * locomotion), Color.blue, 0.5f);
        Debug.DrawLine(transform.position, transform.TransformPoint(10 * momentum), Color.green, 0.5f);

        pastLocomotion = locomotion;
        pastMomentum = momentum;
        controller.Move(transform.TransformPoint(velocity) - transform.position);
    }

    private float collisionCooldown = 0.05f;
    private float collisionTimer = 0f;
    private void OnControllerColliderHit(ControllerColliderHit hit) {
        //if ( collisionTimer > 0) return;
        //Treat what we collided with as at rest, get our normal from our past position to our collided position
        //Vector3 normal = ((2 * transform.InverseTransformPoint(hit.point)) - velocity); 
        //Vector3 normalV = velocity * Vector3.Dot(normal, velocity);
        //momentum -= normalV;
        
        //collisionTimer = collisionCooldown;
        //Still a bug where bouncing against a wall moves momentum from in and up to in and down instead of out and up...
        Vector3 normal = (transform.InverseTransformPoint(hit.point)).normalized;
        normal = normal * Vector3.Dot(normal, momentum); //transform.InverseTransformDirection(hit.moveDirection));
        momentum -= normal;
        Debug.DrawLine(transform.position, transform.TransformPoint(normal), Color.red, 2f);
        Debug.Log(hit.collider.ToString() + " with normal force: " + normal );
    }
}

