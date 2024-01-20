using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Idle-Seth", menuName = "Player Logic/Idle Logic/Seth")]
public class PlayerIdleSeth : PlayerIdleSOBase
{
    [SerializeField] private float groundDrag;
    private Vector2 inputVector;

    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
        //rb.drag = groundDrag;
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
        if (playerInputActions.Player.Jump.ReadValue<float>() == 1f)
        {
            Jump();
        }
        GetInput();
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

    private void GetInput()
    {
        inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();

    }
}
