using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BoostRamp : MonoBehaviour
{
    private Vector3 slopeDirection;
    private PlayerStateMachine stateMachine;
    [SerializeField] private float boostForce = 100f;
    [SerializeField] private float boostMaxSpeedIncrease = 1f;

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            stateMachine = collision.gameObject.GetComponent<PlayerStateMachine>();
            stateMachine.StopAllCoroutines();
            stateMachine.Boosting = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //stateMachine.moveSpeed = initialMoveSpeed;
            stateMachine = null;
            
        }
    }

    private void FixedUpdate()
    {
        if (stateMachine != null)
        {
            Debug.DrawRay(stateMachine.gameObject.transform.position, slopeDirection * 100f, Color.blue);
            slopeDirection = Vector3.Cross(stateMachine.slopeHit.normal, Vector3.down);
            Vector3 cross = Vector3.Cross(stateMachine.slopeHit.normal, slopeDirection);
            slopeDirection = cross;
            stateMachine.moveSpeed += boostMaxSpeedIncrease * 0.1f;
            stateMachine.rb.AddForce(slopeDirection * boostForce * 100f, ForceMode.Force);
        }
    }
}
