using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scoreboard : MonoBehaviour
{
    private TextMesh _score, _combo, _timer, _bestScore;
    private int _comboCount, _time;
    private float _bestCurrentScore, _comboIncrease, _currentSeconds;

    private const int _BASE_POINTS = 50;

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
        }

        _bestCurrentScore = 0;

        ResetStats();
    }

    private void Update()
    {
        if (GameMode.CurrentGameModeState == GameMode.GameModeState.Gameplay)
            UpdateTimerText();
    }

    public void SetScore(int level)
    {
        int i = level + 1;
        int bonus = 1;
        if (level > 1)
            bonus = 2;
        int addedBonus = i * bonus;
        int basePoints = addedBonus * _BASE_POINTS;
        //float comboPoints = _comboCount + (i * _comboIncrease);

        //Current seconds minus 10 minutes.
        int minMinutes = 2;
        float timeBonus = (60 * minMinutes) - _currentSeconds;
        float timePoints = timeBonus * ((float)addedBonus / 100);

        //CurrentScore += basePoints + comboPoints;
        CurrentScore += basePoints + timePoints;
        if (level == -1)
        {
            if (CurrentScore > _bestCurrentScore)
            {
                _bestCurrentScore = CurrentScore;
                UpdateText(_bestScore, (int)_bestCurrentScore);
            }
            CurrentScore = 0;
        }

        ResetTimer();

        UpdateText(_score, (int)CurrentScore);//Convert to int to show whole numbers on GUI.
    }

    public void SetCombo(bool isCombo)
    {
        if (!isCombo)
        {
            _comboCount = 0;
            UpdateText(_combo, _comboCount);
        }
        else
        {
            UpdateText(_combo, _comboCount);
            _comboCount++;
        }
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

        _currentSeconds += Time.deltaTime;

        string time = (_currentSeconds - Time.deltaTime).ToString();
        time = string.Format("{0:0}:{1:00}", Mathf.Floor(_currentSeconds / 60), _currentSeconds % 60);
        _timer.text = _timer.text.Replace(split[1], time);
    }

    public void ResetStats()
    {
        ResetTimer();

        _comboCount = 0;
        _comboIncrease = 0.1f;

        CurrentScore = 0;
        UpdateText(_score, (int)CurrentScore);//Convert to int to show whole numbers on GUI.
        //UpdateText(_combo, 0);
    }

    public void ResetTimer()
    {
        _currentSeconds = 0;
        UpdateTimerText();
    }

    public bool TimeOut
    {
        get { return _currentSeconds <= 0; }
    }

    public float CurrentScore { get; private set; }
}
