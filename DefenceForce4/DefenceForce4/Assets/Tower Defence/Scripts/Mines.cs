using Photon.Pun;
using UnityEngine;

public class Mines : MonoBehaviour
{
    public string Towername = string.Empty;
    internal float damage = 40f;
    internal int minePlaceingIndex = 0;
    [SerializeField]
    internal PhotonView photonview = null;

    private void OnEnable()
    {
        Init();
    }
    private void Init()
    {
        damage = TowerDefence.TowerManager.GetTurretData(Towername, TowerDefence.TowerManager.TurretsInfo.damage);
        transform.SetParent(TowerDefence.TowerManager.instance.towerParent.transform);
        if (Towername.Equals(Constant.str_balloonMine))
            transform.position = new Vector3(transform.position.x, 1.3f, transform.position.z);
    }
}
