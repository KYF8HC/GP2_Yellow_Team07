using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchableProps : Obstacle
{
    [Range(1.0f, 100.0f)][SerializeField] float launchMultiplier;
    Rigidbody rigidBody;
    public override void Initialize(GameInstance game)
    {
        rigidBody = gameObject.GetComponent<Rigidbody>();
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Hit");
        if(other.CompareTag("Player") && activated)
        {
            Debug.Log("Yeet");
            Vector3 direction = (transform.position - other.transform.position) * launchMultiplier;
            rigidBody.AddForce(direction);
        }
    }
}
