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
        {
            Collider hitCollider = hitInfo.collider;
            int hitLayer = hitCollider.gameObject.layer;

            Transform hitParent = hitCollider.transform.parent;

            if (hitCollider.CompareTag("HitCollider"))
            {
                hitParent.GetComponent<RobotAI>().TakeDamage(55f);
                // Debug.Log("Hit collider registered.");
            } else if (hitCollider.CompareTag("NearMissCollider"))
            {
                hitParent.GetComponent<RobotAI>().DetectNearMiss();
                // Debug.Log("NearMiss collider registered.");
            }
            
            if (hitLayer == 10)
            {
                // Debug.Log("hitLayer = " + hitLayer.ToString());
                // Debug.Log("Cover object hit!");
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
