using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RivetLobby : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(PostRequest());
    }

    private string GetToken()
    {
        return
            "dev_staging.eyJ0eXAiOiJKV1QiLCJhbGciOiJFZERTQSJ9.CLWIi8eVMhC1sMaJoDEaEgoQZpmvE_BdTSmXdoVT7VmZMyIvQi0KEgoQmfId86_FTHuxaP1VTZQ1rhoJMTI3LjAuMC4xIgwKB2RlZmF1bHQQkD8.yZ0sd8aLG4CNDrYkNlZRZUKAYqYdvHz4G1kaQNbnV6AvU-63On_vcZVvSDx6z-r80B9FtHo-PNnLO559ozb_Dw";
    }

    private IEnumerator PostRequest()
    {
        Debug.Log("Sending post request");
        
        var www = UnityWebRequest.Post("https://matchmaker.api.rivet.gg/v1/lobbies/ready", "{}", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + GetToken());

        // Set the JSON body data
        var bodyRaw = System.Text.Encoding.UTF8.GetBytes("{}");
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Error: " + www.error);
        }
        else
        {
            Debug.Log("Received: " + www.downloadHandler.text);
        }
    }
}

