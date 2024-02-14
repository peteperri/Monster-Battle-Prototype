using UnityEngine;

public class Battlefield : MonoBehaviour
{
    private Trainer _player1;
    private BattlePosition _p1MonA;
    private BattlePosition _p1MonB;
    
    private Trainer _player2;
    private BattlePosition _p2MonA;
    private BattlePosition _p2MonB;
    
    private void Start()
    {
        
        Initialize();
        
        TeamCheck(_player1);
        TeamCheck(_player2);

        SendOutMonsters(_player1, _p1MonA, _p1MonB);
        SendOutMonsters(_player2, _p2MonA, _p2MonB);
    }

    private void Update()
    {
        
    }

    private void Initialize()
    {
        Transform players = transform.Find("Players");
        _player1 = players.GetChild(0).GetComponent<Trainer>();
        _player2 = players.GetChild(1).GetComponent<Trainer>();

        Transform positions = transform.Find("Positions");
        Transform player1Positions = positions.GetChild(0);
        Transform player2Positions = positions.GetChild(1);

        _p1MonA = player1Positions.GetChild(0).GetComponent<BattlePosition>();
        _p1MonB = player1Positions.GetChild(1).GetComponent<BattlePosition>();

        _p2MonA = player2Positions.GetChild(0).GetComponent<BattlePosition>();
        _p2MonB = player2Positions.GetChild(1).GetComponent<BattlePosition>();
    }

    private static void SendOutMonsters(Trainer player, BattlePosition spotA, BattlePosition spotB)
    {
        spotA.SendMonster(player.team[0]);
        spotB.SendMonster(player.team[1]);
    }

    private static void TeamCheck(Trainer player)
    {
        if (player.team.Length == 0)
        {
            MonsterSpecies[] allSpecies = Resources.LoadAll<MonsterSpecies>("Monsters");
            GiveRandomTeam(player, allSpecies);
        }
        else
        {
            Debug.Log($"{player.team == null} {player.team.Length}");
        }
    }

    private static void GiveRandomTeam(Trainer player, MonsterSpecies[] allSpecies)
    {
        Debug.Log("Giving Random Team");
        MonsterUnit[] randomTeam = new MonsterUnit[6];
        for (int i = 0; i < 6; i++)
        {
            int randIndex = Random.Range(0, allSpecies.Length - 1);
            MonsterSpecies randomMonster = allSpecies[randIndex];
            randomTeam[i] = new MonsterUnit(randomMonster);
        }
        player.team = randomTeam;
    }
}
