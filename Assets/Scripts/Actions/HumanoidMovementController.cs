using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class HumanoidMovementController : MonoBehaviour
{
    public Ground ground;
    public Look look;
    public Move move;

    void Awake()
    {
        ground = gameObject.GetOrAddComponent<Ground>();
        look = gameObject.GetOrAddComponent<Look>();
        move = gameObject.GetOrAddComponent<Move>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public class Ground : MonoBehaviour{
        public bool grounded = false;
        public float floatHeight = 0.2f;
        public float floatBuffer = 0.1f;
        public Transform lfootik;
        public Transform rfootik;

        public float springStrength = 15f;
        public float springDampen = 5f;

        private float epsilon = 0.01f;
        
        public RaycastHit hit;
        public Vector3 normal {get => grounded ? hit.normal : Vector3.up; }

        public Character character;
        public Rigidbody body;
        void Start() {
            character = gameObject.GetOrAddComponent<Character>();
            body = character.body;
            lfootik = character.humanoidBody.leftfootiktarget;
            rfootik = character.humanoidBody.rightfootiktarget;
        }

        void FixedUpdate() {
            Vector3 feet = transform.TransformPoint(character.collider.center - 0.5f * new Vector3(0f, character.collider.size.y, 0f)) + (transform.up * epsilon);
            Vector3 front = feet + transform.forward * character.collider.size.z * 0.5f;
            Debug.DrawLine(feet, feet + (-transform.up * (floatHeight + floatBuffer)), Color.red, 0.02f);
            Debug.DrawLine(front, front + (Vector3.RotateTowards(-transform.up, character.velocity, (float)Math.PI / 6f, 0f) * (floatHeight + floatBuffer)), Color.red, 0.02f);
            List<RaycastHit> hits = new();
            hits.AddRange(Physics.RaycastAll(front, Vector3.RotateTowards(-transform.up, character.velocity, (float)Math.PI / 6f, 0f), floatHeight + floatBuffer));
            hits.AddRange(Physics.RaycastAll(feet, -transform.up, floatHeight + floatBuffer + epsilon));
            grounded = false;
            character.humanoidBody.stepping = false;
            if (hits.Count > 0){
                hit = hits[0];
                foreach (RaycastHit h in hits){
                    //Debug.Log(h.distance + " and " + h.collider);
                    Bone b = h.collider.gameObject.GetComponent<Bone>();
                    Character c = h.collider.gameObject.GetComponent<Character>();
                    if ( b == null && c == null){
                        hit = (h.distance < hit.distance && h.distance > epsilon) ? h : hit;
                        grounded = true;
                        character.humanoidBody.stepping = true;
                    }         
                }
                if (!grounded){
                    body.AddForce(Physics.gravity);
                    lfootik.localPosition = Vector3.MoveTowards(lfootik.localPosition, new Vector3(-0.35f * character.humanoidBody.shoulderwidth, character.humanoidBody.leglength * -0.5f ,0.15f), 0.03f);
                    rfootik.localPosition = Vector3.MoveTowards(rfootik.localPosition, new Vector3(0.35f * character.humanoidBody.shoulderwidth, character.humanoidBody.leglength * -0.5f ,0.25f), 0.03f);
                    character.transform.rotation = Quaternion.RotateTowards(character.transform.rotation, Quaternion.FromToRotation(character.transform.up, normal) * character.transform.rotation, 2.5f);
                    return;
                }
                Debug.Log(hits.Count);
                Debug.Log(hit.distance + " and " + hit.collider);

                Vector3 groundVelocity = Vector3.zero;
                Rigidbody hitBody = hit.rigidbody;
                if (hitBody != null){
                    groundVelocity = hitBody.velocity;
                }

                float characterSpeed = Vector3.Dot(body.velocity, -transform.up );
                float groundSpeed = Vector3.Dot(groundVelocity, -transform.up);

                float relativeSpeed = characterSpeed - groundSpeed;
                //delta is a fraction from -1 to 0, 0 to 1 depending on if the ground is above or below the target height
                //If we are above delta is negative and we linearly interpolate in the gravity force
                float delta = (floatHeight - (hit.distance - epsilon));
                delta = delta < 0f ? (delta * Physics.gravity.magnitude) / (springStrength * floatBuffer) : delta / floatHeight;
                //We use a relative speed dampening force to avoid oscillations despite the physics gravity having potential to do that, this way should be faster and therefore smoother hopefully
                float springForce = (delta * springStrength) + (relativeSpeed * springDampen);
                body.AddForce(springForce * Vector3.up);//transform.up);
                if (hitBody != null){ //Maybe add more conditions so this force is only added on collisions??
                    hitBody.AddForceAtPosition(springForce * Vector3.down, hit.point);
                }
                if (!character.humanoidBody.stepping){
                    lfootik.localPosition = Vector3.MoveTowards(lfootik.localPosition, new Vector3(-0.4f * character.humanoidBody.hipwidth, character.humanoidBody.leglength * -1f ,-0.15f), 0.05f);
                    rfootik.localPosition = Vector3.MoveTowards(rfootik.localPosition, new Vector3(0.4f * character.humanoidBody.hipwidth, character.humanoidBody.leglength * -1f ,0.25f), 0.05f);
                }
            } else{
                body.AddForce(Physics.gravity);
                grounded = false;
                character.humanoidBody.stepping = false;
                lfootik.localPosition = Vector3.MoveTowards(lfootik.localPosition, new Vector3(-0.35f * character.humanoidBody.shoulderwidth, character.humanoidBody.leglength * -0.5f ,0.15f), 0.03f);
                rfootik.localPosition = Vector3.MoveTowards(rfootik.localPosition, new Vector3(0.35f * character.humanoidBody.shoulderwidth, character.humanoidBody.leglength * -0.5f ,0.25f), 0.03f);
            }
            character.transform.rotation = Quaternion.RotateTowards(character.transform.rotation, Quaternion.FromToRotation(character.transform.up, normal) * character.transform.rotation, 2.5f);
        }
    }

    public class Look : MonoBehaviour {
        bool looking = true;

        new public Transform camera;
        public Transform center;
        public Transform headIK;
        public Transform lhandik;
        public Transform rhandik;
        public Character character;
        public Vector3 cameraDirection;
        public float cameraDistance = 0f;
        public float sensitivity = 0.5f;
        public float zoomSensitivity = 0.05f;

        public RaycastHit focus {get => LookingAt(); set => LookAt(value);}

        public Vector3 offset;

        public float minZoom = 0f;
        public float maxZoom = 10f;

        // Update is called once per frame
        private void Awake() {
            character = gameObject.GetOrAddComponent<Character>();
            camera = character.camera.transform;
            Cursor.lockState = CursorLockMode.Locked;
            center = new GameObject("player camera arm").transform;
            camera.parent = center;
            offset = new Vector3(0f, character.humanoidBody.headheight, 0f);
            camera.Translate(offset);
            headIK = character.humanoidBody.headiktarget;
            lhandik = character.humanoidBody.lefthandiktarget;
            rhandik = character.humanoidBody.righthandiktarget;

            
        }
        public void OnLook(InputAction.CallbackContext context)
        {
            if (!looking) return;
            Vector2 lookDirection = context.ReadValue<Vector2>();
            //Need to clamp these rotatoins and do some more tweaking of head ik but that can wait until i get rotation and blending so it can include torso as part of the chain
            //Rotate around the characters position
            center.RotateAround(center.position, center.up, lookDirection.x * sensitivity);
            camera.RotateAround(center.position, center.right, lookDirection.y * sensitivity);
            headIK.RotateAround(character.transform.position, character.transform.up, lookDirection.x * sensitivity);
            headIK.RotateAround(character.transform.position, character.transform.right, lookDirection.y * sensitivity * 0.2f);
            
        }
        public void LateUpdate(){
            center.position = character.transform.position;

            //Probably change this up to use mouse relative location on screen or somethign instead of camera angle...
            float hdelta = Mathf.Clamp(-50f * Vector3.SignedAngle(center.forward, character.transform.forward, character.transform.up), -60f, 60f);
            float vdelta = Mathf.Clamp(-50f * (Vector3.SignedAngle(camera.forward, character.transform.forward, character.transform.right) + 25f), -30f, 30f);
            Vector3 handpos = Quaternion.Euler(vdelta, hdelta, 0f) * new Vector3(0f, character.humanoidBody.headheight * 0.65f, character.humanoidBody.armlength * 0.75f);
            lhandik.localPosition = Vector3.MoveTowards(lhandik.localPosition, handpos, 0.02f);
            rhandik.localPosition = Vector3.MoveTowards(rhandik.localPosition, handpos, 0.02f);
            

        }
        public void OnZoom(InputAction.CallbackContext context){
            if (!looking) return;
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

    public class Move : MonoBehaviour {
        Character character;
        public float force = 10f;
        public float speed = 15f;
        public float deaccelerationForce = 20f;
        public float arielControlFactor = 0.7f;
        public float rotationSpeed = 5f; //degrees an update

        public void Awake(){
            character = gameObject.GetOrAddComponent<Character>();
        }
        //We are given the movement direction local to the character
        public void MoveHorizontal(Vector3 direction){
            //Do something to project the character local direction to the slope of the ground.... this is fine since the default when not grounded is vector.up    
            direction = Vector3.ProjectOnPlane(direction, character.movement.ground.normal).normalized;
            if (direction.magnitude < Mathf.Epsilon){ //If no input apply deacceleration
                var horizontalVelocity = Vector3.ProjectOnPlane(character.velocity, character.movement.ground.normal);
                character.body.AddForce(horizontalVelocity * -deaccelerationForce * ((character.movement.ground.grounded) ? 1f : arielControlFactor));
                return;
            } else if ( Vector3.Dot(direction, character.velocity) < speed * ((character.movement.ground.grounded) ? 1f : arielControlFactor)){ // If we are under the top speed in our input direction
                var horizontalVelocity = Vector3.ProjectOnPlane(character.velocity, character.movement.ground.normal);
                var momentum = direction * Vector3.Dot(horizontalVelocity, direction);
                var inertia =  horizontalVelocity - momentum;
                //Dampening force (drag)
                character.body.AddForce(-inertia * deaccelerationForce * (character.movement.ground.grounded ? 0.5f : arielControlFactor * 0.5f));
                //Move force
                character.body.AddForce(direction * force * (character.movement.ground.grounded ? 1f : arielControlFactor));
                character.transform.Rotate(0f, Math.Clamp(Vector3.SignedAngle(character.transform.forward, character.velocity, character.transform.up), -rotationSpeed, rotationSpeed), 0f );
            }
            //Move the character to face the move direction
            //I wanted to use torques but then the movement is too clunky and unresponsive

            
        }
        private int jumpTimer = -1;
        public int jumpCD = 35;
        public float jumpStrength = 700f;
        public int doubleJumps = 3;
        private int jumpCount = 0;
        public void MoveVertical(float magnitude){
            //If trying to jump, on the ground and timer hasn't elapsed...
            if (magnitude > 0f && jumpTimer == jumpCD ){
                if (character.movement.ground.grounded ){
                    jumpTimer = 0;
                    jumpCount = 0;
                    character.body.AddForce(Vector3.RotateTowards(character.movement.ground.normal, character.velocity.normalized, (float)Math.PI / 6f, 0f) * jumpStrength);
                } else if (jumpCount < doubleJumps){
                    jumpTimer = 0;
                    jumpCount++;
                    var velocity = character.velocity;
                    if (velocity.y < 0f) velocity.y = 0;
                    character.velocity = velocity;
                    character.body.AddForce(Vector3.RotateTowards(character.movement.ground.normal, character.velocity.normalized, (float)Math.PI / 8f, 0f) * jumpStrength);
                }
            }

        }
        public void FixedUpdate()
        {
            //Dont register jumps within jump CD ticks of each other
            if (jumpTimer < jumpCD) jumpTimer++;
        }

    }
}
