using System.Collections.Generic;
using UnityEngine;

public class HoleSoundEffects : MonoBehaviour
{
    public List<AudioClip> cheers;
    public List<AudioClip> applauses;
    public List<AudioClip> boos;

    private AudioSource _audioSource;

    void Start ()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        switch(GameMode.CurrentGameplayState)
        {
            case GameMode.GameplayState.Paused:
                if (_audioSource.isPlaying)
                    _audioSource.Pause();
                break;
            case GameMode.GameplayState.Playing:
                if (!_audioSource.isPlaying)
                    _audioSource.UnPause();
                break;
        }
    }

    public void PlayCheers()
    {
        PlaySound(cheers, applauses);
    }

    public void PlayBoos()
    {
        PlaySound(boos, null);
    }

    public void PlayGasp()
    {
        PlaySound(boos, null);
    }

    private void PlaySound(List<AudioClip> clips1, List<AudioClip> clips2)
    {
        AudioClip clip = clips1[GetRandom(clips1.Count)];
        _audioSource.PlayOneShot(clip);
        if (clips2 != null)
        {
            clip = clips2[GetRandom(clips2.Count)];
            _audioSource.PlayOneShot(clip);
        }
    }

    private int GetRandom(int max)
    {
        return Random.Range(0, max);
    }
}
