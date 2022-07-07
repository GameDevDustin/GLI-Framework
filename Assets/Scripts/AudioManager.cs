using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoSingleton<AudioManager>
{
    [Range(0,1)]
    [SerializeField] private float _audioVolume;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioSource _bgMusicAudioSource;
    [SerializeField] private AudioClip _rifleFired;
    [SerializeField] private AudioClip _coverImpactHit;
    [SerializeField] private AudioClip _aiReachedEnd;
    [SerializeField] private AudioClip _victoryCondition;
    [SerializeField] private AudioClip _defeatCondition;


    private void Start() {
        DoNulLChecks();
    }

    public void PlayerRifleFired() {
        _audioSource.clip = _rifleFired;
        _audioSource.Play();
    }

    public void PlayCoverImpactHit()
    {
        StartCoroutine(CoverImpactDelay());
    }

    public void PlayAIReachedEnd() {
        _audioSource.clip = _aiReachedEnd;
        _audioSource.Play();
    }

    public void PlayVictoryCondition() {
        _audioSource.clip = _victoryCondition;
        _audioSource.Play();
    }

    public void PlayDefeatCondition() {
        _audioSource.clip = _defeatCondition;
        _audioSource.Play();
    }

    public void MuteAudio()
    {
        _audioSource.enabled = false;
        _bgMusicAudioSource.enabled = false;
    }

    public void UnmuteAudio()
    {
        _audioSource.enabled = true;
        _bgMusicAudioSource.enabled = true;
    }
    
    private IEnumerator CoverImpactDelay()
    {
        yield return new WaitForSeconds(0.075f);
        _audioSource.clip = _coverImpactHit;
        _audioSource.volume = _audioVolume / 8;
        // Debug.Log("_audioSource volume = " + _audioSource.volume);
        _audioSource.Play();
        _audioSource.volume = _audioVolume;
    }

    private void DoNulLChecks()
    {
        if (_audioSource == null) { _audioSource = GetComponent<AudioSource>(); }
        if (_bgMusicAudioSource == null) { Debug.LogError("AudioManager:DoNullChecks() _bgMusicAudioSource is NULL!"); }
        if (_rifleFired == null) { Debug.LogError("AudioManager:DoNullChecks() _rifleFired is NULL!"); }
        if (_coverImpactHit == null) { Debug.LogError("AudioManager:DoNullChecks() _coverImpactHit is NULL!"); }
        if (_aiReachedEnd == null) { Debug.LogError("AudioManager:DoNullChecks() _aiReachedEnd is NULL!"); }
        if (_victoryCondition == null) { Debug.LogError("AudioManager:DoNullChecks() _victoryCondition is NULL!"); }
        if (_defeatCondition == null) { Debug.LogError("AudioManager:DoNullChecks() _defeatCondition is NULL!"); }
    }
}
