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
        if (playerInputActions.Player.Jump.ReadValue<float>() == 1f)
        {
            Jump();
        }
        base.DoUpdateState();
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }

    private void Jump()
    {
        stateMachine.ChangeState(stateMachine.MovingState);
    }
}
