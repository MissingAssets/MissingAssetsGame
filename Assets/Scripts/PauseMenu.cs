using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MenuBehavior
{
    // Start is called before the first frame update
    void Start()
    {
        menuUI.SetActive(false);
        IsGamePaused = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsGamePaused && Input.GetKeyDown(KeyCode.Escape)) {
            Pause();
        }
    }
}