using UnityEngine;
using Photon.Pun;
using System;

namespace TowerDefence
{
    public class TowerManager : MonoBehaviour
    {
        private RaycastHit hit;
        public LayerMask raycastlayer;
        public TurretsData[] Turrets = null;
        internal float offset = 100f, turretspawnOffset = 0.8f;
        public Vector3 lasthitpos = Vector3.zero, airUnitstartPos = new Vector3(-1f, 2f, 6f);
        public Sprite green_grid = null, red_grid = null;
        public Material gridMat = null;
        public static bool isHit { get; set; }
        public static bool isItemDrag { get; set; }
        public static TowerManager instance { get; set; }
        public GameObject DragTower = null, towerParent = null;
        public GameObject[] DragTowers = null;
        public TowerData Tower = null;
        public enum TurretsInfo { damage = 1, range = 2, attackspeed = 3, cost = 4, health = 5 };

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
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
            if (!GameManager.isGameStart)
                return;
            if (Input.GetMouseButton(0))
            {
                Ray ray = CameraManager.instance.maincamera.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y + offset, Input.mousePosition.z));
               // Debug.DrawRay(ray.origin, ray.direction * 20, Color.green);
                if (Physics.Raycast(ray, out hit, raycastlayer))
                {                   
                    if (hit.collider != null && Tower != null && !hit.collider.CompareTag(Constant.str_wave))
                    {
                        isHit = true;
                        TowerCheck tower = hit.collider.gameObject.GetComponent<TowerCheck>();
                        if (tower == null)
                        {
                            if (hit.collider.name.Equals(Constant.str_turret_1) && Tower.TowerName.Equals(Constant.str_turret_1))
                                ShowGridColor(green_grid);
                            else
                                ShowGridColor(red_grid);
                        }
                        else if (tower != null)
                        {
                            if (hit.collider.CompareTag(Constant.str_InnerGrid) && (Tower.towername.Equals(Constant.str_wall) || Tower.towername.Equals(Constant.str_mine) || Tower.towername.Equals(Constant.str_balloonMine) || Tower.towername.StartsWith("Turret")))
                            {
                                if (!tower.CanplaceTower)
                                    ShowGridColor(green_grid);
                                else
                                    ShowGridColor(red_grid);
                            }
                            else if (hit.collider.CompareTag(Constant.str_OuterGrid))
                                ShowGridColor(red_grid);
                        }
                        else
                            ShowGridColor(red_grid);

                        if (hit.collider.CompareTag(Constant.str_InnerGrid) || hit.collider.CompareTag(Constant.str_OuterGrid) || Tower.TowerName.Equals(Constant.str_turret_1))
                        {
                            lasthitpos = hit.transform.position;
                            DragTower.SetActive(true);
                        }
                        else if (!hit.collider.CompareTag(Constant.str_wave))
                            DragTower.SetActive(false);
                        MoveTower(lasthitpos);
                    }
                }
                else
                {
                    if (!InputManager.isdragscroller)
                    {
                        UiManager.instance.units_scroll.StopMovement();
                        UiManager.instance.units_scroll.enabled = false;
                        isHit = false;
                    }
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                lasthitpos = Vector3.zero;
                isItemDrag = false;
                TowerSpawn();
            }
        }
        internal void MoveTower(Vector3 temmpposition)
        {
            if (DragTower != null)
            {
                if (!DragTower.gameObject.activeInHierarchy)
                    return;
                UiManager.instance.units_scroll.StopMovement();
                UiManager.instance.units_scroll.enabled = false;
                DragTower.transform.position = new Vector3(temmpposition.x, DragTower.transform.position.y, temmpposition.z);
                DragTower.SetActive(true);
            }
        }
        internal void SetSelectedTower(Vector3 pos)
        {
            if (Tower == null)
                return;
            DragTowers[Tower.towerIndex].SetActive(true);
            DragTower.transform.localPosition = new Vector3(pos.x, DragTower.transform.position.y, pos.z);
            DragTower.SetActive(true);
        }
        internal void TowerSpawn()
        {
            if (!isHit || Tower == null || hit.collider.CompareTag(Constant.str_wave))
                DeselectTower();
            else if (hit.collider.gameObject != null && !hit.collider.CompareTag(Constant.str_wave))                                                                        
            {
                if (hit.collider.CompareTag(Constant.str_InnerGrid) || hit.collider.name.Equals(Constant.str_turret_1))
                {
                    TowerCheck temptower = hit.collider.gameObject.GetComponent<TowerCheck>();
                    if (temptower != null && !temptower.CanplaceTower)
                        PlaceTower();
                    else if (hit.collider.name.Equals(Constant.str_turret_1) && Tower.TowerName.Equals(Constant.str_turret_1))
                        StackedBasicTurret();
                    else if (temptower == null)
                        GameManager.instance.StartCoroutine(GameManager.instance.ShowCustomMessage(Constant.str_itemplace_errormsg));
                    else
                        GameManager.instance.StartCoroutine(GameManager.instance.ShowCustomMessage("Already item placed at this position!"));
                }
                else
                    GameManager.instance.StartCoroutine(GameManager.instance.ShowCustomMessage(Constant.str_itemplace_errormsg));
                DeselectTower();
            }
            else
                DeselectTower();
            isHit = false;
        }
        private void StackedBasicTurret()
        {
            TowerShoot tempShoot = hit.collider.GetComponent<TowerShoot>();
            if (!tempShoot.photonview.IsMine)
                return;
            GameManager.turret_build_count += 1;
            GameManager.total_coins -= Tower.TowerPrice;
            GameManager.currency_spent += Tower.TowerPrice;
            UiManager.instance.ShowCoinsOnUI(GameManager.total_coins);
            GameObject tempObject = PhotonNetwork.Instantiate(Tower.TowerName, new Vector3(hit.transform.position.x, 0.7f + tempShoot.turretoffset, hit.transform.position.z), Quaternion.identity, 0, null);
            GameManager.instance.SetPlayerInfoRPC(GameManager.playerInfo.turret_build, GameManager.turret_build_count, UserData.GetUserId());
            GameManager.instance.SetPlayerInfoRPC(GameManager.playerInfo.currency_Spent, GameManager.currency_spent, UserData.GetUserId());
            GameManager.instance.PlayParticle(GameManager.instance.objectplace_particle, hit.transform.position);
            SoundManager.instance.PlaySfx(SoundManager.instance.objectplace_sound, 1f);
            tempObject.GetComponent<BoxCollider>().enabled = false;
            tempShoot.stackedtTowers.Add(tempObject);
            tempShoot.turretoffset +=  0.1f;
            tempShoot.attack_range += 0.5f;
        }

        private void PlaceTower()
        {
            GameObject temp = null;
            TowerCheck temptower = hit.collider.gameObject.GetComponent<TowerCheck>();
            GridManager.instance.GridDataRPC(temptower.GridIndex , true);
            Quaternion rotation = Quaternion.identity;

            if(!Tower.towername.Equals(Constant.str_balloonMine))
            {
                temptower.isTowerPlace = true;
                if (Tower.towername.Equals(Constant.str_turret_1))
                    turretspawnOffset = 0.7f;
                else
                    turretspawnOffset = 0.8f;                                         
                temp = PhotonNetwork.Instantiate(Tower.TowerName, new Vector3(hit.transform.position.x, turretspawnOffset, hit.transform.position.z), rotation, 0, null);
                ActiveTurretParticle(temp.GetComponent<PhotonView>(), temp.transform);
            }
            GameManager.total_coins -= Tower.TowerPrice;
            GameManager.currency_spent += Tower.TowerPrice;
            UiManager.instance.ShowCoinsOnUI(GameManager.total_coins);
            if (Tower.towername.Equals(Constant.str_wall))
            {
                GameManager.wall_produced += 1;
                if (temp.GetComponent<Wall>() != null)
                    temp.GetComponent<Wall>().wallPlaceingIndex = temptower.GridIndex;
                GameManager.instance.SetPlayerInfoRPC(GameManager.playerInfo.walls_produced, GameManager.wall_produced, UserData.GetUserId());
            }
            else
            {
                GameManager.turret_build_count += 1;
                if (temp != null && temp.GetComponent<TowerShoot>() != null)
                    temp.GetComponent<TowerShoot>().towerplaceIndex = temptower.GridIndex;
                GameManager.instance.SetPlayerInfoRPC(GameManager.playerInfo.turret_build, GameManager.turret_build_count, UserData.GetUserId());
                GameManager.instance.SetPlayerInfoRPC(GameManager.playerInfo.currency_Spent, GameManager.currency_spent, UserData.GetUserId());
            }
            if (Tower.towername.Equals(Constant.str_mine))
            {
                if (temp.GetComponent<Mines>() != null)
                    temp.GetComponent<Mines>().minePlaceingIndex = temptower.GridIndex;
            }
            else if (Tower.towername.Equals(Constant.str_balloonMine))
            {
                GameObject airUnit = PhotonNetwork.Instantiate(Constant.str_airUnit, airUnitstartPos, Quaternion.identity, 0, null);
                airUnit.GetComponent<AirUnitManager>().StartAirMineMovement(hit.transform.position, temptower.GridIndex);
            }
            GameManager.instance.PlayParticle(GameManager.instance.objectplace_particle, hit.transform.position);
            SoundManager.instance.PlaySfx(SoundManager.instance.objectplace_sound, 1f);
        }
        private void ShowGridColor(Sprite colorsprite)
        {
            gridMat.mainTexture = colorsprite.texture;
        }
        internal void DeselectTower()
        {
            if (Tower != null)
                DragTowers[Tower.towerIndex].SetActive(false);
            DragTower.SetActive(false);
            UiManager.instance.units_scroll.enabled = true;
            Tower = null;
        }

        internal static float GetTurretData(string name, TurretsInfo data)
        {
            int lemgth = SpawnEnemy.instance.Enemy.turrets.Length;
            for (int i = 0; i < lemgth; i++)
            {
                if (name.Equals(SpawnEnemy.instance.Enemy.turrets[i].turretName))
                {
                    if (data.Equals(TurretsInfo.damage))
                        return SpawnEnemy.instance.Enemy.turrets[i].damage;
                    else if (data.Equals(TurretsInfo.range))
                        return SpawnEnemy.instance.Enemy.turrets[i].turretRange;
                    else if (data.Equals(TurretsInfo.attackspeed))
                        return SpawnEnemy.instance.Enemy.turrets[i].attackSpeed;
                    else if (data.Equals(TurretsInfo.cost))
                        return SpawnEnemy.instance.Enemy.turrets[i].cost;
                    else if (data.Equals(TurretsInfo.health))
                        return SpawnEnemy.instance.Enemy.turrets[i].health;
                }
            }
            return 0;
        }
        private void OnGameOver()
        {
            int length = DragTowers.Length;
            for (int i = 0; i < length; i++)
            {
                DragTowers[i].SetActive(false);
            }
            DragTower.SetActive(false);
            TurretsOwnerShipsTransfer(); 
        }
        private void TurretsOwnerShipsTransfer()
        {
            try
            {
                if (PhotonNetwork.PlayerListOthers.Length > 0)
                {
                    int slefTowerCount = 0, counter = 0, distributetower_count = 0;
                    for (int i = 0; i < towerParent.transform.childCount; i++)
                    {
                        if(towerParent.transform.GetChild(i).gameObject.activeInHierarchy)
                        {
                            if (towerParent.transform.GetChild(i).GetComponent<PhotonView>().IsMine)
                                slefTowerCount++;
                        }
                    }
                    int length = PhotonNetwork.PlayerListOthers.Length;
                    distributetower_count = Mathf.RoundToInt(slefTowerCount / length);
                    for (int k = 0; k < length; k++)
                    {
                        for (int i = counter; i < towerParent.transform.childCount; i++)
                        {
                            if (towerParent.transform.GetChild(i).gameObject.activeInHierarchy)
                            {
                                if (towerParent.transform.GetChild(i).GetComponent<PhotonView>().IsMine)
                                {
                                    counter++;
                                    towerParent.transform.GetChild(i).GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerListOthers[k].ActorNumber);
                                    if (i == (distributetower_count - 1))
                                        break;
                                }
                            }   
                        }
                    }
                }
                else
                    DestroyTurrets();
            }
            catch (Exception e)
            {
                Debug.Log("Turrets Owner Ships Transfer " + e.Message);
            }
        }
        internal void ActiveTurretParticle(PhotonView photonview, Transform parentObj)
        {
            try
            {
                if (parentObj.childCount.Equals(0) || !photonview.IsMine)
                    return;
                ParticleSystem glow_ps = parentObj.GetChild(0).GetComponent<ParticleSystem>();
                if (glow_ps != null)
                {
                    ParticleSystem.MainModule settings = glow_ps.main;
                    settings.startColor = new ParticleSystem.MinMaxGradient(Constant.turret_glowcolor[0]);
                    glow_ps.Play();
                }
            }
            catch (Exception e)
            {
                Debug.Log("ActiveTurretParticle " + e.Message);
            }
        }
        internal void DestroyTurrets()
        {
            try
            {
                if (towerParent.transform.childCount <= 0)
                    return;
                for (int i = 0; i < towerParent.transform.childCount; i++)
                {
                    if (towerParent.transform.GetChild(i) != null)
                        Destroy(towerParent.transform.GetChild(i).gameObject);
                }
            }
            catch(Exception e)
            {
                Debug.Log("DestroyTurrets " + e.Message);
            }
        }
    }
    [System.Serializable]
    public class TurretsData
    {
        public string TowerName = string.Empty;
        public Sprite[] TowerSprite = null;
        public Quaternion[] turretrotation;
    }
}

