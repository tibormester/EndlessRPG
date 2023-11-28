using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal.Internal;

public class Look : CharacterAction {

    new public Transform camera;
    public Vector3 cameraDirection;
    public float cameraDistance = 0f;
    public float sensitivity = 0.5f;
    public float zoomSensitivity = 0.05f;

    public RaycastHit focus {get => LookingAt(); set => LookAt(value);}

    public Vector3 offset = Vector3.zero;

    public float minZoom = 0f;
    public float maxZoom = 10f;

    // Update is called once per frame
    protected override void Start()
    {
        base.Start();
        Cursor.lockState = CursorLockMode.Locked;
        camera.Translate(offset);
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        if (!acting) return;
        Vector2 lookDirection = context.ReadValue<Vector2>();
        
        //Rotate around the characters position
        camera.RotateAround(transform.position, transform.up, lookDirection.x * sensitivity);
        camera.RotateAround(transform.position, camera.right, lookDirection.y * sensitivity);

        //character.lookDirection = transform.InverseTransformDirection(focus.point - transform.position);

        
    }
    public void OnZoom(InputAction.CallbackContext context){
        if (!acting) return;
        float zoom = context.ReadValue<float>();
        float delta = cameraDistance;
        cameraDistance = Mathf.Clamp(cameraDistance + (zoom * zoomSensitivity), minZoom, maxZoom);
        delta -= cameraDistance;
        if (Mathf.Abs(delta) > Mathf.Epsilon) camera.Translate(0f, 0f, delta);
    }

    public RaycastHit LookingAt(float distance = 100f){
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, distance)){
            return hit;
        } else {
            return new();
        }
    }

    public void LookAt(RaycastHit hit){
        LookAt(hit.rigidbody.transform.position);
    }
    public void LookAt(Vector3 global_position){
        //TODO IMPLEMENT LATER
        Debug.Log("Need to implement code to both look at a global position and a relative position, while making sure we dont over rotate");
    }
}
