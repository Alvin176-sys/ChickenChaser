using Interfaces;
using UnityEngine;
using Utilities;

public abstract class Chicken : MonoBehaviour, IVisualDetectable, ITrappable
{
    [SerializeField] protected ChickenStats stats;
    

    [Header("Objects")]
    [SerializeField] protected Transform Head;
    [SerializeField] protected Transform Foot;
    protected float Visibility = 1;

    protected Rigidbody ThisRigidBody;
    protected Animator AnimatorController;
    protected Collider BodyCollider;
    public bool IsGrounded { get; protected set; }
    public float CurrentSpeed { get; protected set; }

    protected float CurrentFallTime;
    protected Vector3 SlopeNormal;

    protected virtual void Awake()
    {
        ThisRigidBody = GetComponent<Rigidbody>();
        AnimatorController = GetComponentInChildren<Animator>();
        BodyCollider = GetComponentInChildren<Collider>();
    }

    private void FixedUpdate()
    {
        HandleGroundState();
        HandleMovement();
    }

    private void Update()
    {
        
    }
    public Vector3 DistanceFromZero(Vector3 zero)
    {
        return zero - transform.position;
    }
    private void HandleGroundState()
    {
        //returns true if the sphere sweep intersects any collider...otherwise returns false
        bool newGroundedState = Physics.SphereCast(Foot.position, stats.FootRadius, Vector3.down, out RaycastHit slope, stats.FootDistance);

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

    public void AddVisibility(float visibility)
    {
        Visibility += visibility;
    }

    public void RemoveVisibility(float visibility)
    {
        Visibility -= Mathf.Max(0, visibility);
        
    }

    public float GetVisibility()
    {
        return Visibility;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public bool CanBeTrapped()
    {
        return isActiveAndEnabled;
    }

    public void OnPreCapture()
    {
        enabled = false;
    }
}
