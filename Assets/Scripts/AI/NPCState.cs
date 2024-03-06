using System.Collections.Generic;
public class NPCState
{
    private Dictionary<string, IState> states;
    private IState currentState;

    public NPCState()
    {
        states = new Dictionary<string, IState>();
    }

    public void AddState(string stateName, IState state)
    {
        states[stateName] = state;
    }

    public void SetInitialState(string stateName)
    {
        if (states.ContainsKey(stateName))
        {
            currentState = states[stateName];
            currentState.Enter();
        }
        else
        {
            throw new System.ArgumentException($"State '{stateName}' does not exist.");
        }
    }

    public void ChangeState(string stateName)
    {
        if (states.ContainsKey(stateName))
        {
            currentState.Exit();
            currentState = states[stateName];
            currentState.Enter();
        }
        else
        {
            throw new System.ArgumentException($"State '{stateName}' does not exist.");
        }
    }

    public void Update()
    {
        currentState.Update();
    }
}

public interface IState
{
    void Enter();
    void Update();
    void Exit();
}