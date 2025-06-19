using System;
using System.Collections.Generic;
using UnityEngine;

public class PausableUpdateManager : MonoBehaviour
{
    public static PausableUpdateManager instance;
    [SerializeField] private List<IPausableTick> tickables = new();
    private List<IPausableTick> toBeRemoved = new List<IPausableTick>();
    private bool isPaused = false;

    private void Awake() => instance = this;

    public void Register(IPausableTick t) => tickables.Add(t);
    public void Unregister(IPausableTick t) => toBeRemoved.Add(t);

    private void Update()
    {
        if (isPaused) return;

        for (int i = tickables.Count - 1; i >= 0; i--)
        {
            tickables[i].Tick();
        }

        // Remove any tickables that have been unregistered
        for (int i = toBeRemoved.Count - 1; i >= 0; i--)
        {
            tickables.Remove(toBeRemoved[i]);
        }
        toBeRemoved.Clear();
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