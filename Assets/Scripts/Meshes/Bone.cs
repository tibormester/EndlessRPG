using System;
using System.Collections.Generic;
using System.Linq;
using DitzelGames.FastIK;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem.Users;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

public class Bone : MonoBehaviour{
        public Rigidbody rb;
        public Collider boxcollider;
        //world space orientation of the bone
        public Vector3 direction {get => transform.forward * length; set => transform.rotation = Quaternion.FromToRotation(Vector3.forward, value);}
        //Rotates from z-forward to default direction
        public float length = 1f;
        public float u = 1f;
        public float v = 1f;

        public Vector3 root {get => transform.position - 0.5f * direction; 
                            set => transform.position = value + 0.5f * direction;}
        public Vector3 tip {get => transform.position + 0.5f * direction; 
                            set => transform.position = value - 0.5f * direction;}
        public Bone parent;
        public List<Bone> children = new();
        public CharacterJoint attachment = null;

        public static Bone NewBone(string name, Vector3 orientation, float u = 1f, float v = 1f){
            GameObject go = new GameObject(name);
            Bone b = go.AddComponent<Bone>();
            b.u = u;
            b.v = v;
            b.length = orientation.magnitude;
            b.parent = b;

            b.addBoxCollider();
            b.addRigidBody();
            b.direction = orientation;
            return b;
        }

        public void addBoxCollider(){
            BoxCollider box = this.AddComponent<BoxCollider>();
            box.size = new Vector3(u, v, length);
            boxcollider = box;
        }
        public void addRigidBody(bool gravity = false, bool kinematic = true, string exclude = "Default"){
            rb = this.AddComponent<Rigidbody>();
            rb.useGravity = gravity;
            rb.isKinematic = kinematic; //Isn't affected by physics...
            rb.excludeLayers = LayerMask.GetMask(exclude);// MAYBE only need to exclude from colliders?
        }
        public void attachRB(Rigidbody rb, Vector3 localposition, float min = 0f, float max = 0f, float s1 = 0f, float s2 = 0f){
            CharacterJoint joint = this.AddComponent<CharacterJoint>();
            rb.transform.parent = this.transform;
            rb.transform.localPosition = Vector3.zero;
            joint.anchor = localposition;
            joint.connectedBody = rb;
            setJointLimits(joint, min, max, s1, s2);
        }
        /**
            It is crucial to set the transforms to the default position (T-pose) before assigning the joint's connected body.
            The joint uses the axis and limits as relative rotations from the default pose, so adjusting the pose will simply have the 
            rigidbodies move back to their previous location once their non kinematic, this took a few too many hours to discover
        **/
        public void attachBone(Bone bone, Vector3 localposition, float min = 0f, float max = 0f, float s1 = 0f, float s2 = 0f){
            if (bone.parent != bone || bone.attachment != null) detachBone();
            //Reparent the bone
            bone.transform.parent = this.transform;
            bone.parent = this;
            children.Add(bone);
            //Moves the bone so that the root is at our tip
            bone.root = transform.TransformPoint(localposition);
            //Creates the joint
            CharacterJoint joint = this.AddComponent<CharacterJoint>();
            bone.attachment = joint;
            joint.anchor = localposition;
            joint.axis = transform.right;
            joint.connectedBody = bone.rb;
            setJointLimits(joint, min, max, s1, s2);
            
        }
        public void attachBone(Bone bone, float min = 0f, float max = 0f, float s1 = 0f, float s2 = 0f){
            attachBone(bone, transform.InverseTransformPoint(this.tip), min, max, s1, s2);
        }
        public void detachBone(){
            if(parent != this){
                //Repparent to the scene and set parent to self
                parent.children.Remove(this);
                transform.parent = null;
                parent = this;
            } if(attachment != null){
                //Change the joint
                UnityEngine.Object.Destroy(attachment);
            }
        }
        public static void setJointLimits(CharacterJoint joint, float min = 0f, float max = 0f, float swing1 = 0f, float swing2 = 0f){
            SoftJointLimit limit = joint.lowTwistLimit; 
            limit.limit = min;
            joint.lowTwistLimit = limit;
            limit = joint.highTwistLimit; 
            limit.limit = max;
            joint.highTwistLimit = limit;
            limit = joint.swing1Limit;
            limit.limit = swing1;
            joint.swing1Limit = limit;
            limit = joint.swing2Limit;
            limit.limit = swing2;
            joint.swing2Limit = limit;
        }
        public List<Bone> getAttachedBones(){
            List<Bone> bones = new();
            foreach (CharacterJoint joint in this.GetComponents<CharacterJoint>()){
                Bone b = joint.connectedBody.gameObject.GetComponent<Bone>();
                if ( b != null) bones.Add(b);
            }
            return bones;
        }
}
