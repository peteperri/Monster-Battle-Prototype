using UnityEngine;

public class BattlePosition : MonoBehaviour
{
    [SerializeField] private MonsterUnit monsterHere;
    private SpriteRenderer _spriteRenderer;


    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
        {
            gameObject.AddComponent<SpriteRenderer>();
        }
    }

    public void SendMonster(MonsterUnit monster)
    {
        monsterHere = monster;
        _spriteRenderer.sprite = monster.GetSpecies().Sprite;
    }

}
