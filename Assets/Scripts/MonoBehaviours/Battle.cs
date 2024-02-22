using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

//the battlefield class will handle the basic gameplay loop of a battle
public class Battle : MonoBehaviour
{
    private Trainer _player1;
    private BattlePosition _p1MonA;
    private BattlePosition _p1MonB; //only used in double battles
    
    private Trainer _player2;
    private BattlePosition _p2MonA;
    private BattlePosition _p2MonB; //only used in double battles

    private Transform _actionPrompt;
    private Transform _attackTextObjects;
    private Transform _switchTextObjects;

    private TextMeshProUGUI _actionPromptText;
    private TextMeshProUGUI _attackOptionsText;
    private TextMeshProUGUI _switchOptionsText;

    private BattleState _state;
    private int _p1Choice = -1;
    private int _p2Choice = -1;
    
    
    [SerializeField] private bool isDoubleBattle = false;
    
    
    private void Awake()
    {
        //initialize players
        Transform players = transform.Find("Players");
        _player1 = players.GetChild(0).GetComponent<Trainer>();
        _player2 = players.GetChild(1).GetComponent<Trainer>();

        //initialize positions
        Transform positions = transform.Find("Positions");
        Transform player1Positions = positions.GetChild(0);
        Transform player2Positions = positions.GetChild(1);
        _p1MonA = player1Positions.GetChild(0).GetComponent<BattlePosition>();
        _p2MonA = player2Positions.GetChild(0).GetComponent<BattlePosition>();

        //check for double battle, initialize those positions if necessary
        if (isDoubleBattle)
        {
            _p1MonB = player1Positions.GetChild(1).GetComponent<BattlePosition>();
            _p2MonB = player2Positions.GetChild(1).GetComponent<BattlePosition>();
        }
        
        //initialize UI references 
        Transform canvas = GameObject.Find("MainCanvas").transform;
        _actionPrompt = canvas.GetChild(0);
        _attackTextObjects = _actionPrompt.GetChild(0);
        _switchTextObjects = _actionPrompt.GetChild(1);

        _actionPromptText = _actionPrompt.GetComponent<TextMeshProUGUI>();
        _attackOptionsText = _attackTextObjects.GetChild(1).GetComponent<TextMeshProUGUI>();
        _switchOptionsText = _switchTextObjects.GetChild(1).GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        TeamCheck(_player1);
        TeamCheck(_player2);

        if (isDoubleBattle)
        {
            SendOutMonsters(_player1, _p1MonA, _p1MonB);
            SendOutMonsters(_player2, _p2MonA, _p2MonB);
        }
        else
        {
            SendOutMonsters(_player1, _p1MonA);
            SendOutMonsters(_player2, _p2MonA);
        }

        _state = BattleState.WaitingOnPlayer1Choice;
        ShowActionPrompts(1, _p1MonA.MonsterHere);
    }

    private void Update()
    {
        for (int i = 0; i <= 9; i++)
        {
           KeyCode key = KeyCode.Alpha0 + i; // KeyCode.Keypad0 is the starting enum value for the keypad numbers
           if (Input.GetKeyDown(key))
           {
               SelectPlayerChoice(i);
               break; // Exit the loop once a key is found and processed
           }
        }
    }

    private void ExecutePlayerActions()
    {
        PlayerAction p1Action = SelectPlayerActionType(1);
        PlayerAction p2Action = SelectPlayerActionType(2);
        
        //executed if anyone selected forfeit
        HandleForfeit(p1Action, p2Action);
           
        //executed if both players attack
        BothPlayersAttack(p1Action, p2Action);
           
           
           
        //resets everything at the end 
        ShowActionPrompts(1, _p1MonA.MonsterHere);
    }

