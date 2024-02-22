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
    }

    public void SendMonster(MonsterUnit monster)
    {
        //if there is a monster here already...
        if (MonsterHere != null)
        {
            //tell the monster that's here right now that it isn't.
            MonsterHere.PositionInBattle = null;
        }

        //set the monster here to the new monster, passed in by the parameter
        MonsterHere = monster;
        
        //set that monster's position in battle to this battle position
        monster.PositionInBattle = this;
        
        //update sprite 
        _spriteRenderer.sprite = monster.GetSpecies().Sprite;
        
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
