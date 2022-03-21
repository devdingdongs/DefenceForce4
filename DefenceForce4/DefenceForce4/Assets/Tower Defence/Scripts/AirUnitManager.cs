using Photon.Pun;
using UnityEngine;

public class AirUnitManager : MonoBehaviour
{
    [SerializeField]
    private PhotonView photonview = null;
    private float speed = 3f, mindistance = 0.1f, mineIndex = 0f, distance = 0f;
    private bool isMove = false, isMineDrop = false;
    private Vector3 targetPosition = Vector3.zero;

    private void Update()
    {
        if (!isMove || !photonview.IsMine)
            return;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * speed);
        transform.LookAt(targetPosition);
        distance = Vector3.Distance(transform.position, targetPosition);
        if (distance < 5f && !isMineDrop)
        {
            if (speed >= 0.5f)
                speed -= Time.deltaTime;
        }
        else if (speed <= 3f)
            speed += Time.deltaTime;
        if (distance < mindistance && !isMineDrop)
            SpawnBallonMine();
        else if (distance < mindistance && isMineDrop)
            PhotonNetwork.Destroy(this.gameObject);
    }
    internal void StartAirMineMovement(Vector3 position, float mineplaceIndex)
    {
        targetPosition = new Vector3(position.x, 2f, position.z);
        mineIndex = mineplaceIndex;
        isMove = true;
    }
    private void SpawnBallonMine()
    {
        isMineDrop = true;
        GameObject temp = PhotonNetwork.Instantiate(Constant.str_balloonMine, new Vector3(targetPosition.x, 1.3f, targetPosition.z) , Quaternion.identity, 0, null);
        if (temp.GetComponent<Mines>() != null)
            temp.GetComponent<Mines>().minePlaceingIndex = (int)mineIndex;
        targetPosition = new Vector3(targetPosition.x, 2f, targetPosition.z - 20f);
    }
}
