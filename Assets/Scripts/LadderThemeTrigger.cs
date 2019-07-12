using UnityEngine;

public class LadderThemeTrigger : LadderTheme
{
    private AudioSource _audio;
    private FadeState _fadeState;
    
    private enum FadeState
    {
        None, FadeIn, FadeOut
    }

    private void Start()
    {
        _audio = GetComponent<AudioSource>();
        _audio.playOnAwake = false;
        _audio.volume = int.Parse(name[name.Length - 1].ToString()) == 1 ? 1 : 0;
        _fadeState = FadeState.None;
    }

    private void Update()
    {
        switch(_fadeState)
        {
            case FadeState.None:
                break;
            case FadeState.FadeIn:
                if (Fade(_audio, true))
                    _fadeState = FadeState.None;
                break;
            case FadeState.FadeOut:
                if (Fade(_audio, false))
                    _fadeState = FadeState.None;
                break;
        }
    }

    protected override bool Fade(AudioSource song, bool increase)
    {
        return base.Fade(song, increase);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Ball")
            _fadeState = FadeState.FadeIn;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name == "Ball")
            _fadeState = FadeState.FadeOut;
    }
}
