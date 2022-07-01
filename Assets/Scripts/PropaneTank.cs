using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropaneTank : MonoBehaviour
{
    enum TankSize { small, medium, large }

    [SerializeField] private TankSize _tankSize;
    [SerializeField] private float _durability;
    private GameObject _vfxExplosionGO;
    [Space]
    [SerializeField] private float _defaultLargeDurability;
    [SerializeField] private float _defaultMediumDurability;
    [SerializeField] private float _defaultSmallDurability;

    private void Start() {
         DoNullChecks();
         if (_durability < 1) { AssignDurability(); }
         _vfxExplosionGO = transform.GetChild(0).gameObject;
    }

    public void TakeDamage(float damageAmount) {
        _durability -= damageAmount;
        if (_durability <= 0) { Explode(); }
    }

    private void Explode() {
        _vfxExplosionGO.SetActive(true);
        StartCoroutine(DelayHide());
    }

    private IEnumerator DelayHide()
    {
        yield return new WaitForSeconds(3.8f);
        transform.gameObject.SetActive(false);
    }

    private void AssignDurability() {
        switch (_tankSize) {
            case TankSize.small: _durability = _defaultSmallDurability; break;
            case TankSize.medium: _durability = _defaultMediumDurability; break;
            case TankSize.large: _durability = _defaultLargeDurability; break;
        }
    }
    
    private void DoNullChecks() {
        if (_durability < 0) { _durability = 100; Debug.Log("_health is < 1! Set to 100."); }
        if (_defaultSmallDurability < 1) { _defaultSmallDurability = 50; Debug.Log("_defaultSmallDurability < 1! Set to 50."); }
        if (_defaultMediumDurability < 1) { _defaultMediumDurability = 125; Debug.Log("_defaultMediumDurability < 1! Set to 125."); }
        if (_defaultLargeDurability < 1) { _defaultLargeDurability = 250; Debug.Log("_defaultLargeDurability < 1! Set to 250."); }
    }
}