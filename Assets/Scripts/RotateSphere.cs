using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class RotateSphere : MonoBehaviour
{
    [SerializeField] private Transform _sphereTransform;
    [SerializeField] private float _speed;

    private void Start()
    {
        _sphereTransform = this.transform;
        _speed = 33f;
    }

    private void Update()
    {
        _sphereTransform.Rotate(0f, _speed * Time.deltaTime, 0f);
    }
}
