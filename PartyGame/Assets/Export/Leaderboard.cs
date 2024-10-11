using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using Photon.Pun.UtilityScripts;
using TMPro;
using UnityEngine.InputSystem;

public class Leaderboard : MonoBehaviour
{
    public GameObject playersHolder;

    [Header("Options")] 
    public float refreshRate = 1f;

    [Header("UI")] 
    public Transform leaderboardItemsParent;
    public GameObject leaderboardItem;

    private InputAction _toggleLeaderboardAction;
    private InputSystem_Actions inputSystem;

    private void Awake()
    {
        inputSystem = new InputSystem_Actions(); // Initialize input system
    }

    void OnEnable()
    {
        _toggleLeaderboardAction = inputSystem.Player.LeaderboardToggle; // Bind to leaderboard toggle action
        _toggleLeaderboardAction.performed += OnToggleLeaderboard; // Subscribe to the performed event
        _toggleLeaderboardAction.Enable();
    }

    void OnDisable()
    {
        _toggleLeaderboardAction.Disable();
    }

    private void OnToggleLeaderboard(InputAction.CallbackContext context)
    {
        playersHolder.SetActive(!playersHolder.activeSelf); // Toggle leaderboard visibility
    }

    private void Start()
    {
        InvokeRepeating(nameof(Refresh), 1f, refreshRate);
    }

    private void Refresh()
    {
        // Clear existing leaderboard items
        foreach (Transform slot in leaderboardItemsParent.transform)
        {
            Destroy(slot.gameObject);
        }

        // Get and sort players by score
        var sortedPlayerList = 
            (from player in PhotonNetwork.PlayerList orderby player.GetScore() descending select player).ToList();

        int currentPlace = 1;
        int myPlace = 999;
        
        foreach (var player in sortedPlayerList)
        {
            GameObject _item = Instantiate(leaderboardItem, leaderboardItemsParent);

            if (player.UserId == PhotonNetwork.LocalPlayer.UserId)
            {
                myPlace = currentPlace; // Track player's place
            }

            _item.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = player.NickName;
            _item.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = player.GetScore().ToString();

            currentPlace++;
        }

        Debug.Log("My place in leaderboard " + currentPlace);
    }
}
