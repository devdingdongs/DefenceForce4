using UnityEngine;
using System.Collections.Generic;

public class TowerCheck : MonoBehaviour
{
    public int GridIndex = 0;
    public bool isTowerPlace = false;
    public List<GameObject> towers = new List<GameObject>();
    public bool CanplaceTower
    {
        get
        {
            return isTowerPlace;
        }
    }
    internal void DestroyTower()
    {
        if (towers.Count > 0)
            Destroy(towers[towers.Count - 1]);
    }
}
