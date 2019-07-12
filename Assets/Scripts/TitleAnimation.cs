using System.Collections;
using UnityEngine;

public class TitleAnimation : MonoBehaviour
{
    private Transform _titleBall, _pauseBall;
    private Camera _mainCamera;
    private Vector3 _ballStartPos;
    private Rigidbody _ballRigidbody;
    private Vector3 _startRot;
    private float _moveSpeed, _rotSpeed;
    private float _duration, _tick;
    private bool _up, _clockwise;
    private State _state;

    private enum State
    {
        None, Start, Phase1, Phase2, Phase3, Phase4, Max
    }

    void Start ()
    {
        _startRot = transform.rotation.eulerAngles;
        _titleBall = transform.GetChild(1).transform;
        _pauseBall = transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).transform;
        _pauseBall.gameObject.SetActive(false);
        _mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        transform.position = GetStartPosition;

        _ballStartPos = _titleBall.position;
        _ballRigidbody = _titleBall.GetComponent<Rigidbody>();

        _state = State.Start;

        StartCoroutine(DelayDefaultSpeed());
    }

    private IEnumerator DelayDefaultSpeed()
    {
        yield return new WaitForSeconds(1);
        float moveSpeed = GameObject.Find("Player").GetComponent<Platform>().moveSpeed;
        float rotSpeed = GameObject.Find("Player").GetComponent<Platform>().rotSpeed;
        _moveSpeed = moveSpeed;
        _rotSpeed = rotSpeed;
    }
    
    void Update ()
    {
        ResetBall();
        MovePlatform();
    }

    private void MovePlatform()
    {
        switch (_state)
        {
            case State.None:
                break;
            case State.Start:
                StartValues();
                break;
            case State.Phase1:
                MoveAndSetNext(false);
                break;
            case State.Phase2:
                MoveAndSetNext(true);
                break;
            case State.Phase3:
                MoveAndSetNext(false);
                break;
            case State.Phase4:
                MoveAndSetNext(false);
                break;
        }
    }

    private void MoveAndSetNext(bool clockwise)
    {
        if (!TimeOut())
        {
            MovePlatformDirection(_up);
            RotatePlatform(_clockwise);
        }
        else
        {
            _tick = 0;
            _clockwise = clockwise;
            _state += 1;
            if (_state == State.Max)
                StartValues();
        }
    }

    private void StartValues()
    {
        _up = !_up;
        _clockwise = true;
        _duration = 3;
        _tick = 0;
        _state = State.Phase1;
    }

    private bool TimeOut()
    {
        _tick += Time.unscaledDeltaTime;
        return _duration <= _tick;
    }

    private void MovePlatformDirection(bool up)
    {
        Vector3 dir = up ? Vector3.up : Vector3.down;
        transform.position += dir * _moveSpeed;
    }

    private void RotatePlatform(bool clockwise)
    {
        float speed = _rotSpeed * (clockwise ? 1 : -1);
        transform.Rotate(0, 0, speed);
    }

    private void ResetBall()
    {
        Vector2 screenPos = _mainCamera.WorldToScreenPoint(_titleBall.position);
        if (screenPos.y <= 0)
        {
            _titleBall.position = _ballStartPos + (Vector3.up * 60);
            _ballRigidbody.velocity = Vector3.zero;
        }
    }

    public void Enable(bool enable)
    {
        if (enable)
        {
            _state = State.Start;
            transform.position = GetStartPosition;
            transform.rotation = Quaternion.Euler(_startRot);
            _pauseBall.gameObject.SetActive(true);
        }

        if (!enable)
            _titleBall.gameObject.SetActive(false);

        foreach (Transform t in transform)
        {
            if (t.gameObject.activeSelf != enable)
            {
                if (t == _titleBall)
                    continue;
                t.gameObject.SetActive(enable);
            }
        }
    }

    private Vector3 GetStartPosition
    {
        get
        {
            return new Vector3(transform.position.x, _mainCamera.transform.position.y, transform.position.z);
        }
    }
}
