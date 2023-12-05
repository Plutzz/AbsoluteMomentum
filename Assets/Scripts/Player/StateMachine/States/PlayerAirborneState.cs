using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAirborneState: PlayerState
{
    public PlayerAirborneState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void EnterLogic()
    {
        stateMachine.PlayerAirborneBaseInstance.DoEnterLogic();
    }

    public override void ExitLogic()
    {
        stateMachine.PlayerAirborneBaseInstance.DoExitLogic();
    }
    public override void UpdateState()
    {
        stateMachine.PlayerAirborneBaseInstance.DoUpdateState();
    }

    public override void FixedUpdateState()
    {
        stateMachine.PlayerAirborneBaseInstance.DoFixedUpdateState();
    }
}
