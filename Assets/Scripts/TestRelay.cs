using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class TestRelay : MonoBehaviour
{
    [SerializeField] private TMP_InputField textField;
    [SerializeField] private TextMeshProUGUI lobbyCode;


    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void CreateRelay()
    {
        // Lobby Size (not including host)
        try
        {
           Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);

           string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

           Debug.Log("Join Code: " + joinCode);
           lobbyCode.text = joinCode;

           RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
           NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

           NetworkManager.Singleton.StartHost();
        } catch (RelayServiceException e)
        {
            Debug.Log(e);
        }

    }

    public async void JoinRelay()
    {
        try
        {
            string joinCode = textField.text;

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
    }
}
