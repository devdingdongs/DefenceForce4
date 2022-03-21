using UnityEngine;
using Photon.Pun;

public class Bullet : MonoBehaviour
{
    internal float damage = 0f, destroyTimer = 3f;
    internal bool isProjectileMotion = false;
    public string bulletName = string.Empty;
    internal Transform target = null;
    [SerializeField]
    internal Rigidbody rigidBody = null;
    [SerializeField]
    private PhotonView photonview = null;

    private void Update()
    {
        if (!GameManager.isGameStart)
            return;
        DestroyBullet();
    }
    private void DestroyBullet()
    {
        if (!photonview.IsMine)
            return;
        if (target != null)
        {
            isProjectileMotion = true;
            transform.LookAt(target.transform.position);
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, Time.deltaTime * 2);
        }
        else if(isProjectileMotion && target == null)
            PhotonNetwork.Destroy(gameObject);
        if (isProjectileMotion)
            return;
        if (destroyTimer >= 0)
            destroyTimer -= Time.deltaTime;
        else
            PhotonNetwork.Destroy(gameObject);
    }
}
