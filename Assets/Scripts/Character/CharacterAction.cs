using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterAction : MonoBehaviour
{   

    //To create multistage actions create an action that cycles through several character actions....
    new public string name = "Default Action";
    public bool acting = false; //If the action is being performed or not
    

    protected Character character;
    protected Rigidbody body;
    protected CapsuleCollider capsule;

    protected virtual void UpdateTick(){

    }
    protected virtual void FixedUpdateTick(){

    }

    /// <summary>
    /// Returns True if the action can be started and false if the character shouldnt be allowed to perform the action
    /// By default only returns fault if the action is already acting
    /// </summary>
    /// <returns></returns>
    public virtual bool CanStart(){
        if (acting) return false;
        else return true;
    }
    /// <summary>
    /// Starts that action, is not meant to be called directly, please use try instead
    /// </summary>
    protected virtual void Begin(){
        Debug.Log(name + " Started!");
        acting = true;
    }
    /// <summary>
    /// Calls start if can start, or if forced is true
    /// </summary>
    /// <returns></returns>
    public virtual bool Try(bool forced = false){
        if (forced || this.CanStart()){
            Begin();
            return true;
        } else {
            return false;
        }
    }
    /// <summary>
    /// Called after the action is finished, is not mean to be called externally
    /// </summary>
    protected virtual void End(){
        Debug.Log(name + " Finished!");
        acting = false;
    }
    /// <summary>
    /// An alternative to the finish action meant to be called externally, by default simply calls the finish action
    /// </summary>
    public virtual void Interrupt(){
        Debug.Log(name + " Interrupted!");
        if (acting){
            End();
        }
    }

    protected virtual void Awake(){

    }
    protected virtual void Start(){
        character = GetComponent<Character>();
        body = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
    }

    private void Update(){
        if(acting){
            UpdateTick();
        }
    }
    private void FixedUpdate() {
        if(acting){
            FixedUpdateTick();
        }
    }
    
}
