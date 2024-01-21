using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HumanoidBody : MonoBehaviour
{
    public Transform leftfootiktarget;
    public Transform rightfootiktarget;
    public Transform lefthandiktarget;
    public Transform righthandiktarget;
    public Transform headiktarget;

    public Bone root;
    public GameObject body;
    public Character character;

    public float armlength;
    public float shoulderwidth;
    public float leglength;
    public float hipwidth;
    public float headheight;

    public bool kinematic = true;
    public bool toggle = false;

    public float feetTimer = 0f;
    public bool stepping = false;
    
    void Start(){
        character = gameObject.GetComponent<Character>();

    }
    void Update(){
        //if (!generatedAnims)GenerateAnimationClips();
        if (toggle)ToggleAll(root, !kinematic);
    }
    void FixedUpdate(){
        stepping = (character.velocity.magnitude < 0.05f) ? false : stepping;
        if(stepping)TakeSteps(character.velocity.normalized * stridelength, stepspeed);
    }
    private float lsteptimer = 0f;
    public float stepspeed = 2f; //seconds a step
    public float stridelength = 0.5f; //m / step
    private Vector3 ltarget;
    private Vector3 lold;
    private float rsteptimer  = 0f;
    private Vector3 rtarget;
    private Vector3 rold;
    public void TakeSteps(Vector3 localdirection, float speed = 1.5f){
        float steptime = 0.75f * speed;
        float dragtime = speed;
        rsteptimer = (lsteptimer + (0.5f * speed)) % speed;
        if (lsteptimer < steptime){//Move the left foot in an arc based on the step time from old position to new position
            float r = (ltarget - lold).magnitude / 2f;
            Vector3 center = (ltarget + lold) / 2f;
            float ratio = lsteptimer / steptime;
            // sin (ratio * 180) = y / radius
            // cos ( ratio * 180) = z / radius
            float y = Mathf.Clamp(Mathf.Sin(ratio * Mathf.PI) * r, -0.2f, 0.5f);
            float x = -Mathf.Cos(ratio * Mathf.PI) * r;
            leftfootiktarget.localPosition = center + new Vector3(0f, y, x);
            lsteptimer += Time.fixedDeltaTime;
        } else if(lsteptimer > dragtime + 0.02f){//When finished dragging, calculate a new target and reset
            lsteptimer = 0f;
            ltarget = localdirection * speed + new Vector3(-0.4f * hipwidth, -leglength, 0f);
            lold = leftfootiktarget.localPosition;
        } else if (lsteptimer > steptime){//Otherwise drag along
            float ratio = (lsteptimer - steptime) / (dragtime - steptime);
            Vector3 lnew = -localdirection * stridelength + new Vector3(-0.4f * hipwidth, -leglength, 0f);
            float distance = (ltarget - lnew).magnitude;
            leftfootiktarget.localPosition = Vector3.MoveTowards(ltarget, lnew, ratio * distance);
            lsteptimer += Time.fixedDeltaTime;
        }
        if (rsteptimer < steptime){//Move the left foot in an arc based on the step time from old position to new position
            float r = (rtarget - rold).magnitude / 2f;
            Vector3 center = (rtarget + rold) / 2f;
            float ratio = rsteptimer / steptime;
            // sin (ratio * 180) = y / radius
            // cos ( ratio * 180) = z / radius
            float y = Mathf.Clamp(Mathf.Sin(ratio * Mathf.PI) * r, -0.2f, 0.5f);
            float x = -Mathf.Cos(ratio * Mathf.PI) * r;
            rightfootiktarget.localPosition = center + new Vector3(0f, y, x);
        } else if(rsteptimer >= dragtime - 0.02f){//When finished dragging, calculate a new target and reset
            rtarget = localdirection * speed + new Vector3(0.4f * hipwidth, -leglength, 0f);
            rold = rightfootiktarget.localPosition;
        } else if (rsteptimer > steptime){//Otherwise drag along
            float ratio = (rsteptimer - steptime) / (dragtime - steptime);
            Vector3 rnew = -localdirection * stridelength + new Vector3(0.4f * hipwidth, -leglength, 0f);
            float distance = (rtarget - rnew).magnitude;
            rightfootiktarget.localPosition = Vector3.MoveTowards(rtarget, rnew, ratio * distance);
        }
     
    }

    public void ToggleAll(Bone root, bool kinematic = false){
        toggle = false;
        this.kinematic = kinematic;
        Rigidbody[] rbs = root.GetComponentsInChildren<Rigidbody>();
        foreach(Rigidbody current in rbs){
            toggleState(current.gameObject, kinematic, !kinematic, kinematic, kinematic);
        }
    }
    public static void toggleState(GameObject go, bool kinematic = false, bool gravity = true, bool enableik = false, bool resetik = false){
        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb != null){
            rb.isKinematic = kinematic;
            rb.useGravity = gravity;
        }
        CustomIK ik = go.GetComponent<CustomIK>();
        if (ik != null){
            ik.enabled = enableik;
            if(resetik) ik.ResetTarget();
            
        }
    }
}
