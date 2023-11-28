using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class DoubleJump : CharacterAction
{
    public float jumpSpeed = 0.6f;
    public int maxJumps = 4;
    public int jumps = 0;
    public int cooldown = 10;//number of ticks cd
    public int timer = 0;
    protected override void FixedUpdateTick()
    {
        base.FixedUpdateTick();

        if (!character.grounded.active ){
            if(timer <= 0) {
                if (jumps < maxJumps && character.moveDirection.y > 0){
                    jump();
                }
                if (timer < 0){
                    timer = 0;
                }
            } else {
                timer--;
            }
        } else {
            End();
        }
    }
    private void jump(){
        jumps++;
        timer = cooldown;
        //For a more responsive double jump, if the momentum is small enough just override it
        if (character.momentum.magnitude < jumpSpeed){
            character.momentum = character.moveDirection * jumpSpeed;
        } else { //If the character is moving to fast only alter the trajectory
            character.momentum += character.moveDirection * jumpSpeed;
        }
    }

    protected override void Begin()
    {
        base.Begin();
        timer = cooldown;
    }

    protected override void End()
    {
        base.End();
        jumps = 0;
    }

    public override bool CanStart()
    {
        return (base.CanStart() && !character.grounded.active);
    }
}


