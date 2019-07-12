using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scoreboard : MonoBehaviour
{
    private TextMesh _score, _timer, _bestScore, _legacy;
    private int _time, _bonusTimeStart, _storedLegacy, _legacyCounter;
    private float _bestCurrentScore, _currentSeconds;

    private const int _BASE_POINTS = 1;

    void Start ()
    {
        foreach(Transform t in transform)
        {
            if (t.name == "ScoreText")
                _score = t.GetComponent<TextMesh>();
            //if (t.name == "ComboText")
            //    _combo = t.GetComponent<TextMesh>();
            if (t.name == "TimerText")
                _timer = t.GetComponent<TextMesh>();
            if (t.name == "BestScoreText")
                _bestScore = t.GetComponent<TextMesh>();
            if (t.name == "LegacyText")
                _legacy = t.GetComponent<TextMesh>();
        }

        _storedLegacy = 0;
        _legacyCounter = 1;
        _bestCurrentScore = 0;
        _bonusTimeStart = 61;

        ResetStats();
    }

    private void Update()
    {
        if (GameMode.CurrentGameModeState == GameMode.GameModeState.Gameplay 
            && GameMode.CurrentGameplayState == GameMode.GameplayState.Playing 
            && _currentSeconds >= 1)
            UpdateTimerText();

        if (Input.GetKeyDown(KeyCode.Space))
            CurrentScore += 100;
    }

    public void SetScore(int level)
    {
        if (level == -1)
        {
            if (CurrentScore > _bestCurrentScore)
            {
                _bestCurrentScore = CurrentScore;
                UpdateText(_bestScore, (int)_bestCurrentScore);
                _storedLegacy++;
            }
            CurrentScore = 0;
            _legacyCounter += _storedLegacy;
            UpdateText(_legacy, _legacyCounter);
            _storedLegacy = 0;
        }
        else
        {
            int i = level + 1;
            int bonus = 1;
            if (level > 1)
                bonus = 2;
            if (level == 4)
                _storedLegacy++;
            bonus *= _legacyCounter;
            int addedBonus = i * bonus;
            int basePoints = addedBonus * _BASE_POINTS;
            //float comboPoints = _comboCount + (i * _comboIncrease);

            float timeBonus = _bonusTimeStart + _currentSeconds;
            if (_currentSeconds < 1)
                timeBonus = 0;

            //CurrentScore += basePoints + comboPoints;
            CurrentScore += basePoints + timeBonus;
        }

        ResetTimer();

        UpdateText(_score, (int)CurrentScore);//Convert to int to show whole numbers on GUI.
    }

    private void UpdateText(TextMesh text, float num)
    {
        string[] split = text.text.Split(' ');
        string numText = split[1];
        numText = num.ToString();
        text.text = text.text.Replace(split[1], numText);
    }

    private void UpdateTimerText()
    {
        string[] split = _timer.text.Split(' ');

        _currentSeconds -= Time.deltaTime;

        string time = (_currentSeconds - Time.deltaTime).ToString();
        time = string.Format("{0:0}:{1:00}", Mathf.Floor(_currentSeconds / 60), Mathf.Floor(_currentSeconds % 60));
        _timer.text = _timer.text.Replace(split[1], time);
    }

    public void ResetStats()
    {
        ResetTimer();

        CurrentScore = 0;
        UpdateText(_score, (int)CurrentScore);//Convert to int to show whole numbers on GUI.
    }

    public void ResetTimer()
    {
        _currentSeconds = _bonusTimeStart;
        UpdateTimerText();
    }

    public void EnableScoreboardGUI(bool enable)
    {
        _score.gameObject.SetActive(enable);
        _timer.gameObject.SetActive(enable);
        _bestScore.gameObject.SetActive(enable);
        _legacy.gameObject.SetActive(enable);
    }

    public bool TimeOut
    {
        get { return _currentSeconds <= 0; }
    }

    public float CurrentScore { get; private set; }
}
