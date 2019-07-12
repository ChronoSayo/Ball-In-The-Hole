using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SimpleFileBrowser;

public class GameMode : MonoBehaviour
{
    private Gyroscope _gyroscope;
    private RectTransform _quitBar;
    private GameObject _helpText, _creditsText;
    private Vector2 _quitBarSize;
    private Ball _ballScript;
    private Wall _wallScript;
    private Platform _platformScript;
    private Scoreboard _scoreboardScript;
    private LadderTheme _ladderThemeScript;
    private TitleAnimation _titleAnimationScript;
    private float _holdQuitTick, _holdQuitTime;
    private bool _paused;
    private List<Renderer> _wallRenders;

    public static GameModeState CurrentGameModeState = GameModeState.Start;
    public static GameplayState CurrentGameplayState = GameplayState.End;
    public static PlayMode CurrentPlayMode = PlayMode.None;
    public static float LadderTimer = 1;
    public static float ReturnSpeed = 10;

    private static Vector3 _GRAVITY = new Vector3(0, -98.1f, 0);

    public enum GameModeState
    {
        None, Start, FileSearching, GradientMode, Info, SetupGameplay, Gameplay
    }
    
    public enum GameplayState
    {
        None, Playing, Paused, Resetting, Reset, Ending, End
    }

    /// <summary>
    /// Ladder: Goal changes level depending on points.
    /// Free: Each level has a goal.
    /// </summary>
    public enum PlayMode
    {
        None, Ladder, Free
    }

    void Start ()
    {
        _gyroscope = Input.gyro;
        _gyroscope.enabled = true;

        _quitBar = GameObject.Find("Bar").GetComponent<RectTransform>();
        _quitBar.gameObject.SetActive(false);
        _quitBarSize = _quitBar.localScale;

        _helpText = GameObject.Find("HelpText");
        _helpText.SetActive(false);
        _creditsText = GameObject.Find("CreditsText");
        _creditsText.SetActive(false);
        
        _ballScript = GameObject.Find("Ball").GetComponent<Ball>();
        _wallScript = GameObject.Find("Main Wall").GetComponent<Wall>();
        _platformScript = GameObject.Find("Player").GetComponent<Platform>();
        _scoreboardScript = GameObject.Find("Scoreboard").GetComponent<Scoreboard>();
        _ladderThemeScript = GameObject.Find("Ladder Theme").GetComponent<LadderTheme>();
        _titleAnimationScript = GameObject.Find("Title objects").GetComponent<TitleAnimation>();

        _paused = false;

        _holdQuitTick = 0;
        _holdQuitTime = 1.2f;

        Physics.gravity = _GRAVITY;

        _wallRenders = new List<Renderer>();
        foreach (Transform t in _wallScript.transform)
            _wallRenders.Add(t.GetComponent<Renderer>());

        FileBrowser.SetFilters(false, new FileBrowser.Filter("Images", ".jpg", ".png"));
        FileBrowser.AddQuickLink("Dumb Shit", "E:\\Pictures\\Dumb Shit");
    }

    void Update()
    {
        GameModeHandler();
    }

    private void GameModeHandler()
    {
        switch (CurrentGameModeState)
        {
            case GameModeState.None:
                break;
            case GameModeState.Start:
                StartMenuButtonPress();
                QuitButtonPress();
                break;
            case GameModeState.SetupGameplay:
                break;
            case GameModeState.FileSearching:
                CurrentGameModeState = FileBrowser.IsOpen ? GameModeState.FileSearching : GameModeState.Start;
                break;
            case GameModeState.GradientMode:
                SetGradient();
                StartMenuButtonPress();
                break;
            case GameModeState.Info:
                QuitButtonPress();
                break;
            case GameModeState.Gameplay:
                GameplayButtonPress();
                GameplayStateHandler();
                QuitButtonPress();
                break;
        }
    }

    private IEnumerator DelayGameplay()
    {
        yield return new WaitForSeconds(0.2f);
        CurrentGameModeState = GameModeState.Gameplay;
        CurrentGameplayState = GameplayState.Playing;
        _ladderThemeScript.PlaySong(true);
        _wallScript.SetupTeleport();
        _wallScript.SetupGoal();
    }

    private void GameplayStateHandler()
    {
        switch (CurrentGameplayState)
        {
            case GameplayState.None:
                break;
            case GameplayState.Playing:
                if (_ballScript.EnteredHole || _ballScript.transform.position.y < -10)
                    CurrentGameplayState = GameplayState.Resetting;
                break;
            case GameplayState.Paused:
                break;
            case GameplayState.Resetting:
                if (_platformScript.Ready && _ballScript.Ready)
                    CurrentGameplayState = GameplayState.Reset;
                break;
            case GameplayState.Reset:
                CurrentGameplayState = GameplayState.Playing;
                break;
            case GameplayState.Ending:
                if (_platformScript.Ready && _ballScript.Ready)
                    CurrentGameplayState = GameplayState.End;
                break;
        }
    }

