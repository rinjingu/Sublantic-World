using System.Collections.Generic;
using Unity.FPS.AI;
using UnityEngine;
using UnityEngine.Events;

public class NPCController: MonoBehaviour
{
    private NPCManager m_NPCManager;
    private Actor m_Actor;
    private ActorsManager m_ActorsManager;
    public GameObject npcObject;
    public NPCNavigation nPCNavigation;

    public UnityAction OnAttack;
    public UnityAction OnDetectedTarget;
    public UnityAction OnLostTarget;
    public UnityAction OnDamaged;

    private void Start()
    {
        m_NPCManager = FindObjectOfType<NPCManager>();
        m_NPCManager.RegisterNPC(this);

        m_Actor = GetComponent<Actor>();
        m_ActorsManager = FindObjectOfType<ActorsManager>();
        m_ActorsManager.RegisterActor(m_Actor);

        nPCNavigation = GetComponent<NPCNavigation>();
        nPCNavigation.OnDetectedTarget += OnDetectedTarget;
        nPCNavigation.OnLostTarget += OnLostTarget;
        OnAttack += nPCNavigation.TryAttack;
    }

    public bool TryAttack()
    {
        if (OnAttack != null)
        {
            OnAttack.Invoke();
            return true;
        }
        return false;
    }
}