    private void HandleForfeit(PlayerAction p1Action, PlayerAction p2Action)
    {
        //if anyone forfeits, restart the game 
        if (p1Action == PlayerAction.Forfeit || p2Action == PlayerAction.Forfeit)
        {
            //TODO: make this better
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void BothPlayersAttack(PlayerAction p1Action, PlayerAction p2Action)
    {
        //if they are both attacking, speed now matters.
        if (p1Action == PlayerAction.Attack && p2Action == PlayerAction.Attack)
        {
            int p1AttackIndex = _p1Choice - 1;
            int p2AttackIndex = _p2Choice - 1;
               
            MonsterUnit p1Mon = _p1MonA.MonsterHere;
            MonsterUnit p2Mon = _p2MonA.MonsterHere;
               
            Attack p1Attack = p1Mon.KnownAttacks[p1AttackIndex];
            Attack p2Attack = p2Mon.KnownAttacks[p2AttackIndex];

            //if they are both attacking, and those attacks have equivalent priority, the result comes down to the readiness stat (speed, in pokemon).
            if (p1Attack.Priority == p2Attack.Priority)
            {
                int p1Speed = _p1MonA.MonsterHere.GetReadiness();
                int p2Speed = _p2MonA.MonsterHere.GetReadiness();

                if (p1Speed > p2Speed)
                {
                    ActuallyAttack(p1Mon, p1AttackIndex, p2Mon, p2AttackIndex);
                }
                else if (p1Speed < p2Speed)
                {
                    ActuallyAttack(p2Mon, p2AttackIndex, p1Mon, p1AttackIndex);
                }
                   
                //speed tie! whoever attacks first is RANDOM!
                else
                {
                    SpeedTieAttack(p1Mon, p1AttackIndex, p2Mon, p2AttackIndex);
                }
            }
        }
    }

    private void ActuallyAttack(MonsterUnit first, int firstAttackIndex, MonsterUnit second, int secondAttackIndex)
    {
        first.UseAttack(firstAttackIndex, new[]{second});

        if (!second.Fainted)
        {
            second.UseAttack(secondAttackIndex, new[]{first});
        }

        _state = BattleState.WaitingOnPlayer1Choice;
    }

    private void SpeedTieAttack(MonsterUnit p1, int p1Index, MonsterUnit p2, int p2Index)
    {
        int rand = Random.Range(0, 1);

        if (rand == 0)
        {
            ActuallyAttack(p1, p1Index, p2, p2Index);
        }
        else
        {
            ActuallyAttack(p2, p2Index, p1, p1Index);
        }
    }

    private PlayerAction SelectPlayerActionType(int playerNum)
    {
        switch (playerNum)
        {
            case 1:
            case 2:
            case 3:
            case 4:
                return PlayerAction.Attack;
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
                return PlayerAction.Switch;
            case 0:
                return PlayerAction.Forfeit;
        }

        //THIS SHOULD NEVER HAPPEN
        return PlayerAction.Empty;
    }

    private void SelectPlayerChoice(int buttonPressed)
    {
        Debug.Log($"Pressed {buttonPressed}");
        if (_state == BattleState.WaitingOnPlayer1Choice)
        {
            _p1Choice = buttonPressed;
            _state = BattleState.WaitingOnPlayer2Choice;
            ShowActionPrompts(2, _p2MonA.MonsterHere);
        }
        else if (_state == BattleState.WaitingOnPlayer2Choice)
        {
            _p2Choice = buttonPressed;
            _state = BattleState.ExecutingActions;
            ExecutePlayerActions();
        }
    }

    //temp UI solution for prototyping purposes
    private void ShowActionPrompts(int playerNum, MonsterUnit monsterBattling)
    {
        _actionPrompt.gameObject.SetActive(false);
        _attackOptionsText.gameObject.SetActive(false);
        _switchOptionsText.gameObject.SetActive(false);
        
        UpdatePromptText(playerNum, monsterBattling);
        UpdateAttackText(monsterBattling);
        UpdateSwitchText(playerNum, monsterBattling);
    }

    
    //TODO: refactor all three "update text" methods, as they contain a lot of repeated logic.
    private void UpdatePromptText(int playerNum, MonsterUnit monsterBattling)
    {
        //reset the options text to the default, so we can replace stuff again
        
        const string defaultActionPromptText = "What will {PLAYER}'s {MONSTER} do?";
        _actionPromptText.text = defaultActionPromptText;
        
        string playerText = "Player " + playerNum;
        string newActionPromptText = defaultActionPromptText.Replace("{PLAYER}", playerText);
        string monsterName = monsterBattling.UnitName;
        newActionPromptText = newActionPromptText.Replace("{MONSTER}", monsterName);
        _actionPromptText.text = newActionPromptText;
        _actionPrompt.gameObject.SetActive(true);
    }
    
    private void UpdateAttackText(MonsterUnit monsterBattling)
    {
        //reset the options text to the default, so we can replace stuff again
        const string defaultAttackPromptText = "1- {ATTACK_1}\n2- {ATTACK_2}\n3- {ATTACK_3}\n4- {ATTACK_4}\n0- Forfeit (end battle)";
        _attackOptionsText.text = defaultAttackPromptText;
        
        const int moveCount = 4; //a monster should only have 4 moves... for now!
        string replaceMeTemplate = "{ATTACK_";
        string newAttackPromptText = _attackOptionsText.text;
        for (int i = 1; i <= moveCount; i++)
        {
            string toReplace = replaceMeTemplate + i + "}";
            string attackName = monsterBattling.KnownAttacks[i - 1].AttackName;
            newAttackPromptText = newAttackPromptText.Replace(toReplace, attackName);
        }
        _attackOptionsText.text = newAttackPromptText;
        _attackOptionsText.gameObject.SetActive(true);
    }

    private void UpdateSwitchText(int playerNum, MonsterUnit monsterBattling)
    {
        //reset the options text to the default, so we can replace stuff again
        const string defaultSwitchPromptText = "5- {MONSTER_1}\n6- {MONSTER_2}\n7- {MONSTER_3}\n8- {MONSTER_4}\n9- {MONSTER_5}";
        _switchOptionsText.text = defaultSwitchPromptText;
        
        string replaceMeTemplate = "{MONSTER_";
        Trainer trainer = playerNum == 1 ? _player1 : _player2;
        MonsterUnit[] team = trainer.team;
        string newSwitchPromptText = _switchOptionsText.text;
        int uiNum = 1;

        for (int i = 0; i < team.Length; i++)
        {
            MonsterUnit mon = team[i];
            if (mon == monsterBattling || mon.Fainted)
            {
                continue;
            }

            string toReplace = replaceMeTemplate + uiNum + "}";
            string monName = mon.UnitName;
            newSwitchPromptText = newSwitchPromptText.Replace(toReplace, monName);
            uiNum++;
        }

        //deal with fainted mons
        //TODO: refactor into upper loop?
        for (int i = 1; i <= 5; i++)
        {
            string leftoverTemplate = "{MONSTER_" + i + "}";
            newSwitchPromptText = newSwitchPromptText.Replace(leftoverTemplate, "Fainted!");
        }

        _switchOptionsText.text = newSwitchPromptText;
        _switchOptionsText.gameObject.SetActive(true);
    }


    private static void SendOutMonsters(Trainer player, BattlePosition spotA, BattlePosition spotB = null)
    {
        spotA.SendMonster(player.team[0]);

        //spotB is null if this is a single battle! spotB has a value if this is a double battle
        if (spotB == null) return;
        spotB.SendMonster(player.team[1]);
    }

    //this method detects if a player has no team. if they have no team, give them a random one.
    private static void TeamCheck(Trainer player)
    {
        if (player.team.Length == 0)
        {
            MonsterSpecies[] allSpecies = Resources.LoadAll<MonsterSpecies>("Monster Species");
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
             using random natures to ensure stat calculation is working, 
             random teams will all have neutral natures normally*/
            randomTeam[i] = new MonsterUnit(randomMonster, NatureHelper.GetRandomNature());
        }
        player.team = randomTeam;
    }
}
