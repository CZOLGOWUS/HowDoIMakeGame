using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform playerTransform;
    void Update()
    {
        transform.position = playerTransform.position;
    }
}
