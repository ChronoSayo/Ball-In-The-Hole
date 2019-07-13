using UnityEngine;

public class LadderThemeTrigger : MonoBehaviour
{
    private LadderTheme _ladderThemeScript;
    private AudioSource _audio;
    private FadeState _fadeState;
    
    private enum FadeState
    {
        None, FadeIn, FadeOut
    }

    private void Start()
    {
        _ladderThemeScript = transform.parent.GetComponent<LadderTheme>();
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
                if (_ladderThemeScript.Fade(_audio, true))
                    _fadeState = FadeState.None;
                break;
            case FadeState.FadeOut:
                if (_ladderThemeScript.Fade(_audio, false))
                    _fadeState = FadeState.None;
                break;
        }
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
