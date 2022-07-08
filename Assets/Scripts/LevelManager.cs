using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoSingleton<LevelManager> {
    [SerializeField] private bool _levelCompleted;
    [SerializeField] private bool _onLastWave;
    [Space]
    [SerializeField] private int _enemiesAlive;
    [SerializeField] private int _enemiesKilled;

    
    public bool GetLevelCompleted() { return _levelCompleted; }
    public void SetLevelCompleted(bool isCompleted) { _levelCompleted = isCompleted; }
    public bool GetOnLastWave() { return _onLastWave; }
    public void SetOnLastWave(bool isOnLastWave) { _onLastWave = isOnLastWave; }
    public int GetEnemiesAlive() { return _enemiesAlive; }
    public int GetEnemiesKilled() { return _enemiesKilled; }

    private void Start() { _levelCompleted = false; _onLastWave = false; }

    public void AddEnemiesAlive(int numEnemiesToAdd) { _enemiesAlive += numEnemiesToAdd; UpdateUI();}
    public void DeductEnemiesAlive(int numEnemiesToSubtract) { _enemiesAlive -= numEnemiesToSubtract; UpdateUI(); }
    public void AddEnemiesKilled(int numEnemiesKilled) {
        _enemiesKilled += numEnemiesKilled; 
        DeductEnemiesAlive(numEnemiesKilled);
        if (_onLastWave && _enemiesAlive <= 0) {
            _levelCompleted = true;
            UIManager.Instance.WinConditionMet();
        }
    }
    
    private void UpdateUI() { UIManager.Instance.UpdateEnemyCount(_enemiesAlive); }
}
