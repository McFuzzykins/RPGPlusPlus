using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Enemy : MonoBehaviourPun
{
    [Header("Info")]
    public string enemyName;
    public float moveSpeed;

    public int curHp;
    public int maxHp;

    public float chaseRange;
    public float attackRange;

    private PlayerController targetPlayer;

    public float playerDetectRate = 0.2f;
    private float lastPlayerDetectTime;

    public string objectToSpawnOnDeath;

    [Header("Attack")]
    public int dmg;
    public float attackRate;
    private float lastAttackTime;

    [Header("Components")]
    public HeaderInfo healthBar;
    public SpriteRenderer sr;
    public Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        healthBar.Initialize(enemyName, maxHp);
    }

    // Update is called once per frame
    void Update()
    {
       if(!PhotonNetwork.IsMasterClient)
        {
            return;
        }

       if(targetPlayer != null)
        {
            //calculate dist
            float dist = Vector3.Distance(transform.position, targetPlayer.transform.position);

            //if we're able to attack: do so
            if(dist < attackRange && Time.time - lastAttackTime >= attackRate)
            {
                Attack();
            }
            //otherwise, do we move after the player?
            else if(dist > attackRange)
            {
                Vector3 dir = targetPlayer.transform.position - transform.position;
                rb.velocity = dir.normalized * moveSpeed;
            }
            else
            {
                rb.velocity = Vector2.zero;
            }
        }

        DetectPlayer();
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        targetPlayer.photonView.RPC("TakeDamage", targetPlayer.photonPlayer, dmg);
    }

    void DetectPlayer()
    {
        
        if(Time.time - lastPlayerDetectTime > playerDetectRate)
        {
            //loop through all players
            foreach(PlayerController player in GameManager.instance.players)
            {
                //calculate dist between us and player
                float dist = Vector2.Distance(transform.position, player.transform.position);

                if(player == targetPlayer)
                {
                    if(dist > chaseRange)
                    {
                        targetPlayer = null;
                    }
                }
                else if(dist < chaseRange)
                {
                    if(targetPlayer == null)
                    {
                        targetPlayer = player;
                    }
                }
            }
        }
    }

    [PunRPC]
    public void TakeDamage (int damage)
    {
        curHp -= damage;

        //update healthbar UI
        healthBar.photonView.RPC("UpdateHealthBar", RpcTarget.All, curHp);

        if(curHp <= 0)
        {
            Die();
        }
        else
        {
            photonView.RPC("FlashDamage", RpcTarget.All);
        }
    }

    [PunRPC]
    void FlashDamage()
    {
        StartCoroutine(DamageFlash());

        IEnumerator DamageFlash ()
        {
            sr.color = Color.blue;
            yield return new WaitForSeconds(0.05f);
            sr.color = Color.white;
        }
    }

    void Die()
    {
        if(objectToSpawnOnDeath != string.Empty)
        {
            PhotonNetwork.Instantiate(objectToSpawnOnDeath, transform.position, Quaternion.identity);

            //destroy object across network
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
