using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Wallrun-Default", menuName = "Player Logic/Wallrun Logic/Default")]
public class PlayerWallrunDefault : PlayerWallrunSOBase
{
    [SerializeField] private float wallrunSpeed = 35f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float wallrunGravity = 10f;
    [SerializeField] private float wallstickStrength = 10f;

    [SerializeField] private float wallJumpVerticalForce = 10f;
    [SerializeField] private float wallJumpSpeedBoost = 10f;

    [SerializeField] private float jumpCooldown;
    private bool readyToJump;

    [SerializeField] private float maxWallrunTime; 
    private float dropTimer;
    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
        rb.useGravity = false;
        stateMachine.canWallrun = false;
        dropTimer = maxWallrunTime;
        playerInputActions.Player.Jump.performed += WallJump;
    }

    public override void DoExitLogic()
    {
        rb.useGravity = true;
        stateMachine.canWallrun = false;
        stateMachine.timeOfLastWallrun = Time.time;
        playerInputActions.Player.Jump.performed -= WallJump;
        base.DoExitLogic();
    }

    public override void DoFixedUpdateState()
    {
        WallrunMovement();
        SpeedControl();
        base.DoFixedUpdateState();
    }

    public override void DoUpdateState()
    {
        base.DoUpdateState();

        GetInput();
        MovementSpeedHandler();
        dropTimer -= Time.deltaTime;
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }

    public override void CheckTransitions()
    {
        if (stateMachine.GroundedCheck())
        {
            if (inputVector == Vector2.zero)
            {
                stateMachine.ChangeState(stateMachine.IdleState);
            }
            else
            {
                stateMachine.ChangeState(stateMachine.MovingState);
            }
        }

        else if ((dropTimer < 0) || !stateMachine.WallCheck())
        {
            stateMachine.ChangeState(stateMachine.AirborneState);
        }
    }
    private void GetInput()
    {
        inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();

        if (stateMachine.timeOfLastJump + jumpCooldown < Time.time)
        {
            ResetJump();
        }
    }
    private void WallJump(InputAction.CallbackContext context)
    {
        if (!readyToJump)
            return;

        readyToJump = false;

        stateMachine.moveSpeed += wallJumpSpeedBoost;

        Vector3 _moveDir = stateMachine.orientation.forward * inputVector.y + stateMachine.orientation.right * inputVector.x;
        Vector3 _force = stateMachine.player.up * wallJumpVerticalForce + _moveDir * 100f;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(_force, ForceMode.Impulse);

        stateMachine.ChangeState(stateMachine.AirborneState);
    }

    private void WallrunMovement()
    {
        // Gravity
        rb.AddForce(Vector3.down * wallrunGravity, ForceMode.Force);

        Vector3 _orientation = stateMachine.orientation.forward * inputVector.y + stateMachine.orientation.right * inputVector.x;

        Vector3 _normal = stateMachine.isWallRight ? stateMachine.wallRight.normal : stateMachine.wallLeft.normal;
        Vector3 _moveDirection = Vector3.ProjectOnPlane(_orientation, _normal);

        Debug.DrawRay(stateMachine.transform.position, _moveDirection);
        rb.AddForce(_moveDirection * 100f, ForceMode.Force);

        // Wall Stick
        rb.AddForce(-_normal * wallstickStrength, ForceMode.Force);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void SpeedControl()
    {
        Vector3 _flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (_flatVel.magnitude > stateMachine.moveSpeed)
        {
            Vector3 _limitedVelTarget = _flatVel.normalized * stateMachine.moveSpeed;
            rb.velocity = new Vector3(_limitedVelTarget.x, rb.velocity.y, _limitedVelTarget.z);
        }
    }

    private void MovementSpeedHandler()
    {
        stateMachine.desiredMoveSpeed = wallrunSpeed;

        if (Mathf.Abs(stateMachine.desiredMoveSpeed - stateMachine.lastDesiredMoveSpeed) > 1f && stateMachine.moveSpeed != 0)
        {
            Debug.Log("START COROUTINE");
            stateMachine.StopCoroutine(stateMachine.SmoothlyLerpMoveSpeed(acceleration));
            stateMachine.StartCoroutine(stateMachine.SmoothlyLerpMoveSpeed(acceleration));
        }
        else
        {
            stateMachine.moveSpeed = stateMachine.desiredMoveSpeed;
        }

        stateMachine.lastDesiredMoveSpeed = stateMachine.desiredMoveSpeed;

    }
}
