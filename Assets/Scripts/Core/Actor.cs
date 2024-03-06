using System;
using UnityEngine;

public class Actor : MonoBehaviour {
    private ActorsManager m_ActorsManager;
    public GameObject GameObject;
    public Affiliation Affiliation;

    private void Start() {
        m_ActorsManager = FindObjectOfType<ActorsManager>();
        
        if (!m_ActorsManager.Actors.Contains(this)) {
            m_ActorsManager.Actors.Add(this);
        }
    }

    private void OnDestroy() {
        if (m_ActorsManager.Actors.Contains(this)) {
            m_ActorsManager.Actors.Remove(this);
        }
    }
}