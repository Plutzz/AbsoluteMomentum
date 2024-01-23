using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    struct JumpPadTarget
    {
        public float ContactTime;
        public Vector3 ContactVelocity;
    }

    [SerializeField] float LaunchDelay = 0.1f;
    [SerializeField] float LaunchForce = 100f;
    [SerializeField] float PlayerLaunchForceMultiplier = 5f;
    [SerializeField] ForceMode LaunchMode = ForceMode.Impulse;
    [SerializeField] float ImpactVelocityScale = 0.05f;
    [SerializeField] float MaxImpactVelocityScale = 2f;
    [SerializeField] float MaxDistortionWeight = 0.25f;

    Dictionary<Rigidbody, JumpPadTarget> Targets = new Dictionary<Rigidbody, JumpPadTarget>();

    List<Rigidbody> TargetsToClear = new List<Rigidbody>();
    private void FixedUpdate()
    {
        float thresholdTime = Time.timeSinceLevelLoad - LaunchDelay;
        foreach (var kvp in Targets)
        {
            if (kvp.Value.ContactTime >= thresholdTime)
            {
                BenLaunch(kvp.Key, kvp.Value.ContactVelocity);
                TargetsToClear.Add(kvp.Key);
            }
        }
        foreach (var target in TargetsToClear)
            Targets.Remove(target);
        TargetsToClear.Clear();
    }
    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb;
        if (collision.gameObject.TryGetComponent<Rigidbody>(out rb))
        {
            Targets[rb] = new JumpPadTarget()
            {
                ContactTime = Time.timeSinceLevelLoad,
                ContactVelocity = collision.relativeVelocity
            };
        }
    }

    private void OnCollisionExit(Collision collision)
    {

    }
    void Launch(Rigidbody targetRB, Vector3 contactVelocity)
    {
        Vector3 launchVector = transform.up;

        Vector3 distortionVector = transform.forward * Vector3.Dot(contactVelocity, transform.forward) +
        transform.right * Vector3.Dot(contactVelocity, transform.right);

        launchVector = (launchVector + MaxDistortionWeight * distortionVector.normalized).normalized;

        float contactProjection = Vector3.Dot(contactVelocity, transform.up);
        if (contactProjection < 0)
        {
            launchVector *= Mathf.Min(MaxImpactVelocityScale, 1f + Mathf.Abs(contactProjection * ImpactVelocityScale));
        }
        if (targetRB.CompareTag("Player"))
            launchVector *= PlayerLaunchForceMultiplier;

        targetRB.AddForce(transform.up * LaunchForce, LaunchMode);
    }

    void BenLaunch(Rigidbody targetRB, Vector3 contactVelocity)
    {
        //Gives normal of jump pad face
        Vector3 playerHorizontalVelocity = new Vector3(targetRB.velocity.x, 0, targetRB.velocity.z);

        //Vertical Handling
        targetRB.velocity = playerHorizontalVelocity; // Reset Vertical Velocity
        targetRB.AddForce(Vector3.up * LaunchForce, LaunchMode); // Apply Force

        //Horizontal Handling
        Vector3 launchVector = transform.up;
        launchVector = new Vector3(launchVector.x, 0, launchVector.z);
        targetRB.velocity = launchVector * playerHorizontalVelocity.magnitude;
    }
}
