#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Managing;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public struct FindLobbyRequest
{
    [JsonProperty("game_modes")] public string[] GameModes { get; set; }
    [JsonProperty("regions")] public string[]? Regions { get; set; }
}

public struct FindLobbyResponse
{
    [JsonProperty("lobby")] public RivetLobby Lobby { get; set; }
    [JsonProperty("ports")] public Dictionary<string, RivetLobbyPort> Ports { get; set; }
    [JsonProperty("player")] public RivetPlayer Player { get; set; }
}

public struct RivetLobby
{
    [JsonProperty("lobby_id")] public string LobbyId { get; set; }
    [JsonProperty("host")] public string Host { get; set; }
    [JsonProperty("port")] public int Port { get; set; }
}

public struct RivetLobbyPort
{
    [JsonProperty("hostname")] public string? Hostname { get; set; }
    [JsonProperty("port")] public ushort Port { get; set; }
    [JsonProperty("is_tls")] public bool IsTls { get; set; }
}

public struct RivetPlayer
{
    [JsonProperty("token")] public string Token { get; set; }
}

public class RivetManager : MonoBehaviour
{
    public const ushort ServerPort = 8080;
        
    public string? rivetToken = null;
    
    #region References
    private NetworkManager _networkManager = null!;
    private RivetAuthenticator _authenticator = null!;
    #endregion

    /// <summary>
    /// The response from the last <see cref="FindLobby"/> call. Used to maintain information about the Rivet player &
    /// lobby.
    /// </summary>
    public FindLobbyResponse? FindLobbyResponse { get; private set; }

    private void Start()
    {
        _networkManager = FindObjectOfType<NetworkManager>();
        
        _authenticator = gameObject.AddComponent<RivetAuthenticator>();
        
        // TODO: Only do if server
        // Start server
        _networkManager.ServerManager.StartConnection(ServerPort);
        _networkManager.ServerManager.SetAuthenticator(_authenticator);
        StartCoroutine(LobbyReady(() => { Debug.Log("Lobby ready"); }, _ => { }));
    }

    #region Lobby

    /// <summary>
    /// <a href="https://rivet.gg/docs/matchmaker/api/lobbies/find">Documentation</a>
    /// </summary>
    /// 
    /// <param name="request"></param>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    /// <returns></returns>
    public IEnumerator FindLobby(FindLobbyRequest request, Action<FindLobbyResponse> success,
        Action<string> fail)
    {
        yield return PostRequest<FindLobbyRequest, FindLobbyResponse>("https://matchmaker.api.rivet.gg/v1/lobbies/find", request, res =>
        {
            // Save response
            FindLobbyResponse = res;
            
            // Connect to server
            // TODO: Don't auto-boot server
            var port = res.Ports["default"];
            _networkManager.ClientManager.StartConnection(port.Hostname, port.Port);
        
            success(res);
        }, fail);
    }

    /// <summary>
    /// <a href="https://rivet.gg/docs/matchmaker/api/lobbies/ready">Documentation</a>
    /// </summary>
    /// 
    /// <param name="success"></param>
    /// <param name="fail"></param>
    /// <returns></returns>
    public IEnumerator LobbyReady(Action success, Action<string> fail)
    {
        yield return PostRequest<Dictionary<string, string>, object>("https://matchmaker.api.rivet.gg/v1/lobbies/ready",
            new Dictionary<string, string>(), (_) => success(), fail);
    }

    #endregion

    #region Player

    /// <summary>
    /// <a href="https://rivet.gg/docs/matchmaker/api/players/connected">Documentation</a>
    /// </summary>
    /// 
    /// <param name="playerToken"></param>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    /// <returns></returns>
    public IEnumerator PlayerConnected(string playerToken, Action success, Action<string> fail)
    {
        yield return PostRequest<Dictionary<string, string>, object>(
            "https://matchmaker.api.rivet.gg/v1/players/connected",
            new Dictionary<string, string>
            {
                { "player_token", playerToken },
            }, (_) => success(), fail);
    }

    /// <summary>
    /// <a href="https://rivet.gg/docs/matchmaker/api/players/disconnected">Documentation</a>
    /// </summary>
    /// 
    /// <param name="playerToken"></param>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    /// <returns></returns>
    public IEnumerator PlayerDisconnected(string playerToken, Action success, Action<string> fail)
    {
        yield return PostRequest<Dictionary<string, string>, object>(
            "https://matchmaker.api.rivet.gg/v1/players/disconnected", new Dictionary<string, string>
            {
                { "player_token", playerToken },
            }, (_) => success(), fail);
    }

    #endregion

    #region Requests

    private string? GetToken()
    {
        var token = Environment.GetEnvironmentVariable("RIVET_TOKEN");
        if (token != null)
        {
            return token;
        }

        if (rivetToken != null && rivetToken.Length > 0)
        {
            return rivetToken;
        }

        throw new Exception("RIVET_TOKEN not set");
    }

    private IEnumerator PostRequest<TReq, TRes>(string url, TReq requestBody, Action<TRes> success, Action<string> fail)
    {
        var requestBodyStr = JsonConvert.SerializeObject(requestBody);
        Debug.Log("POST " + url + " " + requestBodyStr);

        var www = UnityWebRequest.Post(url, requestBodyStr, "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + GetToken());

        yield return www.SendWebRequest();

        switch (www.result)
        {
            case UnityWebRequest.Result.InProgress:
                Debug.Log("In progress");
                break;
            case UnityWebRequest.Result.Success:
                if (www.responseCode == 200)
                {
                    Debug.Log("Success: " + www.downloadHandler.text);
                    var responseBody = JsonConvert.DeserializeObject<TRes>(www.downloadHandler.text);
                    success(responseBody!);
                }
                else
                {
                    string statusError = "Error status " + www.responseCode + ": " + www.downloadHandler.text;
                    Debug.LogError(statusError);
                    fail(statusError);
                }

                break;
            case UnityWebRequest.Result.ConnectionError:
                string connectionError = "ConnectionError: " + www.error;
                Debug.LogError(connectionError);
                fail(connectionError);
                break;
            case UnityWebRequest.Result.ProtocolError:
                string protocolError = "ProtocolError: " + www.error + " " + www.downloadHandler.text;
                Debug.LogError(protocolError);
                fail(protocolError);
                break;
            case UnityWebRequest.Result.DataProcessingError:
                string dpe = "DataProcessingError: " + www.error;
                Debug.LogError(dpe);
                fail(dpe);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    #endregion
}