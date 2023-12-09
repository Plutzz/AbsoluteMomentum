using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Moving-Momentum", menuName = "Player Logic/Moving Logic/Momentum")]
public class PlayerMovingMomentum : PlayerMovingSOBase
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float jumpHeight = 5f;
    [SerializeField] private float turnSmoothTime = 0.1f;
    //[SerializeField] private float deceleration = 1f;
    [SerializeField] private float acceleration = 1f;
    [SerializeField] private float groundDrag = 1f;
    private Transform cam;

    private float turnSmoothVelocity;
    public override void Initialize(GameObject gameObject, PlayerStateMachine stateMachine, PlayerInputActions playerInputActions)
    {
        base.Initialize(gameObject, stateMachine, playerInputActions);
        cam = Camera.main.transform;
    }
    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
        if(playerInputActions.Player.Jump.ReadValue<float>() == 1f)
        {
            Jump();
        }

        playerInputActions.Player.Jump.performed += JumpPressed;
        rb.drag = groundDrag;
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
        playerInputActions.Player.Jump.performed -= JumpPressed;
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
    }
    private void JumpPressed(InputAction.CallbackContext context)
    {
        Jump();
    }
    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
    }

    // moves the player by adding a force
    private void Move()
    {
        float _targetAngle = Mathf.Atan2(inputVector.x, inputVector.y) * Mathf.Rad2Deg + cam.eulerAngles.y;
        float _angle = Mathf.SmoothDampAngle(gameObject.transform.eulerAngles.y, _targetAngle, ref turnSmoothVelocity, turnSmoothTime);
        gameObject.transform.rotation = Quaternion.Euler(0f, _angle, 0f);

        Vector3 _moveDir = Quaternion.Euler(0f, _targetAngle, 0f) * Vector3.forward;

        // sprint logic (for now)
        if (playerInputActions.Player.Sprint.ReadValue<float>() == 1)
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
        if (_flatVel.magnitude > speed)
        {
            Vector3 _limitedVel = _flatVel.normalized * speed;
            //Mathf.MoveTowards(rb.velocity.x, _limitedVel.x, deceleration);
            //Mathf.MoveTowards(rb.velocity.z, _limitedVel.z, deceleration);
            rb.velocity = new Vector3(_limitedVel.x, rb.velocity.y, _limitedVel.z);
        }

    }
}
