using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.Burst;
using Unity.VisualScripting;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class FreeCamController : MonoBehaviour
{
    public float freeLookSensitivity = 1f;
    public float sprintSpeed = 2f;

    public float speed = 1f;

    private bool isRotating = false;
    private bool isSprinting = false;

    private Vector3 moveDirection;

    private Vector2 lookDirection;

    //what we want the camera to do when we left click...
    public clickActions clickAction;

    //used for pushing so that the force originates from above the surface instead of on it
    public float forceOffset = 0.1f;
    private bool isTriggering = false;
    public enum clickActions {
        //used to pickup items, good for testing ragdolls
        Drag,
        //used for impact damage, spring deforms elastic, can shatter rigid meshes
        Push,
        
        //used for slashing damage, cut deforms until cleaving elastic and can shatter rigid meshes, 
        Slash,
        //used for piercing damage, dent deforms until piercing elastic and shatters rigids,
        Pierce,

    };

    public float clickStrength = 1f;

    float clickCooldown = 0f;

    // Start is called before the first frame update
    void Start()
    {
        

    }

    // Update is called once per frame
    void Update()
    {
        if (isTriggering){
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100)){
                Debug.Log(hit.collider);
                if (clickAction == clickActions.Push){
                    DeformableMesh deformer = hit.collider.GetComponent<DeformableMesh>();
                    if (deformer) {
                        Vector3 point = hit.point;
                        //Go back along the point so we are clicking from the angle of the camera
                        point += ray.direction * -1 * forceOffset;
                        deformer.AddDeformingForce(point, clickStrength);
                    }
                }
                else if (clickAction == clickActions.Pierce && clickCooldown <= 0f){
                    DeformableMesh deformer = hit.collider.GetComponent<DeformableMesh>();
                    if (deformer){
                        Vector3 point = hit.point;
                        //use the camera strength and click strength to give the force a direction and depth
                        Vector3 force = ray.direction * clickStrength;
                        deformer.AddDentingForce(point, force);
                    }
                }
            }
            clickCooldown = 0.5f;
        }
        if (moveDirection != null && moveDirection != Vector3.zero){
            //Uses the camera's global transform to convert the local direction to a global direction 
            var direction = this.transform.TransformDirection(moveDirection);
            if (isSprinting){
                transform.position += direction * Time.deltaTime * sprintSpeed;
            } else {
                transform.position += direction * Time.deltaTime * speed;
            }
        }
        if (lookDirection != null && lookDirection != Vector2.zero){
            if (isRotating){ //If holding right click we can rotate, and the mouse doesnt move
                Cursor.lockState = CursorLockMode.Locked;
                float newRotationX = transform.localEulerAngles.y + lookDirection.x * freeLookSensitivity;
                float newRotationY = transform.localEulerAngles.x + lookDirection.y * freeLookSensitivity;
                transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
            }
            else { //Otherwise let the cursor be free
                Cursor.lockState = CursorLockMode.None;
            }
        }
        clickCooldown -= Time.deltaTime;
        if (clickCooldown < -1){
            clickCooldown = 0;
        }
    }

    public void OnMove(InputValue value){
        moveDirection = value.Get<Vector3>();
        //Debug.Log("on Move: " + moveDirection);
    }

    public void OnLook(InputValue value){
        //Debug.Log("Looking!");
        lookDirection = value.Get<Vector2>();
        lookDirection.y *= -1;
        //Debug.Log("on Look: " + lookDirection);
    }

    public void OnRotate(InputValue value){
        isRotating = !isRotating;
        Debug.Log("rotation input value:" + value.ToString());
        //Debug.Log("on Rotate: " + isRotating);
    }
    
    public void OnSprint(InputValue value){
        isSprinting = !isSprinting;
        Debug.Log("sprint input value:" + value.ToString());
        //Debug.Log("on Sprint: " + isSprinting);
    }

    public void OnTrigger(){
        //Debug.Log("on Trigger: N/A");//+ ctx.ReadValue<Button>.ToString);
        isTriggering = !isTriggering;
    }
}
