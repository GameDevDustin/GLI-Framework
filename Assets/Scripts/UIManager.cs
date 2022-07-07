using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoSingleton<UIManager>
{
   [SerializeField] private TMP_Text _txtAmmoCount;
   [SerializeField] private TMP_Text _txtScore;
   [SerializeField] private TMP_Text _txtEnemyCount;
   [SerializeField] private TMP_Text _txtNotifications;
   [SerializeField] private TMP_Text _txtTimeRemaining;
   [SerializeField] private TMP_Text _txtGameOver;
   [SerializeField] private TMP_Text _txtGameOverTips;
   private int _currEnemyCount;
   private float _timeRemaining;
   private int _timesNotificationHasFlickered;
   [SerializeField] private Material _redNotificationMaterial;
   [SerializeField] private Material _blueNotificationMaterial;
   [SerializeField] private bool _checkGameOverInput;
   [SerializeField] private bool _gameOver;

   private void Start() {
      DoNullChecks();
      SetDefaultUIValues();
   }

   private void Update() {
      if (_timeRemaining > 0) { DecrementTimeRemaining(); }
      if (_checkGameOverInput) { CheckGameOverInput(); }
   }

   private void CheckGameOverInput() {
      if (Keyboard.current.escapeKey.wasPressedThisFrame) { Application.Quit();}
      if (Keyboard.current.rKey.wasPressedThisFrame) { SceneManager.LoadScene(0); }
   }
   
   private void SetDefaultUIValues() {
      _txtNotifications.text = "WARNING";
      _txtScore.text = "0";
      _txtEnemyCount.text = "0";
      _txtTimeRemaining.text = "Time Remaining: 0 seconds";
      _currEnemyCount = 0;
      _timesNotificationHasFlickered = 0;
      _checkGameOverInput = false;
      _gameOver = false;
   }

   private void DecrementTimeRemaining() {
      _timeRemaining -= Time.deltaTime;
      UpdateTimeRemaining(_timeRemaining);
   }

   public void UpdateAmmoCount(int ammoCount) {
      _txtAmmoCount.text = ammoCount.ToString();
   }

   public void UpdateScore(int score) {
      _txtScore.text = score.ToString();
   }

   public void UpdateEnemyCount(int enemyCount) {
      _currEnemyCount = enemyCount;
      _txtEnemyCount.text = _currEnemyCount.ToString();
   }

   public void SetNotificationText(string notificationText, int currentWave) {
      if (notificationText == "WARNING") {
         _timesNotificationHasFlickered = 0;
         _txtNotifications.fontMaterial = _redNotificationMaterial;
         StartCoroutine(FlickerNotificationText(notificationText, currentWave));
      }
   }

   public void UpdateTimeRemaining(float timeRemaining) {
      _timeRemaining = timeRemaining;
      _txtTimeRemaining.text = "Time Remaining: " + Mathf.RoundToInt(timeRemaining).ToString() + " seconds";
   }

   public void LoseConditionMet() {
      _txtGameOver.text = "You lose!";
      GameOverTasks();
      AudioManager.Instance.PlayDefeatCondition();
   }

   public void WinConditionMet() {
      _txtGameOver.text = "You win!";
      GameOverTasks();
      AudioManager.Instance.PlayVictoryCondition();
   }

   public bool GetGameOverStatus() {
      return _gameOver;
   }

   private void GameOverTasks()
   {
      GameOverSetTextsActive();
      _gameOver = true;
      _checkGameOverInput = true;
      AudioManager.Instance.MuteAudio();
   }
   
   private void GameOverSetTextsActive()
   {
      _txtGameOver.gameObject.SetActive(true);
      _txtGameOverTips.gameObject.SetActive(true);
   }
   private IEnumerator FlickerNotificationText(string notificationText, int currentWave) {
      _txtNotifications.text = notificationText;
      yield return new WaitForSeconds(0.5f);
      _txtNotifications.text = "";
      yield return new WaitForSeconds(0.5f);
      _timesNotificationHasFlickered += 1;
      
      if (_timesNotificationHasFlickered < 5) {
         StartCoroutine(FlickerNotificationText(notificationText, currentWave));
      }
      else { 
         _txtNotifications.fontMaterial = _blueNotificationMaterial; 
         _txtNotifications.text = "Wave " + currentWave.ToString(); }
   }
   
   private void DoNullChecks() {
      if (_txtAmmoCount == null) { Debug.Log("_txtAmmoCount is NULL!"); }
      if (_txtScore == null) { Debug.Log("_txtScore is NULL!"); }
      if (_txtEnemyCount == null) { Debug.Log("_txtEnemyCount is NULL!"); }
      if (_txtNotifications == null) { Debug.Log("_txtNotifications is NULL!"); }
      if (_txtTimeRemaining == null) { Debug.Log("_txtTimeRemaining is NULL!"); }
   }
}
