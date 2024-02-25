using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip[] music;
    [SerializeField] private AudioClip[] victoryMusic;
    private AudioSource _audioSource;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        int rand = Random.Range(0, music.Length - 1);
        _audioSource.clip = music[rand];
        _audioSource.Play();
    }

    public void PlayWinMusic()
    {
        _audioSource.Stop();
        int rand = Random.Range(0, victoryMusic.Length - 1);
        _audioSource.clip = victoryMusic[rand];
        _audioSource.Play();
    }
}
