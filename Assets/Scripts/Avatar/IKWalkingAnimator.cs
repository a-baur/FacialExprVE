using System.Collections;
using System.Collections.Generic;
using Avatar;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class IKWalkingAnimator : MonoBehaviour
{
    public Vector3 footOffset;
    
    // Weights
    [Range(0,1)]
    public float rightFootPosWeight = 1f;
    [Range(0,1)]
    public float leftFootPosWeight = 1f;
    [Range(0,1)]
    public float rightFootRotWeight = 1f;
    [Range(0,1)]
    public float leftFootRotWeight = 1f;
    
    private XRAvatarController _avatarController;
    private Animator _animator;
    private Vector3 _previousPos;
    private const float SpeedThreshold = 0.1f;


    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _avatarController = GetComponent<XRAvatarController>();
        _previousPos = _avatarController.transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        SetWalkingAnimation();
    }
    
    private void OnAnimatorIK(int layerIndex)
    {
        AdjustIKFoot(AvatarIKGoal.RightFoot, rightFootPosWeight, rightFootRotWeight);
        AdjustIKFoot(AvatarIKGoal.LeftFoot, leftFootPosWeight, leftFootRotWeight);
    }

    void SetWalkingAnimation()
    {
        Vector3 currentPosition = _avatarController.transform.position;
        
        // Compute global speed of player
        Vector3 headsetSpeed = (currentPosition - _previousPos) / Time.deltaTime;
        headsetSpeed.y = 0;  // Ignore vertical movement
        
        // To local speed
        Vector3 headsetLocalSpeed = transform.InverseTransformDirection(headsetSpeed);
        _previousPos = currentPosition;
        
        // Set animator values
        _animator.SetBool("IsMoving", headsetLocalSpeed.magnitude > SpeedThreshold);
        _animator.SetFloat("DirectionX", Mathf.Clamp(headsetLocalSpeed.x, -1, 1));
        _animator.SetFloat("DirectionY", Mathf.Clamp(headsetLocalSpeed.z, -1, 1));
    }
    
    /*
     * Adjust position and rotation of feed to ground level.
     */
    private void AdjustIKFoot(AvatarIKGoal ikFoot, float footPosWeight, float footRotWeight)
    {
        Vector3 rightFootPos = _animator.GetIKPosition(ikFoot);  // current pos
        RaycastHit hit;

        bool hasHit = Physics.Raycast(rightFootPos + Vector3.up, Vector3.down, out hit);
        if(hasHit)
        {
            // Adjust foot position to ground
            _animator.SetIKPositionWeight(ikFoot, footPosWeight);
            _animator.SetIKPosition(ikFoot, hit.point + footOffset);

            // Adjust foot rotation to ground
            Quaternion footRotation = Quaternion.LookRotation(
                Vector3.ProjectOnPlane(transform.forward, hit.normal), hit.normal);
            _animator.SetIKRotationWeight(ikFoot, footRotWeight);
            _animator.SetIKRotation(ikFoot, footRotation);
        }
        else
        {
            // No adjustment if no ground
            _animator.SetIKPositionWeight(ikFoot, 0);
        }
    }
}
