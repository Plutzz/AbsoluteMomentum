using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Airborne-Momentum", menuName = "Player Logic/Airborne Logic/Momentum")]
public class PlayerAirborneMomentum : PlayerAirborneSOBase
{
    [SerializeField] private float acceleration = 40f; // player is only able to decelerate during this state
    [SerializeField] private float drag = 0f;
    [SerializeField] private float minimumSlideVelocity = 9f;
    private bool sprinting;
    private float speedOnEnter; // speed while entering the state
    public override void Initialize(GameObject gameObject, PlayerStateMachine stateMachine, PlayerInputActions playerInputActions)
    {
        base.Initialize(gameObject, stateMachine, playerInputActions);
    }
    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
        rb.drag = drag;
        speedOnEnter = stateMachine.moveSpeed;
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
    }

    public override void DoFixedUpdateState()
    {
        base.DoFixedUpdateState();
        Move();
        SpeedControl();
    }

    public override void DoUpdateState()
    {
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
        if (sprinting)
        {
            rb.AddForce(_moveDir.normalized * acceleration * 2, ForceMode.Force);
        }
        else
        {
            rb.AddForce(_moveDir.normalized * acceleration, ForceMode.Force);
        }
    }

    public override void CheckTransitions()
    {
        if(stateMachine.GroundedCheck() || stateMachine.SlopeCheck())
        {
            // Airborne => Sliding
            if (stateMachine.crouching && playerInputActions.Player.Jump.ReadValue<float>() == 0 && rb.velocity.magnitude > minimumSlideVelocity)
            {
                stateMachine.ChangeState(stateMachine.SlidingState);
            }
            // Airborne => Moving
            else if (playerInputActions.Player.Movement.ReadValue<Vector2>() != Vector2.zero)
            {
                stateMachine.ChangeState(stateMachine.MovingState);
            }
            // Airborne => Idle
            else if (playerInputActions.Player.Movement.ReadValue<Vector2>() == Vector2.zero)
            {
                stateMachine.ChangeState(stateMachine.IdleState);
            } 
        }
        // Airborne => Wallrunning
        else if (stateMachine.WallCheck())
        {
            stateMachine.ChangeState(stateMachine.WallrunState);
        }


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
