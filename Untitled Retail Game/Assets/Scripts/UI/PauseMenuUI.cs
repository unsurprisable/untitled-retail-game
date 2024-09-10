using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuUI : Menu
{
    public static PauseMenuUI Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
}