    private void SetGradient()
    {
        if (SystemInfo.supportsGyroscope)
        {
            Quaternion q = _gyroscope.attitude;
            Color c = _wallRenders[0].material.color;
            foreach (Renderer r in _wallRenders)
                r.material.color = Color.Lerp(new Color(q.x, q.y, q.z), c, Mathf.SmoothStep(0.0f, 1.0f, Time.deltaTime / 10000));
            //Debug.Log("Rate UnBiased: " + _gyroscope.rotationRateUnbiased);
            //Debug.Log("Attitude: " + _gyroscope.attitude);
        }
        else
        {
            Vector3 v = Input.mousePosition;
            Color c = _wallRenders[0].material.color;
            int scale = 1000;
            foreach (Renderer r in _wallRenders)
                r.material.color = new Color(v.x / scale, v.y / scale, (v.x + v.y) / scale);
        }
    }

    private void GameplayButtonPress()
    {
        if(ButtonHandler.PauseButton != _paused)
            PauseGame();
        QuitButtonPress();
    }

    private void StartMenuButtonPress()
    {
        if (ButtonHandler.StartFreeButton)
        {
            ButtonHandler.StartFreeButton = false;
            RunStart(true);
        }
        else if (ButtonHandler.StartLadderButton)
        {
            ButtonHandler.StartLadderButton = false;
            RunStart(false);
        }
        else if(ButtonHandler.BackgroundButton)
        {
            if(!FileBrowser.IsOpen)
            {
                CurrentGameModeState = GameModeState.FileSearching;
                FileBrowser.ShowLoadDialog((path) => { StartCoroutine(WaitForDownload(path)); }, 
                    null, false, FileBrowser.Result);
            }
            ButtonHandler.BackgroundButton = false;
        }
        else if(ButtonHandler.HelpButton)
        {
            CurrentGameModeState = GameModeState.Info;
            ButtonHandler.HelpButton = true;
            _helpText.gameObject.SetActive(true);
            _scoreboardScript.EnableScoreboardGUI(false);
        }
        else if (ButtonHandler.CreditsButton)
        {
            CurrentGameModeState = GameModeState.Info;
            ButtonHandler.CreditsButton = true;
            _creditsText.SetActive(true);
            _scoreboardScript.EnableScoreboardGUI(false);
        }

        if (ButtonHandler.GradientButton)
            CurrentGameModeState = GameModeState.GradientMode;
        else if(CurrentGameModeState == GameModeState.GradientMode && !ButtonHandler.GradientButton)
            CurrentGameModeState = GameModeState.Start;
    }

    private void RunStart(bool freeMode)
    {
        CurrentPlayMode = freeMode ? PlayMode.Free : PlayMode.Ladder;
        CurrentGameModeState = GameModeState.SetupGameplay;
        StartCoroutine(DelayGameplay());

        _titleAnimationScript.Enable(false);
    }

    private IEnumerator WaitForDownload(string path)
    {
        string fixedPath = "file:///" + System.Uri.EscapeUriString(path.Replace('\\', '/'));
        using (WWW www = new WWW(fixedPath))
        {
            yield return www;
            foreach(Renderer r in _wallRenders)
                r.material.mainTexture = www.texture;
        }
    }

    private void QuitButtonPress()
    {
        if (ButtonHandler.QuitButton)
            HoldQuitTimer();
        if (_holdQuitTick > 0 && !ButtonHandler.QuitButton)
        {
            _holdQuitTick = 0;
            _quitBar.localScale = _quitBarSize;
            _quitBar.gameObject.SetActive(false);
        }
    }

    private void PauseGame()
    {
        bool paused = ButtonHandler.PauseButton;

        Time.timeScale = paused ? 0 : 1;
        CurrentGameplayState = paused ? GameplayState.Paused : GameplayState.Playing;

        _ladderThemeScript.PauseSong(!paused);

        _paused = paused;

        _titleAnimationScript.Enable(paused);
    }

    private void HoldQuitTimer()
    {
        if (!_quitBar.gameObject.activeSelf)
            _quitBar.gameObject.SetActive(true);

        _holdQuitTick += Time.unscaledDeltaTime;
        if (_holdQuitTick >= _holdQuitTime)
        {
            _holdQuitTick = 0;

            ButtonHandler.QuitButton = false;

            _quitBar.localScale = _quitBarSize;
            _quitBar.gameObject.SetActive(false);

            if (CurrentGameModeState == GameModeState.Gameplay)
            {
                if (Time.timeScale != 1)
                    Time.timeScale = 1;

                CurrentGameModeState = GameModeState.Start;
                CurrentPlayMode = PlayMode.None;

                _scoreboardScript.ResetStats();

                ButtonHandler.DisableAllButtons();

                _titleAnimationScript.Enable(true);

                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            else if (CurrentGameModeState == GameModeState.Info)
            {
                CurrentGameModeState = GameModeState.Start;
                if (_helpText.activeSelf)
                {
                    ButtonHandler.HelpButton = false;
                    _helpText.SetActive(false);
                }
                else if (_creditsText.activeSelf)
                {
                    ButtonHandler.CreditsButton = false;
                    _creditsText.SetActive(false);
                }
                _scoreboardScript.EnableScoreboardGUI(true);
            }
            else
                Application.Quit();
        }
        else
        {
            float scale = (Screen.width / _holdQuitTime) * Time.unscaledDeltaTime;
            _quitBar.localScale += new Vector3(scale, 0);
        }
    }
}
