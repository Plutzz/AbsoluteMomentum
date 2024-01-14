using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class JumpHandler : NetworkBehaviour
{
    private PlayerStateMachine stateMachine;
    private PlayerState currentState;
    private PlayerInputActions playerInputActions;
    private Rigidbody rb;

    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float jumpCooldown = 0.25f;
    [SerializeField] private float coyoteTime = 0.25f;
    private float coyoteTimer;

    public bool canJump = true;

    [SerializeField] private float maxJumpTime = 0.5f;
    [SerializeField] private float endJumpEarlyMultiplier = 2f;
    [SerializeField] private float jumpBufferTime = 0.2f;
    private float jumpTimer = 0f;

    private float lastJumpPressed;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        stateMachine = GetComponent<PlayerStateMachine>();
        rb = stateMachine.rb;
        playerInputActions = stateMachine.playerInputActions;

        playerInputActions.Player.Jump.performed += JumpPressed;
        canJump = true;
    }


    void Update()
    {
        currentState = stateMachine.currentState;
        if (stateMachine.timeOfLastJump + jumpCooldown <= Time.time)
        {
            ResetJump();
        }
        HandleTimers();


        // Returns if the player is not in a state where they can jump
        if (currentState == stateMachine.AirborneState || currentState == stateMachine.WallrunState) return;


        // Jump Buffering

        if (canJump && lastJumpPressed + jumpBufferTime >= Time.time)
        {
            GroundedJump();
        }
    }

    private void JumpPressed(InputAction.CallbackContext context)
    {
        lastJumpPressed = Time.time;

        if (coyoteTimer <= 0 && (currentState == stateMachine.AirborneState || currentState == stateMachine.WallrunState)) return;

        if (canJump)
        {
            GroundedJump();
        }
    }

    private void GroundedJump()
    {
        canJump = false;
        jumpTimer = maxJumpTime;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        stateMachine.timeOfLastJump = Time.time;
    }

    private void EndJumpEarly()
    {
        rb.AddForce(Vector3.down * endJumpEarlyMultiplier, ForceMode.Impulse);
        jumpTimer = 0f;
    }

    private void ResetJump()
    {
        canJump = true;
    }

    public void StartCoyoteTime()
    {
        coyoteTimer = coyoteTime;
    }

    private void HandleTimers()
    {
        if(coyoteTimer > 0)
        {
            coyoteTimer -= Time.deltaTime;
        }

        // Time Left in Jump
        if(jumpTimer > 0)
        {
            jumpTimer -= Time.deltaTime;
            if (playerInputActions.Player.Jump.ReadValue<float>() == 0f)
            {
                EndJumpEarly();
            }
        }
    }
}
