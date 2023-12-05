
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Moving-NoMomentum", menuName = "Player Logic/Moving Logic/NoMomentum")]
public class PlayerMovingNoMomentum : PlayerMovingSOBase
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float jumpHeight = 5f;
    [SerializeField] private float turnSmoothTime = 0.1f;
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
        playerInputActions.Player.Jump.performed += Jump;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
        playerInputActions.Player.Jump.performed -= Jump;
    }

    public override void DoFixedUpdateState()
    {
        base.DoFixedUpdateState();
    }

    public override void DoUpdateState()
    {
        GetInput();
        Move();
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
    private void Jump(InputAction.CallbackContext context)
    {
        //Debug.Log(context);
        //Debug.Log("Jump!" + context.phase);
        rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
    }

    private void Move()
    {
        float targetAngle = Mathf.Atan2(inputVector.x, inputVector.y) * Mathf.Rad2Deg + cam.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(gameObject.transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
        gameObject.transform.rotation = Quaternion.Euler(0f, angle, 0f);
        float speed = this.speed;

        if(playerInputActions.Player.Sprint.ReadValue<float>() == 1)
        {
            speed = speed * 2;
        }

        Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        rb.velocity = new Vector3(moveDir.x * speed, rb.velocity.y, moveDir.z * speed);
    }
}
