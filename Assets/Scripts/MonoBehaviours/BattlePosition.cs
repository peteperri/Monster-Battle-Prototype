using UnityEngine;

public class BattlePosition : MonoBehaviour
{
    [field: SerializeField] public MonsterUnit MonsterHere { get; private set; }
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
        MonsterHere = monster;
        _spriteRenderer.sprite = monster.GetSpecies().Sprite;
    }

}
