using System;
using System.Collections.Generic;
using UnityEngine;

public class PausableUpdateManager : MonoBehaviour
{
    public static PausableUpdateManager instance;
    [SerializeField] private List<IPausableTick> tickables = new();
    private bool isPaused = false;

    private void Awake() => instance = this;

    public void Register(IPausableTick t) => tickables.Add(t);
    public void Unregister(IPausableTick t) => tickables.Remove(t);

    private void Update()
    {
        if (isPaused) return;

        foreach (var tickable in tickables)
            tickable.Tick();
    }

    public void PauseOrResume()
    {
        if (isPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        isPaused = true;
    }

    public void Resume() 
    {
        isPaused = false;
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}