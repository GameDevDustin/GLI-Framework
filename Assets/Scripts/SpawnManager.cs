using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.Events;

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
    [SerializeField] private int _totalEnemiesAllWaves;
    [SerializeField] private int _numOfEnemiesReachedEnd;
    [Space]
    [SerializeField] private int[] _amountForWave;
    [SerializeField] private List<float> _waveLengths;
    private Wave[] _spawnWaves;
    [Space]
    [SerializeField] private UnityEvent<int> enemyCountChanged;
    [SerializeField] private UnityEvent enemyReachedEnd;
    [SerializeField] private UnityEvent<float> timeRemainingChanged;
    [SerializeField] private UnityEvent<string, int> setNotificationText;
    [SerializeField] private UnityEvent loseCondition;
    [Space] [Space]
    [SerializeField] List<GameObject> _spawnedPoolGOs;
    
    public override void Init() {
        //Debug.Log("SpawnManager:Init() - SpawnManager singleton has been initialized.");
    }

    private void OnEnable() {
        _spawnWaves = new Wave[_numOfWaves];
        _amountForWave = new int[_numOfWaves];
        
        for (int i = 0; i < _numOfWaves; i++) { _waveLengths.Add(-1f); }

        _nextPooledGO_ID = 0;
        _numOfEnemiesReachedEnd = 0;
    }

    private void Start() {
        DoNullChecks();
        _spawnedPoolGOs = GeneratePoolGOs(_amountOfGOsInPool);
        SetPositionsAndRotations(_spawnedPoolGOs, _defaultGOPosition, _defaultGORotation);
        _currWave = 0;
        GenerateWaves(ref _spawnWaves, _numOfWaves);
        
        StartCoroutine(StartWaves());
    }

    private void Update() {
        _currTime = Time.time;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.transform.parent.CompareTag("RobotAI")) {
            other.GetComponentInParent<RobotAI>().ReachedEnd();
            _numOfEnemiesReachedEnd += 1;
            enemyCountChanged.Invoke(-1);
            enemyReachedEnd.Invoke();
            if (_numOfEnemiesReachedEnd > (_totalEnemiesAllWaves / 2)) {
                loseCondition.Invoke();
            } else if (LevelManager.Instance.GetOnLastWave() == true && LevelManager.Instance.GetEnemiesAlive() <= 0) {
                UIManager.Instance.WinConditionMet();
            }
        }
    }

    private IEnumerator StartWaves() {
        if (_numOfWaves > 0 && UIManager.Instance.GetGameOverStatus() == false) {
            _currWaveStartTime = Time.time;
            UpdateUI(_spawnWaves[_currWave].spawnAmount, _spawnWaves[_currWave].length);
            LevelManager.Instance.AddEnemiesAlive(_spawnWaves[_currWave].spawnAmount);
            setNotificationText.Invoke("WARNING", _currWave + 1);
            StartCoroutine(EnableWaveGOs(_nextPooledGO_ID, _spawnWaves[_currWave].spawnAmount));
            yield return new WaitForSeconds(_spawnWaves[_currWave].length);
            
            if (_currWave < _numOfWaves) {
                if (_numOfRemainingGOsInPool < _maxSpawnsPerWave)
                { _nextPooledGO_ID = 0; }
                else { _nextPooledGO_ID += _spawnWaves[_currWave].spawnAmount; }
                _currWave += 1;
                StartCoroutine(StartWaves());
            }
            else if (_currWave + 1 == _numOfWaves) { LevelManager.Instance.SetOnLastWave(true); }
        }
        else { Debug.Log("SpawnManager:StartWaves() _numWaves <= 0!"); }
    }

    private IEnumerator EnableWaveGOs(int firstGO_ID, int numToEnable) {
        for (int i = firstGO_ID; i < firstGO_ID + numToEnable; i++) {
            float delaySpawn = Random.Range(0, 11) / 2;
            _spawnedPoolGOs[i].SetActive(true);
            _numOfRemainingGOsInPool = (_amountOfGOsInPool - i) -1;
            yield return new WaitForSeconds(delaySpawn);
        }
    }
    
    private void GenerateWaves(ref Wave[] wavesArray, int numOfWaves) {
        for (int i = 0; i < numOfWaves; i++) {
            wavesArray[i].id = i;
            wavesArray[i].length = Random.Range(_minLengthOfWave, _maxLengthOfWave);
            _waveLengths[i] = wavesArray[i].length;
            wavesArray[i].spawnAmount = Random.Range(_minSpawnsPerWave, _maxSpawnsPerWave);
            _amountForWave[i] = wavesArray[i].spawnAmount;
            _totalEnemiesAllWaves += _amountForWave[i];
        }
    }
    
    List<GameObject> GeneratePoolGOs(int amount) {
        List<GameObject> gos = new List<GameObject>();
        
        for (int i = 0; i < amount; i++) {
            GameObject go = Instantiate(_prefabGO);
            go.transform.parent = _parentGO.transform;
            go.SetActive(false);
            gos.Add(go);
        }

        return gos;
    }
    
    private void SetPositionsAndRotations(List<GameObject> listOfGOs, Vector3 position, Vector3 rotation) {
        listOfGOs.ForEach(delegate(GameObject obj)
            { obj.transform.SetPositionAndRotation(position, quaternion.Euler(rotation.x, rotation.y, rotation.z)); });
    }

    private void UpdateUI(int numOfEnemies, float timeRemaining) {
        enemyCountChanged.Invoke(numOfEnemies);
        timeRemainingChanged.Invoke(timeRemaining);
    }

    public int GetTotalNumOfEnemiesAllWaves()
    {
        return _totalEnemiesAllWaves;
    }
    
    private void DoNullChecks() {
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
