using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class RaceManager : NetworkBehaviour
{
    public struct PlayerStats
    {
        public string player;
        public float raceTime;
        public string raceTimeFormatted;
    };

    // Queue for race finishes
    public Queue<PlayerStats> raceResults = new Queue<PlayerStats>();

    [SerializeField]
    [Tooltip("Stopwatch for the race")]
    private StopwatchController swControl;

    // Keep array of all active players in the race
    private NetworkObject[] networkObjList;
    private List<GameObject> playerList;

    [Header("Player Spawner System")]
    // Number of players per row
    [SerializeField]
    [Tooltip("Amount of players per row")]
    private int playersPerRow = 2;

    // Gap between size
    [SerializeField]
    [Tooltip("Gap between player")]
    private float gap = 1f;

    // Starting Location for player
    // can change transform to vector3 if you want a very specific point
    // Otherwise just drag empty game object
    [SerializeField]
    [Tooltip("Gameobject of start point")]
    private Transform startingPoint;

    // Which XYZ positions are you using
    [SerializeField]
    [Tooltip("If True, lines up players by Z axis, else lines up by X axis")]
    private bool lineUpOnZ;

    [SerializeField]
    [Tooltip("Countdown till race starts")]
    private float countdown = 3f;

    private void Awake()
    {
        playerList = new List<GameObject>();
    }

    void Start()
    {
        // Function works if in start instead of awake
        if (IsServer)
        {
            Debug.Log("IS in server");

            // Network objects are not players, players are a separate list
            networkObjList = FindObjectsOfType<NetworkObject>();

            RepositionPlayers();

            StartCoroutine(StartCountdown());
        }
    }

    private void RepositionPlayers() 
    {
        int row = 0;
        int col = 0;

        // Takes list of players and repositions them to a specific point on spawn
        for (int i = 0; i < networkObjList.Length; i++)
        {
            // Finds the network objects that are players
            Transform playerTransform = networkObjList[i].transform.Find("Player");

            if (playerTransform != null)
            {
                GameObject player = playerTransform.gameObject;

                // Vector3 currPos = startingPlayer;
                Vector3 currPos = startingPoint.position;

                // Lines up players on X or Z axis
                if (lineUpOnZ)
                {
                    Vector3 currSpace = new Vector3(row * gap, 0, col * gap);
                    currPos += currSpace;
                }
                else 
                {
                    Vector3 currSpace = new Vector3(col * gap, 0, row * gap);
                    currPos += currSpace;
                }

                player.transform.position = currPos;

                // Disables player movement
                player.GetComponent<PlayerStateMachine>().enabled = false;

                // Adds player to list
                playerList.Add(player);
            }
            else
            {
                // Handle the case when the Player object doesn't exist
                Debug.LogWarning("Player object not found for playerList[" + i + "]");
            }
            
        }
    }

    // Possibly a couroutine?
    IEnumerator StartCountdown()
    {
        // Count down from 3
        // Unfreeze players
        // Start timer

        yield return new WaitForSeconds(countdown);

        Debug.Log("Timer has finished");

        for (int i = 0; i < playerList.Count; i++)
        {
            GameObject currPlayer = playerList[i];
            currPlayer.GetComponent<PlayerStateMachine>().enabled = true;
        }

        swControl.StartStopwatch();
    }

    private void EndRace()
    {
        Debug.Log("Finished Race");
        swControl.StopStopwatch();

        // Just dequeue the players to rank each one
        int rank = 1;
        while (raceResults.Count > 0)
        {
            PlayerStats playerstats = raceResults.Dequeue();
            Debug.Log(string.Format("Rank {0}: {1} {2}", rank, playerstats.player, playerstats.raceTimeFormatted));
        }
    }

    // On colliding with finish line
    private void OnTriggerEnter( Collider col ) 
    {
        Debug.Log("Touched");
        PlayerStats playerStats = new PlayerStats();

        // Saves player object name
        playerStats.player = col.gameObject.name;

        playerStats.raceTime = swControl.GetStopwatchTime();

        // Saves time of player object
        playerStats.raceTimeFormatted = swControl.FormatTime(playerStats.raceTime);

        raceResults.Enqueue(playerStats);

        

        // Prints all the results in the console.
        Debug.Log(raceResults);
        Debug.Log(raceResults.Peek().player);
        Debug.Log(raceResults.Peek().raceTime);
        Debug.Log(raceResults.Peek().raceTimeFormatted);

        if (raceResults.Count >= playerList.Count)
        {
            EndRace();
        }

        // if (col.CompareTag("Player"))
        // {
            
        // }
    }

    // Call when player reaches end of map (collider/trigger)
    // private void PlayerFinishRace()
    // {
    //     // Get time of player when they finish
    // }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green; // Set Gizmos color

        if (startingPoint != null)
        {
            Gizmos.DrawWireCube(startingPoint.position, new Vector3(playersPerRow * gap, 1f, playersPerRow * gap));
        }

        Gizmos.color = Color.red; // End point
        
        BoxCollider boxCollider = GetComponent<BoxCollider>(); 
        if (boxCollider != null)
        {
            Gizmos.DrawCube(transform.position, boxCollider.size);
            Gizmos.DrawWireCube(transform.position, boxCollider.size);
        }
    }

    // If player tries to exit the field, they reset to center of finish line
    // Bug where camera disconnects from player position
    // private void OnTriggerExit( Collider col )
    // {
    //     Debug.Log("Exit Attempt");
    //     BoxCollider boxCollider = GetComponent<BoxCollider>(); 

    //     col.gameObject.transform.position = transform.position;
    // }

}
