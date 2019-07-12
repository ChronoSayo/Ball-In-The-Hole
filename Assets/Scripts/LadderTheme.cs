using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderTheme : MonoBehaviour
{
    public bool mute;
    public AudioClip goalJingleClip, missJingleClip;
    public List<AudioClip> songs;
    
    private float _jingleTick, _jingleTime;
    private int _playLevel, _stopLevel;
    private bool _playJingle, _fade, _muteBase, _setToDefault;
    private AudioSource _goalJingleAudioSource, _missJingleAudioSource, _activeAudioSource;
    private List<AudioSource> _songs;

    void Start ()
    {
        _goalJingleAudioSource = gameObject.AddComponent<AudioSource>();
        _goalJingleAudioSource.clip = goalJingleClip;
        _goalJingleAudioSource.loop = true;
        _goalJingleAudioSource.Play();
        _goalJingleAudioSource.volume = 0;

        _missJingleAudioSource = gameObject.AddComponent<AudioSource>();
        _missJingleAudioSource.clip = missJingleClip;
        _missJingleAudioSource.loop = true;
        _missJingleAudioSource.Play();
        _missJingleAudioSource.volume = 0;

        _songs = new List<AudioSource>();
        foreach (Transform t in transform)
            _songs.Add(t.GetComponent<AudioSource>());
        
    }

    private void Update()
    {
        if(_setToDefault)
            FadingAllSongs();
    }

    protected virtual bool Fade(AudioSource song, bool increase)
    {
        float crossfadeSpeed = 0.05f;
        song.volume += increase ? crossfadeSpeed : -crossfadeSpeed;
        return increase ? song.volume >= 1 : song.volume <= 0;
    }

    public void SetToDefault()
    {
        _setToDefault = true;
    }

    public void FadingAllSongs()
    {
        foreach (AudioSource song in _songs)
        {
            if (song.transform.name.EndsWith("1"))
                Fade(song, true);
            if (Fade(song, false))
                _setToDefault = false;
        }
    }

    public void SetSong(int i, bool muteBase)
    {
        _muteBase = muteBase;
        _playLevel = i;
        _fade = true;
    }

    public void StopSong(int i)
    {
        _stopLevel = i;
    }

    public void PlaySong(bool play)
    {
        foreach (AudioSource audio in _songs)
        {
            if (play)
                audio.Play();
            else
                audio.Stop();
        }
    }

    public void PauseSong(bool pause)
    {
        foreach (AudioSource audio in _songs)
        {
            if (pause)
                audio.Play();
            else
                audio.Pause();
        }
    }

    public void PlayJingle(bool goal)
    {
        AudioSource jingle = goal ? _goalJingleAudioSource : _missJingleAudioSource;
        jingle.volume = 1;
        StartCoroutine(JingleDuration(jingle, 2));
    }

    private IEnumerator JingleDuration(AudioSource jingle, float sec)
    {
        yield return new WaitForSecondsRealtime(sec);
        if (GameMode.CurrentGameplayState != GameMode.GameplayState.Playing)
            StartCoroutine(JingleDuration(jingle, 0.5f));
        else
            jingle.volume = 0;
    }
}
