using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LobbyTrigger : NetworkBehaviour
{
    private NetworkVariable<int> numPlayersInCollider = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            numPlayersInCollider.Value += 1;
            
            if(numPlayersInCollider.Value >= 1)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("Racing Level", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (!IsOwner) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            numPlayersInCollider.Value -= 1;
        }
    }



}
