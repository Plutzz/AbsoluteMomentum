using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Moving-Momentum", menuName = "Player Logic/Moving Logic/Momentum")]
public class PlayerMovingMomentum : PlayerMovingSOBase
{
    [Header("Movement Variables")]
    [SerializeField] private float walkSpeed = 7f;
    [SerializeField] private float sprintSpeed = 15f;
    [SerializeField] private float acceleration = 1f;
    [SerializeField] private float groundDrag = 1f;
    private bool sprinting = false;
    //[SerializeField] private float deceleration = 1f;

    [Header("Jump Variables")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float jumpCooldown = 0.25f;
    private float timeOfLastJump;
    private bool readyToJump = true;
    public override void Initialize(GameObject gameObject, PlayerStateMachine stateMachine, PlayerInputActions playerInputActions)
    {
        base.Initialize(gameObject, stateMachine, playerInputActions);
        readyToJump = true;
    }
    public override void DoEnterLogic()
    {
        base.DoEnterLogic();

        rb.drag = groundDrag;
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
        SpeedControl();

        if (playerInputActions.Player.Jump.ReadValue<float>() == 1f)
        {
            Jump();
        }

        if (timeOfLastJump + jumpCooldown < Time.time)
        {
            ResetJump();
        }



        base.DoUpdateState();
    }



    public override void ResetValues()
    {
        base.ResetValues();
    }

    //Helper Methods
    private void GetInput()
    {
        inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
        sprinting = playerInputActions.Player.Sprint.ReadValue<float>() != 0;
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
        Vector3 _moveDir = stateMachine.orientation.forward * inputVector.y + stateMachine.orientation.right * inputVector.x;

        if(sprinting)
        {
            rb.AddForce(_moveDir.normalized * acceleration * 2, ForceMode.Force);
        }
        else
        {
            rb.AddForce(_moveDir.normalized * acceleration, ForceMode.Force);
        }

    }

    // Limits the speed of the player to speed
    private void SpeedControl()
    {
        Vector3 _flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        // limit velocity if needed
        if (!sprinting && _flatVel.magnitude > walkSpeed)
        {
            Vector3 _limitedVel = _flatVel.normalized * walkSpeed;
            //Mathf.MoveTowards(rb.velocity.x, _limitedVel.x, deceleration);
            //Mathf.MoveTowards(rb.velocity.z, _limitedVel.z, deceleration);
            rb.velocity = new Vector3(_limitedVel.x, rb.velocity.y, _limitedVel.z);
        }
        else if(sprinting && _flatVel.magnitude > sprintSpeed) 
        {
            Vector3 _limitedVel = _flatVel.normalized * sprintSpeed;
            rb.velocity = new Vector3(_limitedVel.x, rb.velocity.y, _limitedVel.z);
        }

    }
}
