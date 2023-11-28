using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Gravity : CharacterAction {
    Grounded grounded;
    float acceleration = 0.012f;
    float terminal_velocity = 0.6f;

    float drag = 0.003f;

    float floatHeight = 0.1f;
    float floatBuffer = 0.1f;

    float springStrength = 1f;
    float springDampen = 1f;
    new void Start(){
        base.Start();
        grounded = GetComponent<Grounded>();
    }
    // Update is called once per frame
    override protected void FixedUpdateTick()
    {
        Vector3 downDirection = -1 * transform.up;
        RaycastHit hit;
        if(Physics.Raycast(character.bottom, downDirection, out hit, floatHeight + floatBuffer)){
            grounded.start();

            Vector3 velocity = body.velocity;
            Vector3 groundVelocity = Vector3.zero;

            Rigidbody ground = hit.rigidbody;
            if (ground != null){
                groundVelocity = ground.velocity;
            }

            float characterSpeed = Vector3.Dot(downDirection, velocity);
            float groundSpeed = Vector3.Dot(groundVelocity, groundVelocity);

            float relativeSpeed = characterSpeed - groundSpeed;

            float delta = hit.distance - floatHeight;

            float springForce = (delta * springStrength) - (relativeSpeed * springDampen);

            body.AddForce(springForce * downDirection);
            if (ground != null){
                body.AddForceAtPosition(-springForce * downDirection, hit.point);
            }
        } else{
            grounded.stop();
        }


        //If on the ground remove any vertical momentum
        //TODO: add an impact force to whatever we collide with scaling with the speed we reached....
        if (grounded.active && character.momentum.y <= 0f){
            character.momentum.y = 0f;
        //Otherwise if we are slower than terminal velocity speed up
        } else {
            Vector3 verticalVelocity = Vector3.zero;
            verticalVelocity.y  = character.momentum.y;
            Vector3 horizontalVelocity = character.momentum - verticalVelocity;
            //we want to cap our vertical acceleration if we are exceeding terminal velocity
            if (verticalVelocity.y > -1 * terminal_velocity) {
                verticalVelocity.y -= acceleration;
            }
            //We want to use our drag to bleed off our horizontal momentum, ignoring locomotion
            if (horizontalVelocity.magnitude > 0f){
                horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, drag);
            }
            character.momentum = verticalVelocity + horizontalVelocity;
        }
    }
}
