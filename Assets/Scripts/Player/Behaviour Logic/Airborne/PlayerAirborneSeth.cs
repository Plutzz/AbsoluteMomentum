using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Airborne-Seth", menuName = "Player Logic/Airborne Logic/Seth")]
public class PlayerAirborneSeth : PlayerAirborneSOBase
{
    
    [SerializeField] private float minimumSlideVelocity = 9f;

    [SerializeField] private float upwardGravityAcceleration;   //When the player is moving upward there will be less gravity applied
    [SerializeField] private float downwardGravityAcceleration; //When the player is moving downward there will be more gravity applied
    [SerializeField] private float quickFallGravityIncrease;

    [SerializeField] private float inputVelocityControl;
    [SerializeField] private float decelerationRate;    //Drop acceleration while in air, but keep speed
    [SerializeField] private float minAirspeed; //does not affect the regular current speed

    private bool quickFalling;

    private bool sprinting;
    private float speedOnEnter; // speed while entering the state
    public override void Initialize(GameObject gameObject, PlayerStateMachine stateMachine, PlayerInputActions playerInputActions)
    {
        base.Initialize(gameObject, stateMachine, playerInputActions);
    }
    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
        stateMachine.StopAllCoroutines();
        rb.drag = 0;
        speedOnEnter = stateMachine.moveSpeed;

        stateMachine.Boosting = false;
        rb.useGravity = false;
        quickFalling = false;
    }

    public override void DoExitLogic()
    {
        rb.useGravity = true;
        base.DoExitLogic();
    }

    public override void DoFixedUpdateState()
    {
        base.DoFixedUpdateState();
        stateMachine.kinematicsVariables.currentAcceleration -= decelerationRate * Time.fixedDeltaTime;
        UpdateVerticalVelocity(Time.fixedDeltaTime);
        UpdateHorizontalVelocity(Time.fixedDeltaTime);
    }

    
    public override void DoUpdateState()
    {
        GetInput();
        
        base.DoUpdateState();
    }

    public override void ResetValues()
    {
        speedOnEnter = 0f;
        base.ResetValues();
    }

    private void GetInput()
    {
        inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
        sprinting = playerInputActions.Player.Sprint.ReadValue<float>() != 0;

        quickFalling = playerInputActions.Player.Crouch.IsInProgress();
            //Input.GetKey(KeyCode.LeftControl);
    }
    

    public override void CheckTransitions()
    {
        if (stateMachine.canWallrun && stateMachine.WallCheck())
        {
            stateMachine.ChangeState(stateMachine.WallrunState);
        }
        // Airborne => Sliding
        else if ((stateMachine.SlopeCheck() || stateMachine.GroundedCheck()) && stateMachine.crouching && playerInputActions.Player.Jump.ReadValue<float>() == 0
            && rb.velocity.magnitude > minimumSlideVelocity)
        {
            stateMachine.ChangeState(stateMachine.SlidingState);
        }
        // Airborne => Moving
        else if (stateMachine.GroundedCheck() && playerInputActions.Player.Movement.ReadValue<Vector2>() != Vector2.zero)
        {
            stateMachine.ChangeState(stateMachine.MovingState);
        }
        // Airborne => Idle
        else if (stateMachine.GroundedCheck() && playerInputActions.Player.Movement.ReadValue<Vector2>() == Vector2.zero)
        {
            stateMachine.ChangeState(stateMachine.IdleState);
        }
        // Airborne => Wallrunning
        // Might need to add a check that you are pressing an input key into the wall to "latch on" to the wall


    }

    private void UpdateVerticalVelocity(float tick)
    {
        if (rb.velocity.y > 0)
        {
            rb.velocity += Vector3.down * upwardGravityAcceleration * tick;
        }
        else
        {
            rb.velocity += Vector3.down * downwardGravityAcceleration * tick;
        }

        if (quickFalling)
        {
            rb.velocity += Vector3.down * quickFallGravityIncrease * tick;
        }
    }

    private void UpdateHorizontalVelocity(float tick) {
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        Vector3 worldInput = stateMachine.orientation.forward * inputVector.y + stateMachine.orientation.right * inputVector.x;


        /*if (worldInput == Vector3.zero)
        {       //Player passively slows to stop
            stateMachine.kinematicsVariables.currentSpeed -= decelerationRate * 2 * tick;
        }*/

        //
        horizontalVelocity += worldInput * inputVelocityControl;

        horizontalVelocity = horizontalVelocity.normalized * Mathf.Max(stateMachine.kinematicsVariables.currentSpeed, minAirspeed);

        rb.velocity = new Vector3(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.z);

    }
}
