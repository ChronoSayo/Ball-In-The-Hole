using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Ball : MonoBehaviour
{
    private Wall _wallScript;
    private Platform _platformScript;
    private LadderTheme _ladderThemeScript;
    private Scoreboard _scoreboardScript;
    private Vector3 _holeEntered, _startPosition;
    private Rigidbody _rigidbody;
    private float _lastSpeed;
    private bool _isGoal;
    private State _state;

    private enum State
    {
        None, Rolling, EnteringHole, EnteredHole
    }

    void Start ()
    {
        _rigidbody = transform.GetComponent<Rigidbody>();

        _wallScript = GameObject.Find("Wall").GetComponent<Wall>();
        _platformScript = GameObject.Find("Player").GetComponent<Platform>();
        _ladderThemeScript = GameObject.Find("Ladder Theme").GetComponent<LadderTheme>();
        _scoreboardScript = GameObject.Find("Scoreboard").GetComponent<Scoreboard>();

        _startPosition = GameObject.Find("StartHole").transform.position;

        _isGoal = false;

        _state = State.Rolling;
    }
    
    void Update ()
    {
        switch(GameMode.CurrentGameplayState)
        {
            case GameMode.GameplayState.None:
                break;
            case GameMode.GameplayState.Playing:
                PlayingState();
                break;
            case GameMode.GameplayState.Resetting:
            case GameMode.GameplayState.Ending:
                if (_platformScript.Ready)
                    ResetState();
                break;
            case GameMode.GameplayState.Reset:
                break;
        }
    }

    private void PlayingState()
    {
        switch (_state)
        {
            case State.None:
                break;
            case State.Rolling:
                Rolling();
                break;
            case State.EnteringHole:
                EnterHole();
                break;
            case State.EnteredHole:
                break;
        }
    }

    private void Rolling()
    {
        float distOffset = 1.5f, heightOffset = 0.5f;
        foreach (Transform t in _wallScript.Holes)
        {
            Vector3 v = t.position;
            if (Vector3.Distance(transform.position, v) < distOffset &&
                transform.position.y >= v.y - heightOffset &&
                transform.position.y <= v.y + heightOffset)
                SetupEnterHole(t);
        }
    }

    private void ResetState()
    {
        _isGoal = false;
        transform.GetComponent<SphereCollider>().enabled = true;
        transform.GetComponent<Renderer>().enabled = true;
        transform.position = _startPosition;
        _rigidbody.useGravity = true;
        _rigidbody.velocity = Vector3.zero;
        _state = State.Rolling;
    }

    private void SetupEnterHole(Transform enteredHole)
    {
        MoveIntoHole(enteredHole);
        CalculateScore(enteredHole);

        _state = State.EnteringHole;
    }

    private void MoveIntoHole(Transform enteredHole)
    {
        transform.GetComponent<SphereCollider>().enabled = false;
        _rigidbody.useGravity = false;
        StartCoroutine(Freeze());
        _holeEntered = enteredHole.position - (Vector3.back * 2);

        float currentSpeed = _rigidbody.velocity.magnitude;
        float min = 6;
        if (currentSpeed > min) _lastSpeed = currentSpeed;
        else _lastSpeed = min;
        _rigidbody.velocity = Vector3.zero;
    }

    private void CalculateScore(Transform enteredHole)
    {
        int goalIndex = _wallScript.GetGoalIndex(enteredHole.GetInstanceID());
        _isGoal = goalIndex == -1 ? false : true;

        _scoreboardScript.SetScore(goalIndex);
        //_scoreboardScript.SetCombo(_isGoal);

        if (_isGoal)
            if(GameMode.CurrentPlayMode == GameMode.PlayMode.Free)
                _wallScript.PickRandomGoalFreeMode(goalIndex);

        if (GameMode.CurrentPlayMode == GameMode.PlayMode.Ladder)
            _wallScript.PickRandomGoalLadderMode(((int)_scoreboardScript.CurrentScore));

        _ladderThemeScript.PlayJingle(_isGoal);
    }

    private void EnterHole()
    {
        transform.position += (_holeEntered - transform.position).normalized * _lastSpeed * Time.deltaTime;
        if (Vector3.Distance(transform.position, _holeEntered) < 0.3f)
        {
            _state = State.EnteredHole;
            _ladderThemeScript.SetToDefault();
            transform.GetComponent<Renderer>().enabled = false;
        }
    }

    private IEnumerator Freeze()
    {
        yield return new WaitForSeconds(1.5f);
        _rigidbody.velocity = Vector3.zero;
    }

    public bool EnteredHole
    {
        get { return _state == State.EnteredHole; }
    }

    public bool Ready
    {
        get { return Vector3.Distance(transform.position, _startPosition) < 0.4f; }
    }
}
