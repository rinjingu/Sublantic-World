using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance { get; private set; }

    public List<NPCController> NPCs { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            NPCs = new List<NPCController>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterNPC(NPCController npc)
    {
        NPCs.Add(npc);
    }

    public void UnregisterNPC(NPCController npc)
    {
        NPCs.Remove(npc);
    }

    public void SpawnNPC(NPCController npc, Vector3 position)
    {
        // check whether the position is valid, if not, raise an error and return
        if (Physics.OverlapSphere(position, 1f).Length > 0)
        {
            Debug.LogError("Cannot spawn NPC at " + position + " because there is something in the way");
            return;
        }

        // check whether the npc is already in the list, if not, add it
        if (!NPCs.Contains(npc))
        {
            RegisterNPC(npc);
        }

        // instantiate the npc at the position
        Instantiate(npc.npcObject, position, Quaternion.identity);
    }
}
