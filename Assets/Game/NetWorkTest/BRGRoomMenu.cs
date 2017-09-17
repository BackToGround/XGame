using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BRGRoomMenu : Photon.PunBehaviour
{
    // Use this for initialization
    void Start()
    {
        //PhotonNetwork.networkingPeer.AuthValues = new AuthenticationValues();
        //PhotonNetwork.networkingPeer.AuthValues.AuthType = CustomAuthenticationType.Custom;
        //PhotonNetwork.networkingPeer.AuthValues.AddAuthParameter("user", "tachen");
        //PhotonNetwork.networkingPeer.AuthValues.AddAuthParameter("pass", "tachen01");

        PhotonNetwork.ConnectUsingSettings("v1.0");
        PhotonNetwork.automaticallySyncScene = true; 
    }

    public override void OnJoinedLobby()
    {
        PhotonNetwork.JoinOrCreateRoom("room1", null, null);
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 100, 30), "players: " + PhotonNetwork.playerList.Length);

        if (PhotonNetwork.isMasterClient && GUI.Button(new Rect(10, 40, 100, 30), "start"))
        {
            PhotonNetwork.LoadLevel("Game/Scenes/BTGXGameTest");
        }
    }
}
