using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Walk : CharacterAction
{
    Grounded grounded;
    public float speed = 0.2f;
    public float jumpSpeed = 0.5f;
    public float deacceleration = 0.5f;
    public float friction = 0.1f;

    public bool arielControl = true;
    public float arielSpeed  = 0.1f;

    private Vector3 verticalVelocity;
    private Vector3 horizontalVelocity;
    private Vector3 verticalMomentum;
    private Vector3 horizontalMomentum;

    new void Start(){
        base.Start();
        grounded = GetComponent<Grounded>();
    }
    // Update is called once per frame
    override protected void FixedUpdateTick()
    {
        DoubleJump doubleJump;
        if (grounded.active){
            verticalVelocity = Vector3.zero;
            verticalVelocity.y  = character.moveDirection.y;
            horizontalVelocity = character.moveDirection - verticalVelocity;

            verticalMomentum = Vector3.zero;
            verticalMomentum.y = character.momentum.y;
            horizontalMomentum = character.momentum - verticalMomentum;

            

            //If we aren't trying to move and on the ground move our horizontal momentum towards zero
            //Regardless, we need to bleed our horizontal momentum by our drag amount
            drag((horizontalVelocity.magnitude == 0) ? deacceleration + friction : friction);
        
            //Otherwise multiply our input direciton by our speed
            walk(speed);
            
            //If we are trying to jump and on the ground jump
            if (verticalVelocity.y > 0){
                jump();
            }

            //Set the locomotion to our jump + our walk
            character.locomotion = verticalVelocity + horizontalVelocity;
            character.momentum = verticalMomentum + horizontalMomentum;
            
        } else if (arielControl){
            verticalVelocity = Vector3.zero;
            verticalVelocity.y  = character.moveDirection.y;
            horizontalVelocity = character.moveDirection - verticalVelocity;

            drag((horizontalVelocity.magnitude == 0) ?  friction : 0f);

            walk(arielSpeed);

            if (verticalVelocity.y > 0){
                if(TryGetComponent<DoubleJump>(out doubleJump))
                    doubleJump.Try();
            }

            character.locomotion = horizontalVelocity;
        } else{
            
            character.locomotion = Vector3.zero;
            
            if (verticalVelocity.y > 0){
                if(TryGetComponent<DoubleJump>(out doubleJump))
                    doubleJump.Try();
            }

            End(); //We stop walking if we are in the air....
        }
    }

    private void jump(){
        verticalVelocity *= jumpSpeed;
        //Since we are jumping we want to add our velocity to our momentum and set our locomotion to zero instead
        if (horizontalMomentum.magnitude < speed * 2){
            horizontalMomentum += horizontalVelocity;
        } 
        verticalMomentum += verticalVelocity;
        //Set our locomotion to zero
        verticalVelocity = horizontalVelocity = Vector3.zero;
    }

    private void drag(float amt){
        horizontalMomentum = Vector3.MoveTowards(horizontalMomentum, Vector3.zero, amt);
    }

    private void walk(float speed){
        horizontalVelocity *= speed;
    }

}
