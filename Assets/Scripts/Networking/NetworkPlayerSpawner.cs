using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Management;

namespace Networking
{
    public class NetworkPlayerSpawner : MonoBehaviourPunCallbacks
    {
        public GameObject playerPrefab;
        private GameSettings _gameSettings;
        private GameObject _spawnedPlayerPrefab;
        private GazeFocusLogger _gazeFocusLogger;
        private XRInputSubsystem _xrInput;

        private Transform _spawnPoint1;
        private Transform _spawnPoint2;

        public void Start()
        {
            _gazeFocusLogger = FindObjectOfType<GazeFocusLogger>();
            _gameSettings = FindObjectOfType<GameSettings>();

            // var xrLoader = XRGeneralSettings.Instance.Manager.activeLoader;
            // _xrInput = xrLoader.GetLoadedSubsystem<XRInputSubsystem>();

            _spawnPoint1 = GameObject.Find("SpawnPoint_1").transform;
            _spawnPoint2 = GameObject.Find("SpawnPoint_2").transform;

            if (_spawnPoint1 == null | _spawnPoint2 == null)
                Debug.LogError("[PHOTON] No spawn points found!");

        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();

            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            Vector3 initPosition;
            Quaternion initRotation;

            Debug.Log($"[PHOTON] Player joined the room (Player count: {playerCount})");


            // Define initial positions and rotations of players by spawning order.
            switch (playerCount)
            {
                case 1:
                    initPosition = _spawnPoint1.position;
                    initRotation = _spawnPoint1.rotation;
                    break;

                case 2:
                    initPosition = _spawnPoint2.position;
                    initRotation = _spawnPoint1.rotation;
                    break;

                default:
                    initPosition = new Vector3(1, 2, 3);
                    initRotation = new Quaternion(0, 1, 0, 1);
                    break;
            }

            // Postion and rotation irrelevant, set with SetupAvatar function
            _spawnedPlayerPrefab = PhotonNetwork.Instantiate(playerPrefab.name, initPosition, initRotation); 

            SetupAvatar(initPosition, initRotation);

            Debug.LogError($"Player Position: {initPosition.x}, {initPosition.y}, {initPosition.z}");

            // If gaze tracking is activated, pass instance of spawned player so it can be tracked.
            if (_gazeFocusLogger == null || !_gazeFocusLogger.activateGazeTracking) return;
            foreach (Transform child in _spawnedPlayerPrefab.transform)
            {
                _gazeFocusLogger.AddFocusObject(child);
            }
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();
            PhotonNetwork.Destroy(_spawnedPlayerPrefab);
            Debug.Log("[PHOTON] Player Left The Room.");
        }

        /**
         * Return the spawned player as GameObject.
         */
        public GameObject GetSpawnedPlayer()
        {
            return _spawnedPlayerPrefab != null ? _spawnedPlayerPrefab : null;
        }

        /**
         * Set player position by moving XRRig.
        */
        public void SetupAvatar(Vector3 targetPosition, Quaternion targetRotation)
        {
            Transform xrRig = FindObjectOfType<XRRig>().transform;
            Transform avatarHead = xrRig.Find("Camera Offset/Main Camera");

            Vector3 rigHeadOffset = xrRig.position - avatarHead.position;

            xrRig.position = targetPosition - rigHeadOffset;
            xrRig.rotation = targetRotation;
        }
    }
}
