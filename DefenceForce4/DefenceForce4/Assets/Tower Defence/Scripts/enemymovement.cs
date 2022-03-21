using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System.Collections;
using System;

public class enemymovement : MonoBehaviourPun,IPunObservable
{
    private RaycastHit hit;
    internal float health = 100f;
    public LayerMask raycastlayer, otherlayer;
    public Transform particlePos = null, enemyChild = null;
    private int currentwaypoint = 0, rotateSpeed = 8;
    private GameObject Attackwall = null, prevGridObj = null;
    private enemymovement hitWave = null;
    private List<GameObject> waypoints = null;
    private string exitCabin = string.Empty, sideCabin = string.Empty;
    private float speed = 0.5f, damage = 1f, wallDamageSpeed = 12f, minDistance = 0.01f;
    private bool isDie = false, isattacksidewall = false, isattackwall = false, ishitwave = false, ismove = true, isfrontmove = false, ishitleft = false, ishitright = false, ishitfront = false, isShift = false;
    public WaveType wavetype = WaveType.Ground;
    private Vector3 currentPos = Vector3.zero, prevPos = Vector3.zero, enemyPos = Vector3.zero;
    public enum targetpoint { Null, TopCenterLeft, TopCenterRight, BottomCenterLeft, BottomCenterRight, TopRight, TopLeft, BottomLeft, BottomRight }
    public targetpoint targetDirection = targetpoint.Null;
    private Quaternion enemyRotaion = Quaternion.identity, enemyChildRot = Quaternion.identity;
    [SerializeField]
    private PhotonView photonview = null;
    [SerializeField]
    private Animator animator = null;
    public enum WaveType { Ground = 0, Air = 1 };

    private void Start()
    {
        InitData();
    }
    
    private void InitData()
    {
        waypoints = new List<GameObject>();
        speed = SpawnEnemy.instance.currentwaveSpeed;
        health = SpawnEnemy.instance.Enemy.uints[GameManager.wave].health;

        if (wavetype.Equals(WaveType.Ground))
        {
            int length = SpawnEnemy.instance.waypoints[SpawnEnemy.instance.waypointindex].waypoint[SpawnEnemy.instance.randomValue].waypoint.Count;
            for (int i = 0; i < length; i++)
                waypoints.Add(SpawnEnemy.instance.waypoints[SpawnEnemy.instance.waypointindex].waypoint[SpawnEnemy.instance.randomValue].waypoint[i]);
            SetTargetDirection();
        }
        else
            waypoints = SpawnEnemy.instance.FlyEnemyWaypoints[SpawnEnemy.instance.waypointindex].waypoint[SpawnEnemy.instance.randomValue].waypoint;
        gameObject.transform.localRotation = waypoints[0].transform.localRotation;
        transform.SetParent(SpawnEnemy.instance.EnemyParent);
        if (wavetype.Equals(WaveType.Ground))
            AttackState(false);
    }
    private void Update()
    {
        if (!GameManager.isGameStart || isDie)
            return;
        if (photonview.IsMine)
        {
            if (wavetype.Equals(WaveType.Ground))
                EnemyRaycast();
            if (ismove)
            {
                Transform endPosition = waypoints[currentwaypoint + 1].transform;
                transform.position = Vector3.MoveTowards(transform.position, endPosition.position, Time.deltaTime * speed);
                float distance = Vector3.Distance(transform.position, waypoints[currentwaypoint + 1].transform.position);
                if (distance <= minDistance)
                {
                    if (currentwaypoint < waypoints.Count - 2)
                        currentwaypoint++;
                    else
                    {
                        photonview.RPC("PlayerHealthRPC", RpcTarget.All);
                        PhotonNetwork.Destroy(this.gameObject);
                    }
                    ResetData();
                    if (prevGridObj != null)
                        GridManager.instance.GridDataRPC(prevGridObj.GetComponent<TowerCheck>().GridIndex, false);
                }
            }
        }
        else
        {
            if(wavetype.Equals(WaveType.Ground))
            {
                if (!enemyPos.Equals(Vector3.zero))
                {
                    transform.position = Vector3.Lerp(transform.position, enemyPos, Time.deltaTime * 5);
                    transform.rotation = enemyRotaion;
                    enemyChild.rotation = Quaternion.Lerp(enemyChild.rotation, enemyChildRot, Time.deltaTime * rotateSpeed);
                }     
            }
        }     
    }
       
