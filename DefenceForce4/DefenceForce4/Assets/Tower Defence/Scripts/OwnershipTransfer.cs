using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class OwnershipTransfer : MonoBehaviour, IPunOwnershipCallbacks
{
    private void Awake()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }
    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
    {
        if (targetView != GetComponent<PhotonView>())
            return;
        GetComponent<PhotonView>().TransferOwnership(requestingPlayer);
        Debug.Log("OnOwnership Request " + requestingPlayer.NickName);
    }

    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {
        Debug.Log("OnOwnership Transfered" + previousOwner.NickName);
    }

    public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
    {
        Debug.Log("OnOwnership Transfer Failed");
    }
}
