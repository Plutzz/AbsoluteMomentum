using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndRaceTrigger : MonoBehaviour
{
    public StopwatchController swTime;

    public Queue<ArrayList> raceResults = new Queue<ArrayList>();
    public int playerCount;

    private void Start()
    {
        // swTime = GetComponent<StopwatchController>();
    }

    private void OnTriggerEnter( Collider col ) 
    {
        float time = swTime.GetStopwatchTime();

        Debug.Log("Touched");
        ArrayList playerTime = new ArrayList();

        // Saves player object
        playerTime.Add(col.gameObject);

        // Saves time of player object
        playerTime.Add(swTime.FormatTime(time));

        raceResults.Enqueue(playerTime);

        FinishRace();

        Debug.Log(raceResults);
        Debug.Log(raceResults.Peek()[0]);
        Debug.Log(raceResults.Peek()[1]);

        // if (col.CompareTag("Player"))
        // {
            
        // }
    }

    private void FinishRace()
    {
        if (raceResults.Count >= playerCount)
        {
            Debug.Log("Finished Race");
            swTime.StopStopwatch();
        }
    }

}
