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

    [Header("Jump Variables")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float jumpCooldown = 0.25f;
    [SerializeField] private float coyoteTime = 0.25f;
    public bool readyToJump = true;
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //variables for variable jump and no auto jump
    private float jumpTime = 0f;
    [SerializeField] private float maxJumpTime = 0.5f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private bool isJumping = false;
    [SerializeField] private bool airborne = false;
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private bool jumping;
    private bool bhopFrame;
    private int bhopFrames;
    [SerializeField] private int numBhopFrames;


    public override void Initialize(GameObject gameObject, PlayerStateMachine stateMachine, PlayerInputActions playerInputActions)
    {
        base.Initialize(gameObject, stateMachine, playerInputActions);
        readyToJump = true;
        jumping = false;
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
        readyToJump = true;
        base.DoExitLogic();
    }

    public override void DoFixedUpdateState()
    {
        if(!stateMachine.GroundedCheck())
        {
            airborne = true;
        }

        GetInput();
        Move();
        SpeedControl();

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
        if (bhopFrame) return;

        // Type - Sprinting
        if (sprinting && !stateMachine.crouching)
        {
            stateMachine.desiredMoveSpeed = sprintSpeed;
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

    private void GetInput()
    {
        inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
        sprinting = playerInputActions.Player.Sprint.ReadValue<float>() == 1f;
        jumping = playerInputActions.Player.Jump.ReadValue<float>() == 1f;

        
        if(airborne)
            {
                if(stateMachine.GroundedCheck())
                {
                    readyToJump = false;
                    airborne = false;
                }
            }

        //prevents auto jump
        if (stateMachine.timeOfLastJump + jumpCooldown < Time.time && playerInputActions.Player.Jump.ReadValue<float>() == 0f && stateMachine.GroundedCheck())
        {
            ResetJump();
        }


        if(readyToJump)
        {
            //start of jump
            if (jumping && !isJumping)
            {
                isJumping = true;
                jumpTime = 0f;
                rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            }

            //if holding jump, add more force
            if (jumping && isJumping)
            {
                if (jumpTime < maxJumpTime)
                {
                    jumpTime += Time.fixedDeltaTime;
                    rb.AddForce(Vector3.up * jumpForce * Time.fixedDeltaTime, ForceMode.Impulse);
                }
            }

            if (jumping)
            {
                isJumping = false;

                // Apply additional gravity to create variable jump height
                if (rb.velocity.y > 0)
                {
                    rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * (1f / fallMultiplier), rb.velocity.z);
                }
            }

            // Apply increased gravity during the fall to make the jump feel more natural
            if (rb.velocity.y < 0)
            {
                rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
            }
            else if (rb.velocity.y > 0 && !jumping)
            {
                rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
            }
        }
    }

    private void Jump()
    {
        if(readyToJump)
        {
            
            readyToJump = false;

            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            stateMachine.timeOfLastJump = Time.time;
            stateMachine.exitingGround = true;

        }
    }

    private void ResetJump()
    {
        stateMachine.exitingGround = false;
        readyToJump = true;
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

    public override void CheckTransitions()
    {
        // Moving => Airborne
        if (!stateMachine.GroundedCheck() && !stateMachine.SlopeCheck())
        {
            if(readyToJump)
            {
                // Coyote Frames
                stateMachine.StartCoroutine(stateMachine.CoyoteFrames(coyoteTime));
            }
            else
            {
                stateMachine.ChangeState(stateMachine.AirborneState);
            }

        }
        // Moving => Idle
        else if (playerInputActions.Player.Movement.ReadValue<Vector2>() == Vector2.zero)
        {
            stateMachine.ChangeState(stateMachine.IdleState);
        }
        // Moving => Wallrun
        else if (stateMachine.WallCheck() && jumping && !stateMachine.GroundedCheck())
            stateMachine.ChangeState(stateMachine.WallrunState);
    }

    // Limits the speed of the player to speed
    private void SpeedControl()
    {
        //exitingGround = timeOfLastJump + exitingGroundTimer > Time.time;



        // If the player is mid jump don't limit velocity
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
