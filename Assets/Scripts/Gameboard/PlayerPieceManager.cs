using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPieceManager : MonoBehaviour
{
    [SerializeField] private GameObject damageParticlesPrefab;

    [HideInInspector] public PlayerManager playerManager;

    public void MovePlayerPieceEvent() //triggered at the end of lowering animation so piece is teleported to new position
    {
        Vector2Int newPos = playerManager.pos; //get player's new position
        transform.SetParent(GameboardManager.instance.tiles[newPos.x, newPos.y].transform.GetChild(0), false); //set player piece's new parent TC (keeping same local position)
    }

    public void Damage()
    {
        Vector3 spawnPos = transform.position + new Vector3(0, 1.66f, 0); 
        Instantiate(damageParticlesPrefab, spawnPos, Quaternion.identity); //when damaged, instantiate damage particles at player piece (mesh) position
        Debug.Log("damage particles spawned for " + playerManager.playerName);
    }
}
