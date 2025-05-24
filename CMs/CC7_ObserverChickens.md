# Chicken Chaser CM 7 - Ai Chicken Observer

---
## Final Result
![CM7.gif](Resources/CM7/CM7.gif)
* [7.1 Trappable Chickens](#71-trappable-chickens)
  *   [7.1a) ITrappable](#71a-itrappable)
  *   [7.1b) Player Capture and Release Action](#71b-player-capture-and-release-action)
  *   [7.1c) AI Capture and Release Action](#71c-ai-capture-and-release-action)

* [7.2) Fixing AI](#72-fixing-ai)
  * [7.2a) Enable the EndGoal](#72a-enable-the-endgoal)
  * [7.2b) Player Escaping](#72b-player-escaping)

* [End Results](#end-results)
    * [Chicken.cs](#chickencs)
    * [PlayerChicken.cs](#playerchickencs)
    * [AiChicken.cs](#aichickencs)
    * [EndGoal.cs [COMPLETE]](#endgoalcs-complete)

## [Observer Design Pattern](https://github.com/RealProgrammingInstructors/Shared-Content/blob/main/Content/Coding/Observer.md)

---

## 7.1 Trappable Chickens

### 7.1a) ITrappable

First, to make our chickens trappable, all we need to do is have our parent class implement ITrappable.
All the logic for the humans handles the rest. This also means you can have things like cages that can be trapped, but not captured.

in [Chicken.cs](../Assets/Scripts/Characters/Chicken/Chicken.cs)
```csharp
using Interfaces;
using UnityEngine;
using Utilities;

//Add ITrappable to class definintion
public abstract class Chicken : MonoBehaviour, IVisualDetectable, ITrappable
{
    // OTHER FUNCTIONS AND VARIABLES
    
    // Add these functions
    public void OnPreCapture()
    {
        enabled = false;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public bool CanBeTrapped()
    {
        return isActiveAndEnabled;
    }
    // OTHER FUNCTIONS
}
```
![Capturable.gif](Resources/CM7/Capturable.gif)
Okay so clearly this isn't right. to correct this, first in Chicken we need to disable our collider as the traps are responsible for that.

in [Chicken.cs](../Assets/Scripts/Characters/Chicken/Chicken.cs)

```csharp
public abstract class Chicken : MonoBehaviour, IVisualDetectable, ITrappable
{
    [SerializeField] protected ChickenStats stats;
    
    [Header("Objects")] 
    [SerializeField] protected Transform head;
    [SerializeField] protected Transform foot;
    
    protected Rigidbody PhysicsBody;
    protected Animator AnimatorController;
    //---------- ADDED ---------//
    protected Collider BodyCollider;
    //--------------------------//
    protected bool IsGrounded;
    
    protected float currentSpeed;
    protected float currentFallTIme;
    protected Vector3 slopeNormal;
    
    protected float Visibility = 1;

    
    protected virtual void Awake()
    {
        PhysicsBody = GetComponent<Rigidbody>();
        AnimatorController = GetComponentInChildren<Animator>();
        //---------- ADDED ---------//
        BodyCollider = GetComponentInChildren<Collider>();
        //--------------------------//
    }
}
```
### 7.1b) Player Capture and Release Action
Next, we need to edit PlayerChicken

in [PlayerChicken.cs](../Assets/Scripts/Characters/Chicken/PlayerChicken.cs)

```csharp
using System;
using UnityEngine;
using Utilities;

public class PlayerChicken : Chicken
{
    private Vector3 _moveDirection;
    private Vector2 _lookDirection;
    
    [Header("Looking")] 
    [SerializeField , Range(0,90)] private float pitchLimit;
    [SerializeField, Range(0,180)] private float yawLimit;
    [SerializeField] private float lookSpeed;

    [Header("Abilities")]
    [SerializeField] private AbstractAbility jumpAbility;
    [SerializeField] private AbstractAbility cluckAbility;
    [SerializeField] private AbstractAbility dashAbility;
    
    //-------------------------- ADDED ---------------------//
    //Observable player events
    public static Action<Vector3> OnPlayerCaught;
    public static Action<Vector3> OnPlayerEscaped;
    public static Action OnPlayerRescued; // It's not out of the picture that another chicken can rescue the player.
     //--------------------------------------------------------------------//
    
    protected override void Awake()  { ... }

    //-------------------------- ADDED ---------------------//
    private void OnEnable()
    {
        //Enable components for good measure
        PhysicsBody.isKinematic = false;
        BodyCollider.enabled = true;
    }
    //--------------------------------------------------------------------//
    
    private void OnDisable()
    {
        //-------------------------- ADDED ---------------------//
        //Disable components for good measure
        PhysicsBody.isKinematic = true;
        BodyCollider.enabled = false;
        //--------------------------------------------------------------------//
        
        PlayerControls.DisablePlayer();  
        jumpAbility.ForceCancelAbility();
        cluckAbility.ForceCancelAbility();
        dashAbility.ForceCancelAbility();
    }

    protected override void HandleMovement() { ... }

    public override void OnFreedFromCage()
    {
        //-------------------------- ADDED ---------------------//
        enabled = true;
        PlayerControls.UseGameControls();
        OnPlayerRescued?.Invoke();

        //Stop Using the cluck ability
        cluckAbility.StopUsingAbility();
        //--------------------------------------------------------------------//
    }

    public override void OnEscaped(Vector3 position)
    {
         //-------------------------- ADDED ---------------------//
        OnPlayerEscaped?.Invoke(transform.position);
         //--------------------------------------------------------------------//
    }

    public override void OnCaptured()
    {
        //-------------------------- ADDED ---------------------//
        //Optional debug line
        Debug.Log("Player has been captured");
        
        //Fix the players hopping animations
        AnimatorController.SetFloat(StaticUtilities.MoveSpeedAnimID, 0);
        cluckAbility.StartUsingAbility(); //Cluck to bring all chickens to you (done later)

        //Unlock the mouse and make it visible
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        
        //Make humans ignore us
        Visibility = 0;
        
        //Notify our subscribers (?) if we have any
        OnPlayerCaught?.Invoke(transform.position);
        //--------------------------------------------------------------------//
    }
    
    //other functions 
}
```

### 7.1c) AI Capture and Release Action

Now we finally just need to update the AI with similar logic, but also ensure observe player events

in [AiChicken.cs](../Assets/Scripts/Characters/Chicken/AiChicken.cs)

```csharp
public class AiChicken : Chicken, IDetector
{

    //other variables and functions

    private void OnEnable()
    {
        _faceTarget.enabled = false;
        _agent.enabled = true;
        
        _audioDetection.SetStats(activeHearing);
        
        //-------------------------- ADDED ---------------------//
        //Enable object collisions
        BodyCollider.enabled = true;
        
        //Do cluck anim so we're not static, without the particle we just look like we're waving
        AnimatorController.SetBool(StaticUtilities.CluckAnimID, true);
        AnimatorController.enabled = true;
        
        //Subscribe to player events
        PlayerChicken.OnPlayerCaught += MoveTo;
        PlayerChicken.OnPlayerEscaped += MoveTo;
        //--------------------------------------------------//
    }
    
    //-------------------------- ADDED ---------------------//
    private void OnDisable()
    {
        //Unsubscribe from player events (as we can't complete them)
        PlayerChicken.OnPlayerCaught -= MoveTo;
        PlayerChicken.OnPlayerEscaped -= MoveTo;
        
        //Disable any active anims
        AnimatorController.SetBool(StaticUtilities.CluckAnimID, false);
        AnimatorController.enabled = false;
        
        //Stop the agent
        _agent.ResetPath();
        _agent.enabled = false;
        
        //Disable collisions
        BodyCollider.enabled = false;
        
        //Enable the face target component
        _faceTarget.enabled = true;
    }

    private void MoveTo(Vector3 location)
    {
        _agent.SetDestination(location);
    }
    
    //--------------------------------------------------//

    protected override void HandleMovement() { ... }

    public override void OnFreedFromCage()
    {
        //-------------------------- ADDED ---------------------//
        enabled = true;
        //----------------------------------------------------//
    }

    public override void OnEscaped(Vector3 position)
    {
        
    }

    public override void OnCaptured()
    {
        //-------------------------- ADDED ---------------------//
        AnimatorController.SetFloat(StaticUtilities.MoveSpeedAnimID, 0);
        //----------------------------------------------------//
    }
    // other functions
}
```

![Caught.gif](Resources/CM7/Caught.gif)

## 7.2) Fixing AI

### 7.2a) Enable the EndGoal

First we need to enable the EndGoal, it's a premade script but because it directly depends on the chicken it had to be disabled. Simply re-enable it.

[EndGoal.cs](../Assets/Scripts/Game/EndGoal.cs)

    NOTE you are not creating a new script. 
    This script is connected to some pre-placed objects.
```csharp
using Characters;
using UnityEngine;

public class EndGoal : MonoBehaviour
{
    [SerializeField] private Transform moveToLocation;
    [SerializeField] private LayerMask allowedLayers;

    void OnTriggerEnter(Collider other)
    {
        //If they're not a desired layer.
        if (((1 << other.gameObject.layer) & allowedLayers) == 0) return;
        
        //If they're a type of chicken
        if (other.attachedRigidbody.TryGetComponent(out Chicken c))
        {
            //Move to the exit
            c.OnEscaped(moveToLocation.position);
        }
    }
}
```

### 7.2b) Player Escaping

Next, our player now has the ability to escape, let's populate the function and notify everyone.

in [PlayerChicken.cs](../Assets/Scripts/Characters/Chicken/PlayerChicken.cs)
```csharp
public class PlayerChicken : Chicken
{
    //Variables and other functions
     public override void OnEscaped(Vector3 position)
    {
        //----------------------- ADDED -------------------//
        //Optional Debug
        Debug.Log("Player won the game!");
        
        //Notify anyone who wants to know when we escape
        OnPlayerEscaped?.Invoke(transform.position);
        
        //Create an AI to take over for us
        NavMeshAgent agent = gameObject.AddComponent<NavMeshAgent>();
        agent.enabled = true;
        agent.baseOffset = 0.16f;
        agent.height = 0.32f;
        agent.radius = 0.2f;
        agent.agentTypeID = 0;
        agent.SetDestination(position);
        
        //Enable the animations
        AnimatorController.SetFloat(StaticUtilities.MoveSpeedAnimID, stats.MaxSpeed);

        //Prevent any weirdness
        enabled = false;
        //-------------------------------------------------------------//
    }
    // other functions 
    
}
```

### 7.2c) AI Escaping
Finally, let's make the AI escape as well

```csharp
public class AiChicken : Chicken, IDetector
{
    //Other functions
 public override void OnEscaped(Vector3 position)
    {
        //------------------- ADDED ------------//
        //Print who is trying to escape (When doing , gameObject the debug will show that object when pressed on in unity)
        Debug.Log("I'm trying to escape", gameObject);
        
        //Move to the location to escape
        MoveTo(position);
            
        //We should not escape just yet because the AI needs time to actually get to the exit...
        //let's start a coroutine and see if we've escaped.
        StartCoroutine(CheckForEscaped());

        //Hide the AI so that we don't have 'unfair' captures
        Visibility = 0;
        //--------------------------------------------------//
    }
    //------------------- ADDED ------------//
    private IEnumerator CheckForEscaped()
    {
        //CACHED Move until the path is done generating and we reach the target
        WaitUntil target = new WaitUntil(() => _agent.hasPath && _agent.remainingDistance <= _agent.stoppingDistance);

        //Use cached variable
        yield return target;
            
        Debug.Log("I'm trying to escape");
        
        //Destroy ourselves
        Destroy(gameObject);
    }
    //--------------------------------------------------//
    //Other functions
 }
```

## End Results
![CM7.gif](Resources/CM7/CM7.gif)

### [Chicken.cs](../Assets/Scripts/Characters/Chicken/Chicken.cs)
```csharp
using Interfaces;
using UnityEngine;
using Utilities;

public abstract class Chicken : MonoBehaviour, IVisualDetectable, ITrappable
{
    [SerializeField] protected ChickenStats stats;
    
    [Header("Objects")] 
    [SerializeField] protected Transform head;
    [SerializeField] protected Transform foot;
    
    protected Rigidbody PhysicsBody;
    protected Animator AnimatorController;
    protected Collider BodyCollider;
    protected bool IsGrounded;
    
    protected float currentSpeed;
    protected float currentFallTIme;
    protected Vector3 slopeNormal;
    
    protected float Visibility = 1;

    
    protected virtual void Awake()
    {
        PhysicsBody = GetComponent<Rigidbody>();
        AnimatorController = GetComponentInChildren<Animator>();
        BodyCollider = GetComponentInChildren<Collider>();
    }

    private void FixedUpdate()
    {
        HandleGroundState();
        HandleMovement();
        HandleAnims();
    }



    private void HandleGroundState()
    {
        //We're going to spherecast downwards, and detect if we've hit the floor.
        //Basic Spherecast check, NOTE: StaticUtilites.GroundLayers helps the code know which layers to look at for floors.
        // Preventing players from registering grounded on illegal objects.
        bool newGroundedState = Physics.SphereCast(foot.position, stats.FootRadius, Vector3.down, out RaycastHit slope, stats.FootDistance, StaticUtilities.GroundLayers);
       
        //If the ground state is different
        if (newGroundedState != IsGrounded)
        {
            //We should enter that state
            IsGrounded = newGroundedState;
            //Then we should update our grounded state.
            AnimatorController.SetBool(StaticUtilities.IsGroundedAnimID, IsGrounded);

            //If we were falling
            if (currentFallTIme >= 0)
            {
                //Handle Landing
                HandleLanding();
                currentFallTIme = 0;
            }
        }

        //If we're not grounded then update the air time
        if (!IsGrounded) currentFallTIme += Time.deltaTime;
        //If we are grounded keep track of the slope normal so that Movement is smoother.
        else slopeNormal = slope.normal;
    }

    protected virtual void HandleLanding()
    {
        
    }

    protected virtual void HandleAnims()
    {
        AnimatorController.SetFloat(StaticUtilities.MoveSpeedAnimID, currentSpeed);
    }

    protected abstract void HandleMovement();
    
    public abstract void OnFreedFromCage();
    
    public abstract void OnEscaped(Vector3 position);
    public void OnPreCapture()
    {
        enabled = false;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public bool CanBeTrapped()
    {
        return isActiveAndEnabled;
    }

    public abstract void OnCaptured();
    
    public bool GetIsGrounded()
    {
        return IsGrounded;
    }
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    public Vector3 GetLookDirection()
    {
        return head.forward;
    }

    public void AddVisibility(float visibility)
    {
        Visibility += visibility;
    }

    public void RemoveVisibility(float visibility)
    {
        Visibility -= Mathf.Max(0,visibility);
    }

    public float GetVisibility()
    {
        return Visibility;
    }
}

```
### [PlayerChicken.cs](../Assets/Scripts/Characters/Chicken/PlayerChicken.cs)
```csharp
using System;
using UnityEngine;
using UnityEngine.AI;
using Utilities;

public class PlayerChicken : Chicken
{
    private Vector3 _moveDirection;
    private Vector2 _lookDirection;
    
    [Header("Looking")] 
    [SerializeField , Range(0,90)] private float pitchLimit;
    [SerializeField, Range(0,180)] private float yawLimit;
    [SerializeField] private float lookSpeed;

    [Header("Abilities")]
    [SerializeField] private AbstractAbility jumpAbility;
    [SerializeField] private AbstractAbility cluckAbility;
    [SerializeField] private AbstractAbility dashAbility;
    
    //Observable player events
    public static Action<Vector3> OnPlayerCaught;
    public static Action<Vector3> OnPlayerEscaped;
    public static Action OnPlayerRescued; // It's not out of the picture that another chicken can rescue the player.
    
    protected override void Awake()
    {
        base.Awake();
        HudManager.Instance.BindPlayer(this);
        PlayerControls.Initialize(this);
        PlayerControls.UseGameControls();
    }

    private void OnEnable()
    {
        //Enable components for good measure
        PhysicsBody.isKinematic = false;
        BodyCollider.enabled = true;
    }

    private void OnDisable()
    {
        
        //Disable components for good measure
        PhysicsBody.isKinematic = true;
        BodyCollider.enabled = false;
        
        PlayerControls.DisablePlayer();  
        jumpAbility.ForceCancelAbility();
        cluckAbility.ForceCancelAbility();
        dashAbility.ForceCancelAbility();
    }

    protected override void HandleMovement()
    {
        Vector3 direction = _moveDirection;
        if (IsGrounded)
        {
            //If we're grounded, then the direction we want to move should be projected onto the plane.
            //Doing this will help us move up steep slopes easier.
            direction = Vector3.ProjectOnPlane(_moveDirection, slopeNormal);
        }
            
        PhysicsBody.AddForce(transform.rotation * direction * stats.Speed, ForceMode.Acceleration);

        //Note: we don't care about falling speed, only XZ speed.
        Vector2 horizontalVelocity = new Vector2(PhysicsBody.linearVelocity.x, PhysicsBody.linearVelocity.z);
        currentSpeed = horizontalVelocity.magnitude; 

        //Check if our speed is exceeding the max speed
        if (currentSpeed > stats.MaxSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * stats.MaxSpeed;
            //Limit the speed, but be sure to keep the gravity speed.
            PhysicsBody.linearVelocity = new Vector3(horizontalVelocity.x, PhysicsBody.linearVelocity.y, horizontalVelocity.y);
            
            //Lock the speed to prevent weird bugs
            currentSpeed = stats.MaxSpeed;
        }
        
        HandleLooking();
    }

    public override void OnFreedFromCage()
    {
        enabled = true;
        PlayerControls.UseGameControls();
        OnPlayerRescued?.Invoke();

        //Stop Using the cluck ability
        cluckAbility.StopUsingAbility();
    }

    public override void OnEscaped(Vector3 position)
    {
        //Optional Debug
        Debug.Log("Player won the game!");
        
        //Notify anyone who wants to know when we escape
        OnPlayerEscaped?.Invoke(transform.position);
        
        
        //Create an AI to take over for us
        NavMeshAgent agent = gameObject.AddComponent<NavMeshAgent>();
        agent.enabled = true;
        agent.baseOffset = 0.16f;
        agent.height = 0.32f;
        agent.radius = 0.2f;
        agent.agentTypeID = 0;
        agent.SetDestination(position);
        
        //Enable the animations
        AnimatorController.SetFloat(StaticUtilities.MoveSpeedAnimID, stats.MaxSpeed);

        //Prevent any weirdness
        enabled = false;
    }

    public override void OnCaptured()
    {
        //Optional debug line
        Debug.Log("Player has been captured");
        
        //Fix the players hopping animations
        AnimatorController.SetFloat(StaticUtilities.MoveSpeedAnimID, 0);
        cluckAbility.StartUsingAbility(); //Cluck to bring all chickens to you (done later)

        //Unlock the mouse and make it visible
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        
        //Make humans ignore us
        Visibility = 0;
        
        //Notify our subscribers (?) if we have any
        OnPlayerCaught?.Invoke(transform.position);
    }

    public void SetDashState(bool state)
    {
        if(state) dashAbility.StartUsingAbility();
        else dashAbility.StopUsingAbility();
    }

    public void SetCluckState(bool state)
    {
        if(state) cluckAbility.StartUsingAbility();
        else cluckAbility.StopUsingAbility();
    }

    public void SetJumpState(bool state)
    {
        if(state) jumpAbility.StartUsingAbility();
        else jumpAbility.StopUsingAbility();
    }

    public void SetMoveDirection(Vector2 direction)
    {
        //In unity, Y is up, so we need to convert to vector3, and have WS affect the forward (Z) axis.
        _moveDirection = new Vector3(direction.x, 0, direction.y);
    }

    public void SetLookDirection(Vector2 direction)
    {
        _lookDirection = direction;
    }

    private void HandleLooking()
    {
        //Caching the Time.deltaTime is important if you're using it more than once. It saves RAM.
        float timeShift = Time.deltaTime;
        float pitchChange = head.localEulerAngles.x - lookSpeed * _lookDirection.y * timeShift;
        float yawChange = transform.localEulerAngles.y + lookSpeed * _lookDirection.x * timeShift;
        
        //Apply limits so we don't Gimbal Lock ourselves
        // (Quaternion rotation would correct this but this does the job)
        if (pitchChange > pitchLimit && pitchChange < 180) pitchChange = pitchLimit;
        else if (pitchChange < 360-pitchLimit && pitchChange > 180) pitchChange = -pitchLimit;
        if (yawChange > yawLimit && yawChange < 180) yawChange = yawLimit;
        else if (yawChange < 360-yawLimit && yawChange > 180) yawChange = -yawLimit;

        //Apply the modifications to each part, be sure to use LOCAL euler angles, so that other systems work correctly.
        transform.localEulerAngles = new Vector3(0, yawChange, 0);
        head.localEulerAngles = new Vector3(pitchChange, 0, 0);
    }

    public AbstractAbility GetCluckAbility()
    {
        return cluckAbility;
    }
    
    public AbstractAbility GetJumpAbility()
    {
        return jumpAbility;
    }
    
    public AbstractAbility GetDashAbility()
    {
        return dashAbility;
    }
}

```
### [AiChicken.cs](../Assets/Scripts/Characters/Chicken/AiChicken.cs)
```csharp
using System.Collections;
using AI;
using Interfaces;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.AI;
using Utilities;

public class AiChicken : Chicken, IDetector
{

    //Either make these all serialized, or access them all in awake with GetComponent
    private FaceTarget _faceTarget;
    private AudioDetection _audioDetection;
    private NavMeshAgent _agent;

    [SerializeField] private HearStats activeHearing;
    
    protected override void Awake()
    {
        base.Awake();
        
        _faceTarget = GetComponent<FaceTarget>();
        _audioDetection = GetComponent<AudioDetection>();
        _agent = GetComponent<NavMeshAgent>();

        _agent.speed = stats.MaxSpeed;
        _agent.acceleration = stats.Speed;
    }

    private void OnEnable()
    {
        _faceTarget.enabled = false;
        _agent.enabled = true;
        
        _audioDetection.SetStats(activeHearing);
        
        //Enable object collisions
        BodyCollider.enabled = true;
        
        //Do cluck anim so we're not static, without the particle we just look like we're waving
        AnimatorController.SetBool(StaticUtilities.CluckAnimID, true);
        AnimatorController.enabled = true;
        
        //Subscribe to player events
        PlayerChicken.OnPlayerCaught += MoveTo;
        PlayerChicken.OnPlayerEscaped += MoveTo;
    }

    private void OnDisable()
    {
        //Unsubscribe from player events (as we can't complete them)
        PlayerChicken.OnPlayerCaught -= MoveTo;
        PlayerChicken.OnPlayerEscaped -= MoveTo;
        
        //Disable any active anims
        AnimatorController.SetBool(StaticUtilities.CluckAnimID, false);
        AnimatorController.enabled = false;
        
        //Stop the agent
        _agent.ResetPath();
        _agent.enabled = false;
        
        //Disable collisions
        BodyCollider.enabled = false;
        
        //Enable the face target component
        _faceTarget.enabled = true;
    }

    private void MoveTo(Vector3 location)
    {
        _agent.SetDestination(location);
    }

    protected override void HandleMovement()
    {
        //Move close to the target and decelerate when near them
        currentSpeed = Mathf.Max(0,_agent.remainingDistance - _agent.stoppingDistance + 0.2f);
        AnimatorController.SetFloat(StaticUtilities.MoveSpeedAnimID, currentSpeed);
    }

    public override void OnFreedFromCage()
    {
        enabled = true;
    }

    public override void OnEscaped(Vector3 position)
    {
        //Print who is trying to escape (When doing , gameObject the debug will show that object when pressed on in unity)
        Debug.Log("I'm trying to escape", gameObject);
        
        //Move to the location to escape
        MoveTo(position);
            
        //We should not escape just yet because the AI needs time to actually get to the exit...
        //let's start a coroutine and see if we've escaped.
        StartCoroutine(CheckForEscaped());

        //Hide the AI so that we don't have 'unfair' captures
        Visibility = 0;
    }
    
    private IEnumerator CheckForEscaped()
    {
        //CACHED Move until the path is done generating and we reach the target
        WaitUntil target = new WaitUntil(() => _agent.hasPath && _agent.remainingDistance <= _agent.stoppingDistance);

        //Use cached variable
        yield return target;
            
        Debug.Log("I'm trying to escape");
        
        //Destroy ourselves
        Destroy(gameObject);
    }

    public override void OnCaptured()
    {
        AnimatorController.SetFloat(StaticUtilities.MoveSpeedAnimID, 0);
    }

    public void AddDetection(Vector3 location, float detection, EDetectionType type)
    {
        if (!enabled || detection < 1) return;
        print("I'm moving towards: " + location);
        _agent.SetDestination(location);
        AnimatorController.SetBool(StaticUtilities.CluckAnimID, false);
    }
}

```
### [EndGoal.cs [COMPLETE]](../Assets/Scripts/Game/EndGoal.cs)
```csharp
using Characters;
using UnityEngine;

public class EndGoal : MonoBehaviour
{
    //Our action can be static, as we will only have one EndGoal.
    //public static Action onGameWon;
    [SerializeField] private Transform moveToLocation;
    [SerializeField] private LayerMask allowedLayers;

    void OnTriggerEnter(Collider other)
    {
        //If they're not a desired layer.
        if (((1 << other.gameObject.layer) & allowedLayers) == 0) return;
        
        if (other.attachedRigidbody.TryGetComponent(out Chicken c))
        {
            c.OnEscaped(moveToLocation.position);
        }
    }
}

```