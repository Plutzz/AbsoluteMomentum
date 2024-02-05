using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

public class DeathBox : NetworkBehaviour
{
    private Transform player;
    [SerializeField] private Vector3 respawnPos;
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Respawn Player Trigger EXIT");
            player = other.GetComponentInParent<PlayerStateMachine>().transform;
            RespawnPlayerClientRpc();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        
    }

    [ClientRpc]
    private void RespawnPlayerClientRpc()
    {
        player.position = respawnPos;
        player = null;
    }

    private void OnDrawGizmosSelected()
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
