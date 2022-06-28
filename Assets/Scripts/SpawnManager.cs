using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnManager : MonoSingleton<SpawnManager>
{
    struct Wave { public int id { get; set; } public float length { get; set; } public int spawnAmount { get; set; } }
    
    [SerializeField] private Vector3 _defaultGOPosition;
    [SerializeField] private Vector3 _defaultGORotation;
    [SerializeField] private GameObject _parentGO;
    [SerializeField] private GameObject _prefabGO;
    [SerializeField] private int _amountOfGOsInPool;
    [SerializeField] private int _numOfRemainingGOsInPool;
    [SerializeField] private int _nextPooledGO_ID;
    [Space] [Space] 
    [SerializeField] private int _numOfWaves;
    [SerializeField] private int _currWave;
    [SerializeField] private float _currWaveStartTime;
    [SerializeField] private float _currTime;
    [Space]
    [SerializeField] private int _maxSpawnsPerWave;
    [SerializeField] private int _minSpawnsPerWave;
    [SerializeField] private float _minLengthOfWave;
    [SerializeField] private float _maxLengthOfWave;
    [Space]
    [SerializeField] private int[] _amountForWave;
    [SerializeField] private List<float> _waveLengths;

    private Wave[] _spawnWaves;
    [Space] [Space]
    [SerializeField] List<GameObject> _spawnedPoolGOs;
    
    public override void Init()
    {
        //Debug.Log("SpawnManager:Init() - SpawnManager singleton has been initialized.");
    }

    private void OnEnable()
    {
        _spawnWaves = new Wave[_numOfWaves];
        _amountForWave = new int[_numOfWaves];
        
        for (int i = 0; i < _numOfWaves; i++) { _waveLengths.Add(-1f); }

        _nextPooledGO_ID = 0;
    }

    private void Start()
    {
        DoNullChecks();
        _spawnedPoolGOs = GeneratePoolGOs(_amountOfGOsInPool);
        SetPositionsAndRotations(_spawnedPoolGOs, _defaultGOPosition, _defaultGORotation);
        _currWave = 0;
        GenerateWaves(ref _spawnWaves, _numOfWaves);
        StartCoroutine(StartWaves());
    }

    private void Update()
    {
        _currTime = Time.time;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("RobotAI"))
        {
            other.GetComponent<RobotAI>().ReachedEnd();
        }
    }

    private IEnumerator StartWaves()
    {
        if (_numOfWaves > 0)
        {
            _currWaveStartTime = Time.time;
            StartCoroutine(EnableWaveGOs(_nextPooledGO_ID, _spawnWaves[_currWave].spawnAmount));
            yield return new WaitForSeconds(_spawnWaves[_currWave].length);
            if (_currWave < _numOfWaves)
            {
                if (_numOfRemainingGOsInPool < _maxSpawnsPerWave)
                { _nextPooledGO_ID = 0; }
                else { _nextPooledGO_ID += _spawnWaves[_currWave].spawnAmount; }
                _currWave += 1;
                StartCoroutine(StartWaves());
            }
        }
        else { Debug.Log("SpawnManager:StartWaves() _numWaves <= 0!"); }
    }

    private IEnumerator EnableWaveGOs(int firstGO_ID, int numToEnable)
    {
        for (int i = firstGO_ID; i < firstGO_ID + numToEnable; i++)
        {
            float delaySpawn = Random.Range(0, 11) / 2;
            _spawnedPoolGOs[i].SetActive(true);
            _numOfRemainingGOsInPool = (_amountOfGOsInPool - i) -1;
            yield return new WaitForSeconds(delaySpawn);
        }
    }
    
    private void GenerateWaves(ref Wave[] wavesArray, int numOfWaves)
    {
        for (int i = 0; i < numOfWaves; i++)
        {
            wavesArray[i].id = i;
            wavesArray[i].length = Random.Range(_minLengthOfWave, _maxLengthOfWave);
            _waveLengths[i] = wavesArray[i].length;
            wavesArray[i].spawnAmount = Random.Range(_minSpawnsPerWave, _maxSpawnsPerWave);
            _amountForWave[i] = wavesArray[i].spawnAmount;
        }
    }
    
    List<GameObject> GeneratePoolGOs(int amount)
    {
        List<GameObject> gos = new List<GameObject>();
        
        for (int i = 0; i < amount; i++)
        {
            GameObject go = Instantiate(_prefabGO);
            go.transform.parent = _parentGO.transform;
            go.SetActive(false);
            gos.Add(go);
        }

        return gos;
    }
    
    private void SetPositionsAndRotations(List<GameObject> listOfGOs, Vector3 position, Vector3 rotation)
    {
        listOfGOs.ForEach(delegate(GameObject obj)
            { obj.transform.SetPositionAndRotation(position, quaternion.Euler(rotation.x, rotation.y, rotation.z)); });
    }

    private void DoNullChecks()
    {
        if (_prefabGO == null) { Debug.LogError("SpawnManager:DoNullChecks() _prefabGo is NULL!"); }

        if (_amountOfGOsInPool == null | _amountOfGOsInPool < 1)
        { Debug.Log("SpawnManager:DoNullChecks() _amountToSpawn is NULL or < 1! Assigned to 100."); _amountOfGOsInPool = 100; }
        
        if (_defaultGOPosition == Vector3.zero) { Debug.LogError("SpawnManager:DoNullChecks() _defaultPosition is Vector3.zero!");}
        if (_defaultGORotation == Vector3.zero) { Debug.LogError("SpawnManager:DoNullChecks() _defaultRotation is Vector3.zero!");}

        if (_numOfWaves == null | _numOfWaves < 1)
        { Debug.Log("SpawnManager:DoNullChecks() _numOfWaves is NULL or < 1! Assigned to 10."); _numOfWaves = 10; }

        if (_maxSpawnsPerWave < 1)
        { Debug.Log("SpawnManager:DoNullChecks() _maxSpawnsPerWave is < 1! Set to 25."); _maxSpawnsPerWave = 25; }

        if (_minSpawnsPerWave < 1)
        { Debug.Log("SpawnManager:DoNullChecks() _minSpawnsPerWave is < 1! Set to 7."); _minSpawnsPerWave = 7; }

        if (_maxLengthOfWave < 1)
        { Debug.Log("SpawnManager:DoNullChecks() _maxLengthOfWave < 1! Set to 90."); _maxLengthOfWave = 90; }

        if (_minLengthOfWave < 1)
        { Debug.Log("SpawnManager:DoNullChecks() _minLengthOfWave < 1! Set to 45."); _minLengthOfWave = 45; }
    }
}
