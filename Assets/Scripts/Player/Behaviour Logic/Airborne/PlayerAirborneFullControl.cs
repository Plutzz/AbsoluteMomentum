using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Airborne-FullControl", menuName = "Player Logic/Airborne Logic/Full Control")]
public class PlayerAirborneFullControl : PlayerAirborneSOBase
{
    [SerializeField] private float speed = 5f;
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
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
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
        rb.velocity = Vector3.zero;
    }

    private void GetInput()
    {
        inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
    }
    private void Move()
    {
        if (inputVector == Vector2.zero)
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            return;
        }

        float targetAngle = Mathf.Atan2(inputVector.x, inputVector.y) * Mathf.Rad2Deg + cam.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(gameObject.transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
        gameObject.transform.rotation = Quaternion.Euler(0f, angle, 0f);
        float speed = this.speed;

        if (playerInputActions.Player.Sprint.ReadValue<float>() == 1)
        {
            speed = speed * 2;
        }

        Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        rb.velocity = new Vector3(moveDir.x * speed, rb.velocity.y, moveDir.z * speed);
    }
}

