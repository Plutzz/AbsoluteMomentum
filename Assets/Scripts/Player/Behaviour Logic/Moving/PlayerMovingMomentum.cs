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

    [SerializeField] private int bhopFrames;


    public override void Initialize(GameObject gameObject, PlayerStateMachine stateMachine, PlayerInputActions playerInputActions)
    {
        base.Initialize(gameObject, stateMachine, playerInputActions);
        readyToJump = true;
        timeOfLastJump = Time.time;
        jumping = false;
    }
    public override void DoEnterLogic()
    {
        moveDirection = Vector3.zero;
        rb.drag = groundDrag;
        playerInputActions.Player.Crouch.performed += TryStartSlide;
        base.DoEnterLogic();
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
        playerInputActions.Player.Crouch.performed -= TryStartSlide;
    }

    public override void DoFixedUpdateState()
    {
        Move();

        base.DoFixedUpdateState();
    }

    public override void DoUpdateState()
    {
        GetInput();
        MovementSpeedHandler();




        base.DoUpdateState();
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }

    #region Helper Methods
    private void MovementSpeedHandler()
    {
        // Type - Sprinting
        if (sprinting && !crouching)
        {
            stateMachine.desiredMoveSpeed = sprintSpeed;
        }
        // Type - Crouching
        else if (crouching)
        {
            stateMachine.desiredMoveSpeed = crouchSpeed;
        }
        // Type - Walking
        else
        {
            stateMachine.desiredMoveSpeed = walkSpeed;
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
            stateMachine.exitingSlope = true;
            readyToJump = false;

            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            timeOfLastJump = Time.time;



        }
    }

    private void ResetJump()
    {
        readyToJump = true;
        stateMachine.exitingSlope = false;
    }

    // moves the player by adding a force
    private void Move()
    {
        // sprint logic (for now)
        moveDirection = stateMachine.orientation.forward * inputVector.y + stateMachine.orientation.right * inputVector.x;

        if(stateMachine.SlopeCheck())
        {
            rb.AddForce(stateMachine.GetSlopeMoveDirection(moveDirection) * acceleration, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * acceleration, ForceMode.Force);
        }
    }

    
    private void TryStartSlide(InputAction.CallbackContext context)
    {
        if (sprinting || stateMachine.SlopeCheck())
        {
            stateMachine.ChangeState(stateMachine.SlidingState);
        }
    }



    #endregion
}
