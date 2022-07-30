using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace MyProject
{
    public class Lobby : MonoBehaviourPunCallbacks
    {
        [SerializeField] private TextMeshProUGUI playerText = null;

        private void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;

            PhotonNetwork.LocalPlayer.NickName = "Player " + Random.Range(1000, 10000); ;
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("OnConnectedToMaster");
            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedLobby()
        {
            Debug.Log("OnJoinedLobby");
            PhotonNetwork.JoinRandomOrCreateRoom();
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogError("OnCreateRoomFailed");
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogError("OnJoinRoomFailed");
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            string roomName = "Room " + Random.Range(1000, 10000);

            RoomOptions options = new RoomOptions { MaxPlayers = 2 };

            PhotonNetwork.CreateRoom(roomName, options, null);
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("OnJoinedRoom");
            OnPlayerListChanged();
            CheckWhetherGetReadyToStart();
        }

        public override void OnLeftRoom()
        {
            Debug.Log("OnLeftRoom");
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.LogError($"OnPlayerEnteredRoom({newPlayer})\nPlayer count: {PhotonNetwork.CurrentRoom.PlayerCount}\nMaster: {PhotonNetwork.IsMasterClient}");
            OnPlayerListChanged();
            CheckWhetherGetReadyToStart();
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.Log($"OnPlayerLeftRoom({otherPlayer})");
            OnPlayerListChanged();
        }

        private void CheckWhetherGetReadyToStart()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
                {
                    Debug.Log("#player = 2");
                    PhotonNetwork.LoadLevel("Omok");
                }
            }
        }

        private void OnPlayerListChanged()
        {
            var builder = new StringBuilder();
            builder.AppendLine("Player list");
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                builder.AppendLine(p.NickName);
            }

            this.playerText.text = builder.ToString();
        }
    }
}