using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Idle-Default", menuName = "Player Logic/Idle Logic/Default")]
public class PlayerIdleDefault : PlayerIdleSOBase
{
    [SerializeField] private float groundDrag;
    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
        rb.drag = groundDrag;
        stateMachine.StopAllCoroutines();
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
        base.DoUpdateState();
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }

    public override void CheckTransitions()
    {
        // Idle => Airborne
        if (!stateMachine.SlopeCheck() && !stateMachine.GroundedCheck() && !stateMachine.crouching)
        {
            stateMachine.ChangeState(stateMachine.AirborneState);
        }
        // Idle => Moving
        else if (playerInputActions.Player.Movement.ReadValue<Vector2>() != Vector2.zero)
        {
            stateMachine.ChangeState(stateMachine.MovingState);
        }
    }
}
