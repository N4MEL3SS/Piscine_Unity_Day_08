using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{

    [HideInInspector] public float str = 20;
    [HideInInspector] public float agi = 20;
    [HideInInspector] public float con = 20;
    [HideInInspector] public float armor = 20;
    
    [HideInInspector] public float maxHitPoints;
    [HideInInspector] public float hitPoints;
    [HideInInspector] public float minDmg;
    [HideInInspector] public float maxDmg;
    [HideInInspector] public float level;
    [HideInInspector] public float xp;
    [HideInInspector] public float nextLevel;
    [HideInInspector] public float money;
    [HideInInspector] public bool isAlive = true; 
    
    public float attackSpeed = 10f;
    public float attackRange = 3;
    public float armorPlayer = 20;
    public float statPoints = 5;

    public Text hpText;
    public Image hpCurrent;
    
    public Text xpText;
    public Image xpCurrent;
    public Text levelText;

    public Image enemyHp;
    public Image enemyMaxHp;
    public Text enemyLvl; 
    
    public Text strText; 
    public Text agiText; 
    public Text conText; 
    public Text dmgText; 
    public Text armorText; 
    public Text valetsText; 
    public Text statPointsText;
    
    public Image statMenu;
    
    public Button[] pluses;
    
    private NavMeshAgent _agent;
    private RaycastHit _hit;
    private Camera _cam;
    private GameObject _target;
    private GameObject _selection;
    private Animator _animator;
    private float _dmgTimer;
    private bool _statVisible;
    
    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _cam = Camera.main;
        _agent.updateRotation = false;
        InitPlayer();
    }
    
    private void Update()
    {
        if (!isAlive)
            return;
        
        PlayerMoveAnimation();
        AttackToEnemy();
        MoveToEnemy();
        
        if (Input.GetKeyDown(KeyCode.C))
            _statVisible = !_statVisible;

        UpdateUI();
    }

    private void PlayerMoveAnimation()
    {
        if(_agent.velocity.magnitude <= 0.1f)
            _animator.SetBool("Walk", false);
        else
        {
            transform.rotation = Quaternion.LookRotation(_agent.velocity.normalized);
            _animator.SetBool("Walk", true);
        }
    }

    private void MoveToEnemy()
    {
        if (Mathf.Abs(Input.GetAxis("Mouse X")) >= 0.01 || Mathf.Abs(Input.GetAxis("Mouse Y")) >= 0.01)
        {
            if (_target == null)
            {
                RaycastHit hit;
                if (Physics.Raycast(_cam.ScreenPointToRay(Input.mousePosition), out hit))
                {
                    if (hit.transform.CompareTag("Enemy"))
                        _selection = hit.transform.gameObject;
                    else
                        _selection = null;
                }
                else
                    _selection = null;
            }
        }
    }
    
    private void AttackToEnemy()
    {
        if (Input.GetKeyDown("mouse 0"))
        {
            if (Physics.Raycast(_cam.ScreenPointToRay(Input.mousePosition), out _hit))
            {
                if (_hit.transform.CompareTag("Enemy"))
                {
                    _target = _hit.transform.gameObject;
                    var enemy = _target.GetComponent<EnemyLogic>();
                    
                    if (!enemy.isAlive)
                    {
                        _target = null;
                        _agent.destination = _hit.point;
                    }
                    else
                        StartCoroutine(PrepareAttack());
                }
                else
                    _agent.destination = _hit.point;
            }
        }
    }
    
    private void UpdateUI()
    {

        foreach (var btn in pluses)
            btn.gameObject.SetActive(statPoints > 0);

        enemyHp.gameObject.SetActive(false);
        enemyMaxHp.gameObject.SetActive(false);
        enemyLvl.gameObject.SetActive(false);
        statMenu.gameObject.SetActive(_statVisible);
        
        hpText.text = hitPoints.ToString("0") + "/" + maxHitPoints.ToString("0");
        hpCurrent.fillAmount = 1 - (maxHitPoints - hitPoints) / maxHitPoints;
        xpText.text = xp.ToString("0") + "/" + nextLevel.ToString("0");
        xpCurrent.fillAmount = 1 - (nextLevel - xp) / nextLevel;
        levelText.text = "Lvl: " + level.ToString("0");
        
        strText.text = str.ToString("0");
        agiText.text = agi.ToString("0");
        conText.text = con.ToString("0");
        statPointsText.text = statPoints.ToString("0");
        dmgText.text = "Damage: " + minDmg.ToString("0") + "-" + maxDmg.ToString("0");
        armorText.text = "Armor: " + armor.ToString("0");
        valetsText.text = "Valets: " + money.ToString("0");
        
        if (_target != null)
        {
            var enemy = _target.GetComponent<EnemyLogic>();
            
            if (enemy)
            {
                enemyHp.gameObject.SetActive(true);
                enemyMaxHp.gameObject.SetActive(true);
                enemyLvl.gameObject.SetActive(true);
                    
                enemyHp.fillAmount = 1 - (enemy.maxHitPoints - enemy.hitPoints) / enemy.maxHitPoints;
                enemyLvl.text = "Lvl: " + enemy.level.ToString("0") + "   " + "" + enemy.hitPoints.ToString("0") + "/" + enemy.maxHitPoints.ToString("0");
            }
        }
        else if (_selection != null)
        {
            var enemy = _selection.GetComponent<EnemyLogic>();
            
            if (enemy)
            {
                enemyHp.gameObject.SetActive(true);
                enemyMaxHp.gameObject.SetActive(true);
                enemyLvl.gameObject.SetActive(true);
                
                enemyHp.fillAmount = 1 - (enemy.maxHitPoints - enemy.hitPoints) / enemy.maxHitPoints;
                enemyLvl.text = "Lvl: " + enemy.level.ToString("0") + "   " + "" + enemy.hitPoints.ToString("0") + "/" + enemy.maxHitPoints.ToString("0");
            } 
        }
    }
    
    private IEnumerator Death()
    {
        _animator.SetTrigger("Death");
        yield return new WaitForSeconds(5f);
        StartCoroutine(MoveDown());
    }
    
    private IEnumerator MoveDown()
    {
        while (true)
        {
            if (_agent.baseOffset <= -0.05)
                yield break;
            _agent.baseOffset -= 0.0001f;
            yield return null;
        }
    }

    private IEnumerator Attack()
    {
        while (Input.GetKey("mouse 0"))        
        {
            if (!isAlive)
                yield break;
            if (_target)
            {
                var enemy = _target.GetComponent<EnemyLogic>();
                
                if (enemy)
                {
                    transform.LookAt(_target.transform.position);
                    _animator.SetTrigger("Melee");
                    
                    if (!_target.GetComponent<EnemyLogic>().TakeDamage(GetDamage()))
                    {
                        xp += enemy.xpHolds;
                        if (xp >= nextLevel)
                            LevelUp();
                        _target = null;
                        yield break;
                    }
                    yield return new WaitForSeconds(10f / attackSpeed); 
                }
                else
                    yield break;
            }
            else
                yield break;
            yield return null;
        } 
        yield return null;
    }

    private IEnumerator PrepareAttack()
    {
        _agent.destination = _target.transform.position;
        var dist = Vector3.Distance(_target.transform.position, transform.position);
        
        while (dist > attackRange)
        {
            if (_target != null)
                dist = Vector3.Distance(_target.transform.position, transform.position);
            else
                yield break;
            yield return null;
        }
        
        var enemy = _target.GetComponent<EnemyLogic>();
        transform.LookAt(_target.transform.position);
        _agent.destination = transform.position;
        _animator.SetTrigger("Melee");
        
        if (!_target.GetComponent<EnemyLogic>().TakeDamage(GetDamage()))
        {
            xp += enemy.xpHolds;
            if (xp >= nextLevel)
                LevelUp();
            _target = null;
            yield break;
        }
        
        yield return new WaitForSeconds(10f / attackSpeed);
        StartCoroutine(Attack());
    }

    
    public void TakeDamage(float damage)
    {
        if (!isAlive)
            return;
        hitPoints -= damage;
        
        if (hitPoints <= 0)
        {
            hitPoints = 0;
            isAlive = false;
            Die();
        }
    }
    
    private float GetDamage()
    {                
        var baseDamage = Random.Range(minDmg, maxDmg);
        
        if (_target != null)
        {
            var enemy = _target.GetComponent<EnemyLogic>();
            
            if (enemy != null)
            {
                baseDamage *= (1 - enemy.armor / 200f);
                var chance = 75 + agi - enemy.agi;
                
                if (Random.Range(0f, 100f) < chance)
                    baseDamage *= 0;
            }
        }
        return baseDamage;
    }

    private void LevelUp()
    {
        xp = 0;
        nextLevel += 10;
        level++;
        statPoints += 5;
        hitPoints = maxHitPoints;
        
        if (armor < 190)
            armor += 10;
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
        xp = 0;
        nextLevel = 100;
    }

    public void UpStr()
    {
        if (statPoints <= 0)
            return;
        statPoints--;
        str++;
        minDmg = str / 2;
        maxDmg = minDmg + 4;
    }
    
    public void UpAgi()
    {
        if (statPoints <= 0)
            return;
        statPoints--;
        agi++;
    }
    
    public void UpCon()
    {
        if (statPoints <= 0) 
            return;
        statPoints--;
        con++;
        maxHitPoints = con * 5;
    }
    
    public void Plus()
    {
        _statVisible = true;
    }
}
