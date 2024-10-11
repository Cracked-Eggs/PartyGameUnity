using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Lobby : MonoBehaviourPunCallbacks
{
    public static Lobby Instance;
    
    [Header("UI")] public GameObject loadingGO;
    public Button createRoomButton;
    public Transform roomListParent;
    public GameObject roomListItemPrefab;
    public Timer gameTimer;

    List<RoomInfo> cachedRoomList = new List<RoomInfo>();
    string roomToCreateName = "";

    void Awake()
    {
        loadingGO.SetActive(true);
        createRoomButton.interactable = false;
        Instance = this;
    }

    IEnumerator Start()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.Disconnect();
        }
        
        yield return new WaitUntil(() => !PhotonNetwork.IsConnected);

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        PhotonNetwork.JoinLobby();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        
        if (cachedRoomList.Count <= 0)
        {
            cachedRoomList = roomList;
            loadingGO.SetActive(false);
            createRoomButton.interactable = true;
        }
        else
        {
            foreach (var room in roomList)
            {
                for (int i = 0; i < cachedRoomList.Count; i++)
                {
                    if (cachedRoomList[i].Name == room.Name)
                    {
                        List<RoomInfo> newList = cachedRoomList;

                        if (room.RemovedFromList)
                        {
                            newList.Remove(newList[i]);
                        }
                        else
                        {
                            newList[i] = room;
                        }
                        cachedRoomList = newList;
                    }
                }
            }
        }
        UpdateUI();
    }

    void UpdateUI()
    {
        foreach (Transform roomItem in roomListParent) 
        {
            Destroy(roomItem.gameObject);
        }

        foreach (var room in cachedRoomList)
        {
            GameObject roomItem = Instantiate(roomListItemPrefab, roomListParent);
            roomItem.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = room.Name;
            roomItem.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = room.PlayerCount + "/4";
            roomItem.GetComponent<RoomItem>().roomToJoinOfItem = room.Name;
        }
    }

    public void GameToCreateNameChanged(string _newText)
    {
        roomToCreateName = _newText;
    }

    public void JoinRoomByName(string _name)
    {
        PlayerPrefs.SetString("roomNameToJoinOrCreate", _name);

        SceneManager.LoadScene(1); 
    }

    public void CreateGame()
    {
        PlayerPrefs.SetString("roomNameToJoinOrCreate", roomToCreateName);

        SceneManager.LoadScene(1);
    }
}
