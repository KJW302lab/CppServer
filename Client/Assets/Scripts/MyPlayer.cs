using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class MyPlayer : Player
{
    private NetworkManager _networkManager;
    
    private void Start()
    {
        _networkManager ??= GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        
        StartCoroutine(CoSendPacket());
    }

    IEnumerator CoSendPacket()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.25f);

            C_Move movePacket = new C_Move
            {
                posX = Random.Range(-50, 50),
                posY = 0,
                posZ = Random.Range(-50, 50)
            };

            ArraySegment<byte> segment = movePacket.Write();
            _networkManager.Send(segment);
        }
    }
}
