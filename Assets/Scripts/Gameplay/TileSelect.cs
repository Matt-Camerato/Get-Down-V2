using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AR;

public class TileSelect : MonoBehaviour
{
    private ARGestureInteractor GI;

    private void Awake() => GI = GetComponent<ARGestureInteractor>();
    private void OnEnable() => GI.tapGestureRecognizer.onGestureStarted += OnTileTapped;
    private void OnDisable() => GI.tapGestureRecognizer.onGestureStarted -= OnTileTapped;

    private void OnTileTapped(TapGesture obj)
    {
        if (!GameController.instance.tileSelectEnabled) return; //check if player can select tiles (only can do this during move and attack phases)

        Vector2 tapPos = obj.startPosition;
        if (tapPos.IsPointOverUIObject()) return; //make sure UI isn't in way of tap position

        var ray = Camera.main.ScreenPointToRay(tapPos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (!hit.collider.CompareTag("TileCube")) return; //check if player hit a tile cube
                
            //! ===PLAYER HIT A TILE CUBE===

            TileCubeManager TC = hit.collider.transform.parent.GetComponent<TileCubeManager>();

            if (GameController.instance.currentPhase == "move") GameController.instance.HandleMoveSelect(TC.pos); //! === MOVE PHASE ===
            else if(GameController.instance.currentPhase == "attack") GameController.instance.HandleAttackSelect(TC.pos); //! ===ATTACK PHASE===
        }
    }
}
