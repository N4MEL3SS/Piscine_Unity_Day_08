using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyLogic : MonoBehaviour
{
    [HideInInspector] public float str;
    [HideInInspector] public float agi;
    [HideInInspector] public float con;
    [HideInInspector] public float armor;
    
    [HideInInspector] public float hitPoints;
    [HideInInspector] public float minDmg;
    [HideInInspector] public float maxDmg;
    [HideInInspector] public float level;
    
    [HideInInspector] public bool isAlive = true; 
    [HideInInspector] public float attackSpeed = 1f;
    [HideInInspector] public float attackRange = 4;
    [HideInInspector] public float maxHitPoints;
    [HideInInspector] public float xpHolds;
    
    public GameObject target;
    private NavMeshAgent _agent;
    private RaycastHit _hit;
    private Animator _animator;
    
    private bool _isMoving;
    
    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _agent.updateRotation = false;
        str = Random.Range(10, 20);
        agi = Random.Range(10, 20);
        con = Random.Range(10, 20);
        armor = Random.Range(10, 20);
        InitPlayer();
    }

    private IEnumerator MoveDown()
    {
        while (true)
        {
            _agent.baseOffset -= 0.0001f;
            yield return null;
        }
    }

    private IEnumerator DeleteEnemy()
    {
        yield return new WaitForSeconds(7f);
        Destroy(transform.gameObject);
    }

    private IEnumerator Death()
    {
        _animator.SetTrigger("Death");
        yield return new WaitForSeconds(5f);
        StartCoroutine(MoveDown());
        StartCoroutine(DeleteEnemy());
    }

    
    private void Update()
    {
        if (!isAlive)
            return;
        if(_agent.velocity.magnitude < 0.1f)
            _animator.SetBool("Walk", false);
        else
        {
            transform.rotation = Quaternion.LookRotation(_agent.velocity.normalized);
            _animator.SetBool("Walk", true);
        }
    }

    public bool TakeDamage(float damage)
    {
        if (!isAlive)
            return isAlive;
        hitPoints -= damage;
        
        if (hitPoints <= 0)
        {
            hitPoints = 0;
            isAlive = false;
            Die();
        }
        return isAlive;
    }

    private IEnumerator PrepareAttack()
    {
        if (!isAlive)
            yield break;
        
        while (target != null)
        {
            var player = target.GetComponent<PlayerMovement>();
            if (!player || !player.isAlive)
                yield break;
            
            _agent.destination = target.transform.position;
            var dist = Vector3.Distance(target.transform.position, transform.position);
            
            while (dist > attackRange)
            {
                if (target == null || !isAlive)
                        yield break;
                dist = Vector3.Distance(target.transform.position, transform.position);
                yield return null; 
            }
            
            transform.LookAt(target.transform.position);
            _agent.destination = transform.position;
            
            if (target == null || !isAlive)
                yield break;
            
            _animator.SetTrigger("Melee"); 
            target.GetComponent<PlayerMovement>().TakeDamage(GetDamage());
            yield return new WaitForSeconds(10f / attackSpeed); 
        }
    }

    private float GetDamage()
    {                
        var baseDamage = Random.Range(minDmg, maxDmg);
        if (target != null)
        {
            var player = target.GetComponent<PlayerMovement>();
            if (player != null)
            {
                baseDamage = baseDamage * (1 - player.armorPlayer / 200f);
                var chance = 75 + agi - player.agi;
                if (Random.Range(0f, 100f) < chance)
                    baseDamage *= 0;
            }
        }
        return baseDamage;
    }

    private void Die()
    {
        StartCoroutine(Death());
    }
    
    private void InitPlayer()
    {
        maxHitPoints = con * 5;
        hitPoints = maxHitPoints;
        minDmg = str / 2;
        maxDmg = minDmg + 4;
        level = 1;
        xpHolds = con * level;
    }

    public void SetTarget(GameObject target)
    {
        this.target = target;
        StartCoroutine(PrepareAttack());
    }
}


