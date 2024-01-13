using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RaceManager : NetworkBehaviour
{
    public struct PlayerStats
    {
        public float raceTime;
    };

    // Keep array of all active players in the race
    private NetworkObject[] playerList;

    [Header("Player Spawner System")]
    // Number of players per row
    [SerializeField]
    [Tooltip("Amount of players per row")]
    private int playersPerRow = 2;

    // Gap between size
    [SerializeField]
    [Tooltip("Gap between player")]
    private float gap = 1f;

    // Starting Vector
    [SerializeField]
    [Tooltip("Starting position of first player")]
    private Vector3 startingPlayer;

    // Which XYZ positions are you using
    [SerializeField]
    [Tooltip("If True, lines up players by Z axis, else lines up by X axis")]
    private bool lineUpOnZ;

    private void Awake()
    {
        // if (IsServer)
        // {
        //     Debug.Log("IS in server");

        //     playerList = FindObjectsOfType<NetworkObject>();

        //     RepositionPlayers();
        // }


        // // Get all active players
        // playerList = GameObject.FindGameObjectsWithTag("Player");

        // // Freeze player movement and teleport them to starting positions
        // // Loop through players Spawn player each row and column to fit in system
        // RepositionPlayers();
    }

    void Start()
    {
        // Function works if in start instead of awake
        if (IsServer)
        {
            Debug.Log("IS in server");

            playerList = FindObjectsOfType<NetworkObject>();

            // Moves camera with player but not the player itself
            RepositionPlayers();
        }
    }
    void Update()
    {
        // Check if all players are loaded into scene
        StartCountdown();

        // Make a timer
    }

    private void RepositionPlayers() 
    {
        int row = 0;
        int col = 0;

        // foreach (NetworkObject player in playerList)
        // {
        //     Vector3 currPos = startingPlayer;

        //     if (lineUpOnZ)
        //     {
        //         Vector3 currSpace = new Vector3(row * gap, 0, col * gap);
        //         currPos += currSpace;
        //     }
        //     else
        //     {
        //         Vector3 currSpace = new Vector3(col * gap, 0, row * gap);
        //         currPos += currSpace;
        //     }

        //     // Set the initial position on the server
        //     player.transform.position = currPos;
        //     // player.GetComponentInChildren<GameObject>().transform.position = currPos;

        //     // Update row and col for the next player
        //     if (lineUpOnZ)
        //     {
        //         col++;
        //         if (col >= playersPerRow)
        //         {
        //             col = 0;
        //             row++;
        //         }
        //     }
        //     else
        //     {
        //         row++;
        //         if (row >= playersPerRow)
        //         {
        //             row = 0;
        //             col++;
        //         }
        //     }
        // }

        // Takes list of players and repositions them to a specific point on spawn
        for (int i = 0; i < playerList.Length; i++)
        {
            GameObject currPlayer = playerList[i].transform.Find("Player").gameObject;

            Vector3 currPos = startingPlayer;

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

            currPlayer.transform.position = currPos;
        }
    }

    // Possibly a couroutine?
    private void StartCountdown()
    {
        // Count down from 3
        // Unfreeze players
        // Start timer
    }

    // Call when player reaches end of map (collider/trigger)
    private void PlayerFinishRace()
    {
        // Get time of player when they finish
    }

    private void EndRace()
    {
        // Determine player with the lowest time as the winner
    }

}
