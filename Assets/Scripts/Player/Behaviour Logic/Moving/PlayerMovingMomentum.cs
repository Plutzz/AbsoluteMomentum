using Cinemachine.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
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


    private bool bhopFrame;
    private int bhopFrames;
    [SerializeField] private int numBhopFrames;


    public override void Initialize(GameObject gameObject, PlayerStateMachine stateMachine, PlayerInputActions playerInputActions)
    {
        base.Initialize(gameObject, stateMachine, playerInputActions);
    }
    public override void DoEnterLogic()
    {
        moveDirection = Vector3.zero;
        rb.drag = groundDrag;
        bhopFrame = true;
        bhopFrames = numBhopFrames;
        playerInputActions.Player.Crouch.performed += TryStartSlide;
        stateMachine.StopAllCoroutines();
        base.DoEnterLogic();
    }

    public override void DoExitLogic()
    {
        playerInputActions.Player.Crouch.performed -= TryStartSlide;
        base.DoExitLogic();
    }

    public override void DoFixedUpdateState()
    {
        GetInput();
        Move();
        SpeedControl();

        if (stateMachine.CollisionCheck())
        {

        }

        base.DoFixedUpdateState();
    }

    public override void DoUpdateState()
    {
        MovementSpeedHandler();

        //Disables gravity while on slopes
        rb.useGravity = !stateMachine.SlopeCheck();

        base.DoUpdateState();
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }

    #region Helper Methods
    private void MovementSpeedHandler()
    {
        if (bhopFrame || stateMachine.Boosting) return;

        // Type - Sprinting
        if (sprinting && !stateMachine.crouching)
        {
            stateMachine.desiredMoveSpeed = sprintSpeed;
            //Debug.Log("The sprint speed is " + sprintSpeed);
        }
        // Type - Crouching
        else if (stateMachine.crouching)
        {
            stateMachine.desiredMoveSpeed = crouchSpeed;
        }
        // Type - Walking
        else
        {
            stateMachine.desiredMoveSpeed = walkSpeed;
        }


        if (Mathf.Abs(stateMachine.desiredMoveSpeed - stateMachine.lastDesiredMoveSpeed) > 1f && stateMachine.moveSpeed != 0)
        {
            //Debug.Log("START COROUTINE");
            stateMachine.StopCoroutine(stateMachine.SmoothlyLerpMoveSpeed(acceleration));
            stateMachine.StartCoroutine(stateMachine.SmoothlyLerpMoveSpeed(acceleration));
        }
        else
        {
            stateMachine.moveSpeed = stateMachine.desiredMoveSpeed;
        }

        stateMachine.lastDesiredMoveSpeed = stateMachine.desiredMoveSpeed;

    }

    private void GetInput()
    {
        inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
        sprinting = playerInputActions.Player.Sprint.ReadValue<float>() == 1f;
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
        if (stateMachine.canSlide && (sprinting || (stateMachine.SlopeCheck() && rb.velocity.y < -0.1f)))
        {
            stateMachine.ChangeState(stateMachine.SlidingState);
        }
    }

    public override void CheckTransitions()
    {
        // Moving => Airborne
        if (!stateMachine.GroundedCheck() && !stateMachine.SlopeCheck())
        {
            if(stateMachine.jumpHandler.canJump)
            {
                stateMachine.jumpHandler.StartCoyoteTime();
            }

            stateMachine.ChangeState(stateMachine.AirborneState);
        }
        // Moving => Idle
        else if (playerInputActions.Player.Movement.ReadValue<Vector2>() == Vector2.zero)
        {
            stateMachine.ChangeState(stateMachine.IdleState);
        }
      
    }

    // Limits the speed of the player to speed
    private void SpeedControl()
    {
        
        // If the player is landing don't limit velocity
        if (bhopFrames > 0)
        {
            bhopFrames--;
            return;
        }
        //This resets the sliding boost if the player is not chaining slides
        stateMachine.PlayerSlidingBaseInstance.reachedMaxSpeed = false;
        bhopFrame = false;
        // limit velocity on slope if player is not leaving the slope
        if (stateMachine.SlopeCheck())
        {

            if (rb.velocity.magnitude > stateMachine.moveSpeed)
                rb.velocity = rb.velocity.normalized * stateMachine.moveSpeed;
        }
        // limit velocity on ground
        else
        {
            Vector3 _flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if (_flatVel.magnitude > stateMachine.moveSpeed)
            {
                Vector3 _limitedVelTarget = _flatVel.normalized * stateMachine.moveSpeed;
                rb.velocity = new Vector3(_limitedVelTarget.x, rb.velocity.y, _limitedVelTarget.z);
            }
        }

       
    }

    #endregion

}
