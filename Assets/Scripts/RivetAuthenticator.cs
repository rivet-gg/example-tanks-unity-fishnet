using System;
using FishNet.Authenticating;
using FishNet.Broadcast;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Transporting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class RivetAuthenticator : Authenticator
{
    #region Types

    public struct TokenRequestBroadcast : IBroadcast
    {
        public string Token;
    }

    public struct TokenResponseBroadcast : IBroadcast
    {
        public bool Valid;
    }

    #endregion

    #region References

    private RivetManager _rivetManager;

    #endregion
    
    void Start()
    {
        _rivetManager = FindObjectOfType<RivetManager>();
    }

    #region Authentication
    public override event Action<NetworkConnection, bool> OnAuthenticationResult;
    
    public override void InitializeOnce(NetworkManager networkManager)
    {
        base.InitializeOnce(networkManager);

        base.NetworkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
        base.NetworkManager.ServerManager.RegisterBroadcast<TokenRequestBroadcast>(OnTokenRequestBroadcast, false);
        base.NetworkManager.ClientManager.RegisterBroadcast<TokenResponseBroadcast>(OnTokenResponseBroadcast);
    }

    private void SendAuthenticationResponse(NetworkConnection conn, bool isValid)
    {
        var trb = new TokenResponseBroadcast()
        {
            Valid = isValid,
        };
        base.NetworkManager.ServerManager.Broadcast(conn, trb, false);
        OnAuthenticationResult?.Invoke(conn, isValid);
    }
    #endregion


    #region Connection events

    private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState != LocalConnectionState.Started)
            return;

        // Send request
        var token = _rivetManager.FindLobbyResponse?.Player.Token;
        Debug.Log("Sending authenticate token request: " + token);
        var pb = new TokenRequestBroadcast()
        {
            Token = token
        };
        base.NetworkManager.ClientManager.Broadcast(pb);
    }

    private void OnTokenRequestBroadcast(NetworkConnection conn, TokenRequestBroadcast trb)
    {
        // Client already authenticated, potentially an attack
        if (conn.Authenticated)
        {
            Debug.Log("Client already authenticated");
            conn.Disconnect(true);
            return;
        }
        
        // Check validity
        Debug.Log("Validating token: " + trb.Token);
        StartCoroutine(_rivetManager.PlayerConnected(
            trb.Token,
            () => { SendAuthenticationResponse(conn, true); },
            (_) => { SendAuthenticationResponse(conn, false); }
        ));
    }

    private void OnTokenResponseBroadcast(TokenResponseBroadcast trb)
    {
        Debug.Log("Token response: " + trb.Valid);
        string result = (trb.Valid) ? "Token authenticated." : "Token authentication failed.";
        NetworkManager.Log(result);
    }

    #endregion
}