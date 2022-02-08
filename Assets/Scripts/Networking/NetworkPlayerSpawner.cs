using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Management;

namespace Networking
{
    public class NetworkPlayerSpawner : MonoBehaviourPunCallbacks
    {
        public GameObject playerFemaleSRanipal;
        public GameObject playerFemaleLipSync;
        public GameObject playerMaleSRanipal;
        public GameObject playerMaleLipSync;
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
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();

            int playerNumber = PhotonNetwork.LocalPlayer.ActorNumber;

            Debug.Log($"[PHOTON] Joined the room as Player " + playerNumber);

            GameObject playerPrefab;
            
            // Select player by gender and lip sync settings
            switch (_gameSettings.gender)
            {
                case 'm' when _gameSettings.useLipSync:
                    playerPrefab = playerMaleLipSync;
                    break;
                
                case 'm' when !_gameSettings.useLipSync:
                    playerPrefab = playerMaleSRanipal;
                    break;
                
                case 'f' when _gameSettings.useLipSync:
                    playerPrefab = playerFemaleLipSync;
                    break;
                
                case 'f' when !_gameSettings.useLipSync:
                    playerPrefab = playerFemaleSRanipal;
                    break;
                    
                
                default:
                    Debug.LogError($"[GameSettings] No player prefab for gender '{_gameSettings.gender}' and LipSync mode '{_gameSettings.useLipSync}'");
                    playerPrefab = new GameObject();
                    break;
            }

            // Spawn player prefab
            // Postion and rotation irrelevant, set with SetupAvatar function
            _spawnedPlayerPrefab = PhotonNetwork.Instantiate(
                playerPrefab.name, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));

            SetupAvatar();  // Move avatar to position


            /** 
            var otherPlayers = GetOtherPlayers();

            foreach (GameObject player in otherPlayers)
            {
                Debug.Log("Other Players: " + player.name);
                SetupGazeLogger(player);  // Make avatar target of gaze tracker
            }
            */
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
         * Spawn player by joining order.
        */
        public void SetupAvatar()
        {
            GameObject _spawnPoint1 = GameObject.Find("SpawnPoint_1");
            GameObject _spawnPoint2 = GameObject.Find("SpawnPoint_2");

            if (_spawnPoint1 == null | _spawnPoint2 == null)
                Debug.LogError("[PHOTON] Scene needs two spawnpoints!");

            int playerNumber = PhotonNetwork.LocalPlayer.ActorNumber;

            switch (playerNumber % 2)
            {
                case 1:
                    SetupSpawn(_spawnPoint1);
                    break;

                case 0:
                    SetupSpawn(_spawnPoint2);
                    break;
            }
        }

        public void SetupGazeLogger(GameObject playerPrefab)
        {   
            int gazeLayer =  LayerMask.NameToLayer("GazeObject");

            // If gaze tracking is activated, pass all children with layer 'GazeObject'.
            if (_gazeFocusLogger == null || !_gazeFocusLogger.activateGazeTracking) return;

            foreach (Transform child in playerPrefab.transform)
            { 
                if (child.gameObject.layer == gazeLayer)
                {
                    _gazeFocusLogger.AddFocusObject(child.transform);
                }
            }
        }

        /**
         * Setup player at spawnpoint and activat its teleportation anchor
        */
        private void SetupSpawn(GameObject spawnPoint)
        {
            XRRig xrRig = FindObjectOfType<XRRig>(); 

            Vector3 target_position = spawnPoint.transform.position;
            target_position.y = 1.2f;
            Vector3 target_forward = spawnPoint.transform.forward;
            
            xrRig.MoveCameraToWorldLocation(target_position);
            xrRig.MatchRigUpCameraForward(Vector3.up, target_forward);
        }

        private List<GameObject> GetOtherPlayers()
        {
            var result = new List<GameObject>();

            var photonViews = FindObjectsOfType<PhotonView>();
            foreach (var view in photonViews)
            {
                var player = view.Owner;

                //Objects in the scene don't have an owner, its means view.owner will be null
                if (player != null && !view.IsMine)
                {
                    var playerPrefabObject = view.gameObject;
                    result.Add(playerPrefabObject);
                }
            }

            return result;
        }
    }
}
