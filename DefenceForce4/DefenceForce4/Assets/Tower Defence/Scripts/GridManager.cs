using UnityEngine;
using Photon.Pun;

public class GridManager : MonoBehaviour
{
    [SerializeField]
    private PhotonView photnview = null;
    public static GridManager instance { get; set; }
    public GameObject[] Grid = null;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }
    internal void GridDataRPC(int gridindex, bool state)
    {
        photnview.RPC("UpdateGridData",RpcTarget.All, gridindex, state);
    }
    [PunRPC]
    private void UpdateGridData(int index, bool towerState)
    {
        if (Grid[index].GetComponent<TowerCheck>() == null)
            return;
        TowerCheck tower = Grid[index].GetComponent<TowerCheck>();
        tower.isTowerPlace = towerState;
    }
    internal void InitGridData()
    {
        int length = Grid.Length;
        for (int i = 0; i < length; i++)
        {
            if(Grid[i].GetComponent<TowerCheck>() != null)
            {
                TowerCheck tower = Grid[i].GetComponent<TowerCheck>();
                tower.GridIndex = i;
            } 
        }
    }
    internal void ResetGridData()
    {
        int length = Grid.Length;
        for(int i=0; i< length; i++)
        {
            if (Grid[i].GetComponent<TowerCheck>() != null)
            {
                TowerCheck tower = Grid[i].GetComponent<TowerCheck>();
                tower.isTowerPlace = false;
            }  
        }
    }
}
