using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSlidingSOBase : PlayerStateSOBase
{
    protected PlayerStateMachine stateMachine;
    protected Rigidbody rb;
    protected Transform orientation;
    protected GameObject gameObject;
    protected PlayerInputActions playerInputActions;
    protected Vector2 inputVector;
    public bool reachedMaxSpeed;

    public virtual void Initialize(GameObject gameObject, PlayerStateMachine stateMachine, PlayerInputActions playerInputActions, Transform orientation)
    {
        this.gameObject = gameObject;
        this.stateMachine = stateMachine;
        rb = stateMachine.rb;
        this.playerInputActions = playerInputActions;
        this.orientation = orientation;
    }
}
