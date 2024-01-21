using Cinemachine.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "Moving-Seth", menuName = "Player Logic/Moving Logic/Seth")]
public class PlayerMovingSeth : PlayerMovingSOBase
{
    [Header("Movement Variables")]
    [SerializeField] private float minSpeed;
    [SerializeField] private float maxWalkSpeed;
    [SerializeField] private float maxSprintSpeed;
    //private float currentSpeed;

    [SerializeField] private float defaultAcceleration;
    //private float currentAcceleration;
    [SerializeField] private float maxAcceleration;
    [SerializeField] private float accelerationGrowthRate;
    [SerializeField] private float decelerationRate;

    [SerializeField] private float turningDecay;

    private bool sprinting = false;
    private Vector3 moveDirection;

    [Header("Crouching Variables")]
    [SerializeField] private float crouchSpeed = 3.5f;



    public override void Initialize(GameObject gameObject, PlayerStateMachine stateMachine, PlayerInputActions playerInputActions)
    {
        base.Initialize(gameObject, stateMachine, playerInputActions);
    }
    public override void DoEnterLogic()
    {
        moveDirection = Vector3.zero;
        rb.drag = 0;
        //stateMachine.kinematicsVariables.currentAcceleration = defaultAcceleration;
        //stateMachine.kinematicsVariables.currentSpeed = minSpeed;
        playerInputActions.Player.Crouch.performed += TryStartSlide;
        stateMachine.StopAllCoroutines();
        base.DoEnterLogic();
    }

    public override void DoExitLogic()
    {
        playerInputActions.Player.Crouch.performed -= TryStartSlide;
        base.DoExitLogic();
    }

    public override void DoUpdateState()
    {
        GetInput();
        

        //Disables gravity while on slopes
        rb.useGravity = !stateMachine.SlopeCheck();

        base.DoUpdateState();
    }

    public override void DoFixedUpdateState()
    {
        UpdateAcceleration(Time.fixedDeltaTime);
        UpdateVelocity(Time.fixedDeltaTime);

        DebugAcceleration();
        DebugVelocity();

        base.DoFixedUpdateState();
    }

    public override void CheckTransitions()
    {
        // Moving => Airborne
        if (!stateMachine.GroundedCheck() && !stateMachine.SlopeCheck())
        {
            if (stateMachine.jumpHandler.canJump)
            {
                stateMachine.jumpHandler.StartCoyoteTime();
            }

            stateMachine.ChangeState(stateMachine.AirborneState);
        }
        // Moving => Idle
        else if (playerInputActions.Player.Movement.ReadValue<Vector2>() == Vector2.zero && rb.velocity.magnitude < minSpeed)
        {
            stateMachine.ChangeState(stateMachine.IdleState);
        }

    }

    public override void ResetValues()
    {
        base.ResetValues();
    }

    private void GetInput()
    {
        inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
        sprinting = playerInputActions.Player.Sprint.ReadValue<float>() == 1f;
    }

    #region Helper Methods
    private void UpdateAcceleration(float tick)
    {
        if (sprinting == false) {
            stateMachine.kinematicsVariables.currentAcceleration = defaultAcceleration;
            return;
        }

        stateMachine.kinematicsVariables.currentAcceleration += (accelerationGrowthRate * tick);

        if (stateMachine.kinematicsVariables.currentAcceleration > maxAcceleration) {
            stateMachine.kinematicsVariables.currentAcceleration = maxAcceleration;
        }
    }

    private void UpdateVelocity(float tick) {
        
        

        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        Vector3 worldInput = stateMachine.orientation.forward * inputVector.y + stateMachine.orientation.right * inputVector.x;


        if (worldInput == Vector3.zero) {       //Player passively slows to stop
            stateMachine.kinematicsVariables.currentSpeed -= decelerationRate * 2 * tick;
        }
        else if (stateMachine.kinematicsVariables.currentSpeed < minSpeed) {     //Player moving slowly has little happen
            //Pass
            stateMachine.kinematicsVariables.currentSpeed += stateMachine.kinematicsVariables.currentAcceleration * tick;
        }

        
        else {

            //Accelerate/deccelerate players to the current maxspeed
            float maxSpeed = maxWalkSpeed;
            if (sprinting) {
                maxSpeed = maxSprintSpeed;
            }
            
            if (stateMachine.kinematicsVariables.currentSpeed > maxSpeed)
            {
                stateMachine.kinematicsVariables.currentSpeed = Mathf.Max(maxSpeed, stateMachine.kinematicsVariables.currentSpeed - decelerationRate * tick);
            }
            else {
                stateMachine.kinematicsVariables.currentSpeed += stateMachine.kinematicsVariables.currentAcceleration * tick;
            }

            
            //Help make tight turns while still building acceleration 
            float angleOfChange = Vector3.Angle(horizontalVelocity, worldInput);
            angleOfChange /= 180;
            stateMachine.kinematicsVariables.currentSpeed *= 1 - (angleOfChange * turningDecay * tick);

        }

        horizontalVelocity += worldInput;

        horizontalVelocity = horizontalVelocity.normalized * stateMachine.kinematicsVariables.currentSpeed;

        rb.velocity = new Vector3(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.z);
    }

    
    private void TryStartSlide(InputAction.CallbackContext context)
    {
        if (stateMachine.canSlide && (sprinting || (stateMachine.SlopeCheck() && rb.velocity.y < -0.1f)))
        {
            stateMachine.ChangeState(stateMachine.SlidingState);
        }
    }




    #endregion

    #region Debug Functions
    private void DebugVelocity() {
        Debug.Log("Current Speed" + stateMachine.kinematicsVariables.currentSpeed + "\nIs Sprinting: " + sprinting);
    }

    private void DebugAcceleration()
    {
        Debug.Log("Current Acceleration" + stateMachine.kinematicsVariables.currentAcceleration + "\nIs Sprinting: " + sprinting);
    }

    #endregion

}
