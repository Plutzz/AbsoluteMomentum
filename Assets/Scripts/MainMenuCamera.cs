using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MainMenuCamera : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 100f;
    public void StartOrbit()
    {
    }

    private void Update()
    {
        transform.eulerAngles += new Vector3(0, rotationSpeed * Time.deltaTime, 0);
    }
}
