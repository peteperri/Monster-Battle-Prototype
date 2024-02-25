using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class SFX : MonoBehaviour
{
    [SerializeField] private SerializedDictionary<SoundEffect, AudioClip> soundClips;
    [SerializeField] private AudioSource audioSource;
    
    private void PlaySound(SoundEffect soundToPlay)
    {
        audioSource.PlayOneShot(soundClips[soundToPlay]);
    }

    //refactor to singleton pattern 
    public static void Play(SoundEffect soundToPlay)
    {
        SFX player = FindObjectOfType<SFX>();
        player.PlaySound(soundToPlay);
    }
}


