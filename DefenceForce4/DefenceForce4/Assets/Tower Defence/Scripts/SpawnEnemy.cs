using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

public class SpawnEnemy : MonoBehaviour
{
    public static SpawnEnemy instance { get; set; }
    [SerializeField]
    private PhotonView photonview = null;
    public EnemyData Enemy;
    public Transform EnemyParent = null, bulletParent = null;
    internal float lastSpawnTime, currentwaveSpeed = 0.4f, speedOffset = 0.03f, initialwaveSpeed = 0f, randomSpeed = 0f;
    internal int enemiesSpawned = 0, currentWave = 0, waypointindex = 0, randomValue = 0, maxSpawnWave = 20, spawnIndex = 0, randomIndex = 0;
    public EnemyWaypoint[] waypoints, FlyEnemyWaypoints = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    internal void Inititalize()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        lastSpawnTime = Time.time;
        photonview.RPC("ShowWaveComingPopupRPC", RpcTarget.All, GameManager.wave);
    }
    private void OnEnable()
    {
        GameManager.gameover += OnGameOver;
    }
    private void OnDisable()
    {
        GameManager.gameover -= OnGameOver;
    }
    private void Update()
    {
        if (!GameManager.isGameStart || !GameManager.isWaveStart)
            return;
        if (PhotonNetwork.IsMasterClient)
        {
            currentWave = GameManager.wave;
            if (currentWave < maxSpawnWave)
            {
                float timeInterval = Time.time - lastSpawnTime;
                float spawnInterval = Enemy.uints[currentWave].spawnInterval;
                if (((enemiesSpawned == 0 && timeInterval > Enemy.timeBetweenWaves) || (timeInterval > spawnInterval && enemiesSpawned > 0)) && enemiesSpawned < Enemy.uints[currentWave].maxSpawn)
                {
                    if (enemiesSpawned.Equals(0))
                    {
                        waypointindex = 0;
                        initialwaveSpeed = Enemy.uints[currentWave].movement_speed;
                        currentwaveSpeed = initialwaveSpeed;
                    }
                    lastSpawnTime = Time.time;
                    if (!Enemy.uints[currentWave].IsGrounded)
                        enemiesSpawned++;
                    if (Enemy.uints[currentWave].IsGrounded)
                    {
                        if (waypointindex < waypoints.Length - 1 && enemiesSpawned != 0)
                        {
                            currentwaveSpeed = initialwaveSpeed;
                            waypointindex++;
                            spawnIndex = Random.Range(0, waypoints[waypointindex].waypoint.Length);
                        }
                        else
                        {
                            waypointindex = 0;
                            spawnIndex = 0;
                        }
                        if (waypointindex > 3)
                            currentwaveSpeed = currentwaveSpeed - speedOffset;
                        GameObject tempObj = PhotonNetwork.InstantiateRoomObject(Enemy.uints[currentWave].wavePrefab.name, waypoints[waypointindex].waypoint[spawnIndex].waypoint[0].transform.position, Quaternion.identity);
                        tempObj.transform.name = string.Concat("Wave ", enemiesSpawned.ToString());
                        enemiesSpawned++;
                    }
                    else
                    {
                        if (waypointindex < waypoints.Length - 1)
                            waypointindex++;
                        else
                            waypointindex = 0;
                        PhotonNetwork.InstantiateRoomObject(Enemy.uints[currentWave].wavePrefab.name, FlyEnemyWaypoints[waypointindex].waypoint[randomValue].waypoint[0].transform.position, Quaternion.identity);
                    }
                    randomValue = 0;
                    photonview.RPC("SetSpawnUnitCount", RpcTarget.All, enemiesSpawned, randomValue, waypointindex, spawnIndex, currentwaveSpeed);
                }
               else if (enemiesSpawned.Equals(Enemy.uints[currentWave].maxSpawn) && EnemyParent.childCount.Equals(0))
                {
                    int randomnumber = 0;
                    enemiesSpawned = 0;
                    waypointindex = 0;
                    spawnIndex = 0;
                    GameManager.wave++;
                    lastSpawnTime = Time.time;
                    if (!Enemy.uints[currentWave].IsGrounded)
                        randomnumber = Random.Range(0, FlyEnemyWaypoints.Length);
                    photonview.RPC("ShowWaveComingPopupRPC", RpcTarget.All, GameManager.wave);
                }
            }
            else
                GameManager.instance.GameOver(GameManager.GameState.Win);
        }
    }
    internal void TrasnferWaveOwnerships()
    {
        for(int i = 0; i < EnemyParent.childCount; i ++)
        {
            EnemyParent.transform.GetChild(i).GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer.GetNext());
        }
    }
    internal Transform GetClosestEnemy(Vector3 towerpos, float attackrange, string attackType)
    {
        Transform tempenemy = null;
        float minimumDistance = attackrange;
        for(int i = 0; i < EnemyParent.childCount; i++)
        {
            float distance = Vector3.Distance(towerpos, EnemyParent.GetChild(i).position);
            if (distance < minimumDistance)
            {
                minimumDistance = distance;
                if (EnemyParent.GetChild(i).GetComponent<enemymovement>().wavetype.ToString().Equals(attackType))
                    tempenemy = EnemyParent.GetChild(i).transform;
                else if (distance < attackrange && attackType.Equals("GroundAir"))
                    tempenemy = EnemyParent.GetChild(i).transform;
            }
        }
        if (tempenemy != null)
            return tempenemy;
        else
            return null;
    }
    [PunRPC]
    private void SetSpawnUnitCount(int currentunit, int  waypoint, int wayindex, int spawnindex, float wavespeed)
    {
        waypointindex = wayindex;
        enemiesSpawned = currentunit;
        randomValue = waypoint;
        spawnIndex = spawnindex;
        currentwaveSpeed = wavespeed;
    }
    [PunRPC]
    private void ShowWaveComingPopupRPC(int currentwave)
    {
        GameManager.wave = currentwave;
        StartCoroutine(ShowWaveComingPopup());
    }
    private IEnumerator ShowWaveComingPopup()
    {
        if (GameManager.wave < maxSpawnWave)
        {
            if (Enemy.uints[GameManager.wave].wavesprite != null)
                UiManager.instance.wave_icon.sprite = Enemy.uints[GameManager.wave].wavesprite;
            UiManager.instance.waves_coming_text.text = Enemy.uints[GameManager.wave].waveTitle;
            UiManager.instance.wave_popup.SetActive(true);
            GameManager.instance.ShowWavesOnUI(GameManager.wave);
            yield return new WaitForSeconds(3f);
            UiManager.instance.wave_popup.SetActive(false);
            SoundManager.instance.PlaySfx(SoundManager.instance.wave_sfx, 0.2f);
        }
    }
    private void OnGameOver()
    {
        waypointindex = 0;
        currentWave = 0;
        enemiesSpawned = 0;
        lastSpawnTime = Time.time;
        DestroyAllUnits();
        StopCoroutine(ShowWaveComingPopup());
    }
    private void DestroyAllUnits()
    {
        int length = EnemyParent.childCount;
        for(int i=0; i< length; i++)
        {
           Destroy(EnemyParent.GetChild(i).gameObject);
        }
    }
}
[System.Serializable]
public class EnemyWaypoint
{
    public string name = string.Empty;
    public Ways[] waypoint = null;
}
[System.Serializable]
public class Ways
{
    public List<GameObject> waypoint = null;
}
