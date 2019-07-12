using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSoundEffects : MonoBehaviour
{
    public AudioClip ballRollClip;
    public AudioClip ballIntoHoleClip;
    public AudioClip ballOutOfHoleClip;
    public AudioClip teleport;
    public List<AudioClip> thuds;
    public List<AudioClip> gasps;

    private Transform _gaspedHole;
    private Ball _ballScript;
    private Wall _wallScript;
    private float _distToGround;
    private bool _fallThud, _enteredHoleFX, _gasped;
    private Rigidbody _rigidbody;
    private AudioSource _ballRollAudioSource, _ballOtherAudioSource;

    private const float _SOUND_SPEED_DISTORTION = 7, _DISTANCE_OFFSET = 0.1f;

    void Start ()
    {
        _ballScript = gameObject.GetComponent<Ball>();
        _wallScript = GameObject.Find("Main Wall").GetComponent<Wall>();

        _ballOtherAudioSource = gameObject.AddComponent<AudioSource>();

        _ballRollAudioSource = gameObject.AddComponent<AudioSource>();
        _ballRollAudioSource.clip = ballRollClip;
        _ballRollAudioSource.loop = true;

        _rigidbody = gameObject.GetComponent<Rigidbody>();
        _distToGround = _rigidbody.GetComponent<Collider>().bounds.extents.y;

        _fallThud = _enteredHoleFX = _gasped = false;
    }
    
    void Update ()
    {
        if (GameMode.CurrentGameModeState != GameMode.GameModeState.Gameplay)
            return;
        if (GameMode.CurrentGameplayState == GameMode.GameplayState.Paused)
        {
            _ballRollAudioSource.Stop();
            _ballOtherAudioSource.Stop();
            _gaspedHole = null;
            return;
        }

        if (GameMode.CurrentGameplayState == GameMode.GameplayState.Reset && _enteredHoleFX)
        {
            _ballOtherAudioSource.PlayOneShot(ballOutOfHoleClip);
            _enteredHoleFX = false;
        }

        if (Grounded())
        {

            if (!_ballRollAudioSource.isPlaying)
                _ballRollAudioSource.Play();

            RollSoundBasedOnSpeed();
            GaspDetection();
        }
        else
        {
            if (_ballScript.EnteringHole && !_enteredHoleFX)
            {
                _ballOtherAudioSource.PlayOneShot(ballIntoHoleClip);
                if(_ballScript.Teleporting)
                    _ballOtherAudioSource.PlayOneShot(teleport);
                _enteredHoleFX = true;
            }

            if (_ballRollAudioSource.isPlaying)
                _ballRollAudioSource.Stop();
            
            if (!Physics.Raycast(transform.position, Vector3.down, 0.75f))
                _fallThud = true;
        }
    }

    private void ThudEffects()
    {
        PlayRandomSound(thuds);
        _fallThud = false;
    }

    private void GaspDetection()
    {
        if (_gasped)
            return;

        float heightOffset = 0.25f, gaspOffset = 2.5f, gaspOffsetMax = 1;
        foreach (Transform t in _wallScript.Holes)
        {
            if (_wallScript.Goals.Contains(t) || _wallScript.Teleports.Contains(t) || t == _gaspedHole)
                continue;

            Vector3 v = t.position;
            if (Vector3.Distance(transform.position, v) < gaspOffset && Vector3.Distance(transform.position, v) < gaspOffset + gaspOffsetMax &&
                transform.position.y >= v.y - heightOffset &&
                transform.position.y <= v.y + heightOffset)
                Gasp(t);
        }
    }

    private void Gasp(Transform gaspedHole)
    {
        PlayRandomSound(gasps);
        _gasped = true;
        StartCoroutine(GaspCooldown());
        _gaspedHole = gaspedHole;
    }

    private IEnumerator GaspCooldown()
    {
        yield return new WaitForSeconds(1);
        _gasped = false;
    }

    private void PlayRandomSound(List<AudioClip> clips)
    {
        int randClip = Random.Range(0, clips.Count);
        _ballOtherAudioSource.PlayOneShot(clips[randClip]);
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
