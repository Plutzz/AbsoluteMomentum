using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StopwatchController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("UI for the stopwatch")]
    private TextMeshProUGUI stopwatchUI;

    private float elapsedTime;
    private bool isRunning;

    private string stopwatchText;

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
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        stopwatchUI.text = FormatTime(elapsedTime);
    }

    public string FormatTime(float currTime)
    {
        int minutes = Mathf.FloorToInt(currTime / 60f);
        int seconds = Mathf.FloorToInt(currTime % 60f);
        int milliseconds = Mathf.FloorToInt((currTime * 1000f) % 1000f);

        stopwatchText = string.Format("{0:00}:{1:00}.{2:000}", minutes, seconds, milliseconds);

        return stopwatchText;
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