    private void EnemyRaycast()
    {
        //Debug.DrawRay(transform.position, -transform.right, Color.green);
        if (Physics.Raycast(transform.position, -transform.right, out hit, 0.25f, raycastlayer) && !ishitfront)
        {
            if (hit.collider.CompareTag(Constant.str_wave))
            {
                if (!ishitwave)
                {
                    if (targetDirection.ToString().Equals(hit.collider.gameObject.GetComponent<enemymovement>().waypoints[0].name.ToString()))
                    {
                        ishitwave = true;
                        hitWave = hit.collider.gameObject.GetComponent<enemymovement>();
                        hitWave.ishitwave = true;
                        if (hitWave.ishitwave && ishitwave)
                        {
                            hitWave.ishitfront = true;
                            if (ismove)
                            {
                                ismove = false;
                                if (waypoints[waypoints.Count - 2].GetComponent<TowerCheck>().GridIndex.Equals(hitWave.waypoints[hitWave.waypoints.Count - 2].GetComponent<TowerCheck>().GridIndex))
                                    hitWave.waypoints.RemoveAt(hitWave.waypoints.Count - 2);       
                            }
                            animator.enabled = false;
                        }
                    }
                }
            }
            else
                StartCoroutine(StartMove(5f));

            if (((hit.collider.CompareTag(Constant.str_wall) && !hit.collider.gameObject.name.Equals(sideCabin) && !hit.collider.gameObject.name.Equals(exitCabin)) || hit.collider.CompareTag(Constant.str_centerwall)) && !hit.collider.transform.name.Equals(Constant.str_bottomsideWall) && !hit.collider.transform.name.Equals(Constant.str_topsideWall) && !hit.collider.transform.name.Equals(Constant.str_rightsideWall) && !hit.collider.transform.name.Equals(Constant.str_leftsideWall))
                ishitfront = true;
            else if (IshitsideWalls() || hit.collider.gameObject.name.Equals(sideCabin))
            {
                ishitfront = true;
                if (GetTargetDirectionfromEnemy())
                {
                    isattacksidewall = true;
                    ishitleft = true;
                }
                else
                {
                    isattacksidewall = true;
                    ishitright = true;
                }
            }
            else if (!ishitfront && !isfrontmove && hit.collider.gameObject != null && !hit.collider.CompareTag(Constant.str_wall) && !hit.collider.CompareTag(Constant.str_wave))
            {
                isfrontmove = true;
                ishitleft = false;
                ishitright = false;
                AddNextWaypoint(hit.collider.gameObject);
                //-------------Move Enemy on Forword Direction
            }
            else if (hit.collider.CompareTag(Constant.str_wall) && hit.collider.gameObject.name.Equals(exitCabin))
                RotateEnemy(hit.collider.transform);
        }
        else
        {
            if (!ishitfront)
                return;
         
            if (Physics.Raycast(transform.position, transform.forward, out hit, 0.25f, otherlayer) && !ishitleft)
            {
                if (((hit.collider.CompareTag(Constant.str_wall) && !hit.collider.gameObject.name.Equals(exitCabin)) || hit.collider.CompareTag(Constant.str_centerwall)) && ishitfront)
                {
                    ishitright = false;
                    ishitleft = true;
                }
            }
            if (Physics.Raycast(transform.position, -transform.forward, out hit, 0.25f, otherlayer) && !ishitright)
            {
                if (((hit.collider.CompareTag(Constant.str_wall) && !hit.collider.gameObject.name.Equals(exitCabin)) || hit.collider.CompareTag(Constant.str_centerwall)) && ishitfront)
                {
                    if (Physics.Raycast(transform.position, transform.forward, out hit, 0.25f, otherlayer) && !ishitleft)
                    {
                        if (((hit.collider.CompareTag(Constant.str_wall) && !hit.collider.gameObject.name.Equals(exitCabin)) || hit.collider.CompareTag(Constant.str_centerwall)) && ishitfront)
                            ishitleft = true;
                    }
                    ishitright = true;
                }
            }
            if (ishitfront && ishitleft && ishitright && !isattackwall)
                GetAttackingWall();
            else if (ishitfront && (ishitleft || ishitright) && !isattackwall && isattacksidewall)
                GetAttackingWall();
            else if (!isattackwall)
                ShiftEnemyLeftRight(); //-------------Move Enemy on Left or Right Direction       
        }
        RotateEnemy(waypoints[currentwaypoint + 1].transform);
        if (!isattackwall)
            return;
        AttackOnWall();
    }

