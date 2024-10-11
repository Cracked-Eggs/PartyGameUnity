using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using Photon.Pun.UtilityScripts;
using TMPro;

public class Leaderboard : MonoBehaviour
{
    public GameObject playersHolder;

    [Header("Options")] public float refreshRate = 1f;

    [Header("UI")] public Transform leaderboardItemsParent;
    public GameObject leaderboardItem;

    private void Update()
    {
        playersHolder.SetActive(Input.GetKey(KeyCode.Tab));
    }

    private void Start()
    {
        InvokeRepeating(nameof(Refresh), 1f, refreshRate);
    }

    private void Refresh()
    {
        foreach (Transform slot in leaderboardItemsParent.transform)
        {
            Destroy(slot.gameObject);
        }

        var sortedPlayerList =
            (from player in PhotonNetwork.PlayerList orderby player.GetScore() descending select player).ToList();

        int currentPlace = 1;
        int myPlace = 999;
        
        foreach (var player in sortedPlayerList)
        {
            GameObject _item = Instantiate(leaderboardItem, leaderboardItemsParent);

            if (player.UserId == PhotonNetwork.LocalPlayer.UserId)
            {
                // our player

                myPlace = currentPlace;
            }

            _item.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = player.NickName;
            _item.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = player.GetScore().ToString();


            currentPlace++;
        }
        
        
        Debug.Log("My place in leaderboard " + currentPlace);
    }
}
