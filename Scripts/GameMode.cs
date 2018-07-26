using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using SimpleFileBrowser;

public class GameMode : MonoBehaviour
{
    private GameObject _titleObject;
    private RectTransform _quitBar;
    private Vector3 _titleBallPosition;
    private Vector2 _quitBarSize;
    private Ball _ballScript;
    private Wall _wallScript;
    private Platform _platformScript;
    private Scoreboard _scoreboardScript;
    private LadderTheme _ladderThemeScript;
    private float _holdQuitTick, _holdQuitTime;
    private bool _paused;

    public static GameModeState CurrentGameModeState = GameModeState.Start;
    public static GameplayState CurrentGameplayState = GameplayState.End;
    public static PlayMode CurrentPlayMode = PlayMode.None;
    public static float LadderTimer = 1;
    public static float ReturnSpeed = 6;

    private static Vector3 _GRAVITY = new Vector3(0, -98.1f, 0);

    public enum GameModeState
    {
        None, Start, FileSearching, SetupGameplay, Gameplay
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
        _titleObject = GameObject.Find("Title objects");
        _titleBallPosition = _titleObject.transform.GetChild(1).position;

        _quitBar = GameObject.Find("Bar").GetComponent<RectTransform>();
        _quitBar.gameObject.SetActive(false);
        _quitBarSize = _quitBar.localScale;

        _ballScript = GameObject.Find("Ball").GetComponent<Ball>();
        _wallScript = GameObject.Find("Wall").GetComponent<Wall>();
        _platformScript = GameObject.Find("Player").GetComponent<Platform>();
        _scoreboardScript = GameObject.Find("Scoreboard").GetComponent<Scoreboard>();
        _ladderThemeScript = GameObject.Find("Ladder Theme").GetComponent<LadderTheme>();

        _paused = false;

        _holdQuitTick = 0;
        _holdQuitTime = 1.2f;

        Physics.gravity = _GRAVITY;
        
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
    }

    private void EnableTitleObjects(bool enable)
    {
        _titleObject.SetActive(enable);
        if(enable)
        {
            foreach (Transform t in _titleObject.transform)
            {
                if (t.name.Contains("Ball"))
                    t.position = _titleBallPosition;
                else
                    t.GetComponent<Animator>().Play("Title Head");
            }

        }
    }

    private void RunStart(bool freeMode)
    {
        CurrentPlayMode = freeMode ? PlayMode.Free : PlayMode.Ladder;
        CurrentGameModeState = GameModeState.SetupGameplay;
        StartCoroutine(DelayGameplay());

        EnableTitleObjects(false);
    }

    private IEnumerator WaitForDownload(string path)
    {
        string fixedPath = "file:///" + System.Uri.EscapeUriString(path.Replace('\\', '/'));
        using (WWW www = new WWW(fixedPath))
        {
            yield return www;
            Renderer renderer = _wallScript.transform.GetComponent<Renderer>();
            renderer.material.mainTexture = www.texture;
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

        EnableTitleObjects(paused);
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

                EnableTitleObjects(true);

                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
