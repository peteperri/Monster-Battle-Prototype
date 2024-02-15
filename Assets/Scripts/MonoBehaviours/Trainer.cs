using UnityEngine;

//a trainer is a player with a team of 6 monsters.
public class Trainer : MonoBehaviour
{
    [SerializeField] private int playerNum;
    public MonsterUnit[] team;
}
