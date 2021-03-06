using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorProbeController : MonoBehaviour
{
    public Camera _camera;
    public ReflectionProbe reflectionProbe;

    // Update is called once per frame
    void Update()
    {
        Vector3 targetPos = Vector3.ProjectOnPlane(_camera.transform.forward, transform.up);
        
        reflectionProbe.transform.position = targetPos;
    }
}
