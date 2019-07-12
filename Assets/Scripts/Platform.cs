using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Platform : MonoBehaviour
{
    public float moveSpeed, rotSpeed;
    public AudioClip clip;

    private Quaternion _startRotation;
    private Vector3 _startPosition, _currentStartPosition;
    private AudioSource _audio;
    private float _moveSpeed, _rotSpeed;
    private float _maxHeight;
    private bool _canRotate;
    private List<GameObject> _buttons;
    private List<ButtonPressed> _pressedButtons;


    private struct ButtonPressed
    {
        public string name;
        public bool pressed;
    }

    void Start ()
    {
        _startRotation = transform.rotation;
        _startPosition = transform.position;
        _canRotate = true;

        //bool isAndroid = Application.platform == RuntimePlatform.Android;
        _moveSpeed = moveSpeed; // * (isAndroid ? 2 : 1);
        _rotSpeed = rotSpeed; //* (isAndroid ? 2 : 1);

        _audio = gameObject.AddComponent<AudioSource>();
        _audio.clip = clip;
        _audio.loop = true;
        _audio.volume = 0.35f;

        _maxHeight = GameObject.Find("Top Limit Pos").transform.position.y;
    }
    
    void Update ()
    {
        //For debug purposes.
        if (Input.GetKey(KeyCode.Return))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        if (GameMode.CurrentGameModeState == GameMode.GameModeState.Gameplay)
            HandleState();
    }

    private void HandleState()
    {
        switch(GameMode.CurrentGameplayState)
        {
            case GameMode.GameplayState.None:
                break;
            case GameMode.GameplayState.Paused:
                if (_audio.isPlaying)
                    _audio.Pause();
                break;
            case GameMode.GameplayState.Playing:
                if (!_audio.isPlaying)
                    _audio.UnPause();

                LimitRotation();

                if (ButtonHandler.ControlButton)
                    HandleInput();
                else
                    HandleSwipe();
                break;
            case GameMode.GameplayState.Resetting:
            case GameMode.GameplayState.Ending:
                transform.rotation = Quaternion.Lerp(transform.rotation, _startRotation, 2 * Time.deltaTime);
                if (Vector3.Distance(transform.position, _currentStartPosition) > 0.4f)
                {
                    float platformAddSpeed = 22;
                    transform.position +=
                        (_currentStartPosition - transform.position).normalized *
                        (GameMode.ReturnSpeed + platformAddSpeed) * Time.deltaTime;
                    PlayAudio();
                }
                else
                    transform.position = _currentStartPosition;
                break;
            case GameMode.GameplayState.Reset:
                break;
        }
    }

    /// <summary>
    /// Follows ball. This is for the platform to follow a teleported ball too.
    /// </summary>
    /// <param name="ballPosition">Set position of ball</param>
    public void GetBallPosition(Vector3 ballPosition)
    {
        _currentStartPosition = ballPosition;
    }

    private void HandleInput()
    {
        if (GetInputUp)
            RotatePlatform(GetInputUp);
        else if (GetInputDown)
            RotatePlatform(!GetInputDown);
        else
            _audio.Stop();
    }

    private void HandleSwipe()
    {
        float speed = 0;
        if (SwipeHandler.DownSwipe)
        {
            if (SwipeHandler.LeftSide)
                speed = _rotSpeed;
            else if (SwipeHandler.RightSide)
                speed = -_rotSpeed;
            if (SwipeHandler.LeftSide && SwipeHandler.RightSide)
                speed = 0;
            transform.Rotate(0, 0, speed);
            if (_canRotate && transform.position.y < _maxHeight)
                MovePlatform(false);
        }
        else if (SwipeHandler.UpSwipe)
        {
            if (SwipeHandler.LeftSide)
                speed = -_rotSpeed;
            else if (SwipeHandler.RightSide)
                speed = _rotSpeed;
            if (SwipeHandler.LeftSide && SwipeHandler.RightSide)
                speed = 0;
            transform.Rotate(0, 0, speed);
            if (_canRotate && transform.position.y > _startPosition.y)
                MovePlatform(false);
        }
    }

    private void RotatePlatform(bool up)
    {
        float speed = 0;
        if (up)
        {
            if (ClockwiseUp)
                speed = _rotSpeed;
            else if (CounterClockwiseUp)
                speed = -_rotSpeed;
            if (ClockwiseUp && CounterClockwiseUp)
                speed = 0;
            transform.Rotate(0, 0, speed);
            if (_canRotate && transform.position.y < _maxHeight)
                MovePlatform(true);
        }
        else
        {
            if (ClockwiseDown)
                speed = -_rotSpeed;
            else if (CounterClockwiseDown)
                speed = _rotSpeed;
            if (ClockwiseDown && CounterClockwiseDown)
                speed = 0;
            transform.Rotate(0, 0, speed);
            if (_canRotate && transform.position.y > _startPosition.y)
                MovePlatform(false);
        }

        if (_canRotate)
            PlayAudio();
    }

    private void MovePlatform(bool up)
    {
        Vector3 dir = up ? Vector3.up : Vector3.down;
        transform.position += dir * _moveSpeed;
    }

    private void LimitRotation()
    {
        float maxDegrees = 5;
        float minRotation = -maxDegrees * Mathf.Deg2Rad;
        float maxRotation = maxDegrees * Mathf.Deg2Rad;
        Quaternion currentRotation = transform.rotation;
        currentRotation.z = Mathf.Clamp(currentRotation.z, minRotation, maxRotation);
        transform.rotation = currentRotation;

        CanRotate(currentRotation, maxRotation, minRotation);
    }

    private void CanRotate(Quaternion currentRotation, float max, float min)
    {
        float offset = 0.01f;
        if (currentRotation.z > max - offset || currentRotation.z < min + offset)
            _canRotate = false;
        else
            _canRotate = true;
    }

    private void PlayAudio()
    {
        if (!_audio.isPlaying)
            _audio.Play();
    }

    private bool ClockwiseUp
    {
        get
        {
            return Input.GetKey(KeyCode.UpArrow) || ButtonHandler.UpRightButton;
        }
    }
    private bool ClockwiseDown
    {
        get
        {
            return Input.GetKey(KeyCode.DownArrow) || ButtonHandler.DownRightButton;
        }
    }
    private bool CounterClockwiseUp
    {
        get
        {
            return Input.GetKey(KeyCode.W) || ButtonHandler.UpLeftButton;
        }
    }
    private bool CounterClockwiseDown
    {
        get
        {
            return Input.GetKey(KeyCode.S) || ButtonHandler.DownLeftButton;
        }
    }

    private bool GetInputUp
    {
        get
        {
            return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ||
                ButtonHandler.UpLeftButton || ButtonHandler.UpRightButton;;
        }
    }
    private bool GetInputDown
    {
        get
        {
            return Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ||
                ButtonHandler.DownLeftButton || ButtonHandler.DownRightButton;
        }
    }

    public bool Ready
    {
        get
        {
            bool ready = transform.position == new Vector3(transform.position.x, _currentStartPosition.y, transform.position.z) && transform.rotation == _startRotation;
            if (ready)
                _audio.Stop();
            return ready;
        }
    }
}
