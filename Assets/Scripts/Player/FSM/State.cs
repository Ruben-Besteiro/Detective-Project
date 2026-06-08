using UnityEngine;

public abstract class State
{
    protected PlayerCombatController controller;

    public State(PlayerCombatController controller)
    {
        this.controller = controller;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void HandleInput() { }
    public virtual void Update() { }
}
