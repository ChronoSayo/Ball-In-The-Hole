using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSoundEffects : MonoBehaviour
{
    public AudioClip ballRollClip;
    public List<AudioClip> thuds;

    private float _distToGround;
    private bool _fallThud;
    private Rigidbody _rigidbody;
    private AudioSource _ballRollAudioSource, _thudAudioSource;

    private const float _SOUND_SPEED_DISTORTION = 7, _DISTANCE_OFFSET = 0.1f;

    void Start ()
    {
        _thudAudioSource = gameObject.AddComponent<AudioSource>();

        _ballRollAudioSource = gameObject.AddComponent<AudioSource>();
        _ballRollAudioSource.clip = ballRollClip;
        _ballRollAudioSource.loop = true;

        _rigidbody = gameObject.GetComponent<Rigidbody>();
        _distToGround = _rigidbody.GetComponent<Collider>().bounds.extents.y;

        _fallThud = false;
    }
    
    void Update ()
    {
        if (GameMode.CurrentGameModeState != GameMode.GameModeState.Gameplay)
            return;
        if (GameMode.CurrentGameplayState == GameMode.GameplayState.Paused)
        {
            _ballRollAudioSource.Stop();
            _thudAudioSource.Stop();
            return;
        }

        if (Grounded())
        {
            if (!_ballRollAudioSource.isPlaying)
                _ballRollAudioSource.Play();
            //if (!_thudAudioSource.isPlaying)
            //    ThudEffects();

            RollSoundBasedOnSpeed();
        }
        else
        {
            if(_ballRollAudioSource.isPlaying)
                _ballRollAudioSource.Stop();

            Debug.DrawRay(transform.position, Vector3.down * 0.75f, Color.black);
            if (!Physics.Raycast(transform.position, Vector3.down, 0.75f))
                _fallThud = true;
        }
    }

    private void ThudEffects()
    {
        int randClip = Random.Range(0, thuds.Count);
        _thudAudioSource.PlayOneShot(thuds[randClip]);
        _fallThud = false;
    }

    private bool Grounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, _distToGround + _DISTANCE_OFFSET);
    }

    private void RollSoundBasedOnSpeed()
    {
        _ballRollAudioSource.pitch = GetSpeed / _SOUND_SPEED_DISTORTION;
    }

    private void OnCollisionEnter(Collision collision)
    {
        string colName = collision.transform.name;
        if (GameMode.CurrentGameModeState == GameMode.GameModeState.Gameplay &&
            (colName == "Left" || colName == "Right" || (colName == "Platform" && _fallThud)))
            ThudEffects();
    }

    private float GetSpeed { get { return _rigidbody.velocity.magnitude; } }
}
