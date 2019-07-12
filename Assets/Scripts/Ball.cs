using System.Collections;
using UnityEngine;


public class Ball : MonoBehaviour
{
    private Wall _wallScript;
    private Platform _platformScript;
    private GameCamera _gameCameraScript;
    private LadderTheme _ladderThemeScript;
    private Scoreboard _scoreboardScript;
    private HoleSoundEffects _holeFX;
    private Vector3 _holeEntered, _startPosition, _currentStartPosition;
    private Rigidbody _rigidbody;
    private float _lastSpeed;
    private float _startOffsetFromBall;
    private bool _isGoal;
    private State _state;

    private enum State
    {
        None, Rolling, EnteringHole, EnteredHole
    }

    void Start ()
    {
        _rigidbody = transform.GetComponent<Rigidbody>();

        _wallScript = GameObject.Find("Main Wall").GetComponent<Wall>();
        _platformScript = GameObject.Find("Player").GetComponent<Platform>();
        _ladderThemeScript = GameObject.Find("Ladder Theme").GetComponent<LadderTheme>();
        _scoreboardScript = GameObject.Find("Scoreboard").GetComponent<Scoreboard>();
        _gameCameraScript = GameObject.Find("Main Camera").GetComponent<GameCamera>();

        _startPosition = GameObject.Find("StartHole").transform.position;
        
        _isGoal = false;
        Teleporting = false;

        _startOffsetFromBall = transform.position.y - _platformScript.transform.position.y;

        _state = State.Rolling;

        _holeFX = GameObject.Find("Hole Sound Effects").GetComponent<HoleSoundEffects>();
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
                if(!Teleporting)
                    DetectHole();
                break;
            case State.EnteringHole:
                EnterHole();
                break;
            case State.EnteredHole:
                break;
        }
    }

    private void DetectHole()
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
        transform.position = _currentStartPosition;
        _rigidbody.useGravity = true;
        _rigidbody.velocity = Vector3.zero;
        _state = State.Rolling;
        StartCoroutine(DelayHoleEnterable());
    }

    private void SetupEnterHole(Transform enteredHole)
    {
        MoveIntoHole(enteredHole);
        Teleporting = _wallScript.Teleports.Contains(enteredHole);
        if (Teleporting)
            Teleport(enteredHole);
        else
        {
            _wallScript.SetupTeleport();

            CalculateScore(enteredHole);
            _currentStartPosition = _startPosition;

        }

        Vector3 platformPosition = _platformScript.transform.position;
        float platformY = _currentStartPosition.y - (_startOffsetFromBall * (Teleporting ? 1.5f : 1));
        _platformScript.GetBallPosition(new Vector3(platformPosition.x, platformY, platformPosition.z));
        _gameCameraScript.GetBallPosition(_currentStartPosition);

        _state = State.EnteringHole;

        if (Teleporting)
            return;

        _ladderThemeScript.PlayJingle(_isGoal);

        if (_isGoal)
            _holeFX.PlayCheers();
        else
            _holeFX.PlayBoos();
    }

    private IEnumerator DelayHoleEnterable()
    {
        yield return new WaitForSeconds(0.3f);
        Teleporting = false;
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

    private void Teleport(Transform enteredHole)
    {
        if (enteredHole == _wallScript.Teleports[0])
            _currentStartPosition = _wallScript.Teleports[1].position;
        else
            _currentStartPosition = _wallScript.Teleports[0].position;

        _wallScript.SetupTeleport();
    }

    private void CalculateScore(Transform enteredHole)
    {
        int goalIndex = _wallScript.GetGoalIndex(enteredHole);
        goalIndex--;
        _isGoal = goalIndex == -1 ? false : true;
        
        _scoreboardScript.SetScore(goalIndex);

        if (_isGoal && GameMode.CurrentPlayMode == GameMode.PlayMode.Free)
            _wallScript.PickRandomGoalFreeMode(goalIndex);

        if (GameMode.CurrentPlayMode == GameMode.PlayMode.Ladder)
            _wallScript.PickRandomGoalLadderMode(((int)_scoreboardScript.CurrentScore));
    }

    private void EnterHole()
    {
        transform.position += (_holeEntered - transform.position).normalized * _lastSpeed * Time.deltaTime;
        if (Vector3.Distance(transform.position, _holeEntered) < 0.3f)
        {
            _state = State.EnteredHole;
            transform.GetComponent<Renderer>().enabled = false;
        }
        _ladderThemeScript.SetToDefault();
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
    public bool EnteringHole
    {
        get { return _state == State.EnteringHole; }
    }

    public bool Ready
    {
        get { return Vector3.Distance(transform.position, _currentStartPosition) < 0.4f; }
    }

    public bool Teleporting { get; private set; }
}
