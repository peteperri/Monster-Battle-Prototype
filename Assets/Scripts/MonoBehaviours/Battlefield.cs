using UnityEngine;

//the battlefield class will handle the basic gameplay loop of a battle
public class Battlefield : MonoBehaviour
{
    private Trainer _player1;
    private BattlePosition _p1MonA;
    private BattlePosition _p1MonB;
    
    private Trainer _player2;
    private BattlePosition _p2MonA;
    private BattlePosition _p2MonB;
    
    private void Awake()
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

    private void Start()
    {
        TeamCheck(_player1);
        TeamCheck(_player2);

        SendOutMonsters(_player1, _p1MonA, _p1MonB);
        SendOutMonsters(_player2, _p2MonA, _p2MonB);
    }
    
    private static void SendOutMonsters(Trainer player, BattlePosition spotA, BattlePosition spotB)
    {
        spotA.SendMonster(player.team[0]);
        spotB.SendMonster(player.team[1]);
    }

    //this method detects if a player has no team. if they have no team, give them a random one.
    private static void TeamCheck(Trainer player)
    {
        if (player.team.Length == 0)
        {
            MonsterSpecies[] allSpecies = Resources.LoadAll<MonsterSpecies>("Monsters");
            GiveRandomTeam(player, allSpecies);
        }
    }

    //TODO: Make it so random teams do not feature duplicate monsters
    private static void GiveRandomTeam(Trainer player, MonsterSpecies[] allSpecies)
    {
        MonsterUnit[] randomTeam = new MonsterUnit[6];
        for (int i = 0; i < 6; i++)
        {
            int randIndex = Random.Range(0, allSpecies.Length - 1);
            MonsterSpecies randomMonster = allSpecies[randIndex];
            //randomTeam[i] = new MonsterUnit(randomMonster);
            
            /*TODO: remove/comment out this line and uncomment the above.
             using random natures to ensure stat calculation is working; 
             random teams will all have neutral natures normally*/
            randomTeam[i] = new MonsterUnit(randomMonster, NatureHelper.GetRandomNature());
        }
        player.team = randomTeam;
    }
}
