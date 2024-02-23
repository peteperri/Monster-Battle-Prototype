using System;
using TMPro;
using UnityEngine;

public class BattlePosition : MonoBehaviour
{
    [field: SerializeField] public MonsterUnit MonsterHere { get; private set; } 
    private SpriteRenderer _spriteRenderer;
    
    private Transform _statusTextParent;
    private TextMeshProUGUI _healthText;


    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _statusTextParent = transform.GetChild(0);
        _healthText = _statusTextParent.GetChild(0).GetComponent<TextMeshProUGUI>();
        if (_spriteRenderer == null)
        {
            gameObject.AddComponent<SpriteRenderer>();
        }

        MonsterHere = null;
    }

    public void SwitchMonster(MonsterUnit newMonster, Trainer trainer)
    {
        
        int indexOfMonsterSwitchingOut = Array.IndexOf(trainer.team, MonsterHere);
        
        //swap their positions in the array, so that the monster getting sent out is now at the front of the array. 
        MonsterUnit temp = trainer.team[0];
        trainer.team[0] = trainer.team[indexOfMonsterSwitchingOut];
        trainer.team[indexOfMonsterSwitchingOut] = temp;
        
        SendMonster(newMonster);
    }

    public void SendMonster(MonsterUnit newMonster)
    {

        //tell the monster that's here right now that it isn't, assuming it exists.
        if (MonsterHere != null)
        {
            MonsterHere.PositionInBattle = null;

        }
        
        //set the monster here to the new monster, passed in by the parameter
        MonsterHere = newMonster;
        
        //set that monster's position in battle to this battle position
        newMonster.PositionInBattle = this;
        
        //update sprite 
        _spriteRenderer.sprite = newMonster.GetSpecies().Sprite;
        
        UpdateStatusText();
    }

    public void UpdateStatusText()
    {
        UpdateHealthText();
    }

    private void UpdateHealthText()
    {
        _healthText.text = $"Health: {MonsterHere.GetCurrentHealth()}/{MonsterHere.GetMaxHealth()}";
    }

}
