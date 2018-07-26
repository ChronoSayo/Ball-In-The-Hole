using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderTheme : MonoBehaviour
{
    public bool mute;
    public AudioClip goalJingleClip, missJingleClip;
    public List<AudioClip> songs;
    
    private float _crossfadeSpeed;
    private float _jingleTick, _jingleTime;
    private int _currentSong;
    private bool _playJingle;
    private Crossfade _crossfade;
    private AudioSource _goalJingleAudioSource, _missJingleAudioSource;
    private List<AudioSource> _songs;

    private enum Crossfade
    {
        None, FadeIn, FadeOut, FadeOutToDefault
    }

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
        for (int i = 0; i < songs.Count; i++)
            AddSongData(songs[i]);
        _songs[0].volume = 1;

        _crossfadeSpeed = 0.05f;

        _crossfade = Crossfade.None;
        _currentSong = -1;

        //if (mute)
            _currentSong = 0;
        
        Mute();
    }

    private void Update()
    {
        Mute();

        switch(_crossfade)
        {
            case Crossfade.None:
                break;
            case Crossfade.FadeIn:
                FadeIn();
                break;
            case Crossfade.FadeOut:
                FadeOut();
                break;
            case Crossfade.FadeOutToDefault:
                FadeOutToDefault();
                break;
        }
    }

    private void Mute()
    {
        _songs[_currentSong].mute = _missJingleAudioSource.mute = _goalJingleAudioSource.mute = mute;
    }

    private void FadeIn()
    {
        int i = _currentSong;
        if (_songs[i].volume < 1)
        {
            _songs[i].volume += _crossfadeSpeed;
            if (_songs[i].volume > 1)
            {
                _songs[i].volume = 1;
                _crossfade = Crossfade.None;
            }
        }
    }

    private void FadeOut()
    {
        int i = _currentSong;
        if (_songs[i].volume > 0)
        {
            _songs[i].volume -= _crossfadeSpeed;
            if (_songs[i].volume < 0)
            {
                _songs[i].volume = 0;
                _crossfade = Crossfade.None;
            }
        }
    }
    private void FadeOutToDefault()
    {
        for (int i = _songs.Count - 1; i > 0; i--)
        {
            if (_songs[i].volume > 0)
            {
                _songs[i].volume -= _crossfadeSpeed;
                if (_songs[i].volume < 0)
                {
                    _songs[i].volume = 0;
                    _crossfade = Crossfade.None;
                }
            }
        }
    }

    public void SetSong(int i, bool fadeIn)
    {
        _currentSong = i;
        _crossfade = fadeIn ? Crossfade.FadeIn : Crossfade.FadeOut;
    }

    public void PlaySong(bool play)
    {
        foreach(AudioSource audio in _songs)
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
        StartCoroutine(JingleDuration(jingle));
    }

    private IEnumerator JingleDuration(AudioSource jingle)
    {
        yield return new WaitForSeconds(2);
        jingle.volume = 0;
    }

    public void SetToDefault()
    {
        _crossfade = Crossfade.FadeOutToDefault;
    }

    private int GetAudioSourceIndex(AudioSource audio)
    {
        return _songs.FindIndex(x => x.clip == audio.clip);
    }

    private void AddSongData(AudioClip song)
    {
        AudioSource temp = gameObject.AddComponent<AudioSource>();
        temp.clip = song;
        temp.loop = true;
        temp.playOnAwake = true;
        temp.volume = 0;
        temp.Pause();
        _songs.Add(temp);
    }
}
