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
    private Vector3 _startPosition;
    private AudioSource _audio;
    private float _moveSpeed, _rotSpeed;
    private bool _canRotate;
    private List<GameObject> _buttons;
    private List<ButtonPressed> _pressedButtons;

    private const float _MAX_HEIGHT = 46;

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

        bool isAndroid = Application.platform == RuntimePlatform.Android;
        _moveSpeed = moveSpeed * (isAndroid ? 2 : 1);
        _rotSpeed = rotSpeed * (isAndroid ? 2 : 1);

        _audio = gameObject.AddComponent<AudioSource>();
        _audio.clip = clip;
        _audio.loop = true;
        _audio.volume = 0.35f;
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
            case GameMode.GameplayState.Playing:
                LimitRotation();

                if (ButtonHandler.ControlButton)
                    HandleInput();
                else
                    HandleSwipe();
                break;
            case GameMode.GameplayState.Resetting:
            case GameMode.GameplayState.Ending:
                transform.rotation = Quaternion.Lerp(transform.rotation, _startRotation, 2 * Time.deltaTime);
                if (Vector3.Distance(transform.position, _startPosition) > 0.4f)
                {
                    transform.position +=
                        (_startPosition - transform.position).normalized *
                        (GameMode.ReturnSpeed + 1.5f) * Time.deltaTime;
                    PlayAudio();
                }
                else
                    transform.position = _startPosition;
                break;
            case GameMode.GameplayState.Reset:
                break;
        }
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
            if (_canRotate && transform.position.y < _MAX_HEIGHT)
                transform.position += Vector3.up * _moveSpeed;
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
                transform.position += Vector3.down * _moveSpeed;
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
            if (_canRotate && transform.position.y < _MAX_HEIGHT)
                transform.position += Vector3.up * _moveSpeed;
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
                transform.position += Vector3.down * _moveSpeed;
        }

        if (_canRotate)
            PlayAudio();
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
            bool ready = transform.position == _startPosition && transform.rotation == _startRotation;
            if (ready)
                _audio.Stop();
            return ready;
        }
    }
}
