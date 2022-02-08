using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Avatar
{
    /**
 * <summary>
 * Map position and rotation from source object
 * to target object. Use this to map XRRig on IK targets.
 * </summary>
 */
    [Serializable]
    public class MovementMapper
    {
        public Transform target, source;
        public Vector3 trackingPositionOffset;
        public Vector3 trackingRotationOffset;
        public bool maintainStartingOffset;

        // Call this method on first frame
        public void Init()
        {
            if (maintainStartingOffset)
            {
                trackingPositionOffset = target.position - source.position;
            }
            else
            {
                target.position = source.position + trackingPositionOffset;
            }
        }

        // Call this method every update
        public void Map()
        {
            target.position = source.TransformPoint(trackingPositionOffset);
            target.rotation = source.rotation * Quaternion.Euler(trackingRotationOffset);
        }
        
    }

    /**
 * <summary>
 * Map movements of XRRig to avatar. Attach this component to root of avatar.
 * Movement mapping from
 *      XR Controllers to hand targets
 *      XR MainCamera to head target
 * is dynamically created.
 * Further objects to be mapped can be added through mapping objects list.
 * Use Inverse Kinematics with targets to create coherent movements of body parts.
 * </summary>
 */
    public class XRAvatarController : MonoBehaviour
    {
        // Photon
        [SerializeField] private bool noHeadMode = true;  // Disable all renderers in to_disable
        [SerializeField] private bool onlyHandsMode = false;  // Disable all renderers, show only xr hands
        [SerializeField] List<GameObject> to_disable = new List<GameObject>();
        [SerializeField] private Transform _head;
        [SerializeField] private Transform _waist;
        private Vector3 _initWaistRotation;
        private PhotonView _photonView;


        // Variables for character customization
        [Header("Model Customization")]
        [SerializeField] private Vector3 headPositionOffset;
        [SerializeField] private Vector3 handLeftPositionOffset;
        [SerializeField] private Vector3 handLeftRotationOffset;
        [SerializeField] private Vector3 handRightPositionOffset;
        [SerializeField] private Vector3 handRightRotationOffset;
        [SerializeField] private int turnSmoothness = 5;
        private bool _charRenderer = true;

        // Base objects for movement mapping, found automatically
        private XRRig _xrRig;
        private Transform _handLeftTarget;
        private Transform _handRightTarget;
        private Transform _headTarget;
        private Transform _xrLeftController;
        private Transform _xrRightController;
        private Transform _xrMainCamera;

        [SerializeField] private List<MovementMapper> mappingObjects = new List<MovementMapper>();

        private void Start()
        {
            _initWaistRotation = _waist.up;
            _photonView = GetComponent<PhotonView>();

            if (!_photonView.IsMine) return;

            //====================== Initialize Avatar Objects ======================

            // Find movement sources from XRRig 
            _xrRig = FindObjectOfType<XRRig>();
            _xrMainCamera = _xrRig.transform.Find("Camera Offset/Main Camera");
            _xrLeftController = _xrRig.transform.Find("Camera Offset/LeftHand Controller");
            _xrRightController = _xrRig.transform.Find("Camera Offset/RightHand Controller");

            // Find targets in IK Rig
            _handLeftTarget = transform.Find("Rig 1/IKHandLeft/IKHandLeft_target");
            _handRightTarget = transform.Find("Rig 1/IKHandRight/IKHandRight_target");
            _headTarget = transform.Find("Rig 1/IKHead/IKHead_target");


            //====================== Initialize Sync Objects ======================

            // Create and initialize MovementMappers for all objects to map 
            MovementMapper leftHandMapper = new MovementMapper
            {
                source = _xrLeftController,
                target = _handLeftTarget,
                trackingPositionOffset = handLeftPositionOffset,
                trackingRotationOffset = handLeftRotationOffset
            };

            MovementMapper rightHandMapper = new MovementMapper
            {
                source = _xrRightController,
                target = _handRightTarget,
                trackingPositionOffset = handRightPositionOffset,
                trackingRotationOffset = handRightRotationOffset
            };

            MovementMapper headMapper = new MovementMapper
            {
                source = _xrMainCamera,
                target = _headTarget,
                trackingPositionOffset = headPositionOffset
            };

            mappingObjects.Add(leftHandMapper);
            mappingObjects.Add(rightHandMapper);
            mappingObjects.Add(headMapper);


            foreach (MovementMapper mappingObj in mappingObjects)
            {
                mappingObj.Init();
            }

            if (_charRenderer && _photonView.IsMine)
            {
                // Also en- and disable GazeObject-colliders, so own character is not tracked
                if (noHeadMode) DisableRenderers();
                if (onlyHandsMode) DisableAllButHands();
            }
        }

        // Update is called once per tick
        void LateUpdate()
        {
            if (!_photonView.IsMine) return;

            if (_charRenderer)
            {
                if (noHeadMode) DisableRenderers();
                else if (onlyHandsMode) DisableAllButHands();
            }
            else
            {
                if (!noHeadMode && !onlyHandsMode) EnableAllRenderers();
                else if (!noHeadMode && !onlyHandsMode) EnableAllRenderers();
            }

            // AdjustBodyTilt();
            AdjustBodyPosition();

            foreach (MovementMapper mappingObj in mappingObjects)
            {
                mappingObj.Map();
            }
        }

        // Align tilt of body toward camera
        void AdjustBodyTilt()
        {
            Vector3 directionVector = _headTarget.position - _waist.position;
            // Debug.DrawRay(_waist.position, directionVector);
            _waist.up = directionVector - _initWaistRotation;
        }

        // Move avatar toward camera.
        void AdjustBodyPosition()
        {
            Vector3 autoHeadPositionOffset = transform.position - _head.position;
            transform.position = _headTarget.position + autoHeadPositionOffset;

            transform.forward = Vector3.Lerp(
                transform.forward,  // Lerp from current viewing angle
                Vector3.ProjectOnPlane(_headTarget.forward, Vector3.up).normalized,  // Lerp toward camera angle
                Time.deltaTime * turnSmoothness  // Lerp smoothness
            );
        }

        // Deactivate rendering of own network avatar
        void DisableAllButHands()
        {
            foreach (Renderer item in GetComponentsInChildren<Renderer>())
            {
                // Disable body parts that distract in camera view (head)
                // if (to_disable.Contains(item.gameObject)) item.enabled = false;  
                item.enabled = false;  // Only hands test
            }

            // == Only hands test ==
            SkinnedMeshRenderer rightHandRenderer = _xrRightController.GetComponentInChildren<SkinnedMeshRenderer>();
            SkinnedMeshRenderer leftHandRenderer = _xrLeftController.GetComponentInChildren<SkinnedMeshRenderer>();
            if (rightHandRenderer && leftHandRenderer)
            {
                rightHandRenderer.enabled = true;
                leftHandRenderer.enabled = true;
            }
            else
            {
                Debug.LogError("[XRAvatarController] Can not find hand models in XRRig");
            }
            // =====================

            foreach (Collider item in GetComponentsInChildren<Collider>())
            {
                // Disable all GazeObject colliders so own character doesnt get logged in GazeFocusLogger
                if (item.gameObject.layer == LayerMask.NameToLayer("GazeObject")) item.enabled = false;  
            }
            _charRenderer = false;
        }

        // Deactivate rendering of own network avatar
        void DisableRenderers()
        {
            foreach (Renderer item in GetComponentsInChildren<Renderer>())
            {
                // Disable body parts that distract in camera view (head)
                if (to_disable.Contains(item.gameObject)) item.enabled = false;  
            }

            foreach (Collider item in GetComponentsInChildren<Collider>())
            {
                // Disable all GazeObject colliders so own character doesnt get logged in GazeFocusLogger
                if (item.gameObject.layer == LayerMask.NameToLayer("GazeObject")) item.enabled = false;
            }
            _charRenderer = false;
        }


        // Activate rendering of own network avatar
        void EnableAllRenderers()
        {
            // Disable controller hands if avatar is visible
            SkinnedMeshRenderer rightHandRenderer = _xrRightController.GetComponentInChildren<SkinnedMeshRenderer>();
            SkinnedMeshRenderer leftHandRenderer = _xrLeftController.GetComponentInChildren<SkinnedMeshRenderer>();
            if (rightHandRenderer && leftHandRenderer)
            {
                rightHandRenderer.enabled = false;
                leftHandRenderer.enabled = false;
            }
            else
            {
                Debug.LogError("[XRAvatarController] Can not find hand models in XRRig");
            }

            foreach (Renderer item in GetComponentsInChildren<Renderer>())
            {
                item.enabled = true;
            }
            foreach (Collider item in GetComponentsInChildren<Collider>())
            {
                if (item.gameObject.layer == LayerMask.NameToLayer("GazeObject"))  item.enabled = true;

            }
            _charRenderer = true;
        }

    }
}
    