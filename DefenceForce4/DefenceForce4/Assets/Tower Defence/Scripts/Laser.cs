using UnityEngine;
using Photon.Pun;

public class Laser : MonoBehaviour
{
    private RaycastHit hit;
    public LayerMask raycastlayer;
    public string Towername = string.Empty;
    public Transform enemy = null, firepoint = null;
    public AttackType attackType = AttackType.Null;
    public enum AttackType { Null, Ground, Air, GroundAir }
    public LineRenderer lineRenderer = null;
    public ParticleSystem startparticle = null, impactParticle = null;
    [SerializeField]
    internal PhotonView photonview = null;
    internal float damage = 20f, attackspeed = 2f, attack_range = 3f;
    [SerializeField]
    private AudioSource laserAudioSource = null;

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
            enemy = SpawnEnemy.instance.GetClosestEnemy(firepoint.transform.position, attack_range, attackType.ToString());
        else
        {
            float distance = Vector3.Distance(transform.position, enemy.position);
            if (distance > attack_range)
                enemy = null;
            else
            {
                firepoint.transform.LookAt(enemy.transform.position);
                if (Physics.Raycast(firepoint.transform.position, firepoint.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, raycastlayer))
                {
                    // Debug.DrawRay(firepoint.position, firepoint.transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                    if (hit.collider.CompareTag(Constant.str_wave))
                        ShowLaser();
                    else if (hit.collider.CompareTag(Constant.str_centerwall))
                    {
                        enemy = null;
                        StopLaser();
                    }
                }
            }
        }
        if (enemy == null)
            StopLaser();
    }
    private void StopLaser()
    {
        if (enemy == null)
        {
            if (lineRenderer.enabled)
            {
                startparticle.Stop();
                impactParticle.Stop();
                lineRenderer.enabled = false;
                if (laserAudioSource.isPlaying)
                    laserAudioSource.Stop();
            }
        }
    }
    private void ShowLaser()
    {
        if (!lineRenderer.enabled)
        {
            startparticle.Play();
            lineRenderer.enabled = true;
            impactParticle.Play();
        }
        lineRenderer.SetPosition(0, firepoint.GetChild(0).transform.position);
        lineRenderer.SetPosition(1, enemy.position);
        impactParticle.gameObject.transform.position = new Vector3(enemy.position.x, enemy.position.y + 0.1f , enemy.transform.position.z );
        if (photonview.IsMine)
            enemy.GetComponent<enemymovement>().TakeDamage(damage * Time.deltaTime * attackspeed,string.Empty);
        if (!laserAudioSource.isPlaying)
            laserAudioSource.PlayOneShot(SoundManager.instance.turret4_sfx, 0.2f);
    }
}
