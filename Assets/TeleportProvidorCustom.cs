using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class TeleportProvidorCustom : MonoBehaviour
{
    private XRRig xrRig;
    private GameObject teleportTarget;

    // Start is called before the first frame update
    void Start()
    {
        xrRig = FindObjectOfType<XRRig>();
        teleportTarget = transform.parent.gameObject;
        if (!xrRig) Debug.Log("[Teleport Provider Custom] No XRRig found.");
    }

    public void Teleport()
    {
        Vector3 target_position = teleportTarget.transform.position;
        target_position.y = 1.2f;
        Vector3 target_forward = teleportTarget.transform.forward;

        xrRig.MoveCameraToWorldLocation(target_position);
        xrRig.MatchRigUpCameraForward(Vector3.up, target_forward);
    }
}
