using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grounded : CharacterState {

    override protected void FixedUpdate(){
        Ray ray = new();
        Vector3 origin = controller.center;
        origin.y -= ((controller.height / 2f));
        ray.origin = transform.TransformPoint(origin);
        ray.direction = Vector3.down;
        RaycastHit hit;
        if(controller.isGrounded || Physics.Raycast(ray, out hit, controller.skinWidth * 2)){
            start();
        } else { stop();}
    }

}