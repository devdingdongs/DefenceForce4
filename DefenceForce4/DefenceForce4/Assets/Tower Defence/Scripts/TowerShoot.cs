using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class TowerShoot : MonoBehaviour
{
    public bool ismissile = false;
    private RaycastHit hit;
    public LayerMask raycastlayer;
    internal float turretoffset = 0.15f;
    internal int towerplaceIndex = 0;
    public AttackType attackType = AttackType.Null;
    public enum AttackType {Null, Ground, Air, GroundAir }
    public string Towername = string.Empty;
    public ParticleSystem shootParticle = null, shootParticle2 = null;
    internal float bulletoffset = 0.2f, damage = 20f, attackspeed = 2f, bulletspeed = 3f, attack_range = 4f, lastspawntime = 0f, spawninterval = 3f, currentangle = 0f;
    public GameObject firepoint = null, partToRotate = null;
    private Transform enemy = null, bullet = null;
    internal List<GameObject> stackedtTowers = new List<GameObject>();
    [SerializeField]
    internal PhotonView photonview = null;


    private void OnEnable()
    {
        Initialize();
    }
    private void Initialize()
    {
        transform.name = Towername;
        damage = TowerDefence.TowerManager.GetTurretData(Towername, TowerDefence.TowerManager.TurretsInfo.damage);
        attack_range = TowerDefence.TowerManager.GetTurretData(Towername, TowerDefence.TowerManager.TurretsInfo.range);
        attackspeed = TowerDefence.TowerManager.GetTurretData(Towername, TowerDefence.TowerManager.TurretsInfo.attackspeed);
        if (GetComponent<TowerHealth>() != null)
            GetComponent<TowerHealth>().health = TowerDefence.TowerManager.GetTurretData(Towername, TowerDefence.TowerManager.TurretsInfo.health);
        transform.SetParent(TowerDefence.TowerManager.instance.towerParent.transform); 
    }
    private void Update()
    {
        if (!GameManager.isGameStart)
            return;
        if (enemy == null)
            enemy = SpawnEnemy.instance.GetClosestEnemy(transform.position, attack_range, attackType.ToString());
        else
        {
            partToRotate.transform.LookAt(enemy.transform.position);
            float distance = Vector3.Distance(transform.position, enemy.position);
            if (distance > attack_range)
                enemy = null;
            else
            {
                if (Physics.Raycast(partToRotate.transform.position, partToRotate.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, raycastlayer))
                {
                   // Debug.DrawRay(partToRotate.transform.position, partToRotate.transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                    if (hit.collider.CompareTag(Constant.str_wave))
                    {
                        if (!photonview.IsMine)
                            return;
                        SpawnBullet();
                    }
                    else if (hit.collider.CompareTag(Constant.str_centerwall))
                        enemy = null;
                }
            }
        }
    }
    private void SpawnBullet()
    {
        float timeinterval = (Time.time - lastspawntime) * attackspeed;
        if (timeinterval > spawninterval && enemy != null)
        {
            lastspawntime = Time.time;
            int length = firepoint.transform.childCount;
            for (int i = 0; i < length; i++)
            {
                Bullet tempBullet = null;
                if (ismissile)
                {
                    bullet = PhotonNetwork.Instantiate(Constant.str_cannon, firepoint.transform.GetChild(i).transform.position, Quaternion.identity).transform;
                    tempBullet = bullet.GetComponent<Bullet>();
                    tempBullet.target = enemy;
                    SoundManager.instance.PlaySfx(SoundManager.instance.turret6_sfx, 1f);
                }
                else
                {
                    bullet = PhotonNetwork.Instantiate(Constant.bullet_str, firepoint.transform.GetChild(i).transform.position,Quaternion.identity).transform;
                    Vector3 direction = enemy.position - transform.position;
                    tempBullet = bullet.GetComponent<Bullet>();
                    enemymovement tempenemy = enemy.GetComponent<enemymovement>();
                    if (tempenemy.wavetype.Equals(enemymovement.WaveType.Air))
                        bulletoffset = 0f;
                    else
                        bulletoffset = 0.2f;
                    tempBullet.rigidBody.velocity = new Vector3(direction.x, direction.y - bulletoffset, direction.z) * bulletspeed * attackspeed;
                    if (Towername.Equals(Constant.str_turret_1))
                        SoundManager.instance.PlaySfx(SoundManager.instance.turret1_sfx, 0.2f);
                    else if (Towername.Equals(Constant.str_turret_2))
                        SoundManager.instance.PlaySfx(SoundManager.instance.turret2_sfx, 0.2f);
                    else if (Towername.Equals(Constant.str_turret_3))
                        SoundManager.instance.PlaySfx(SoundManager.instance.turret3_sfx, 0.2f);
                    else if (Towername.Equals(Constant.str_turret_4))
                        SoundManager.instance.PlaySfx(SoundManager.instance.turret4_sfx, 0.2f);
                    else if (Towername.Equals(Constant.str_turret_5))
                        SoundManager.instance.PlaySfx(SoundManager.instance.turret5_sfx, 0.2f);
                }
                tempBullet.damage = damage;
                tempBullet.bulletName = Towername;
                bullet.SetParent(SpawnEnemy.instance.bulletParent.transform);
                shootParticle.Play();
                if (shootParticle2 != null)
                    shootParticle2.Play();
            }
        }
    }
}
