using FishNet.Managing;
using FishNet.Transporting;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class RivetUI : MonoBehaviour
{
    private NetworkManager _networkManager;
    private RivetManager _rivetManager;

    public GameObject joinMenuPanel;
    public TMP_Text connectionInfoText;
    public TMP_InputField lobbyIdInputField;
    public Slider gravitySlider;

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
    
    public void OnClick_Join()
    {
        // Hide menu
        joinMenuPanel.SetActive(false);

        // Find lobby
        StartCoroutine(_rivetManager.JoinLobby(new JoinLobbyRequest
        {
            LobbyId = lobbyIdInputField.text,
        }, _ => UpdateConnectionInfo(), fail => { Debug.Log($"Failed to join lobby: {fail}"); }));
    }
    
    public void OnClick_Create()
    {
        // Hide menu
        joinMenuPanel.SetActive(false);

        // Find lobby
        StartCoroutine(_rivetManager.CreateLobby(new CreateLobbyRequest
        {
            GameMode = "custom",
            LobbyConfig = new JObject
            {
                { "gravity", gravitySlider.value }
            },
        }, _ => UpdateConnectionInfo(), fail => { Debug.Log($"Failed to create lobby: {fail}"); }));
    }
    
    public void OnClick_CopyLobbyId()
    {
        GUIUtility.systemCopyBuffer = _rivetManager.FindLobbyResponse?.Lobby.LobbyId ?? "";
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
            $"Lobby config: {_rivetManager.lobbyConfigRaw ?? "?"}\n" +
            $"Connection state: {connectionState}\n";
    }

    #endregion
}