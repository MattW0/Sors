using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public struct CreatePlayerMessage : NetworkMessage
{
    public string name;
}
