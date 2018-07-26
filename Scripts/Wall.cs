using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public List<Transform> levelHoles1;
    public List<Transform> levelHoles2;
    public List<Transform> levelHoles3;
    public List<Transform> levelHoles4;
    public List<Transform> levelHoles5;

    private List<List<Transform>> _allLevelHoles;

    private const int _LEVELS = 5;

    void Start ()
    {
        Holes = new List<Transform>();
        Goals = new List<Transform>();

        //Each piece of wall has a set number of holes. They are placed in lists levelHoles1/2/3.
        //Those lists are added into a 2D list allLevelHoles.
        //WALLS is the amount of piece of walls in the scene.
        //_holes is a list of all holes, uncategorized.
        //When adding more piece of walls, increase number of WALLS and add another case.
        _allLevelHoles = new List<List<Transform>>();
        for(int i = 0; i < _LEVELS; i++)
        {
            _allLevelHoles.Add(new List<Transform>());
    
            int length = 0;
            List<Transform> levelHoles = new List<Transform>();
            switch(i)
            {
                case 0:
                    length = levelHoles1.Count;
                    levelHoles = levelHoles1;
                    break;
                case 1:
                    length = levelHoles2.Count;
                    levelHoles = levelHoles2;
                    break;
                case 2:
                    length = levelHoles3.Count;
                    levelHoles = levelHoles3;
                    break;
                case 3:
                    length = levelHoles4.Count;
                    levelHoles = levelHoles4;
                    break;
                case 4:
                    length = levelHoles5.Count;
                    levelHoles = levelHoles5;
                    break;
            }
            for(int j = 0; j < length; j++)
                _allLevelHoles[i].Add(levelHoles[j]);
        }

        for (int i = 0; i < _LEVELS; i++)
            Holes.AddRange(_allLevelHoles[i]);
    }

    public void SetupGoal()
    {
        if (GameMode.CurrentPlayMode == GameMode.PlayMode.Free)
            for (int i = 0; i < _LEVELS; i++)
                Goals.Add(_allLevelHoles[i][Random.Range(0, _allLevelHoles[i].Count)]);
        else if (GameMode.CurrentPlayMode == GameMode.PlayMode.Ladder)
            Goals.Add(_allLevelHoles[0][Random.Range(0, _allLevelHoles[0].Count)]);

        SetDefaultLight();
    }
    
    void Update ()
    {
        float speed = 0;
        switch (GameMode.CurrentGameplayState)
        {
            case GameMode.GameplayState.None:
                break;
            case GameMode.GameplayState.Playing:
                speed = 1;
                float flash = Mathf.PingPong(Time.time, speed);
                foreach (Transform t in Goals)
                    t.GetComponent<Light>().intensity = flash;
                break;
            case GameMode.GameplayState.Resetting:
                speed = 1;
                foreach (Transform t in Holes)
                {
                    t.GetComponent<Light>().color = GetRandomColor();
                    //t.GetComponent<Light>().intensity = Mathf.PingPong(Time.time * 2, speed);
                    t.GetComponent<Light>().intensity = Random.Range(0, 2) == 0 ? 0 : 1;
                }
                break;
            case GameMode.GameplayState.Reset:
                SetDefaultLight();
                break;
        }
    }

    private Color GetRandomColor()
    {
        return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
    }

    private void SetDefaultLight()
    {
        foreach (Transform t in Holes)
        {
            t.GetComponent<Light>().intensity = 0;
            t.GetComponent<Light>().color = Color.black;
        }

        Color color = Color.green;

        foreach(Transform t in Goals)
        {
            t.GetComponent<Light>().color = color;
            t.GetComponent<Light>().intensity = 0;
        }
    }

    public void PickRandomGoalFreeMode(int level)
    {
        Transform newGoal = null;
        switch(level)
        {
            case 0:
                newGoal = levelHoles1[Random.Range(0, levelHoles1.Count)];
                break;
            case 1:
                newGoal = levelHoles2[Random.Range(0, levelHoles2.Count)];
                break;
            case 2:
                newGoal = levelHoles3[Random.Range(0, levelHoles3.Count)];
                break;
            case 3:
                newGoal = levelHoles4[Random.Range(0, levelHoles4.Count)];
                break;
            case 4:
                newGoal = levelHoles5[Random.Range(0, levelHoles5.Count)];
                break;
        }
        Goals[level].GetComponent<Light>().intensity = 0;
        Goals[level] = newGoal;
    }

    public void PickRandomGoalLadderMode(int points)
    {
        const int _NEW_LEVEL_INT = 200;
        Transform newGoal = null;

        if (points > _NEW_LEVEL_INT * 2.5f)
            newGoal = levelHoles3[Random.Range(0, levelHoles3.Count)];
        else if (points > _NEW_LEVEL_INT)
            newGoal = levelHoles2[Random.Range(0, levelHoles2.Count)];
        else
            newGoal = levelHoles1[Random.Range(0, levelHoles1.Count)];

        Goals[0].GetComponent<Light>().intensity = 0;
        Goals[0] = newGoal;
    }

    /// <summary>
    /// Returns -1 if the ball entered no goal hole.
    /// </summary>
    /// <param name="instanceID"></param>
    /// <returns></returns>
    public int GetGoalIndex(int instanceID)
    {
        int goal = -1;
        for(int i = 0; i < Goals.Count; i++)
        {
            if(Goals[i].GetInstanceID() == instanceID)
            {
                goal = i;
                break;
            }
        }
        return goal;
    }

    public List<Transform> Holes { get; private set; }
    public List<Transform> Goals { get; private set; }
}
