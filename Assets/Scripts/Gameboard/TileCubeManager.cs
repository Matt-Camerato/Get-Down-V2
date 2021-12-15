using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCubeManager : MonoBehaviour
{
    [Header("Tile Cube Info")]
    [ReadOnly] public Vector2Int pos;
    [ReadOnly] public int cubeCount = 0;

    [Header("Tile Cube Settings")]
    [SerializeField] private float moveSpeed;

    private void Update() //move tile cube and any spawned attack cubes up or down as cube count value is updated
    {
        if (GameController.instance.roundNum == 1) return; //only fix position of TC if past round 1 (doesn't need to be done until recap phase)

        if(transform.localPosition.y < cubeCount * 0.1f) //if cube count is greater than y position (when an attack cube is spawned), move cube up to new y level
        {
            transform.localPosition += Vector3.up * Time.deltaTime * moveSpeed;
            if (transform.localPosition.y > cubeCount * 0.1f) transform.localPosition = new Vector3(transform.localPosition.x, cubeCount * 0.1f, transform.localPosition.z); //once reached new y level, make sure it is set to value exactly
        }
        else if (transform.localPosition.y > cubeCount * 0.1f) //if cube count is less than y position (when an attack cube is removed), set cube to new y level
        {
            transform.localPosition = new Vector3(transform.localPosition.x, cubeCount * 0.1f, transform.localPosition.z); //if cubes are removed, new y level is set directly since gameboard reset should happen instantly
        }
    }
}