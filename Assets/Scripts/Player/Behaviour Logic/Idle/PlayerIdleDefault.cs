using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Idle-Default", menuName = "Player Logic/Idle Logic/Default")]
public class PlayerIdleDefault : PlayerIdleSOBase
{

    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
        playerInputActions.Player.Jump.performed += Jump;
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
        base.DoUpdateState();
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }

    private void Jump(InputAction.CallbackContext context)
    {
        rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
        stateMachine.ChangeState(stateMachine.AirborneState);
    }
}
