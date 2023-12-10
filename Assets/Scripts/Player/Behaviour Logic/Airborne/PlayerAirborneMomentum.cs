using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Airborne-Momentum", menuName = "Player Logic/Airborne Logic/Momentum")]
public class PlayerAirborneMomentum : PlayerAirborneSOBase
{
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float acceleration = 40f;
    [SerializeField] private float drag = 0f;
    private bool sprinting;
    public override void Initialize(GameObject gameObject, PlayerStateMachine stateMachine, PlayerInputActions playerInputActions)
    {
        base.Initialize(gameObject, stateMachine, playerInputActions);
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
        SpeedControl();
        base.DoUpdateState();
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }

    private void GetInput()
    {
        inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
        sprinting = playerInputActions.Player.Sprint.ReadValue<float>() != 0;
    }
    private void Move()
    {
        if(inputVector == Vector2.zero) { return; }


        Vector3 _moveDir = stateMachine.orientation.forward * inputVector.y + stateMachine.orientation.right * inputVector.x;
        if (sprinting)
        {
            rb.AddForce(_moveDir.normalized * acceleration * 2, ForceMode.Force);
        }
        else
        {
            rb.AddForce(_moveDir.normalized * acceleration, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        Vector3 _flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        // limit velocity if needed
        if (_flatVel.magnitude > maxSpeed)
        {
            Vector3 _limitedVel = _flatVel.normalized * maxSpeed;
            //Mathf.MoveTowards(rb.velocity.x, _limitedVel.x, deceleration);
            //Mathf.MoveTowards(rb.velocity.z, _limitedVel.z, deceleration);
            rb.velocity = new Vector3(_limitedVel.x, rb.velocity.y, _limitedVel.z);
        }
    }
}
