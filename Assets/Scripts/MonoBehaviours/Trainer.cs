using UnityEngine;

//a trainer is a player with a team of 6 monsters.
public class Trainer : MonoBehaviour
{
    public int PlayerNum { get; private set; }
    public MonsterUnit[] team;

    private void Awake()
    {
        if (name.Contains("1"))
        {
            PlayerNum = 1;
        }
        
        else if (name.Contains("2"))
        {
            PlayerNum = 2;
        }

    }
}
