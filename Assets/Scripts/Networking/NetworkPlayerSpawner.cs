using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
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

            _spawnPoint1 = GameObject.Find("SpawnPoint_1").transform;
            _spawnPoint2 = GameObject.Find("SpawnPoint_2").transform;

            if (_spawnPoint1 == null | _spawnPoint2 == null)
                Debug.LogError("[PHOTON] No spawn points found!");

        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();

            int playerNumber = PhotonNetwork.LocalPlayer.ActorNumber;

            Debug.Log($"[PHOTON] Joined the room as Player " + playerNumber);

            // Postion and rotation irrelevant, set with SetupAvatar function
            _spawnedPlayerPrefab = PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(0,0,0), new Quaternion(0, 0, 0, 0));

            SetupAvatar();

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
        public void SetupAvatar()
        {
            Transform _spawnPoint1 = GameObject.Find("SpawnPoint_1").transform;
            Transform _spawnPoint2 = GameObject.Find("SpawnPoint_2").transform;

            int playerNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            Vector3 targetPosition;
            Quaternion targetRotation;

            // Define initial positions and rotations of players by spawning order.
            switch (playerNumber%2)
            {
                case 1:
                    targetPosition = _spawnPoint1.position;
                    targetRotation = _spawnPoint1.rotation;
                    break;

                case 0:
                    targetPosition = _spawnPoint2.position;
                    targetRotation = _spawnPoint1.rotation;
                    break;

                default:
                    targetPosition = new Vector3(1, 2, 3);
                    targetRotation = new Quaternion(0, 1, 0, 1);
                    break;
            }

            Transform playerPrefab = _spawnedPlayerPrefab.transform;

            Transform xrRig = FindObjectOfType<XRRig>().transform;
            Transform xrHead = xrRig.Find("Camera Offset/Main Camera");
            Transform avatarHead = playerPrefab.Find("Rig 1/IKHead/IKHead_target");

            Vector3 rigHeadOffset = xrRig.position - xrHead.position;
            Vector3 avatarHeadOffset = playerPrefab.position - avatarHead.position;

            xrRig.position = targetPosition - rigHeadOffset;
            xrRig.rotation = targetRotation;

            playerPrefab.position = avatarHead.position + avatarHeadOffset;

        }
    }
}
