using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip[] music;
    private AudioSource _audioSource;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        int rand = Random.Range(0, music.Length - 1);
        _audioSource.clip = music[rand];
        _audioSource.Play();
    }
}
