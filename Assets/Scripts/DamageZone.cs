using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class DamageZone : MonoBehaviour
{
    [SerializeField] private Transform[] _transformsInCollider;
    [SerializeField] private int _numOfAiInCollider;
    [SerializeField] private bool _doDamagePhase;

    private void Start() {
        _numOfAiInCollider = 0;
        _doDamagePhase = false;
        _transformsInCollider = new Transform[20];
    }

    private void OnTriggerEnter(Collider other) {
        if (_doDamagePhase && other.CompareTag("HitCollider")) {
            Transform parentTransform = other.transform.parent;

            if (parentTransform != null) {
                parentTransform.GetComponent<RobotAI>().TakeDamage(300f);
            }
        }
        else if (other.CompareTag("HitCollider")) {
            _transformsInCollider[_numOfAiInCollider] = other.transform;
            _numOfAiInCollider += 1;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("HitCollider")) {
            if (_transformsInCollider != null && _transformsInCollider[_numOfAiInCollider] != null) {
                _transformsInCollider[_numOfAiInCollider - 1] = null;
            }
            _numOfAiInCollider -= 1;
        }
    }

    public void DoDamage() {
        _doDamagePhase = true;

        foreach (Transform tran in _transformsInCollider) {
            if (tran != null) {
                Transform tranParent = tran.parent;

                if (tranParent.GetComponent<RobotAI>().IsDying() == false) {
                    tranParent.GetComponent<RobotAI>().TakeDamage(300f);
                }
            }
        }
    }
}