    private void GetAttackingWall()
    {
        if (Physics.Raycast(transform.position, -transform.right, out hit, 0.25f, otherlayer))
        {
            if (hit.collider.CompareTag(Constant.str_wall) && hit.collider.GetComponent<TowerHealth>() != null && !isattackwall)
            {
                isattackwall = true;
                GameManager.iswallAdd = false;
                Attackwall = hit.collider.gameObject;              //--------Attack On Center Wall       
            }
            else if (Physics.Raycast(transform.position, transform.forward, out hit, 0.25f, otherlayer))
            {
                if (hit.collider.CompareTag(Constant.str_wall) && hit.collider.GetComponent<TowerHealth>() != null && !isattackwall)
                {
                    isattackwall = true;
                    GameManager.iswallAdd = false;
                    Attackwall = hit.collider.gameObject;              //--------Attack On Left Wall       
                }
                else if (Physics.Raycast(transform.position, -transform.forward, out hit, 0.25f, otherlayer))
                {
                    if (hit.collider.CompareTag(Constant.str_wall) && hit.collider.GetComponent<TowerHealth>() != null && !isattackwall)
                    {
                        isattackwall = true;
                        GameManager.iswallAdd = false;
                        Attackwall = hit.collider.gameObject;              //--------Attack On Right Wall       
                    }
                    else
                        ShiftEnemyLeftRight(); //-------------Move Enemy on Left or Right Direction       
                }
            }
        }
    }
    private IEnumerator StartMove(float time)
    {
        yield return new WaitForSeconds(time);
        if (hitWave != null)
        {
            ismove = true;
            animator.enabled = true;
            ishitwave = false;
            hitWave.ishitwave = false;
            ResetData();
            hitWave = null;
        }
    }
    private void ShiftEnemyLeftRight()
    {
        try
        {
            if (Physics.Raycast(transform.position, transform.forward, out hit, 0.25f, otherlayer) && !ishitleft && !isShift)
            {
                if (ishitfront && !isShift && !hit.collider.CompareTag(Constant.str_wave) && !hit.collider.CompareTag(Constant.str_wall) && !hit.collider.CompareTag(Constant.str_centerwall))
                {
                    isShift = true;
                    AddNextWaypoint(hit.collider.gameObject);
                }
            }
            else if (Physics.Raycast(transform.position, -transform.forward, out hit, 0.25f, otherlayer) && !ishitright && !isShift)
            {
                if (ishitfront && !isShift && !hit.collider.CompareTag(Constant.str_wave) && !hit.collider.CompareTag(Constant.str_wall) && !hit.collider.CompareTag(Constant.str_centerwall))
                {
                    isShift = true;
                    AddNextWaypoint(hit.collider.gameObject);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("ShiftEnemyLeftRight " + e.StackTrace);
        }
    }
    private void AttackOnWall()
    {
        if (Attackwall != null)
        {
            ismove = false;
            AttackState(true);
            RotateEnemy(Attackwall.transform);
            TowerHealth tempHelath = Attackwall.GetComponent<TowerHealth>();
            tempHelath.health -= Time.deltaTime * wallDamageSpeed;
            if (Attackwall.GetComponent<Wall>() != null)
            {
                Wall attackWall = Attackwall.GetComponent<Wall>();
                if (tempHelath.health <= 25 && !attackWall.crackwall.activeInHierarchy)
                    attackWall.ActiveCrackWall(true);
            }
            if (tempHelath.health <= 0)
            {
                isattackwall = false;
                AttackState(false);
                if (Attackwall.GetComponent<Wall>() != null)
                {
                    int length = GameManager.instance.playerData.Count;
                    for (int i = 0; i < length; i++)
                    {
                        if (GameManager.instance.playerData[i].player_actornumber.Equals(Attackwall.GetComponent<PhotonView>().OwnerActorNr))
                        {
                            if(!GameManager.iswallAdd)
                            {
                                GameManager.iswallAdd = true;
                                GameManager.instance.playerData[i].wall_destroyed++;
                                GridManager.instance.GridDataRPC(Attackwall.GetComponent<Wall>().wallPlaceingIndex, false);
                                GameManager.instance.SetPlayerInfoRPC(GameManager.playerInfo.wall_destroyed, GameManager.instance.playerData[i].wall_destroyed, GameManager.instance.playerData[i].user_id);
                            } 
                        }
                    }
                }
                else if (Attackwall.GetComponent<TowerShoot>() != null)
                {
                    int length = GameManager.instance.playerData.Count;
                    for (int i = 0; i < length; i++)
                    {
                        if (GameManager.instance.playerData[i].player_actornumber.Equals(Attackwall.GetComponent<PhotonView>().OwnerActorNr))
                        {
                            if (!GameManager.iswallAdd)
                            {
                                GameManager.iswallAdd = true;
                                GameManager.instance.playerData[i].turret_destroyed++;
                                GridManager.instance.GridDataRPC(Attackwall.GetComponent<TowerShoot>().towerplaceIndex, false);
                                GameManager.instance.SetPlayerInfoRPC(GameManager.playerInfo.turret_destroyed, GameManager.instance.playerData[i].turret_destroyed, GameManager.instance.playerData[i].user_id);
                            }
                        }
                    }
                }
                SoundManager.instance.PlaySfx(SoundManager.instance.wall_destroy_sfx, 0.5f);
                GameManager.instance.PlayParticle(GameManager.instance.deathparticle, transform.position);
                if (Attackwall.GetComponent<TowerShoot>() != null)
                {
                    TowerShoot shoot = Attackwall.GetComponent<TowerShoot>();
                    if (shoot.stackedtTowers.Count > 0)
                    {
                        PhotonNetwork.Destroy(shoot.stackedtTowers[shoot.stackedtTowers.Count - 1].gameObject);
                        shoot.stackedtTowers.RemoveAt(shoot.stackedtTowers.Count - 1);
                        shoot.GetComponent<TowerHealth>().health = 20f;
                    }
                    else
                        photonview.RPC("DestroyWall", RpcTarget.All, Attackwall.GetComponent<PhotonView>().ViewID);
                }
                else
                    photonview.RPC("DestroyWall", RpcTarget.All, Attackwall.GetComponent<PhotonView>().ViewID);

                isfrontmove = false;
                ishitfront = false;
                ishitleft = false;
                ishitright = false;
                ismove = true;
            }
        }
        else
        {
            isfrontmove = false;
            isattackwall = false;
            AttackState(false);
            ishitfront = false;
            ishitleft = false;
            ishitright = false;
            ismove = true;
        }
    }
    [PunRPC]
    private void DestroyWall(int turretId)
    {
        for (int i = 0; i < TowerDefence.TowerManager.instance.towerParent.transform.childCount; i++)
        {
            GameObject childObj = TowerDefence.TowerManager.instance.towerParent.transform.GetChild(i).gameObject;
            if (childObj.GetComponent<PhotonView>().ViewID.Equals(turretId) && childObj != null)
            {
                childObj.SetActive(false);
                Attackwall = null;
                return;
            }
        }
    }
    private bool IshitsideWalls()
    {
        bool ishitSideWall = false;
        if (hit.collider.CompareTag(Constant.str_wall))
        {
            if (hit.collider.transform.name.Equals(Constant.str_bottomsideWall) && (targetDirection.Equals(targetpoint.BottomCenterLeft) || targetDirection.Equals(targetpoint.BottomCenterRight)))
                ishitSideWall = true;
            else if (hit.collider.transform.name.Equals(Constant.str_topsideWall) && (targetDirection.Equals(targetpoint.TopCenterLeft) || targetDirection.Equals(targetpoint.TopCenterRight)))
                ishitSideWall = true;
            else if (hit.collider.transform.name.Equals(Constant.str_rightsideWall) && (targetDirection.Equals(targetpoint.BottomRight) || targetDirection.Equals(targetpoint.TopRight)))
                ishitSideWall = true;
            else if (hit.collider.transform.name.Equals(Constant.str_leftsideWall) && (targetDirection.Equals(targetpoint.TopLeft) || targetDirection.Equals(targetpoint.BottomLeft)))
                ishitSideWall = true;
        }
        return ishitSideWall;
    }
    private void AttackState(bool state)
    {
        animator.SetBool("IsAttack", state);
        animator.SetBool("IsWalk", !state);
    }
    private bool GetTargetDirectionfromEnemy()
    {
        bool isFacefront = false;
        Vector3 forward = transform.TransformDirection(-Vector3.forward);
        Vector3 toOther = waypoints[waypoints.Count - 1].transform.position - transform.position;

        if (Vector3.Dot(forward, toOther) < 0)
            isFacefront = false;
        else if (Vector3.Dot(forward, toOther) > 0)
            isFacefront = true;
        return isFacefront;
    }
    private void SetTargetDirection()
    {
        if (waypoints[waypoints.Count - 1].transform.name.Equals(targetpoint.BottomCenterLeft.ToString()))
        {
            targetDirection = targetpoint.BottomCenterLeft;
            exitCabin = "TopLeftCabin";
            sideCabin =  "TopRightCabin";
        }
        else if ( waypoints[waypoints.Count - 1].transform.name.Equals(targetpoint.BottomCenterRight.ToString()))
        {
            targetDirection = targetpoint.BottomCenterRight;
            exitCabin = "TopRightCabin";
            sideCabin = "TopLeftCabin";
        }
        else if (waypoints[waypoints.Count - 1].transform.name.Equals(targetpoint.TopCenterLeft.ToString()))
        {
            exitCabin = "TopLeftCabin";
            sideCabin = "TopRightCabin";
            targetDirection = targetpoint.TopCenterLeft;
        }
        else if (waypoints[waypoints.Count - 1].transform.name.Equals(targetpoint.TopCenterRight.ToString()))
        {
            exitCabin = "TopRightCabin";
            sideCabin = "TopLeftCabin";
            targetDirection = targetpoint.TopCenterRight;
        }
        else if (waypoints[waypoints.Count - 1].transform.name.Equals(targetpoint.TopLeft.ToString()))
        {
            exitCabin = "LeftTopCabin";
            sideCabin = "LeftBottomCabin";
            targetDirection = targetpoint.TopLeft;
        }
        else if (waypoints[waypoints.Count - 1].transform.name.Equals(targetpoint.BottomRight.ToString()))
        {
            exitCabin = "LeftBottomCabin";
            sideCabin = "LeftTopCabin";
            targetDirection = targetpoint.BottomRight;
        }
        else if (waypoints[waypoints.Count - 1].transform.name.Equals(targetpoint.BottomLeft.ToString()))
        {
            exitCabin = "LeftBottomCabin";
            sideCabin = "LeftTopCabin";
            targetDirection = targetpoint.BottomLeft;
        }
        else if (waypoints[waypoints.Count - 1].transform.name.Equals(targetpoint.TopRight.ToString()))
        {
            exitCabin = "LeftTopCabin";
            sideCabin = "LeftBottomCabin";
            targetDirection = targetpoint.TopRight;
        }  
    }
    private void RotateEnemy(Transform target)
    {
        if (target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            Quaternion _lookRotation = Quaternion.LookRotation(direction);
            enemyChild.rotation = Quaternion.Lerp(enemyChild.rotation, _lookRotation, Time.deltaTime * rotateSpeed);
        }
    }
    private void AddNextWaypoint(GameObject temp)
    {
        currentPos = temp.transform.position;
        if (prevPos != currentPos)
        {
            prevGridObj = temp;
            GridManager.instance.GridDataRPC(temp.GetComponent<TowerCheck>().GridIndex, true);
            currentPos = temp.transform.position;
            prevPos = currentPos;
            waypoints.Insert(waypoints.Count - 1, temp);
        }
    }
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.transform.CompareTag(Constant.bullet_str))
        {
            if (collision.gameObject.GetComponent<PhotonView>().IsMine)
            {
                TakeDamage(collision.gameObject.GetComponent<Bullet>().damage, collision.gameObject.GetComponent<Bullet>().bulletName);
                PhotonNetwork.Destroy(collision.gameObject);
            }    
        }
        else if (collision.transform.CompareTag(Constant.str_mine))
        {
            if (collision.gameObject.GetComponent<PhotonView>().IsMine)
            {
                if (wavetype.Equals(WaveType.Air) && collision.gameObject.name.StartsWith(Constant.str_balloonMine))
                {
                    OnMinesHitDamage(collision.gameObject);
                    GameManager.instance.PlayParticle(GameManager.instance.air_mine_particle, collision.transform.position);
                }
                else if (wavetype.Equals(WaveType.Ground) && collision.gameObject.name.StartsWith(Constant.str_mine))
                {
                    OnMinesHitDamage(collision.gameObject);
                    GameManager.instance.PlayParticle(GameManager.instance.ground_mine_particle, collision.transform.position);
                }
            }
        }
    }
    private void OnMinesHitDamage(GameObject collisionObj)
    {
        GameManager.detonated_mines += 1;
        CameraManager.instance.StartCoroutine(CameraManager.instance.Shake(0.15f, 0.2f));
        if(wavetype.Equals(WaveType.Ground))
            TakeDamage(collisionObj.gameObject.GetComponent<Mines>().damage, string.Empty);
        else
            TakeDamage(collisionObj.gameObject.GetComponent<Mines>().damage, string.Empty);
        GridManager.instance.GridDataRPC(collisionObj.GetComponent<Mines>().minePlaceingIndex, false);
        GameManager.instance.SetPlayerInfoRPC(GameManager.playerInfo.detonated_mines, GameManager.detonated_mines, UserData.GetUserId());
        SoundManager.instance.PlaySfx(SoundManager.instance.ground_mine_explode_sfx, 1f);
        PhotonNetwork.Destroy(collisionObj.gameObject);
    }
    internal void TakeDamage(float damage, string bulletname)
    {
        if (isDie)
            return;
        health -= damage;
        if (health <= 0)
        {
            isDie = true;
            if(wavetype.Equals(WaveType.Ground))
            {
                GameManager.ground_npc_kills++;
                GameManager.instance.SetPlayerInfoRPC(GameManager.playerInfo.ground_kills, GameManager.ground_npc_kills, UserData.GetUserId());
            }
            else
            {
                GameManager.air_npc_kills++;
                GameManager.instance.SetPlayerInfoRPC(GameManager.playerInfo.air_kills, GameManager.air_npc_kills, UserData.GetUserId());
            }
            if ((GameManager.ground_npc_kills + GameManager.air_npc_kills) % 10 == 0)
                GameManager.coinmultiplier += 1;
            GameManager.total_coins += GameManager.coinmultiplier;
            UiManager.instance.ShowCoinsOnUI(GameManager.total_coins);
            GameManager.instance.StartCoroutine(GameManager.instance.ShowCoinFly((GameManager.coinmultiplier), transform.position));
        }
        photonview.RPC("EnemyHealth", RpcTarget.All, health, bulletname);

    }
    [PunRPC]
    private void EnemyHealth(float healthammount, string bulletname)
    {
        health = healthammount;
        if (bulletname.Equals(Constant.str_turret_1))
            GameManager.instance.PlayParticle(GameManager.instance.turret1_hit_effect, particlePos.position);
        else if (bulletname.Equals(Constant.str_turret_2))
            GameManager.instance.PlayParticle(GameManager.instance.turret2_hit_effect, particlePos.position);
        else if (bulletname.Equals(Constant.str_turret_3))
            GameManager.instance.PlayParticle(GameManager.instance.turret3_hit_effect, particlePos.position);
        else if (bulletname.Equals(Constant.str_turret_5))
            GameManager.instance.PlayParticle(GameManager.instance.turret5_hit_effect, particlePos.position);
        else if (bulletname.Equals(Constant.str_turret_6))
        {
            GameManager.instance.PlayParticle(GameManager.instance.turret6_hit_effect, particlePos.position);
            SoundManager.instance.PlaySfx(SoundManager.instance.turret_6_explode_sfx, 1f);
        }
        if (photonview.IsMine && health <= 0)
        {
            if (prevGridObj != null)
                GridManager.instance.GridDataRPC(prevGridObj.GetComponent<TowerCheck>().GridIndex, false);
            GameManager.instance.PlayParticle(GameManager.instance.deathparticle, transform.position);
            PhotonNetwork.Destroy(gameObject);
        }
    }
    [PunRPC]
    private void PlayerHealthRPC()
    {
        GameManager.instance.UpdatePlayerHealth(damage);
    }
    private void ResetData()
    {
        isfrontmove = false;
        ishitfront = false;
        isShift = false;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(enemyChild.rotation);
        } 
        else
        {
            enemyPos = (Vector3)stream.ReceiveNext();
            enemyRotaion = (Quaternion)stream.ReceiveNext();
            enemyChildRot = (Quaternion)stream.ReceiveNext();
        }     
    }
}
