using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public abstract class Subject : MonoBehaviourPun
{
    private readonly ArrayList _observers = new ArrayList();

    public void Attach (Observer observer)
    {
        _observers.Add(observer);
    }

    public void Detach (Observer observer)
    {
        _observers.Remove(observer);
    }

    public void NotifyObservers()
    {
        foreach (Observer observer in _observers)
        {
            observer.Notify(this);
        }
    }
}
