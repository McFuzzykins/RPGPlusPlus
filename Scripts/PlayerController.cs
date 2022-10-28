using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : Subject
{
    public GameUI gameUI;
    public TileManager manage;
    public bool canTrigger = true;
    public float waitTime;
    public float trigRate;
    public bool stairs = false;
    public bool takeDmg = false;
    public bool trapped = false;
    
    [HideInInspector]
    public int id;

    [Header("Info")]
    public float moveSpeed;
    public int gold;
    public int curHp;
    public int maxHp;
    public bool dead;

    [Header("Attack")]
    public int dmg;
    public float attackRange;
    public float attackRate;
    private float lastAttackTime;

    [Header("Components")]
    public Rigidbody2D rb;
    public Player photonPlayer;
    public SpriteRenderer sr;
    public Animator weaponAnim;

    //local player
    public static PlayerController me;

    public HeaderInfo headerInfo;
    
    void Awake()
    {
        gameUI = gameObject.AddComponent<GameUI>();
        manage = gameObject.AddComponent<TileManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!photonView.IsMine)
        {
            return;
        }

        Move();

        if(Input.GetMouseButtonDown(0) && Time.time - lastAttackTime > attackRate)
        {
            Attack();
        }

        float mouseX = (Screen.width / 2) - Input.mousePosition.x;

        if (mouseX < 0)
        {
            weaponAnim.transform.parent.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            weaponAnim.transform.parent.localScale = new Vector3(-1, 1, 1);
        }

        if (Time.time - waitTime > trigRate)
        {
            canTrigger = true;
        }
    }

    [PunRPC]
    public void TakeDamage(int damage)
    {
        takeDmg = true;
        curHp -= damage;

        //update healthbar
        headerInfo.photonView.RPC("UpdateHealthBar", RpcTarget.All, curHp);

        if(curHp <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(DamageFlash());

            IEnumerator DamageFlash()
            {
                sr.color = Color.red;
                yield return new WaitForSeconds(0.05f);
                sr.color = Color.white;
            }
        }
        takeDmg = false;
    }

    void Die()
    {
        dead = true;
        rb.isKinematic = true;

        transform.position = new Vector3(0, 99, 0);

        Vector3 spawnPos = GameManager.instance.spawnPoints[Random.Range(0, GameManager.instance.spawnPoints.Length)].position;

        StartCoroutine(Spawn(spawnPos, GameManager.instance.respawnTime));
    }

    IEnumerator Spawn (Vector3 spawnPos, float timeToSpawn)
    {
        yield return new WaitForSeconds(timeToSpawn);

        dead = false;
        transform.position = spawnPos;
        curHp = maxHp;
        rb.isKinematic = false;

        //update healthbar
        headerInfo.photonView.RPC("UpdateHealthBar", RpcTarget.All, curHp);
    }

    [PunRPC]
    public void Initialize (Player player)
    {
        id = player.ActorNumber;
        photonPlayer = player;
        GameManager.instance.players[id - 1] = this;

        //initialize healthbar
        headerInfo.Initialize(player.NickName, maxHp);

        if (player.IsLocal)
        {
            me = this;
        }
        else
        {
            rb.isKinematic = true;
        }
    }

    void Move()
    {
        //get horizontal and vertical inputs
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        //apply that to our velocity
        rb.velocity = new Vector2(x, y) * moveSpeed;
    }

    //melee attacks towards the mouse
    void Attack()
    {
        lastAttackTime = Time.time;

        //calculate direction
        Vector3 dir = (Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position)).normalized;

        //shoot a raycast in the direction
        RaycastHit2D hit = Physics2D.Raycast(transform.position + dir, dir, attackRange);

        //did we hit enemy?
        if(hit.collider != null && hit.collider.gameObject.CompareTag("Enemy"))
        {
            //get enemy and dmg them
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            enemy.photonView.RPC("TakeDamage", RpcTarget.MasterClient, dmg);

        }

        //play attack anim
        weaponAnim.SetTrigger("Attack");
    }

    [PunRPC]
    void Heal(int amountToHeal)
    {
        curHp = Mathf.Clamp(curHp + amountToHeal, 0, maxHp);

        //update healthbar
        headerInfo.photonView.RPC("UpdateHealthBar", RpcTarget.All, curHp);
    }

    [PunRPC]
    void GiveGold(int goldToGive)
    {
        gold += goldToGive;

        //update gold UI
        GameUI.instance.UpdateGoldText(gold);
    }

    void OnEnable()
    {
        if (gameUI)
        {
            Attach(gameUI);
            Debug.Log("Attached");
        }

        if (manage)
        {
            Attach(manage);
            Debug.Log("Managerial Status");
        }
    }

    void OnDisable()
    {
        if (gameUI)
        {
            Detach(gameUI);
            Debug.Log("Detached");
        }

        if (manage)
        {
            Detach(manage);
            Debug.Log("Fired for assaulting customers");
        }
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (canTrigger == true)
        {
            if (other.gameObject.CompareTag("Down Stairs"))
            {   
                canTrigger = false;
                waitTime = Time.time;
                trigRate = 5.0f;
                stairs = true;
                Debug.Log("Fade Out (Down)");

                NotifyObservers();
                Debug.Log("Notified");  

                Vector3 transferSpot = GameManager.instance.transferPoints[Random.Range(0, GameManager.instance.transferPoints.Length)].position;
                transform.position = transferSpot;
            }

            else if (other.gameObject.CompareTag("Up Stairs"))
            {
                canTrigger = false;
                waitTime = Time.time;
                trigRate = 5.0f;
                stairs = true;
                Debug.Log("Fade Out (Up)");

                NotifyObservers();
                Debug.Log("Notified");
                
                Vector3 returnSpot = GameManager.instance.returnPoints[Random.Range(0, GameManager.instance.returnPoints.Length)].position;
                transform.position = returnSpot;
            }
            else if (other.gameObject.CompareTag("Trap Tile"))
            {
                
                canTrigger = false;
                waitTime = Time.time;
                stairs = false;
                trapped = true;
                
                NotifyObservers();
                Debug.Log("Sent");
            }
            else if (other.gameObject.CompareTag("DmgTile"))
            {
                canTrigger = false;
                waitTime = Time.time;
                trigRate = 0.5f;
                stairs = false;

                TakeDamage(10);
            }
            else
            {
                canTrigger = false;
                waitTime = Time.time;
                trigRate = 0.2f;
                stairs = false;

                TakeDamage(20);
            }
        }
    }
}
