using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class SFX : MonoBehaviour
{
    [SerializeField] private SerializedDictionary<SoundEffect, AudioClip> soundClips;
    private AudioSource _audioSource;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void PlaySound(SoundEffect soundToPlay)
    {
        _audioSource.PlayOneShot(soundClips[soundToPlay]);
    }

    //refactor to singleton pattern 
    public static void Play(SoundEffect soundToPlay)
    {
        SFX player = FindObjectOfType<SFX>();
        player.PlaySound(soundToPlay);
    }
}

public enum SoundEffect
{
    Confirm,
    Deny
}
