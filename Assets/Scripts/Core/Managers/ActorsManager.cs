using System.Collections.Generic;
using UnityEngine;

public class ActorsManager : MonoBehaviour {
    public List<Actor> Actors { get; private set; }
    public GameObject Player { get; private set; }

    public void RegisterPlayer(GameObject player) => Player = player;
    public void UnregisterPlayer() => Player = null;

    public void RegisterActor(Actor actor) {
        if (!Actors.Contains(actor)) {
            Actors.Add(actor);
        }
    }

    public void UnregisterActor(Actor actor) {
        if (Actors.Contains(actor)) {
            Actors.Remove(actor);
        }
    }
    private void Awake() {
        Actors = new List<Actor>();
    }
}