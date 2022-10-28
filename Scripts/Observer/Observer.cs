using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public abstract class Observer : MonoBehaviourPun
{
    public abstract void Notify(Subject subject);
}
