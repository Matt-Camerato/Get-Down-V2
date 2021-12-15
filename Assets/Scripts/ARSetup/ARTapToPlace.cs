using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.AR;

[RequireComponent(typeof(ARRaycastManager))]
public class ARTapToPlace : MonoBehaviour
{
    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    [SerializeField] private GameObject gameMarkerPrefab;
    [SerializeField] private Button confirmARPlacementButton;

    [HideInInspector] public GameObject spawnedGameMarker;
    private ARRaycastManager raycastManager;
    private ARGestureInteractor arGestureInteractor;

    private void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        arGestureInteractor = transform.GetChild(0).GetComponent<ARGestureInteractor>();
    }

    private void OnEnable()
    {
        arGestureInteractor.tapGestureRecognizer.onGestureStarted += OnTapRecognized;
    }

    private void OnDisable()
    {
        arGestureInteractor.tapGestureRecognizer.onGestureStarted -= OnTapRecognized;
    }

    private void OnTapRecognized(TapGesture obj)
    {
        Vector2 tapPos = obj.startPosition;
        if (!tapPos.IsPointOverUIObject()) //make sure UI isn't in way
        {
            if (raycastManager.Raycast(tapPos, hits, TrackableType.PlaneWithinPolygon))
            {
                var hitPose = hits[0].pose;

                if (spawnedGameMarker == null) //only ever spawn 1 game marker in scene
                {
                    spawnedGameMarker = Instantiate(gameMarkerPrefab, hitPose.position, hitPose.rotation);

                    confirmARPlacementButton.interactable = true; //turn on continue button only once a spawned game marker exists
                }
                else
                {
                    spawnedGameMarker.transform.position = hitPose.position;
                }
            }
        }
    }
}
