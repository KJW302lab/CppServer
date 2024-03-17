using System.Collections.Generic;
using UnityEngine;

public class PlayerManager
{
    private MyPlayer _myPlayer;
    private Dictionary<int, Player> _players = new();

    private static PlayerManager _instance;
    public static PlayerManager Instance => _instance ??= new PlayerManager();

    public void Add(S_PlayerList packet)
    {
        Object obj = Resources.Load("Player");

        foreach (var p in packet.players)
        {
            GameObject go = Object.Instantiate(obj) as GameObject;

            if (p.isSelf)
            {
                MyPlayer myPlayer = go.AddComponent<MyPlayer>();
                myPlayer.PlayerId = p.playerId;
                myPlayer.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                _myPlayer = myPlayer;
            }
            else
            {
                Player player = go.AddComponent<Player>();
                player.PlayerId = p.playerId;
                player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                _players.Add(p.playerId, player);
            }
        }
    }
    
    public void Move(S_BroadcastMove packet)
    {
        if (_myPlayer.PlayerId == packet.playerId)
            _myPlayer.transform.position = new Vector3(packet.posX, packet.posY, packet.posZ);
        else
        {
            if (_players.TryGetValue(packet.playerId, out var player))
                player.transform.position = new Vector3(packet.posX, packet.posY, packet.posZ);
        }
    }

    public void EnterGame(S_BroadcastEnterGame packet)
    {
        if (packet.playerId == _myPlayer.PlayerId)
            return;
        
        Object obj = Resources.Load("Player");
        GameObject go = Object.Instantiate(obj) as GameObject;
        
        Player player = go.AddComponent<Player>();
        player.transform.position = new Vector3(packet.posX, packet.posY, packet.posZ);
        _players.Add(packet.playerId, player);
    }
    
    public void LeaveGame(S_BroadcastLeaveGame packet)
    {
        if (_myPlayer.PlayerId == packet.playerId)
        {
            GameObject.Destroy(_myPlayer.gameObject);
            _myPlayer = null;
        }
        else
        {
            if (_players.TryGetValue(packet.playerId, out var player))
            {
                GameObject.Destroy(player.gameObject);
                _players.Remove(packet.playerId);
            }
        }
    }
}
