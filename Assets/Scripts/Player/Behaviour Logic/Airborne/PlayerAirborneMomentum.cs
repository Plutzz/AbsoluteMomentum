using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Airborne-Momentum", menuName = "Player Logic/Airborne Logic/Momentum")]
public class PlayerAirborneMomentum : PlayerAirborneSOBase
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float turnSmoothTime = 0.1f;
    [SerializeField] private float drag = 0f;
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
        rb.drag = drag;
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
    }

    public override void DoFixedUpdateState()
    {
        base.DoFixedUpdateState();
        Move();
    }

    public override void DoUpdateState()
    {
        GetInput();
        base.DoUpdateState();
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }

    private void GetInput()
    {
        inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
    }
    private void Move()
    {
        if(inputVector == Vector2.zero) { return; }

        float targetAngle = Mathf.Atan2(inputVector.x, inputVector.y) * Mathf.Rad2Deg + cam.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(gameObject.transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
        gameObject.transform.rotation = Quaternion.Euler(0f, angle, 0f);
        float speed = this.speed;

        if (playerInputActions.Player.Sprint.ReadValue<float>() == 1)
        {
            speed = speed * 2;
        }

        Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        rb.AddForce(moveDir.normalized * speed, ForceMode.Force);
    }
}
