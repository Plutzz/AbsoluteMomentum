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
}