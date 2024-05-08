
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PlayerMovementAbilityState", menuName = "PlayerMovementAbility")]
public class PlayerMovementAbilityState : UnitState
{
    [SerializeField]
    UnitAbilityState inherited_ability_base;

    public override void OnEnter(Unit unit)
    {
        if (inherited_ability_base == null)
        {
            inherited_ability_instance = UnitStateMachine.UnitStateFactory.CreateUnitState(inherited_ability_base, StateMachine) as UnitAbilityState;
        }

        m_player_controller = unit.GetComponent<PlayerController>();
        StateMachine.QueueAddState(inherited_ability_instance);
    }

    // ~ Handles
    private PlayerController m_player_controller;
    private UnitAbilityState inherited_ability_instance;
}

