using FishNet.Managing;
using FishNet.Transporting;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class RivetUI : MonoBehaviour
{
    private NetworkManager _networkManager;
    private RivetAPI _rivetAPI;

    public GameObject joinMenuPanel;
    public TMP_Text connectionInfoText;

    private FindLobbyResponse? _findLobbyResponse;
    private LocalConnectionState? _connectionState;

    private void Start()
    {
        _networkManager = FindObjectOfType<NetworkManager>();
        _rivetAPI = FindObjectOfType<RivetAPI>();
        
        _networkManager.ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
        _networkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
        
        UpdateConnectionInfo();
    }
    
    private void OnDestroy()
    {
        if (_networkManager == null)
            return;

        _networkManager.ServerManager.OnServerConnectionState -= ServerManager_OnServerConnectionState;
        _networkManager.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
    }

    
    #region Server events
    private void ConnectToLobby(FindLobbyResponse res)
    {
        
        Debug.Log("Connecting to lobby");
        
        // Update UI
        _findLobbyResponse = res;
        UpdateConnectionInfo();

        // TODO: Don't auto-boot server
        var port = res.Ports["default"];
        _networkManager.ServerManager.StartConnection(port.Port);
        _networkManager.ClientManager.StartConnection(port.Hostname, port.Port);
        
        // Update UI again
        UpdateConnectionInfo();
    }
    
    private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs obj)
    {
        Debug.Log("Client connected");
        
        _connectionState = obj.ConnectionState;
        UpdateConnectionInfo();
    }


    private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs obj)
    {
        Debug.Log("Server connected");
    }
    #endregion
    
    #region UI events
    public void OnClick_Find()
    {
        // Hide menu
        joinMenuPanel.SetActive(false);
        
        // Find lobby
        StartCoroutine(_rivetAPI.FindLobby(new FindLobbyRequest
        {
            GameModes = new[] { "default" },
        }, ConnectToLobby, fail => { Debug.Log($"Failed to find lobby: {fail}"); }));
    }
    #endregion
    
    #region UI
    private void UpdateConnectionInfo()
    {
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
        connectionInfoText.text =
            $"Lobby ID: {(_findLobbyResponse.HasValue ? _findLobbyResponse.Value.Lobby.LobbyId : "?")}\n" +
            $"Host: {(_findLobbyResponse.HasValue ? _findLobbyResponse.Value.Ports["default"].Hostname : "?")}\n" +
            $"Port: {(_findLobbyResponse.HasValue ? _findLobbyResponse.Value.Ports["default"].Port : "?")}\n" +
            $"Connection state: {connectionState}\n";

    }
    #endregion
}