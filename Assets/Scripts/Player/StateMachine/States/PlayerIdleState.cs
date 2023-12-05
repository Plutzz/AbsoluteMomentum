using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void EnterLogic()
    {
        stateMachine.PlayerIdleBaseInstance.DoEnterLogic();
    }

    public override void ExitLogic()
    {
        stateMachine.PlayerIdleBaseInstance.DoExitLogic();
    }

    public override void UpdateState()
    {
        stateMachine.PlayerIdleBaseInstance.DoUpdateState();
    }
    public override void FixedUpdateState()
    {
        stateMachine.PlayerIdleBaseInstance.DoFixedUpdateState();
    }
}
