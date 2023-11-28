using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thrust : CharacterAction
{
    
    Vector3 startLocation;
    //Ready, Thrust, Reset durations
    public int[] durations = {2, 2, 2};
    int stage = 0;
    int timer = -1;

    public Transform sword;
    public float length = 0.5f;
    public float reach = 0.25f;

    float distance = 0f;
    Vector3 difference;
    

    
    // Start is called before the first frame update
    protected override void Begin()
    {
        base.Begin();
        startLocation = (character.lookDirection.normalized * controller.radius);
        difference = (transform.TransformPoint(startLocation) - sword.transform.position);
        distance = difference.magnitude;
        stage = 0;
        timer = durations[stage];

    }

    protected override void End()
    {
        base.End();

    }

    protected override void FixedUpdateTick()
    {
        base.FixedUpdateTick();
        if (stage == 0){

        } else if (stage == 1){

        } else if (stage == 2){

        }
        timer--;
        if (timer < 0) {stage++;
        if (stage == durations.Length) End();
        timer = durations[stage];}
    }
    private void ReadyTick(){
        sword.transform.position = Vector3.MoveTowards(sword.transform.position, startLocation, (1f / durations[stage]) );
    }


    private void SwingTick(){
        
    }

    private void ResetTick(){
        
    }
}
