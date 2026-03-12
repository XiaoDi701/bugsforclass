using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InteractEntry
{
    public GameObject triggerObject;
    public List<GameObject> activateObjects;
}

[System.Serializable]
public class ChaosRule
{
    [Tooltip("Trigger when exactly this many chaos objects are active.")]
    public int activeCount;
    public List<GameObject> objectsToEnable;
    public List<GameObject> objectsToDisable;
}
