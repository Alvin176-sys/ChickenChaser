using UnityEngine;

public class PlayerChicken : Chicken
{
    private Vector2 _lookDirection;
    private Vector3 _moveDirection;

    [Header("Looking")]
    [SerializeField, Range(0, 90)] private float pitchLimit = 30; // Partial rotation up and down
    [SerializeField, Range(0, 180)] private float yawLimit = 180; // Full rotation side ways
    [SerializeField] private float lookSpeed = 5;

    [Header("Abilities")]
    [SerializeField] private AbstractAbility _jumpAbility;
    [SerializeField] private AbstractAbility _cluckAbility;
    [SerializeField] private AbstractAbility _dashAbility;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Awake()
    {
        base.Awake();
        PlayerControls.Initialize(this);
        PlayerControls.UseGameControls();
    }

    private void OnDisable()
    {
        PlayerControls.DisablePlayer();
        _jumpAbility.ForceCancelAbility();
        _cluckAbility.ForceCancelAbility();
        _dashAbility.ForceCancelAbility();
    }
   
    public void SetDashState(bool state)
    {
        if (state) _dashAbility.StartUsingAbility();
        else _dashAbility.StopUsingAbility();

        //OPTIONAL for debugging
        Debug.Log("Dash: " + state);
    }
    public void SetCluckState(bool state)
    {
        if (state) _cluckAbility.StartUsingAbility();
        else _cluckAbility.StopUsingAbility();

        //OPTIONAL for debugging
        Debug.Log("Cluck: " + state);
    }
    public void SetJumpState(bool state)
    {
        if (state) _jumpAbility.StartUsingAbility();
        else _jumpAbility.StopUsingAbility();
        //OPTIONAL for debugging
        Debug.Log("Jump: " + state);
    }
    public void SetMoveDirection(Vector2 direction)
    {
        _moveDirection = new Vector3(direction.x, 0, direction.y);
        Debug.Log(_moveDirection);
    }
    public void SetLookDirection(Vector2 direction)
    {
        _lookDirection = direction;
    }

    private void HandleLooking()
    {
        //Caching the Time.deltaTime is important if you're using it more than once. It saves RAM.
        float timeShift = Time.deltaTime;
        float pitchChange = Head.localEulerAngles.x - lookSpeed * _lookDirection.y * timeShift;
        float yawChange = transform.localEulerAngles.y + lookSpeed * _lookDirection.x * timeShift;

        //Apply limits so we don't Gimbal Lock ourselves
        // (Quaternion rotation would correct this but this does the job)
        if (pitchChange > pitchLimit && pitchChange < 180) pitchChange = pitchLimit;
        else if (pitchChange < 360 - pitchLimit && pitchChange > 180) pitchChange = -pitchLimit;
        if (yawChange > yawLimit && yawChange < 180) yawChange = yawLimit;
        else if (yawChange < 360 - yawLimit && yawChange > 180) yawChange = -yawLimit;

        //Apply the modifications to each part, be sure to use LOCAL euler angles, so that other systems work correctly.
        transform.localEulerAngles = new Vector3(0, yawChange, 0);
        Head.localEulerAngles = new Vector3(pitchChange, 0, 0);
    }

    protected override void HandleMovement()
    {
        Vector3 direction = _moveDirection;
        
        //if grounded, then the direction we want to move should be projected onto the plane
        //doing this will help us move up steep slopes easier
        if (IsGrounded)
        {
            direction = Vector3.ProjectOnPlane(_moveDirection, SlopeNormal);
        }

        ThisRigidBody.AddForce(transform.rotation * direction * Speed, ForceMode.Acceleration);

        //we only care about horizontal speed so Y doesn't matter
        Vector2 horizontalVelocity = new Vector2(ThisRigidBody.linearVelocity.x, ThisRigidBody.linearVelocity.z);
        CurrentSpeed = horizontalVelocity.magnitude;

        if (CurrentSpeed > MaxSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * MaxSpeed;

            //limit the speed but make sure to keep the gravity speed
            ThisRigidBody.linearVelocity = new Vector3(horizontalVelocity.x, ThisRigidBody.linearVelocity.y, horizontalVelocity.y);

            //lock speed to prevent weird bugs
            CurrentSpeed = MaxSpeed;
        }

        HandleLooking();
    }

    public override void OnFreedFromCage()
    {
    }

    public override void OnEscaped(Vector3 position)
    {
    }

    public override void OnCaptured()
    {
    }
}
