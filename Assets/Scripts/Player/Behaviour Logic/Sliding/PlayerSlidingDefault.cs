using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[CreateAssetMenu(fileName = "Sliding-Default", menuName = "Player Logic/Sliding Logic/Default")]
public class PlayerSlidingDefault : PlayerSlidingSOBase
{
    [Header("Sliding")]
    [SerializeField] private float maxSlideTime;
    [SerializeField] private float slideSpeed;
    [SerializeField] private float sprintSpeed; //SHOULD BE SAME SPRINT SPEED AS FROM MOVEMENT SCRIPTS
    [SerializeField] private float slideAcceleration;
    private float slideTimer;
    private Vector3 slideDirection;
    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
        StartSlide();
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
    }

    public override void DoUpdateState()
    {
        GetInput();
        MovementSpeedHandler();
        base.DoUpdateState();
    }

    public override void DoFixedUpdateState()
    {

        SlidingMovement();


        base.DoFixedUpdateState();
    }

    private void GetInput()
    {
        inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();

        if(!stateMachine.crouching)
        {
            StopSlide();
        }
        if (playerInputActions.Player.Jump.ReadValue<float>() == 1f)
        {
            Jump();
        }
    }

    private void StartSlide()
    {
        slideTimer = maxSlideTime;
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }

    private void MovementSpeedHandler()
    {
        // if moving downwards on a slope, 
        if (stateMachine.SlopeCheck() && rb.velocity.y < 0.1f)
        {
            stateMachine.desiredMoveSpeed = slideSpeed;
        }
        else
            stateMachine.desiredMoveSpeed = sprintSpeed;
    }
    
    private void SlidingMovement()
    {
        slideDirection = orientation.forward * inputVector.y + orientation.right * inputVector.x;

        //Sliding Normal
        if(!stateMachine.SlopeCheck() || rb.velocity.y > -0.1f)
        {
            rb.AddForce(slideDirection.normalized * slideAcceleration, ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }
        
        //Sliding down a slope
        else
        {
            rb.AddForce(stateMachine.GetSlopeMoveDirection(slideDirection) * slideAcceleration, ForceMode.Force);

        }


        if (slideTimer <= 0)
        {
            StopSlide();
        }

    }

    private void StopSlide()
    {
        stateMachine.ChangeState(stateMachine.MovingState);
    }
    

    private void Jump()
    {
        stateMachine.ChangeState(stateMachine.MovingState);
    }


}
