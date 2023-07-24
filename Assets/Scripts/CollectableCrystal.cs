using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CollectableCrystal : NetworkBehaviour
{
    public ShipManager shipManager;
    public float healthIncrease;


    public void DoCollect()
    {
        shipManager.IncreaseHealth(healthIncrease);
    }
}
