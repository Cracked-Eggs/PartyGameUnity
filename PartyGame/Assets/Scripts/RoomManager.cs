using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.Mathematics;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;
    [Header("Player/Spawning")] public GameObject playerPrefab;
    public Transform[] spawnPoints;

    [Header("UI")] public GameObject roomCam;
    public GameObject nameUI;
    public GameObject connectingUI;

    string name;

    public void ChangeNickname(string _name)
    {
        name = _name;
        PhotonNetwork.LocalPlayer.NickName = name;
    }

    void Awake()
    {
        Instance = this;
        
        if (!PhotonNetwork.IsConnectedAndReady)
            SceneManager.LoadScene(0);

        string roomNameToJoin = PlayerPrefs.GetString("roomNameToJoinOrCreate");

        if (roomNameToJoin == "")
            roomNameToJoin = "room" + Random.Range(0, 999);

        PhotonNetwork.JoinOrCreateRoom(roomNameToJoin, null, null);
        connectingUI.SetActive(true);
        nameUI.SetActive(false);
    }

    public void SpawnPlayer()
    {
        roomCam.SetActive(false);
        Vector3 spawnPos = spawnPoints[Random.Range(0, spawnPoints.Length)].position;

        GameObject _player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, quaternion.identity);

        PhotonView photonView = _player.GetComponent<PhotonView>();
        if (photonView.IsMine)
        {
            _player.GetComponent<PlayerSetup>().IsLocalPlayer();
            _player.GetComponent<PhotonView>().RPC("SetNickname", RpcTarget.AllBufferedViaServer, name);
            _player.GetComponent<Health>().isLocalInstance = true;
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        nameUI.SetActive(true);
        connectingUI.SetActive(false);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            Debug.Log("All players have joined. Starting the game!");
        }
    }
}
