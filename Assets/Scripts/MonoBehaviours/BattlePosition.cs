using System;
using TMPro;
using UnityEngine;

public class BattlePosition : MonoBehaviour
{
    [field: SerializeField] public MonsterUnit MonsterHere { get; private set; } 
    private SpriteRenderer _spriteRenderer;
    
    private Transform _statusTextParent;
    private TextMeshProUGUI _healthText;

    public Trainer Player { get; private set; } 


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

    public void InitializePlayer(Trainer player)
    {
        if (this.Player != null) return;
        this.Player = player;
    }

    public void SwitchMonster(MonsterUnit newMonster)
    {
        Debug.Log("SwitchMonster called");
        
        //find the index of the monster that's switching in
        int indexOfNewMonster = Array.IndexOf(Player.team, newMonster);
        
        //since MonsterHere is currently out, it should be at index 0! 
        
        //swap their positions in the array, so that whatever monster is out is at index 0
        Player.team[0] = newMonster;
        Player.team[indexOfNewMonster] = MonsterHere;
        
        SendMonster(newMonster);
    }

    public void SendMonster(MonsterUnit newMonster)
    {
        //tell the monster that's here right now that it isn't anymore, assuming it exists.
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
        
        Battle.StaticMessage($"{MonsterHere.UnitName} has entered the battlefield!");
        
        UpdateStatusText();
    }

    public void UpdateStatus()
    {

        if (MonsterHere.Fainted)
        {
            _spriteRenderer.sprite = null;
        }

        UpdateStatusText();
    }

    public void UpdateStatusText()
    {
        UpdateHealthText();
    }

    private void UpdateHealthText()
    {
        _healthText.text = $"Health: {MonsterHere.GetStat(Stat.Health)}/{MonsterHere.GetMaxHealth()}";
    }

}
