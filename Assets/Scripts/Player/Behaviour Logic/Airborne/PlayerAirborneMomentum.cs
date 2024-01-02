using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Airborne-Momentum", menuName = "Player Logic/Airborne Logic/Momentum")]
public class PlayerAirborneMomentum : PlayerAirborneSOBase
{
    [SerializeField] private float acceleration = 40f; // player is only able to decelerate during this state
    [SerializeField] private float drag = 0f;
    [SerializeField] private float minimumSlideVelocity = 9f;

    [SerializeField] private float upwardGravityAcceleration;   //When the player is moving upward there will be less gravity applied
    [SerializeField] private float downwardGravityAcceleration; //When the player is moving downward there will be more gravity applied
    [SerializeField] private float quickFallGravityIncrease;

    [SerializeField] private float sprintMovementMultiplier;
    [SerializeField] private float apexMovementMultiplier;
    [SerializeField] private float apexYVelocityThreshold;

    private bool quickFalling;

    private bool sprinting;
    private float speedOnEnter; // speed while entering the state
    public override void Initialize(GameObject gameObject, PlayerStateMachine stateMachine, PlayerInputActions playerInputActions)
    {
        base.Initialize(gameObject, stateMachine, playerInputActions);
    }
    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
        stateMachine.StopAllCoroutines();
        rb.drag = drag;
        speedOnEnter = stateMachine.moveSpeed;

        //rb.useGravity = false;
        quickFalling = false;
    }

    public override void DoExitLogic()
    {
        //rb.useGravity = true;
        base.DoExitLogic();
    }

    public override void DoFixedUpdateState()
    {
        base.DoFixedUpdateState();
        Move();
        SpeedControl();
        stateMachine.WallCheck();

        

        if (rb.velocity.y > 0)
        {
            rb.velocity += Vector3.down * upwardGravityAcceleration * Time.fixedDeltaTime;
        }
        else {
            rb.velocity += Vector3.down * downwardGravityAcceleration * Time.fixedDeltaTime;
        }

        if (quickFalling) {
            rb.velocity += Vector3.down * quickFallGravityIncrease * Time.fixedDeltaTime;
        }
    }

    public override void DoUpdateState()
    {
        stateMachine.WallCheck();
        GetInput();
        MovementSpeedHandler();
        base.DoUpdateState();
    }

    public override void ResetValues()
    {
        speedOnEnter = 0f;
        base.ResetValues();
    }

    private void GetInput()
    {
        inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
        sprinting = playerInputActions.Player.Sprint.ReadValue<float>() != 0;

        quickFalling = Input.GetKey(KeyCode.LeftControl);
    }
    private void MovementSpeedHandler()
    {
        stateMachine.desiredMoveSpeed = speedOnEnter;

        stateMachine.moveSpeed = stateMachine.desiredMoveSpeed;

        stateMachine.lastDesiredMoveSpeed = stateMachine.desiredMoveSpeed;
    }
    private void Move()
    {
        if (inputVector == Vector2.zero) { return; }


        Vector3 _moveDir = stateMachine.orientation.forward * inputVector.y + stateMachine.orientation.right * inputVector.x;
        
        float forceMultiplier = 1;

        if (sprinting) {
            forceMultiplier *= sprintMovementMultiplier;
        }

        if (Mathf.Abs(rb.velocity.y) < apexYVelocityThreshold) {
            forceMultiplier *= apexMovementMultiplier;
        }

        rb.AddForce(_moveDir.normalized * acceleration * forceMultiplier, ForceMode.Force);

        /*if (sprinting)
        {
            rb.AddForce(_moveDir.normalized * acceleration * 2, ForceMode.Force);
        }
        else
        {
            rb.AddForce(_moveDir.normalized * acceleration, ForceMode.Force);
        }*/
    }

    public override void CheckTransitions()
    {
        if (stateMachine.WallCheck())
        {
            stateMachine.ChangeState(stateMachine.WallrunState);
        }
        // Airborne => Sliding
        else if ((stateMachine.SlopeCheck() || stateMachine.GroundedCheck()) && stateMachine.crouching && playerInputActions.Player.Jump.ReadValue<float>() == 0 
            && rb.velocity.magnitude > minimumSlideVelocity)
        {
            stateMachine.ChangeState(stateMachine.SlidingState);
        }
        // Airborne => Moving
        else if (stateMachine.GroundedCheck() && playerInputActions.Player.Movement.ReadValue<Vector2>() != Vector2.zero)
        {
            stateMachine.ChangeState(stateMachine.MovingState);
        }
        // Airborne => Idle
        else if (stateMachine.GroundedCheck() && playerInputActions.Player.Movement.ReadValue<Vector2>() == Vector2.zero)
        {
            stateMachine.ChangeState(stateMachine.IdleState);
        } 
        // Airborne => Wallrunning
        // Might need to add a check that you are pressing an input key into the wall to "latch on" to the wall


    }

    private void SpeedControl()
    {
        // hard limit move speed
        Vector3 _flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (_flatVel.magnitude > stateMachine.moveSpeed)
        {
            Vector3 _limitedVelTarget = _flatVel.normalized * stateMachine.moveSpeed;
            rb.velocity = new Vector3(_limitedVelTarget.x, rb.velocity.y, _limitedVelTarget.z);
        }
    }
}
