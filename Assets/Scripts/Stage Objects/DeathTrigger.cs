using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

public class DeathTrigger : NetworkBehaviour
{
    private Transform player;
    [SerializeField] private Vector3 respawnPos;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Respawn Player Trigger EXIT");
            player = other.GetComponentInParent<PlayerStateMachine>().transform;
            RespawnPlayerClientRpc();
        }
    }

    [ClientRpc]
    private void RespawnPlayerClientRpc()
    {
        player.position = respawnPos;
        player = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red; // End point

        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            Gizmos.DrawCube(transform.position, boxCollider.size);
            Gizmos.DrawWireCube(transform.position, boxCollider.size);
        }
    }
}
