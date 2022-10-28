using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BossSpawner : Observer
{
    public string bossPrefabPath;
    public PlayerController player;
    public float spawnRadius;

    public override void Notify(Subject subject)
    {
        if (!player)
        {
            player = subject.GetComponent<PlayerController>();
        }

        if (player && player.trapped == true)
        {
            GameObject boss = PhotonNetwork.Instantiate(bossPrefabPath, transform.position, Quaternion.identity);
        }
    }
}
