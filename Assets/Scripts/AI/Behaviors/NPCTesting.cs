using UnityEngine;

[RequireComponent(typeof(NPCController))]
public class NPCTesting : MonoBehaviour {
    public Animator animator;
    public UVehicleController vehicleController;
    private NPCController m_NPCController;
    private NPCState m_NPCState;
    public Affiliation Affiliation;
    

    private class IdleState : IState{
        public static string Name => "Idle";
        public void Enter() {}
        public void Update() {}
        public void Exit() {}
            
    }

    private class AttackState : IState{
        public static string Name => "Attack";
        public void Enter() {}
        public void Update() {}
        public void Exit() {}
    }

    private void Start() {
        m_NPCState = new NPCState();
        m_NPCState.AddState(IdleState.Name, new IdleState());
        m_NPCState.AddState(AttackState.Name, new AttackState());
        m_NPCState.SetInitialState("Idle");

        m_NPCController = GetComponent<NPCController>();
        m_NPCController.OnAttack += OnAttack;
        m_NPCController.OnDetectedTarget += OnDetectedTarget;
        m_NPCController.OnLostTarget += OnLostTarget;
        m_NPCController.OnDamaged += OnDamaged;
    }

    private void OnAttack() {
    }

    private void OnDetectedTarget() {
        m_NPCState.ChangeState("Attack");
    }

    private void OnLostTarget() {
        m_NPCState.ChangeState("Idle");
    }

    private void OnDamaged() {
    }
}