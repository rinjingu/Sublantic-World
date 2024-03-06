using System.Collections.Generic;
using UnityEngine;
public class Affiliation
{
    public int AffiliationID;
    public string AffiliationName;
    
    private AffiliationManager m_AffiliationManager;

    public Affiliation(){
        m_AffiliationManager = Object.FindObjectOfType<AffiliationManager>();
        m_AffiliationManager.AffiliationList.Add(this);
    }

    public AffiliationRelationship GetAffiliationRelationship(int affiliationID){
        return AffiliationManager.GetRelationship(AffiliationID, affiliationID);
    }

    public AffiliationRelationship GetAffiliationRelationship(Affiliation affiliation){
        return AffiliationManager.GetRelationship(AffiliationID, affiliation.AffiliationID);
    }
}

