using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] int _robotsInTriggerZone = 0;
    private bool _openDoor;
    private bool _closeDoor;
    private bool _fullyOpen;
    [SerializeField] float _doorSpeed;

    private void start()
    {
        _openDoor = false;
        _closeDoor = false;
        _fullyOpen = false;
        if (_doorSpeed == 0f) { _doorSpeed = 1f; }
    }

    private void Update()
    {
        if (_openDoor) { OpenDoor(); }
        else if (_closeDoor && _fullyOpen) { CloseDoor(); }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("RobotAI")) { _robotsInTriggerZone += 1; }

        _openDoor = true;
        _closeDoor = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("RobotAI")) { _robotsInTriggerZone -= 1; }
        if (_robotsInTriggerZone == 0) { _closeDoor = true; }
    }

    private void OpenDoor()
    {
        if (transform.localPosition.z < 2) { transform.Translate(0, 0, 1 * _doorSpeed * Time.deltaTime, Space.World); } 
        else if (transform.position.z > 1.99) { _fullyOpen = true; _openDoor = false; }
    }

    private void CloseDoor()
    {
        if (transform.localPosition.z > 0f) { transform.Translate(0,0,-1 * _doorSpeed * Time.deltaTime, Space.World); }
        else { _fullyOpen = false; }
    }
}
