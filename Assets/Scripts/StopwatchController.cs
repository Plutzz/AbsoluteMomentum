using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopwatchController : MonoBehaviour
{
    private float elapsedTime;
    private bool isRunning;

    void Start()
    {
        elapsedTime = 0f;
        isRunning = false;
        FormatTime(elapsedTime);
    }

    void Update()
    {
        if (isRunning)
        {
            elapsedTime += Time.deltaTime;
            FormatTime(elapsedTime);
        }
    }

    public void FormatTime(float currTime)
    {
        int minutes = Mathf.FloorToInt(currTime / 60f);
        int seconds = Mathf.FloorToInt(currTime % 60f);
        int milliseconds = Mathf.FloorToInt((currTime * 1000f) % 1000f);

        // timerText.text = string.Format("{0:00}:{1:00}.{2:000}", minutes, seconds, milliseconds);
    }

    public void StartStopwatch()
    {
        isRunning = true;
    }

    public void StopStopwatch()
    {
        isRunning = false;
    }

    public void ResetStopwatch()
    {
        elapsedTime = 0f;
        FormatTime(elapsedTime);
    }

    public float GetStopwatchTime() 
    {
        return elapsedTime;
    }
}
