using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyParticles : MonoBehaviour
{
    private ParticleSystem ps;

    private void Start() => ps = GetComponent<ParticleSystem>();

    private void Update()
    {
        if (!ps.isPlaying) { Destroy(gameObject); }
    }
}
