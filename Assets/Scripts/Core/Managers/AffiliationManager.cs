using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class AffiliationManager : MonoBehaviour {
    public List<Affiliation> AffiliationList;
    static public Dictionary<Tuple<int, int>, AffiliationRelationship> Relationships;
    static public AffiliationRelationship GetRelationship(int affiliation1, int affiliation2){
        if(affiliation1 == affiliation2){
            return AffiliationRelationship.Neutral;
        }
        return Relationships[new Tuple<int, int>(affiliation1, affiliation2)];
    }

    private void Start() {
        Relationships = new Dictionary<Tuple<int, int>, AffiliationRelationship>();
        AffiliationList = new List<Affiliation>();
    }

    public void RegisterAffiliation(Affiliation affiliation) {
        if (!AffiliationList.Contains(affiliation)) {
            AffiliationList.Add(affiliation);
        }
    }
}

public enum AffiliationRelationship{
    Neutral,
    Friendly,
    Hostile
}