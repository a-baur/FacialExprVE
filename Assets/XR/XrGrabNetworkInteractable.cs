using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;

public class XrGrabNetworkInteractable : XRGrabInteractable
{
    private PhotonView _photonView;
    
    
    // Start is called before the first frame update
    void Start()
    {
        _photonView = GetComponent<PhotonView>();
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        _photonView.RequestOwnership();
        base.OnSelectEntered(args);
    }
}
