using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePanelManager : MonoBehaviour
{
    [SerializeField] private float fadeSpeed;

    private void OnEnable() => GetComponent<CanvasGroup>().alpha = 0.5f; //when damage panel is enabled, it will start at half transparency

    private void Update()
    {
        if (GetComponent<CanvasGroup>().alpha <= 0) //if damage panel has become invisible, turn it off
        {
            gameObject.SetActive(false);
            return; 
        }

        GetComponent<CanvasGroup>().alpha -= Time.deltaTime * fadeSpeed; //if damage panel isn't invisible, make it fade until it is
    }
}
