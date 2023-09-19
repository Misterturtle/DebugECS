using System;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[UnityEngine.Scripting.Preserve]
public class NetcodeBootstrap : ClientServerBootstrap
{
    public override bool Initialize(string defaultWorldName)
    {
        AutoConnectPort = 7979;
        Debug.Log("Client Bootstrap");
        return base.Initialize(defaultWorldName);
    }
}