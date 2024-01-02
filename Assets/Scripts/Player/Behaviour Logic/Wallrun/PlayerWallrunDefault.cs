using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Wallrun-Default", menuName = "Player Logic/Wallrun Logic/Default")]
public class PlayerWallrunDefault : PlayerWallrunSOBase
{
    private bool jumping;
    private float jumpCooldown;
    private bool readyToJump;
    [SerializeField] private float maxWallrunTime; 
    private float dropTimer;
    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
        dropTimer = maxWallrunTime;
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
        GetInput();

        dropTimer -= Time.deltaTime;

        base.DoUpdateState();
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }

    public override void CheckTransitions()
    {
        if (stateMachine.GroundedCheck())
        {
            if (inputVector == Vector2.zero)
            {
                stateMachine.ChangeState(stateMachine.IdleState);
            }
            else
            {
                stateMachine.ChangeState(stateMachine.MovingState);
            }
        }

        else if ((dropTimer < 0) || !stateMachine.WallCheck())
        {
            stateMachine.ChangeState(stateMachine.AirborneState);
        }
    }


    private void GetInput()
    {
        inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
        //sprinting = playerInputActions.Player.Sprint.ReadValue<float>() == 1f;
        jumping = playerInputActions.Player.Jump.ReadValue<float>() == 1f;

        if (stateMachine.timeOfLastJump + jumpCooldown < Time.time)
            readyToJump = true;
    }
}
