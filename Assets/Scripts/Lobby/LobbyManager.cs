using IngameDebugConsole;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float lobbyUpdateTimer;
    private string playerName;


    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);

            DebugLogConsole.AddCommandInstance("CreateLobby", "Creates a lobby", "CreateLobby", this);
            DebugLogConsole.AddCommand("ListLobbies", "Lists lobbies", ListLobbies);
            DebugLogConsole.AddCommand<string>("JoinLobbyByCode", "Joins a lobby with a code", JoinLobbyByCode);
            DebugLogConsole.AddCommand("JoinLobby", "Joins a lobby", QuickJoinLobby);
            DebugLogConsole.AddCommandInstance("LeaveLobby", "Leaves a lobby", "LeaveLobby", this);
            DebugLogConsole.AddCommand("KickPlayer", "Kicks a player", KickPlayer);
            DebugLogConsole.AddCommand("DeleteLobby", "Deletes the lobby", DeleteLobby);
            DebugLogConsole.AddCommand<string>("ChangePlayerName", "Updates player name", UpdatePlayerName);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        playerName = "Ben" + UnityEngine.Random.Range(10, 99);
        Debug.Log(playerName);
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdates();
    }
    public async void CreateLobby()
    {
        try
        {
            string _lobbyName = "MyLobby";
            int _maxPlayers = 4;

            CreateLobbyOptions _createLobbyOptions = new CreateLobbyOptions()
            {
                IsPrivate = false,
                Player = GetPlayer()
            };

            Lobby _lobby = await LobbyService.Instance.CreateLobbyAsync(_lobbyName, _maxPlayers, _createLobbyOptions);

            hostLobby = _lobby;
            joinedLobby = hostLobby;

            PrintPlayers(hostLobby);
            Debug.Log("Created Lobby! " + _lobby.Name + " " + _lobby.MaxPlayers + " " + _lobby.Id + " " + _lobby.LobbyCode);
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void ListLobbies()
    {
        try
        {
            // Returns up to 25 lobbies that have more than 1 available slot in decending order based on server uptime
            QueryLobbiesOptions _queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };


            QueryResponse _queryResponse = await Lobbies.Instance.QueryLobbiesAsync(_queryLobbiesOptions);

            Debug.Log("Lobbies found: " + _queryResponse.Results.Count);
            foreach (Lobby _lobby in _queryResponse.Results)
            {
                Debug.Log(_lobby.Name + " " + _lobby.MaxPlayers);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void JoinLobbyByCode(string _lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };

            joinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(_lobbyCode, joinLobbyByCodeOptions);

            Debug.Log("Joined Lobby with code: " + _lobbyCode);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void QuickJoinLobby()
    {
        try
        {
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            playerName = newPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions()
            {
                Data = new Dictionary<string, PlayerDataObject>{
                {"PlayerName",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
                }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

    }


    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                {"PlayerName",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) },
            }
        };
    }

    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Players in lobby " +  lobby.Name);
        foreach(Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }

    private async void HandleLobbyPollForUpdates()
    {
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f)
            {
                float _lobbyUpdateTimerMax = 1.1f;
                lobbyUpdateTimer = _lobbyUpdateTimerMax;

                Debug.Log("Lobby Reference " + joinedLobby);

                Lobby _lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = _lobby;
            }
        }
    }
    // Lobbies that are created become inactive after 30 seconds of not receiving data
    // This is a heartbeat used to keep lobbies active
    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                float _heartbeatTimerMax = 20f;
                heartbeatTimer = _heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    public async void LeaveLobby()
    {
        if (joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);

                joinedLobby = null;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }
    private async void KickPlayer()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[1].Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
}
