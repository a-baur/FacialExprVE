using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


[System.Serializable]
public class DefaultRoom
{
    public string Name;
    public int SceneIndex;
    public int MaxPlayers;
}

public class NetworkManager : MonoBehaviourPunCallbacks
{

    private void Start()
    {
        ConnectToServer();
    }

    public void ConnectToServer()
    {
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("[PHOTON] Trying To Connect To Server...");
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("[PHOTON] Connected To Server.");
    }

    public void InitilizeRoom()
    {
        //Load Scene
        PhotonNetwork.LoadLevel(1);

        // Create Room
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;
        
        PhotonNetwork.JoinOrCreateRoom("Interaction Env", roomOptions, TypedLobby.Default);
    }
    
    public override void OnJoinedRoom()
    {
        Debug.Log("[PHOTON] Joined a Room.");
        base.OnJoinedRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("[PHOTON] New Player Joined The Room.");
        base.OnPlayerEnteredRoom(newPlayer);
    }
}
