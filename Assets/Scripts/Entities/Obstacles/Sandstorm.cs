using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sandstorm : Obstacle
{
    [SerializeField] private float diruptionRatio = 1.5f;
    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Player") || !activated) 
            return;

        Debug.Log("Darude");
        other.transform.forward += (other.transform.position - transform.position).normalized * diruptionRatio * Time.deltaTime;
    }
}
