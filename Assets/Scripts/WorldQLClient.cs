using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NativeWebSocket;

using WorldqlFb;

[System.Serializable]
public class MessageEvent : UnityEvent<WorldqlFb.Messages.MessageT>
{

}

public class WorldQLClient : MonoBehaviour
{
    #region public vars
    //variables
    public string url = "ws://localhost:8080";
    public string uuid{get; private set;}
    #endregion
    #region events
    //events
    public MessageEvent rawMessage;
    public MessageEvent peerConnect;
    public MessageEvent peerDisconnect;
    public MessageEvent areaSubscribe;
    public MessageEvent areaUnsubscribe;
    public MessageEvent globalMessage;
    public MessageEvent localMessage;
    public MessageEvent recordCreate;
    public MessageEvent recordRead;
    public MessageEvent recordUpdate;
    public MessageEvent recordDelete;
    public MessageEvent recordReply;

    #endregion
    #region private vars
    private WebSocket ws;
    #endregion
    #region util/public functions
    async public void sendRawMessage(WorldqlFb.Messages.MessageT message){
        if (ws == null || uuid == null){
            return;
        }
        message.Replication = WorldqlFb.Messages.Replication.ExceptSelf;
        message.SenderUuid = uuid;
        byte[] fbeMessage = message.SerializeToBinary();
        await ws.Send(fbeMessage);
    }
    public void close(){
        ws.Close();
    }
    private void _handshake(WorldqlFb.Messages.MessageT message){
        if (uuid != null) return;
        if (message.Parameter == null) return;
        uuid = message.Parameter;
        WorldqlFb.Messages.MessageT hand = new WorldqlFb.Messages.MessageT();
        hand.Instruction = WorldqlFb.Messages.Instruction.Handshake;
        hand.WorldName = "@global";
        sendRawMessage(hand);
    }
    private void _handleMessage(byte[] data){
        WorldqlFb.Messages.MessageT message = WorldqlFb.Messages.MessageT.DeserializeFromBinary(data);
        Debug.Log("Message Instruction "+message.Instruction);
        rawMessage.Invoke(message);
        switch (message.Instruction)
        {
            case WorldqlFb.Messages.Instruction.Handshake:
                _handshake(message);
            break;
            case WorldqlFb.Messages.Instruction.PeerConnect:
                peerConnect.Invoke(message);
            break;
            case WorldqlFb.Messages.Instruction.PeerDisconnect:
                peerDisconnect.Invoke(message);
            break;
            case WorldqlFb.Messages.Instruction.AreaSubscribe:
                areaSubscribe.Invoke(message);
            break;
            case WorldqlFb.Messages.Instruction.AreaUnsubscribe:
                areaUnsubscribe.Invoke(message);
            break;
            case WorldqlFb.Messages.Instruction.GlobalMessage:
                globalMessage.Invoke(message);
            break;
            case WorldqlFb.Messages.Instruction.LocalMessage:
                localMessage.Invoke(message);
            break;
            case WorldqlFb.Messages.Instruction.RecordCreate:
                recordCreate.Invoke(message);
            break;
            case WorldqlFb.Messages.Instruction.RecordRead:
                recordRead.Invoke(message);
            break;
            case WorldqlFb.Messages.Instruction.RecordUpdate:
                recordUpdate.Invoke(message);
            break;
            case WorldqlFb.Messages.Instruction.RecordDelete:
                recordDelete.Invoke(message);
            break;
            case WorldqlFb.Messages.Instruction.RecordReply:
                recordReply.Invoke(message);
            break;
            default:

            break;
        }
    }
    #endregion
    #region setup
    async void Start()
    {
        //create the UnityEvents if they are not created
        #region events
        if (rawMessage == null){
            rawMessage = new MessageEvent();
        }
        if (peerConnect == null){
            peerConnect = new MessageEvent();
        }
        if (peerDisconnect == null){
            peerDisconnect = new MessageEvent();
        }
        if (areaSubscribe == null){
            areaSubscribe = new MessageEvent();
        }
        if (areaUnsubscribe == null){
            areaUnsubscribe = new MessageEvent();
        }
        if (globalMessage == null) {
            globalMessage = new MessageEvent();
        }
        if (localMessage == null) {
            localMessage = new MessageEvent();
        }
        if (recordCreate == null) {
            recordCreate = new MessageEvent();
        }
        if (recordRead == null) {
            recordRead = new MessageEvent();
        }
        if (recordUpdate == null) {
            recordUpdate = new MessageEvent();
        }
        if (recordDelete == null) {
            recordDelete = new MessageEvent();
        }
        if (recordReply == null) {
            recordReply = new MessageEvent();
        }
        #endregion
        ws = new WebSocket(url);
        ws.OnMessage += (byte[] bytes) =>
        {
            Debug.Log("websocket recieve");
            Debug.Log(bytes);
            _handleMessage(bytes);
        };
        ws.OnOpen += () =>
        {
            Debug.Log("Websocket Connected");
        };
        ws.OnClose += (e) =>
        {
            print(ws.State);
            if (ws.State != WebSocketState.Open){
                Debug.LogWarning("WebSocket Closed: "+e);
            }
        };
        ws.OnError += (string e) =>
        {
            Debug.LogError(e);
        };
        await ws.Connect();
    }
    #endregion
    #region update
    private void Update()
    {
        if(ws == null)
        {
            Debug.LogWarning("Websocket became Null");
            Start(); //re-initiliase if ws is null
            return;
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            WorldqlFb.Messages.MessageT message = new WorldqlFb.Messages.MessageT();
            message.Instruction = WorldqlFb.Messages.Instruction.GlobalMessage;
            message.Parameter = "pingus";
            message.WorldName = "echo";
            sendRawMessage(message);
        }
        ws.DispatchMessageQueue();  
    }
    #endregion

    async void OnApplicationQuit()
    {
        if (ws.State == WebSocketState.Open){
            await ws.Close();
        }
    }
}
