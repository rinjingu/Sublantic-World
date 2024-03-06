using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Affiliation", menuName = "Sublantic World/Affiliation", order = 0)]
public class Affiliation : ScriptableObject
{
    public int AffiliationID;
    public string AffiliationName;
    
    private AffiliationManager m_AffiliationManager;

    public AffiliationRelationship GetAffiliationRelationship(int affiliationID){
        return AffiliationManager.GetRelationship(AffiliationID, affiliationID);
    }

    public AffiliationRelationship GetAffiliationRelationship(Affiliation affiliation){
        return AffiliationManager.GetRelationship(AffiliationID, affiliation.AffiliationID);
    }
}

