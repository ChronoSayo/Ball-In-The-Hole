using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public Transform holesLeader;
    
    private Dictionary<int, List<Transform>> _levelHoles;

    private const int _LEVELS = 5;

    void Start()
    {
        Holes = new List<Transform>();
        Goals = new List<Transform>();
        Teleports = new List<Transform>();
        SetHolesFromLeader();
        
        for (int i = 0; i < _LEVELS; i++)
            Holes.AddRange(_levelHoles[i]);
    }

    private void SetHolesFromLeader()
    {
        _levelHoles = new Dictionary<int, List<Transform>>();
        for (int i = 0; i < _LEVELS; i++)
            _levelHoles.Add(i, new List<Transform>());
        foreach (Transform t in holesLeader)
        {
            int listNum = -1;
            if (!int.TryParse(t.name[t.name.Length - 1].ToString(), out listNum))
                continue;
            foreach (Transform tChild in t)
            {
                if (tChild.tag == "Hole")
                    _levelHoles[listNum - 1].Add(tChild);
            }
        }
    }

    public void SetupGoal()
    {
        if (GameMode.CurrentPlayMode == GameMode.PlayMode.Free)
            for (int i = 0; i < _LEVELS; i++)
                PickRandomGoalFreeMode(i);
        else if (GameMode.CurrentPlayMode == GameMode.PlayMode.Ladder)
            PickRandomGoalLadderMode(0);

        SetDefaultLight();
    }

    public void SetupTeleport()
    {
        List<Transform> allTeleportHoles = new List<Transform>();
        for (int i = 0; i < _levelHoles.Count; i++)
        {
            foreach (Transform t in _levelHoles[i])
            {
                if (t.name.EndsWith("T"))
                    allTeleportHoles.Add(t);
            }
        }

        Teleports.Clear();
        Teleports.Add(allTeleportHoles[Random.Range(0, allTeleportHoles.Count)]);
        allTeleportHoles.Remove(Teleports[0]);
        Teleports.Add(allTeleportHoles[Random.Range(0, allTeleportHoles.Count)]);
    }

    void Update ()
    {
        switch (GameMode.CurrentGameplayState)
        {
            case GameMode.GameplayState.None:
                break;
            case GameMode.GameplayState.Playing:
                FlashHoleLights(Goals, 1);
                FlashHoleLights(Teleports, 2);
                break;
            case GameMode.GameplayState.Resetting:
                foreach (Transform t in Holes)
                {
                    t.GetComponent<Light>().color = GetRandomColor();
                    t.GetComponent<Light>().intensity = Random.Range(0, 2) == 0 ? 0 : 1;
                }
                break;
            case GameMode.GameplayState.Reset:
                SetDefaultLight();
                break;
        }
    }

    private void FlashHoleLights(List<Transform> lights, float speed)
    {
        float flash = Mathf.PingPong(Time.time, speed);
        foreach (Transform t in lights)
            t.GetComponent<Light>().intensity = flash;
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

        foreach(Transform t in Goals)
        {
            t.GetComponent<Light>().color = Color.green;
            t.GetComponent<Light>().intensity = 0;
        }

        foreach (Transform t in Teleports)
        {
            t.GetComponent<Light>().color = Color.blue;
            t.GetComponent<Light>().intensity = 0;
        }
    }

    public void PickRandomGoalFreeMode(int level)
    {
        List<Transform> goalHoles = new List<Transform>();
        goalHoles = _levelHoles[level].FindAll(x => x.name.EndsWith("G"));
        Transform goalHole = goalHoles[Random.Range(0, goalHoles.Count)];
        if (Goals.Count < level + 1)
            Goals.Add(goalHole);
        else
        {
            Goals[level].GetComponent<Light>().intensity = 0;
            Goals[level] = goalHole;
        }
    }

    public void PickRandomGoalLadderMode(int points)
    {
        const int _NEW_LEVEL_INT = 115;
        const float _MULTIPLIER = 2.5f;
        int newLevel = 0;

        for(int i = 1; i < _LEVELS; i++)
        {
            if (points > _NEW_LEVEL_INT * (_MULTIPLIER * i))
                newLevel = i;
            else
                break;
        }

        List<Transform> newGoals = _levelHoles[newLevel].FindAll(x => x == x.name.EndsWith("G"));
        Transform newGoal = newGoals[Random.Range(0, newGoals.Count)];
        if (Goals.Count == 0)
            Goals.Add(newGoal);
        else
        {
            Goals[0].GetComponent<Light>().intensity = 0;
            Goals[0] = newGoal;
        }
    }

    /// <summary>
    /// Returns -1 if the ball entered no goal hole.
    /// </summary>
    /// <param Transform="hole"></param>
    /// <returns></returns>
    public int GetGoalIndex(Transform hole)
    {
        if (!Goals.Contains(hole))
            return 0;

        return int.Parse(hole.parent.name[hole.parent.name.Length - 1].ToString()); ;
    }

    public List<Transform> Holes { get; private set; }
    public List<Transform> Goals { get; private set; }
    public List<Transform> Teleports { get; private set; }
}
