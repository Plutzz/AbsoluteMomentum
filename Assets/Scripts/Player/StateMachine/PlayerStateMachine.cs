using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    #region States Variables

    private PlayerState currentState; // State that player is currently in
    private PlayerState initialState; // State that player starts as

    // References to all player states
    public PlayerIdleState IdleState;
    public PlayerMovingState MovingState;
    public PlayerAirborneState AirborneState;

    #endregion

    #region ScriptableObject Variables

    [SerializeField] private PlayerIdleSOBase playerIdleBase;
    [SerializeField] private PlayerMovingSOBase playerMovingBase;
    [SerializeField] private PlayerAirborneSOBase playerAirborneBase;

    public PlayerIdleSOBase PlayerIdleBaseInstance { get; private set; }
    public PlayerMovingSOBase PlayerMovingBaseInstance { get; private set; }
    public PlayerAirborneSOBase PlayerAirborneBaseInstance { get; private set; }

    #endregion

    #region Player Variables
    public PlayerInputActions playerInputActions { get; private set; }
    public Rigidbody rb { get; private set; }
    public Transform GroundCheck;
    public Vector3 GroundCheckSize;
    [SerializeField] LayerMask groundLayer;

    #endregion

    #region Debug Variables
    public TextMeshProUGUI CurrentStateText;
    public TextMeshProUGUI GroundedText;
    public TextMeshProUGUI VelocityText;
    public TextMeshProUGUI SpeedText;
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        PlayerIdleBaseInstance = Instantiate(playerIdleBase);
        PlayerMovingBaseInstance = Instantiate(playerMovingBase);
        PlayerAirborneBaseInstance = Instantiate(playerAirborneBase);

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();

        IdleState = new PlayerIdleState(this);
        MovingState = new PlayerMovingState(this);
        AirborneState = new PlayerAirborneState(this);

        PlayerIdleBaseInstance.Initialize(gameObject, this, playerInputActions);
        PlayerMovingBaseInstance.Initialize(gameObject, this, playerInputActions);
        PlayerAirborneBaseInstance.Initialize(gameObject, this, playerInputActions);

        initialState = IdleState;
    }
    private void Start()
    {
        currentState = initialState;
        currentState.EnterLogic();
    }

    private void Update()
    {
        //Debug.Log(rb.velocity);
        currentState.UpdateState();
        CurrentStateText.text = "Current State: " + currentState.ToString();
        GroundedText.text = "Grounded: " + GroundedCheck();
        VelocityText.text = "Velocity: " + rb.velocity.x + "," + rb.velocity.z;
        SpeedText.text = "Speed: " + rb.velocity.magnitude;

    }

    private void FixedUpdate()
    {
        currentState.FixedUpdateState();
    }

    public void ChangeState(PlayerState newState)
    {
        //Debug.Log("Changing to: " + newState);
        currentState.ExitLogic();
        currentState = newState;
        currentState.EnterLogic();
    }

    //Consider adding core functionalities here
    // Ex: GroundedCheck
    public bool GroundedCheck()
    {
        return Physics.OverlapBox(GroundCheck.position, GroundCheckSize * 0.5f, Quaternion.identity, groundLayer).Length > 0;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(GroundCheck.transform.position, GroundCheckSize);
    }
}
