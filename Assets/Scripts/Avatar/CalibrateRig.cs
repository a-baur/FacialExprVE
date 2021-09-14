using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

/**
 *  CalibrateByScale() does not work with RigBuilder.
 *  Effectors do not adjust to new size of bones.
 *  Use CalibrateByPosition() instead.
 */

[Serializable]
public class ArmComponents
{
    public Transform upperArm;
    public Transform lowerArm;
    public Transform hand;
    public Transform handTarget;

    public void SetupFromHand()
    {
        if (hand != null)
        {
            lowerArm = hand.parent;
            upperArm = lowerArm.parent;
        }
    }

    public void CalibrateByScale()
    {
        Vector3 targetDistance = upperArm.position - handTarget.position;
        Vector3 handDistance = upperArm.position - hand.position;
        float scaleFactor = targetDistance.magnitude / handDistance.magnitude;

        upperArm.localScale = Vector3.Scale(upperArm.localScale, new Vector3(scaleFactor,1,1));
        lowerArm.localScale = Vector3.Scale(lowerArm.localScale, new Vector3(scaleFactor,1,1));
    }

    public void CalibrateAvatarSize(Transform character)
    {
        Vector3 targetDistance = upperArm.position - handTarget.position;
        Vector3 handDistance = upperArm.position - hand.position;
        float scaleFactor = targetDistance.magnitude / handDistance.magnitude;
        
        character.localScale = Vector3.Scale(character.localScale, new Vector3(scaleFactor,scaleFactor,scaleFactor));
    }
}

[Serializable]
public class HeadComponents
{
    public Transform head;
    public Transform headTarget;
    public Transform xrRig;

    public void CalibrateByScale(Transform character)
    {
        Vector3 targetDistance = xrRig.position - headTarget.position;
        Vector3 headDistance = xrRig.position - head.position;
        float scaleFactor = targetDistance.magnitude / headDistance.magnitude;

        character.localScale = Vector3.Scale(character.localScale, new Vector3(scaleFactor,scaleFactor,scaleFactor));
    }
}

public class CalibrateRig : MonoBehaviour
{
    public ArmComponents leftArmComponents = new ArmComponents();
    public ArmComponents rightArmComponents = new ArmComponents();
    public HeadComponents headComp = new HeadComponents();

    private RigBuilder _rigBuilder;

    private void Start()
    {
        _rigBuilder = GetComponent<RigBuilder>();
        if (_rigBuilder == null)
        {
            Debug.LogError("[CalibrateRig] Rig Has No RigBuilder Attached.");
        }
    }

    public void StartCalibration()
    {
        //headComp.CalibrateByScale(transform);
        leftArmComponents.CalibrateAvatarSize(transform);
        rightArmComponents.CalibrateAvatarSize(transform);

        bool hasBuilt = _rigBuilder.Build();
        Debug.Log("Calibration Success: " + hasBuilt);
    }

    [ContextMenu("Auto Setup Arm From Hand")]
    void AutoSetup()
    {
        leftArmComponents.SetupFromHand();
        rightArmComponents.SetupFromHand();
    }
}
