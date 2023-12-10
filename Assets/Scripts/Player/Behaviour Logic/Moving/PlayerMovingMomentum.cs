using Cinemachine.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "Moving-Momentum", menuName = "Player Logic/Moving Logic/Momentum")]
public class PlayerMovingMomentum : PlayerMovingSOBase
{
    [Header("Movement Variables")]
    [SerializeField] private float walkSpeed = 7f;
    [SerializeField] private float sprintSpeed = 15f;
    [SerializeField] private float acceleration = 1f;
    [SerializeField] private float groundDrag = 1f;
    private bool sprinting = false;
    private float moveSpeed;
    private Vector3 moveDirection;

    [Header("Crouching Variables")]
    [SerializeField] private float crouchSpeed = 3.5f;
    private bool crouching;

    [Header("Jump Variables")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float jumpCooldown = 0.25f;
    private float timeOfLastJump;
    public bool readyToJump = true;
    private bool jumping;


    public override void Initialize(GameObject gameObject, PlayerStateMachine stateMachine, PlayerInputActions playerInputActions)
    {
        base.Initialize(gameObject, stateMachine, playerInputActions);
        readyToJump = true;
        jumping = false;
    }
    public override void DoEnterLogic()
    {
        moveDirection = Vector3.zero;
        base.DoEnterLogic();
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
    }

    public override void DoFixedUpdateState()
    {

        Move();
        base.DoFixedUpdateState();
    }

    public override void DoUpdateState()
    {
        GetInput();
        MovementTypeHandler();
        SpeedControl();



        base.DoUpdateState();
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }

    #region Helper Methods

    private void MovementTypeHandler()
    {
        // Type - Sprinting
        if (sprinting)
        {
            moveSpeed = sprintSpeed;
        }
        // Type - Crouching
        else if (crouching)
        {
            moveSpeed = crouchSpeed;
        }
        // Type - Walking
        else
        {
            moveSpeed = walkSpeed;
        }
      

    }

    private void GetInput()
    {
        inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
        sprinting = playerInputActions.Player.Sprint.ReadValue<float>() == 1f;
        jumping = playerInputActions.Player.Jump.ReadValue<float>() == 1f;
        crouching = playerInputActions.Player.Crouch.ReadValue<float>() == 1f;

        if (jumping)
        {
            Jump();
        }

        if (timeOfLastJump + jumpCooldown < Time.time)
        {
            ResetJump();
        }
    }
    private void Jump()
    {
        if(readyToJump)
        {
            readyToJump = false;

            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            timeOfLastJump = Time.time;



        }
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    // moves the player by adding a force
    private void Move()
    {
        // sprint logic (for now)
        moveDirection = stateMachine.orientation.forward * inputVector.y + stateMachine.orientation.right * inputVector.x;

        if(stateMachine.SlopeCheck())
        {
            rb.AddForce(GetSlopeMoveDirection() * acceleration, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * acceleration, ForceMode.Force);
        }


    }

    // Limits the speed of the player to speed
    private void SpeedControl()
    {
        // if the player is auto bhopping don't limit speed on ground
        if (jumping)
        {
            return;
        }
        rb.drag = groundDrag;

        // limit velocity on slope if player is not jumping
        if(stateMachine.SlopeCheck() && readyToJump)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        // limit velocity on ground
        else
        {
            Vector3 _flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if (_flatVel.magnitude > moveSpeed)
            {
                Vector3 _limitedVelTarget = _flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(_limitedVelTarget.x, rb.velocity.y, _limitedVelTarget.z);
            }
        }

    }
    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, stateMachine.slopeHit.normal).normalized;
    }

    #endregion
}
