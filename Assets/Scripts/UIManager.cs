using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.UI;

public class UIManager : MonoSingleton<UIManager>
{
   [SerializeField] private TMP_Text _txtAmmoCount;
   [SerializeField] private TMP_Text _txtScore;
   [SerializeField] private TMP_Text _txtEnemyCount;
   [SerializeField] private TMP_Text _txtNotifications;
   [SerializeField] private TMP_Text _txtTimeRemaining;
   private int _currEnemyCount;
   private float _timeRemaining;
   private int _timesNotificationHasFlickered;
   [SerializeField] private Material _redNotificationMaterial;
   [SerializeField] private Material _blueNotificationMaterial;


   private void Start() {
      DoNullChecks();
      SetDefaultUIValues();
   }

   private void Update() {
      if (_timeRemaining > 0) { DecrementTimeRemaining(); }
   }

   private void SetDefaultUIValues()
   {
      _txtNotifications.text = "WARNING";
      _txtScore.text = "0";
      _txtEnemyCount.text = "0";
      _txtTimeRemaining.text = "Time Remaining: 0 seconds";
      _currEnemyCount = 0;
      _timesNotificationHasFlickered = 0;
   }

   private void DecrementTimeRemaining() {
      _timeRemaining -= Time.deltaTime;
      UpdateTimeRemaining(_timeRemaining);
   }

   public void UpdateAmmoCount(int ammoCount)
   {
      _txtAmmoCount.text = ammoCount.ToString();
   }

   public void UpdateScore(int score)
   {
      _txtScore.text = score.ToString();
   }

   public void UpdateEnemyCount(int enemyCount) {
      _currEnemyCount += enemyCount;
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
   
   private void DoNullChecks()
   {
      if (_txtAmmoCount == null) { Debug.Log("_txtAmmoCount is NULL!"); }
      if (_txtScore == null) { Debug.Log("_txtScore is NULL!"); }
      if (_txtEnemyCount == null) { Debug.Log("_txtEnemyCount is NULL!"); }
      if (_txtNotifications == null) { Debug.Log("_txtNotifications is NULL!"); }
      if (_txtTimeRemaining == null) { Debug.Log("_txtTimeRemaining is NULL!"); }
   }
}
