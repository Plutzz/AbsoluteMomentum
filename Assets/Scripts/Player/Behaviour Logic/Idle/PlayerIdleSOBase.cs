using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleSOBase : PlayerStateSOBase
{
    protected PlayerStateMachine stateMachine;
    protected Rigidbody rb;
    protected GameObject gameObject;
    protected PlayerInputActions playerInputActions;

    public virtual void Initialize(GameObject gameObject, PlayerStateMachine stateMachine, PlayerInputActions playerInputActions)
    {
        this.gameObject = gameObject;
        this.stateMachine = stateMachine;
        rb = stateMachine.rb;
        this.playerInputActions = playerInputActions;
    }
    public override void CheckTransitions()
    {
        // Idle => Moving
        if (playerInputActions.Player.Movement.ReadValue<Vector2>() != Vector2.zero)
        {
            stateMachine.ChangeState(stateMachine.MovingState);
        }
        // Idle => Airborne *** CHANGED TO ELSE IF POSSIBLE BUG
        else if (rb.velocity.y != 0)
        {
            stateMachine.ChangeState(stateMachine.AirborneState);
        }
    }


}
