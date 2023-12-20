using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallrunState : PlayerState
{
    public PlayerWallrunState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void CheckTransitions()
    {
        stateMachine.PlayerWallrunBaseInstance.CheckTransitions();
    }

    public override void EnterLogic()
    {
        stateMachine.PlayerWallrunBaseInstance.DoEnterLogic();
    }

    public override void ExitLogic()
    {
        stateMachine.PlayerWallrunBaseInstance.DoExitLogic();
    }

    public override void FixedUpdateState()
    {
        stateMachine.PlayerWallrunBaseInstance.DoFixedUpdateState();
    }

    public override void UpdateState()
    {
        stateMachine.PlayerWallrunBaseInstance.DoUpdateState();
    }
}
