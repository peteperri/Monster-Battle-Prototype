
using UnityEngine;

public class BattlePosition : MonoBehaviour
{
    private MonsterUnit monsterHere;
    private SpriteRenderer _spriteRenderer;
        
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SendMonster(MonsterUnit monster)
    {
        monsterHere = monster;
        _spriteRenderer.sprite = monster.GetSpecies().GetSprite();
    }

}
