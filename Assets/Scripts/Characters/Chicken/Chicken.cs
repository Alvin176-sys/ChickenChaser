using UnityEngine;
using Utilities;

public abstract class Chicken : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] protected float Speed;
    [SerializeField] protected float MaxSpeed;

    [Header("Foot Management")]
    [SerializeField] protected float FootRadius;
    [SerializeField] protected float FootDistance;

    [Header("Objects")]
    [SerializeField] protected Transform Head;
    [SerializeField] protected Transform Foot;

    protected Rigidbody ThisRigidBody;
    protected Animator AnimatorController;
    public bool IsGrounded { get; protected set; }
    public float CurrentSpeed { get; protected set; }

    protected float CurrentFallTime;
    protected Vector3 SlopeNormal;

    protected virtual void Awake()
    {
        ThisRigidBody = GetComponent<Rigidbody>();
        AnimatorController = GetComponentInChildren<Animator>();
    }

    private void FixedUpdate()
    {
        HandleGroundState();
        HandleMovement();
    }

    private void Update()
    {
        
    }

    private void HandleGroundState()
    {
        //returns true if the sphere sweep intersects any collider...otherwise returns false
        bool newGroundedState = Physics.SphereCast(Foot.position, FootRadius, Vector3.down, out RaycastHit slope, FootDistance);

        //if the ground state is different
        if (newGroundedState != IsGrounded)
        {
            //we should enter that state
            IsGrounded = newGroundedState;

            AnimatorController.SetBool(StaticUtilities.IsGroundedAnimID, IsGrounded);

            //if we were falling
            if (CurrentFallTime >= 0)
            {
                //handle the landing
                HandleLanding();
                CurrentFallTime = 0;
            }
        }

        //if we're not grounded then update the air time
        if (!IsGrounded) CurrentFallTime += Time.deltaTime;

        //If we are grounded keep track of the slope normal so that movement is smoother
        else SlopeNormal = slope.normal;
    }

    protected virtual void HandleLanding()
    { 
    
    }

    protected virtual void HandleAnims()
    {
        AnimatorController.SetFloat(StaticUtilities.MoveSpeedAnimID, CurrentSpeed);
    }

    protected abstract void HandleMovement();

    public abstract void OnFreedFromCage();
    public abstract void OnEscaped(Vector3 position);
    public abstract void OnCaptured();

    public Vector3 GetLookDirection()
    {
        return Head.forward;
    }
}
