using UnityEngine;

public class Wall : MonoBehaviour
{
    public string Towername = string.Empty;
    internal int wallPlaceingIndex = 0;
    [SerializeField]
    internal GameObject normalwall = null, crackwall = null;

    private void OnEnable()
    {
        transform.name = Towername;
        if (GetComponent<TowerHealth>() != null)
            GetComponent<TowerHealth>().health = TowerDefence.TowerManager.GetTurretData(Towername, TowerDefence.TowerManager.TurretsInfo.health);
        transform.SetParent(TowerDefence.TowerManager.instance.towerParent.transform);
    }
    internal void ActiveCrackWall(bool state)
    {
        normalwall.SetActive(!state);
        crackwall.SetActive(state);
    }
}
