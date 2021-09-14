using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MapPosition : MonoBehaviour
{
    public Transform target;
    public bool maintainOffset = false;
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;

    void Start()
    {
        if (maintainOffset)
        {
            trackingPositionOffset = transform.position - target.position;
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        transform.position = target.TransformPoint(trackingPositionOffset);
        transform.rotation = target.rotation * Quaternion.Euler(trackingRotationOffset);
    }
}
