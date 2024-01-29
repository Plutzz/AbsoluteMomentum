using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject lobbyMenu;
    [SerializeField] private GameObject lobbyHud;
    [SerializeField] private GameObject lobbyStartTrigger;
    [SerializeField] private TextMeshProUGUI lobbyCodeText;
    [SerializeField] private TMP_InputField joinCodeTextInput;


    private async void Start()
    {
        await UnityServices.InitializeAsync();

        if (AuthenticationService.Instance.IsSignedIn == false)
        {
            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
            };
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        else
        {
            mainMenu.transform.parent.gameObject.SetActive(false);
            //LeaveRelay();
        }
    }

    public void OpenLobbyMenu()
    {
        lobbyMenu.SetActive(true);
        mainMenu.SetActive(false);
    }
    public void CloseLobbyMenu()
    {
        lobbyMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void OpenSettings()
    {
        settingsMenu.SetActive(true);
        mainMenu.SetActive(false);
    }
    public void CloseSettings()
    {
        settingsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }



    #region LobbyMethods

    public async void HostRelay()
    {
        // Lobby Size (not including host)
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log("Join Code: " + joinCode);
            lobbyCodeText.text = joinCode;

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }

        lobbyHud.SetActive(true);
        lobbyMenu.SetActive(false);
    }

    public async void JoinRelay()
    {
        try
        {
            string joinCode = joinCodeTextInput.text;

            Debug.Log("Joining Relay with " + joinCode);

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }

        lobbyHud.SetActive(true);
        lobbyMenu.SetActive(false);
    }

    public void LeaveRelay()
    {
        Debug.Log("Disconnected Client");
        NetworkManager.Singleton.GetComponent<UnityTransport>().Shutdown();
        NetworkManager.Singleton.DisconnectClient(NetworkManager.Singleton.LocalClientId);
        NetworkManager.Singleton.Shutdown();
    }
    #endregion




}
