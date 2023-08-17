using FishNet.Managing;
using FishNet.Transporting;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class RivetUI : MonoBehaviour
{
    private NetworkManager _networkManager;
    private RivetManager _rivetManager;

    public GameObject joinMenuPanel;
    public TMP_Text connectionInfoText;

    private LocalConnectionState? _connectionState;

    private void Start()
    {
        _networkManager = FindObjectOfType<NetworkManager>();
        _rivetManager = FindObjectOfType<RivetManager>();

        _networkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;

        UpdateConnectionInfo();
    }

    private void OnDestroy()
    {
        _networkManager.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
    }

    private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs obj)
    {
        _connectionState = obj.ConnectionState;
        UpdateConnectionInfo();
    }

    #region UI events

    public void OnClick_Find()
    {
        // Hide menu
        joinMenuPanel.SetActive(false);

        // Find lobby
        StartCoroutine(_rivetManager.FindLobby(new FindLobbyRequest
        {
            GameModes = new[] { "default" },
        }, _ => UpdateConnectionInfo(), fail => { Debug.Log($"Failed to find lobby: {fail}"); }));
    }

    #endregion

    #region UI

    private void UpdateConnectionInfo()
    {
        // Choose connection state text
        string connectionState;
        switch (_connectionState)
        {
            case null:
                connectionState = "?";
                break;
            case LocalConnectionState.Stopped:
                connectionState = "Stopped";
                break;
            case LocalConnectionState.Started:
                connectionState = "Started";
                break;
            case LocalConnectionState.Starting:
                connectionState = "Starting";
                break;
            default:
                connectionState = "Unknown";
                break;
        }

        // Update UI
        var flr = _rivetManager.FindLobbyResponse;
        connectionInfoText.text =
            $"Lobby ID: {(flr.HasValue ? flr.Value.Lobby.LobbyId : "?")}\n" +
            $"Host: {(flr.HasValue ? flr.Value.Ports["default"].Hostname : "?")}\n" +
            $"Port: {(flr.HasValue ? flr.Value.Ports["default"].Port : "?")}\n" +
            $"Connection state: {connectionState}\n";
    }

    #endregion
}