using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    private float _timeWeaponLastFired;
    [SerializeField] float _currTime;
    [SerializeField] float _weaponFireDelay;
    

    private void Start()
    { DoNullChecks(); }
    
    private void Update()
    { _currTime = Time.time; CheckPlayerInput(); }

    private void CheckPlayerInput()
        { if (Mouse.current.leftButton.wasPressedThisFrame && WeaponIsReadyToFire()) 
            { FireWeapon(); } }

    private void FireWeapon()
    {
        Ray rayOrigin = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.5f));
        RaycastHit hitInfo;
        Transform hitTransform;

        if (Physics.Raycast(rayOrigin, out hitInfo))
        { hitTransform = hitInfo.transform;
            if (hitTransform.CompareTag("RobotAI"))
            {
                hitTransform.GetComponent<RobotAI>().TakeDamage(55f);
            }
        }
        
        _timeWeaponLastFired = Time.time;
    }

    private bool WeaponIsReadyToFire()
    {
        float timeSinceWeaponLastFired;
        timeSinceWeaponLastFired = _currTime - _timeWeaponLastFired;

        if (timeSinceWeaponLastFired > _weaponFireDelay) { return true; }
        else { return false; }
    }

    private void DoNullChecks()
    {
        if (_weaponFireDelay < 1) 
            { _weaponFireDelay = 1f; Debug.Log("Player:DoNullChecks() _weaponFireDelay is < 1! Set to 1."); }
    }
}
