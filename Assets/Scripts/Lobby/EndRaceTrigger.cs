using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndRaceTrigger : MonoBehaviour
{
    private StopwatchController swTime;

    public Queue<ArrayList> raceResults = new Queue<ArrayList>();
    public int playerCount;

    private void Start()
    {
        swTime = GetComponent<StopwatchController>();
    }

    private void OnTriggerEnter( Collider col ) 
    {
        float time = swTime.GetStopwatchTime();

        if (col.CompareTag("Player"))
        {
            Debug.Log("Touched");
            ArrayList playerTime = new ArrayList();

            playerTime.Add(col.gameObject);
            playerTime.Add(swTime.FormatTime(time));
        }
    }

    private void FinishRace()
    {
        if (raceResults.Count >= playerCount)
        {
            Debug.Log("Finished Race");
        }
    }

}
