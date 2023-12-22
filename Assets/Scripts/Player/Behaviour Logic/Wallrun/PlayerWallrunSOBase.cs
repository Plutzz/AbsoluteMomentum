using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallrunSOBase : PlayerStateSOBase
{
    protected PlayerStateMachine stateMachine;
    protected Rigidbody rb;
    protected GameObject gameObject;
    protected PlayerInputActions playerInputActions;
    protected Vector2 inputVector;

    public virtual void Initialize(GameObject gameObject, PlayerStateMachine stateMachine, PlayerInputActions playerInputActions)
    {
        this.gameObject = gameObject;
        this.stateMachine = stateMachine;
        rb = stateMachine.rb;
        this.playerInputActions = playerInputActions;
    }

    // public override void CheckTransitions()
    // {
    //     //if wall detected but no ground, no transitions
    //     if (stateMachine.WallRunning()) return;

    //     // WallRun => Moving
    //     if (playerInputActions.Player.Movement.ReadValue<Vector2>() != Vector2.zero)
    //     {
    //         stateMachine.ChangeState(stateMachine.MovingState);
    //     }
    //     else if (playerInputActions.Player.Movement.ReadValue<Vector2>() == Vector2.zero)
    //     {
    //         stateMachine.ChangeState(stateMachine.IdleState);
    //     }
    // }
}
