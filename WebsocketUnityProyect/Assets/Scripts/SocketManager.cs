using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class SocketManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private TextMeshProUGUI chatText;

    [SerializeField] private GameObject playerGameObject;

    private Dictionary<string, GameObject> playerDictionary = new Dictionary<string, GameObject>();

    [SerializeField] private string serviceServerUrl = "ws://localhost:3000";
    [SerializeField] private string chatServerUrl = "ws://localhost:3001";
    [SerializeField] private string gameServerUrl = "ws://localhost:3002";
    [SerializeField] private string storeServerUrl = "ws://localhost:3003";

    public static WebSocket serviceServerWS { get; private set; }
    public static WebSocket chatWS { get; private set; }
    public static WebSocket gameWS { get; private set; }
    public static WebSocket storeWS { get; private set; }

    private bool isGameServerRunning = true;
    private bool isChatServerRunning = true;
    private bool isStoreServerRunning = true;

    void Start()
    {
        ConnectToServiceServer();
        Debug.Log(gameWS);
    }

    public void SendMessage()
    {
        string message = messageInput.text;

        if (!string.IsNullOrEmpty(message))
        {
            JSONNode json = new JSONObject();
            json["type"] = "chat";
            json["message"] = message;

            chatWS.Send(json.ToString());
            messageInput.text = "";
            chatText.text += "You: " + message + "\n";
        }
    }

    public void SendUsername()
    {
        string username = usernameInput.text;

        if (!string.IsNullOrEmpty(username))
        {
            JSONNode json = new JSONObject();
            json["type"] = "username";
            json["message"] = username;

            chatWS.Send(json.ToString());
        }
    }

    //public static void SendPlayerPosition(Vector2 position)
    //{
    //    if (gameWS != null && gameWS.IsAlive)
    //    {
    //        JSONNode json = new JSONObject();
    //        json["type"] = "position";
    //        json["message"] = Vector2ToString(position);

    //        gameWS.Send(json.ToString());
    //    }
    //    else
    //    {
    //        Debug.LogWarning("Game WS is not connected.");
    //    }
    //}

    Vector2 ParseVector2(string s)
    {
        string[] components = s.Split(',');
        return new Vector2(float.Parse(components[0]), float.Parse(components[1]));
    }

    static string Vector2ToString(Vector2 v)
    {
        return string.Format("{0},{1}", v.x, v.y);
    }

    void HandlePlayerPosition(string username, Vector2 position)
    {
        if (!playerDictionary.ContainsKey(username))
        {
            GameObject gameObject = Instantiate(playerGameObject);
            playerDictionary.Add(username, gameObject);
        }

        GameObject player = playerDictionary[username];
        player.transform.position = position;
    }

    #region ToggleServers
    public void ToggleGameServer()
    {
        isGameServerRunning = !isGameServerRunning;
        if (serviceServerWS != null && serviceServerWS.ReadyState == WebSocketState.Open)
        {
            JSONNode json = new JSONObject();
            json["type"] = "server";
            json["message"] = "game";
            serviceServerWS.Send(json.ToString());
        }
    }

    public void ToggleChatServer()
    {
        isChatServerRunning = !isChatServerRunning;
        if (serviceServerWS != null && serviceServerWS.ReadyState == WebSocketState.Open)
        {
            JSONNode json = new JSONObject();
            json["type"] = "server";
            json["message"] = "chat";
            serviceServerWS.Send(json.ToString());
        }
    }

    public void ToggleStoreServer()
    {
        isStoreServerRunning = !isStoreServerRunning;
        if (serviceServerWS != null && serviceServerWS.ReadyState == WebSocketState.Open)
        {
            JSONNode json = new JSONObject();
            json["type"] = "server";
            json["message"] = "store";
            serviceServerWS.Send(json.ToString());
        }
    }
    #endregion

    #region servers
    public void ConnectToServiceServer()
    {
        serviceServerWS = new WebSocket(serviceServerUrl);
        serviceServerWS.OnOpen += (sender, e) =>
        {
            Debug.Log("Connected to service server");
        };
        serviceServerWS.OnMessage += (sender, e) =>
        {
            JSONNode jsonMessage = JSON.Parse(e.Data);
            string type = jsonMessage["type"];
            string message = jsonMessage["message"];

            
        };
        serviceServerWS.OnClose += (sender, e) =>
        {
            Debug.Log("Disconnected from service server");
        };
        serviceServerWS.Connect();
    }

    public void DisconnectFromServiceServer()
    {
        if (serviceServerWS != null)
        {
            serviceServerWS.Close();
            serviceServerWS = null;
        }
    }

    public void ConnectToChatServer()
    {
        chatWS = new WebSocket(chatServerUrl);
        chatWS.OnOpen += (sender, e) =>
        {
            Debug.Log("Connected to chat server");
        };

        chatWS.OnMessage += (sender, e) =>
        {
            JSONNode jsonMessage = JSON.Parse(e.Data);
            string type = jsonMessage["type"];

            if (type == "chat")
            {
                string username = jsonMessage["username"];
                string message = jsonMessage["message"];
                chatText.text += username + ": " + message + "\n";
            }
        };

        chatWS.OnClose += (sender, e) =>
        {
            Debug.Log("Disconnected from chat server");
        };

        chatWS.Connect();

        if (chatWS.IsAlive)
        {
            SendUsername();
        }
    }

    public void DisconnectFromChatServer()
    {
        if (chatWS != null)
        {
            chatWS.Close();
            chatWS = null;
        }
    }

    public void ConnectToGameServer()
    {
        gameWS = new WebSocket(gameServerUrl);
        gameWS.OnOpen += (sender, e) =>
        {
            Debug.Log("Connected to game server");
        };

        gameWS.OnMessage += (sender, e) =>
        {
            JSONNode jsonMessage = JSON.Parse(e.Data);
            string type = jsonMessage["type"];
            
            if (type == "position")
            {
                string username = jsonMessage["username"];
                Vector2 position = ParseVector2(jsonMessage["message"]);
                HandlePlayerPosition(username, position);
            }
            else if (type == "move")
            {
                GameController.Instance.HandleOpponentMove(jsonMessage["message"]);
            }
        };

        gameWS.OnClose += (sender, e) =>
        {
            Debug.Log("Disconnected from game server");
        };

        gameWS.Connect();
    }

    public void DisconnectFromGameServer()
    {
        if (gameWS != null)
        {
            gameWS.Close();
            gameWS = null;
        }
    }

    public void ConnectToStoreServer()
    {
        storeWS = new WebSocket(storeServerUrl);
        storeWS.OnOpen += (sender, e) =>
        {
            Debug.Log("Connected to store server");
        };

        storeWS.OnMessage += (sender, e) =>
        {
            JSONNode jsonMessage = JSON.Parse(e.Data);
            string type = jsonMessage["type"];

            if (type == "store")
            {
                bool status = jsonMessage["status"];
                Debug.Log("Store status received: " + status);
            }
        };

        storeWS.OnClose += (sender, e) =>
        {
            Debug.Log("Disconnected from store server");

        };

        storeWS.Connect();
    }

    public void DisconnectFromStoreServer()
    {
        if (storeWS != null)
        {
            storeWS.Close();
            storeWS = null;
        }
    }
    #endregion
}
