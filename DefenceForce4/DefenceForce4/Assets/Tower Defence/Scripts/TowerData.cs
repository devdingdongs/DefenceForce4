using UnityEngine;
using UnityEngine.UI;

public class TowerData : MonoBehaviour
{
    [SerializeField]
    internal string towername = string.Empty;
    [SerializeField]
    internal int towerCount = 0, towerIndex = 0;
    [SerializeField]
    internal int towerprice = 0;
    [SerializeField]
    internal Text tower_price_text = null;
    public Sprite unlockSprite = null, lockSpite = null;

    public string TowerName
    {
        get { return towername; }
    }
    public int TowerCount
    {
        get { return towerCount; }
    }
    public int TowerPrice
    {
        get { return towerprice; }
    }
}
