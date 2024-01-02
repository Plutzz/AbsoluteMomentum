using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public struct PlayerStats
    {
        public float raceTime;
    };

    // Keep array of all active players in the race
    private GameObject[] playerList;

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
        // Get all active players
        playerList = GameObject.FindGameObjectsWithTag("Player");

        // Freeze player movement and teleport them to starting positions
        // Loop through players Spawn player each row and column to fit in system
        RepositionPlayers();
    }

    void Start()
    {

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

        // Takes list of players and repositions them to a specific point on spawn
        for (int i = 0; i < playerList.Length; i++)
        {
            GameObject currPlayer = playerList[i];

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
