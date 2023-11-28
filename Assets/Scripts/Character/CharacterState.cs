using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// An interface that basically is a dynamic boolean. Attach this to a character class and have it call fixed/update to keep itself updated
/// to access the value do  if (myState.active == true) {etc...}
/// Alternatively, this could be a way to implement sub actions within an action, have the parent action class handle the start and stopping of each state,
/// and have the update and fixed update methods performing the actions. This doesn't really accomplish much except for encapsulting distinct parts of a CharacterActionxs
/// </summary>
public abstract class CharacterState : MonoBehaviour{

    new public string name;
    public bool active = false;

    //makes sense to have these on instantiation....
    protected Character character;
    protected Rigidbody body;
    protected CapsuleCollider capsule;

    public CharacterState(string n = "Default State"){
        name = n;
        

    }
    /// <summary>
    /// Called when we transition into this state
    /// </summary>
    public virtual void start(){
        active = true;

    }

    public virtual void stop(){
        active = false;
    }
    /// <summary>
    /// supposed to be called every update tick, 
    /// used to check game logic stuff regarding the state of the character
    /// </summary>
    protected virtual void Update(){
        
    }
    /// <summary>
    /// Supposed to be called every fixed update tick to do things like raycast
    /// and for example check if the state should transition to start or stop etc...
    /// </summary>
    protected virtual void FixedUpdate(){

    }

    protected virtual void Start(){
        character = GetComponent<Character>();
        body = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
    }
    protected virtual void Awake(){

    }
    
}
