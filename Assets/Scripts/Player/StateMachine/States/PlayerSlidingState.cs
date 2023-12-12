using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSlidingState : PlayerState
{
    public PlayerSlidingState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void EnterLogic()
    {
        stateMachine.PlayerSlidingBaseInstance.DoEnterLogic();
    }

    public override void ExitLogic()
    {
        stateMachine.PlayerSlidingBaseInstance.DoExitLogic();
    }
    public override void UpdateState()
    {
        stateMachine.PlayerSlidingBaseInstance.DoUpdateState();
    }

    public override void FixedUpdateState()
    {
        stateMachine.PlayerSlidingBaseInstance.DoFixedUpdateState();
    }
}
