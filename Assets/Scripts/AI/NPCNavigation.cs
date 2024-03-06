using System.Linq;
using UnityEngine;
using UnityEngine.Events;
public class NPCNavigation : MonoBehaviour {
    public Transform DetectionSourcePoint;
    public float DetectionFraction = 1f;
    public float KnownTargetTimeout = 4f;
    public Animator Animator;
    public UnityAction OnDetectedTarget;
    public UnityAction OnLostTarget;
    public GameObject KnownDetectedTarget { get; private set; }
    public bool IsTargetInAttackRange { get; private set; }
    public bool IsSeeingTarget { get; private set; }
    public bool HadKnownTarget { get; private set; }
    protected float TimeLastSeenTarget = Mathf.NegativeInfinity;
    private ActorsManager m_ActorsManager;
    
    private void Start() {
        m_ActorsManager = FindObjectOfType<ActorsManager>();
    }

    public virtual void HandleTargetDetection(Actor actor, Collider[] selfColliders, float detectionRange){
        if (KnownDetectedTarget && !IsSeeingTarget && (Time.time - TimeLastSeenTarget) > KnownTargetTimeout){
            KnownDetectedTarget = null;
        }

        var effectiveDetectionRange = detectionRange * DetectionFraction;
        var sqrDetectionRange = effectiveDetectionRange * effectiveDetectionRange;
        IsSeeingTarget = false;
        var closestSqrDistance = Mathf.Infinity;

        foreach (Actor otherActor in m_ActorsManager.Actors){
            if (actor.Affiliation.GetAffiliationRelationship(otherActor.Affiliation) != AffiliationRelationship.Hostile){
                continue;
            }

            float sqrDistance = (otherActor.transform.position - DetectionSourcePoint.position).sqrMagnitude;

            if (sqrDistance > sqrDetectionRange || sqrDistance > closestSqrDistance){
                continue;
            }

            RaycastHit[] hits = Physics.RaycastAll(
                DetectionSourcePoint.position,
                (otherActor.GameObject.transform.position - DetectionSourcePoint.position).normalized,
                effectiveDetectionRange,
                -1,
                QueryTriggerInteraction.Ignore
            );

            RaycastHit closestValidHit = new RaycastHit();
            closestValidHit.distance = Mathf.Infinity;
            bool foundValidHit = false;
            foreach (var hit in hits){
                if (!selfColliders.Contains(hit.collider) && hit.distance < closestValidHit.distance){
                    closestValidHit = hit;
                    foundValidHit = true;
                }
            }

            if (foundValidHit){
                Actor hitActor = closestValidHit.collider.GetComponentInParent<Actor>();
                if (hitActor == otherActor){
                    IsSeeingTarget = true;
                    closestSqrDistance = sqrDistance;
                    KnownDetectedTarget = otherActor.GameObject;
                    TimeLastSeenTarget = Time.time;
                }
            }
        }

        // IsTargetInAttackRange = IsSeeingTarget && closestSqrDistance < actor.AttackRange * actor.AttackRange;

        if (!HadKnownTarget && KnownDetectedTarget != null){
            OnDetect();
        }
        else if (HadKnownTarget && KnownDetectedTarget == null){
            OnLost();
        }
        

    }

    public virtual void OnDetect(){
        OnDetectedTarget?.Invoke();
    }

    public virtual void OnLost(){
        OnLostTarget?.Invoke();
    }
}