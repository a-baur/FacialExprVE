using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.Demo.Cockpit;
using Photon.Realtime;
using UnityEngine;

namespace Networking
{
    public class NetworkManagerDirectConnect : MonoBehaviourPunCallbacks
    {
        void Start()
        {
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("[PHOTON] Trying To Connect To Server...");
        }

        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
            Debug.Log("[PHOTON] Connected To Server.");
            InitilizeRoom();
        }

        public void InitilizeRoom()
        {
            // Create Room
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 2;
            roomOptions.IsVisible = true;
            roomOptions.IsOpen = true;
        
            PhotonNetwork.JoinOrCreateRoom("room", roomOptions, TypedLobby.Default);
            Debug.Log("[PHOTON] Initilized room");
        }
 
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);
            Debug.Log($"[PHOTON] Player {newPlayer.ActorNumber} joined the room.");
        }
    }
}