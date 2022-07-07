using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    private float _timeWeaponLastFired;
    [SerializeField] float _currTime;
    [SerializeField] float _weaponFireDelay;
    [SerializeField] private int _currAmmoCount;
    [SerializeField] private int _currScore;
    [Space]
    [SerializeField] private UnityEvent<int> ammoCountChanged;
    [SerializeField] private UnityEvent<int> scoreChanged;
    [SerializeField] private UnityEvent<int> enemyAmountChange;
    [SerializeField] private UnityEvent playerFiredWeapon;
    [SerializeField] private UnityEvent coverImpactHit;
    [SerializeField] private UnityEvent winCondition;
    [Space] [SerializeField] private int _totalEnemiesKilled;

    private void Start()
    { DoNullChecks(); UpdateAmmoUI();}
    
    private void Update()
    { _currTime = Time.time; CheckPlayerInput(); }

    private void CheckPlayerInput() {
        if (Mouse.current.leftButton.wasPressedThisFrame && WeaponIsReadyToFire())
            { FireWeapon(); }
    }

    private void FireWeapon()
    {
        Ray rayOrigin = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.5f));
        RaycastHit hitInfo;

        playerFiredWeapon.Invoke();
        _currAmmoCount -= 1;
        UpdateAmmoUI();
        
        if (Physics.Raycast(rayOrigin, out hitInfo))
        {
            Collider hitCollider = hitInfo.collider;
            int hitLayer = hitCollider.gameObject.layer;

            Transform hitParent = hitCollider.transform.parent;
            Transform hitTransform = hitCollider.transform;

            switch (hitCollider.tag)
            {
                case "HitCollider":
                    if (hitParent.GetComponent<RobotAI>().TakeDamage(55f)) { UpdateEnemyUI(); _totalEnemiesKilled += 1; }
                    _currScore += 5; UpdateScoreUI();
                    if (_totalEnemiesKilled >= SpawnManager.Instance.GetTotalNumOfEnemiesAllWaves()) {
                        winCondition.Invoke();
                    }
                    break;
                case "NearMissCollider":
                    hitParent.GetComponent<RobotAI>().DetectNearMiss();
                    break;
                case "Explodable":
                    hitTransform.GetComponent<PropaneTank>().TakeDamage(55f);
                    break;
            }
            
            if (hitLayer == 10)
            {
                coverImpactHit.Invoke();
            }
        }
        _timeWeaponLastFired = Time.time;
    }

    private bool WeaponIsReadyToFire()
    {
        float timeSinceWeaponLastFired;
        timeSinceWeaponLastFired = _currTime - _timeWeaponLastFired;

        if (timeSinceWeaponLastFired > _weaponFireDelay && _currAmmoCount > 0) { return true; }
        else { return false; }
    }

    private void UpdateAmmoUI()
    {
        ammoCountChanged.Invoke(_currAmmoCount);
    }

    private void UpdateScoreUI()
    {
        scoreChanged.Invoke(_currScore);
    }

    private void UpdateEnemyUI()
    {
        enemyAmountChange.Invoke(-1);
    }
    
    private void DoNullChecks() {
        if (_weaponFireDelay <= 0) { _weaponFireDelay = 1f; Debug.Log("Player:DoNullChecks() _weaponFireDelay is < 1! Set to 1."); }
        if (_currAmmoCount < 1) { _currAmmoCount = 100; Debug.Log("_currAmmoCount is < 1! Set to 100."); }
        if (_currScore != 0) { _currScore = 0; Debug.Log("_currScore was not equal to 0! Set to 0."); }
    }
}
