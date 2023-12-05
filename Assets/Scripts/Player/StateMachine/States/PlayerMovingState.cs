using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovingState : PlayerState
{
    //Constructor
    public PlayerMovingState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void EnterLogic()
    {
        stateMachine.PlayerMovingBaseInstance.DoEnterLogic();
    }

    public override void ExitLogic()
    {
        stateMachine.PlayerMovingBaseInstance.DoExitLogic();
    }

    public override void UpdateState()
    {
        stateMachine.PlayerMovingBaseInstance.DoUpdateState();
    }

    public override void FixedUpdateState()
    {
        stateMachine.PlayerMovingBaseInstance.DoFixedUpdateState();
    }


}
