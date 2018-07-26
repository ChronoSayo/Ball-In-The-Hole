using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderThemeTrigger : MonoBehaviour
{
    private LadderTheme _ladderThemeScript;
    private int i;

    private void Start()
    {
        _ladderThemeScript = GameObject.Find("Ladder Theme").GetComponent<LadderTheme>();
        i = int.Parse(name[name.Length - 1].ToString());
        i--;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Ball")
            _ladderThemeScript.SetSong(i, true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name == "Ball")
            _ladderThemeScript.SetSong(i, false);
    }
}
