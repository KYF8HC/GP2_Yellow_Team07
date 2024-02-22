using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GyroscopeTest : MonoBehaviour {


    [Range(0.01f, 100.0f)] [SerializeField] private float multiplier = 0.01f;
    [Range(0.01f, 100.0f)][SerializeField] private float fineTuneLimit = 0.01f;

    Vector3 lastFrameGyro = Vector3.zero;

    void Update() {

        Vector3 test = Input.gyro.rotationRate;
        test.z *= 0.0f;
        test.y *= 0.0f;

        Quaternion quaternion = new Quaternion(Input.gyro.attitude.y, Input.gyro.attitude.x, -Input.gyro.attitude.z, -Input.gyro.attitude.w);
        transform.rotation = quaternion;
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x -20, transform.localEulerAngles.y, transform.localEulerAngles.z + 90);
        
       //Vector3 currentFrameGyro = Input.gyro.rotationRate;
       //Vector3 deltaGyro = currentFrameGyro - lastFrameGyro;
       //deltaGyro.y *= 0.0f;
       //deltaGyro.z *= 0.0f;
       //if (deltaGyro.magnitude > fineTuneLimit) {
       //    if (deltaGyro.x > 0.1f || deltaGyro.x < -0.1f)
       //        transform.rotation = Input.gyro.attitude;
       //
       //        //transform.SetLocalPositionAndRotation(transform.position, Quaternion.Euler(multiplier * test));
       //}
       //lastFrameGyro = currentFrameGyro;
       //Debug.Log(Mathf.Cos(Input.gyro.rotationRateUnbiased.x));
    }
}
