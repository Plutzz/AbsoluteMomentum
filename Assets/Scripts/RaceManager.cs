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

    private void Awake()
    {
        // Freeze player movement and teleport them to starting positions
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
