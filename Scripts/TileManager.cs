using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : Observer
{
    public PlayerController player;
    public GameObject[] blocks;
    public bool set = false;

    public override void Notify(Subject subject)
    {
        if (!player)
        {
            player = subject.GetComponent<PlayerController>();
        }

        if (player && player.takeDmg == false)
        {
            
            StartCoroutine(timer(1));
            

            if (set)
            {
                foreach (GameObject i in blocks)
                {
                    i.active = true;
                }
                
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        blocks = GameObject.FindGameObjectsWithTag("Blockade");
        foreach (GameObject i in blocks)
        {
            i.active = false;
        }
    }
    
    IEnumerator timer(float wait)
    {
        yield return new WaitForSeconds(wait);
        set = true;
    }
}
