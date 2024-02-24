using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

//the battlefield class will handle the basic gameplay loop of a battle
public class Battle : MonoBehaviour
{
    private Trainer _player1;
    private BattlePosition _p1MonA;
    private BattlePosition _p1MonB; //only used in double battles, which are not yet implemented. 
    
    private Trainer _player2;
    private BattlePosition _p2MonA;
    private BattlePosition _p2MonB; //only used in double battles, which are not yet implemented.

    private Transform _actionPrompt;
    private Transform _attackTextObjects;
    private Transform _switchTextObjects;

    private TextMeshProUGUI _actionPromptText;
    private TextMeshProUGUI _attackOptionsText;
    private TextMeshProUGUI _switchOptionsText;
    private TextMeshProUGUI _forcedSwitchOptionsText;

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
        _p1MonA.InitializePlayer(_player1);
        _p2MonA.InitializePlayer(_player2);

        //check for double battle, initialize those positions if necessary
        if (isDoubleBattle)
        {
            _p1MonB = player1Positions.GetChild(1).GetComponent<BattlePosition>();
            _p2MonB = player2Positions.GetChild(1).GetComponent<BattlePosition>();
            _p1MonB.InitializePlayer(_player1);
            _p2MonB.InitializePlayer(_player2);
        }
        
        //initialize UI references 
        Transform canvas = GameObject.Find("MainCanvas").transform;
        _actionPrompt = canvas.GetChild(0);
        _attackTextObjects = _actionPrompt.GetChild(0);
        _switchTextObjects = _actionPrompt.GetChild(1);

        _actionPromptText = _actionPrompt.GetComponent<TextMeshProUGUI>();
        _attackOptionsText = _attackTextObjects.GetChild(1).GetComponent<TextMeshProUGUI>();
        _switchOptionsText = _switchTextObjects.GetChild(1).GetComponent<TextMeshProUGUI>();
        _forcedSwitchOptionsText = canvas.GetChild(1).GetComponent<TextMeshProUGUI>();
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

        _state = BattleState.Player1Choice;
        ShowActionPrompts(1, _p1MonA.MonsterHere);
    }

    private void Update()
    {
        for (int i = 0; i <= 9; i++)
        {
           KeyCode key = KeyCode.Alpha0 + i;
           if (Input.GetKeyDown(key))
           {
               SelectPlayerChoice(i);
               break;
           }
        }
    }

    private IEnumerator ExecutePlayerActions()
    {

        HideActionPrompts();
        
        PlayerAction p1Action = SelectPlayerActionType(_p1Choice);
        PlayerAction p2Action = SelectPlayerActionType(_p2Choice);
        
        Debug.Log("Actions Selected. Press Space to see them play out!");
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        
        //executed if anyone selected forfeit
        HandleForfeit(p1Action, p2Action);

        //wait until HandleSwitch has finished to do anything else
        //handle switching for both players
        yield return HandleSwitch(p1Action, _p1Choice, 1);
        yield return HandleSwitch(p2Action, _p2Choice, 2);

        //wait until PlayersAttack has finished to do anything else
        yield return PlayersAttack(p1Action, p2Action);

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        
        yield return HandleFaint();
        
        
        //resets everything at the end 
        Debug.Log("Time to pick actions again!");
        
        _state = BattleState.Player1Choice;
        ShowActionPrompts(1, _p1MonA.MonsterHere);
        
    }

    private IEnumerator HandleFaint()
    {
        if (_p1MonA.MonsterHere.Fainted)
        {
            _state = BattleState.Player1MidTurnSwitch;
            UpdateSwitchText(1, _p1MonA.MonsterHere, _forcedSwitchOptionsText);
        }
        else if (_p2MonA.MonsterHere.Fainted)
        {
            _state = BattleState.Player2MidTurnSwitch;
            UpdateSwitchText(2, _p2MonA.MonsterHere, _forcedSwitchOptionsText);
        }

        yield return new WaitUntil(() => _state != BattleState.Player1MidTurnSwitch && _state != BattleState.Player2MidTurnSwitch);
    }

    public void Pivot(int playerNum)
    {
        Debug.Log($"Battlefield Pivot Secondary Effect {playerNum}");
        if (playerNum == 1)
        {
            Debug.Log("Player 1 is Pivoting");
            _state = BattleState.Player1MidTurnSwitch;
        }
        else if (playerNum == 2)
        {
            Debug.Log("Player 2 is Pivoting");
            _state = BattleState.Player2MidTurnSwitch;
        }
    }

    private IEnumerator HandlePivot()
    {
        //Debug.Log("HandlePivot");
        if (_state == BattleState.Player1MidTurnSwitch)
        {
            Debug.Log("HandlePivot 1");
            UpdateSwitchText(1, _p1MonA.MonsterHere, _forcedSwitchOptionsText);
        }
        else if (_state == BattleState.Player2MidTurnSwitch)
        {
            Debug.Log("HandlePivot 2");
            UpdateSwitchText(2, _p2MonA.MonsterHere, _forcedSwitchOptionsText);
        }
        yield return new WaitUntil(() => _state != BattleState.Player1MidTurnSwitch && _state != BattleState.Player2MidTurnSwitch);
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

    private IEnumerator HandleSwitch(PlayerAction playerAction, int playerChoice, int playerNum)
    {
        Trainer player = playerNum == 1 ? _player1 : _player2;
        BattlePosition position =  playerNum == 1 ? _p1MonA : _p2MonA;
        MonsterUnit monsterBattling = position.MonsterHere;
        
        if (playerAction == PlayerAction.Switch)
        {
            int switchIndex = playerChoice - 4;
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
            Debug.Log($"Player {playerNum} is switching to the monster at index {switchIndex}! That monster is {player.team[switchIndex].UnitName}");
            position.SwitchMonster(player.team[switchIndex]);
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        }
    }

    private IEnumerator PlayersAttack(PlayerAction p1Action, PlayerAction p2Action)
    {
        MonsterUnit p1Mon = _p1MonA.MonsterHere;
        MonsterUnit p2Mon = _p2MonA.MonsterHere;
        
        //if they are both attacking, speed now matters.
        if (p1Action == PlayerAction.Attack && p2Action == PlayerAction.Attack)
        {
            int p1AttackIndex = _p1Choice - 1;
            int p2AttackIndex = _p2Choice - 1;

            Attack p1Attack = p1Mon.KnownAttacks[p1AttackIndex];
            Attack p2Attack = p2Mon.KnownAttacks[p2AttackIndex];

            //if they are both attacking, and those attacks have equivalent priority, the result comes down to the readiness stat (speed, in pokemon).
            if (p1Attack.Priority == p2Attack.Priority)
            {
                int p1Speed = _p1MonA.MonsterHere.GetReadiness();
                int p2Speed = _p2MonA.MonsterHere.GetReadiness();

                if (p1Speed > p2Speed)
                {
                    yield return StartCoroutine(ActuallyAttack(p1Mon, p1AttackIndex, p2Mon, p2AttackIndex));
                }
                else if (p1Speed < p2Speed)
                {
                    yield return StartCoroutine(ActuallyAttack(p2Mon, p2AttackIndex, p1Mon, p1AttackIndex));
                }

                //speed tie! whoever attacks first is RANDOM!
                else
                {
                    SpeedTieAttack(p1Mon, p1AttackIndex, p2Mon, p2AttackIndex);
                }
            }
            else
            {
                if (p1Attack.Priority > p2Attack.Priority)
                {
                    yield return StartCoroutine(ActuallyAttack(p1Mon, p1AttackIndex, p2Mon, p2AttackIndex));
                }
                else if (p1Attack.Priority > p2Attack.Priority)
                {
                    yield return StartCoroutine(ActuallyAttack(p2Mon, p2AttackIndex, p1Mon, p1AttackIndex));
                }
            }

        }

        //if only one is attacking, just tell it to attack whatever is in front of it 
        else if (p1Action == PlayerAction.Attack)
        {
            int p1AttackIndex = _p1Choice - 1;
            StartCoroutine(ActuallyAttack(p1Mon, p1AttackIndex));
            //p1Mon.UseAttack(p1AttackIndex, new[]{p2Mon});
        }
        else if (p2Action == PlayerAction.Attack)
        {
            int p2AttackIndex = _p2Choice - 1;
            StartCoroutine(ActuallyAttack(p2Mon, p2AttackIndex));
            //p2Mon.UseAttack(p2AttackIndex, new[]{p1Mon});
        }
    }

    //first is the monster who moves first. second is the monster who moves second.
    private IEnumerator ActuallyAttack(MonsterUnit first, int firstAttackIndex, MonsterUnit second = null, int secondAttackIndex = -1)
    {
        first.UseAttack(firstAttackIndex, new[]{second});
        yield return HandlePivot();

        if (second != null && !second.Fainted && secondAttackIndex != -1)
        {
            
            //make sure first is still there, and not returned to its trainer after a pivot. a mon that isn't on the field cannot be attacked
            first = ReassignFirst(first, second);
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
            second.UseAttack(secondAttackIndex, new[]{first});
            yield return HandlePivot();
            
        }
        yield return null;
    }

    //in the above method, if first uses a pivot move, then it will return to its trainer, and second will still be targetting
    //first, which is currently not on the field.
    private MonsterUnit ReassignFirst(MonsterUnit first, MonsterUnit second)
    {
        int secondsTrainer = -1;

        if (_player1.team.Contains(second))
        {
            secondsTrainer = 1;
        }
        else if (_player2.team.Contains(second))
        {
            secondsTrainer = 2;
        }
        else
        {
            throw new Exception("ReassignFirst BROKE");
        }

        //if second's trainer is player 1, then first is player 2's mon.
        if (secondsTrainer == 1)
        {
            return _p2MonA.MonsterHere;
        }
        
        //otherwise, first is player 1's mon.
        return _p1MonA.MonsterHere;
        
    }

    private void SpeedTieAttack(MonsterUnit p1, int p1Index, MonsterUnit p2, int p2Index)
    {
        int rand = Random.Range(0, 1);

        if (rand == 0)
        {
            StartCoroutine(ActuallyAttack(p1, p1Index, p2, p2Index));
        }
        else
        {
            StartCoroutine(ActuallyAttack(p2, p2Index, p1, p1Index));
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
        if (_state == BattleState.Player1Choice)
        {
            _p1Choice = buttonPressed;
            if (ChoiceInvalid(1, _p1Choice))
            {
                Debug.Log("Invalid choice from player 1!");
                _p1Choice = -1;
                return;
            }
            _state = BattleState.Player2Choice;
            ShowActionPrompts(2, _p2MonA.MonsterHere);
        }
        else if (_state == BattleState.Player2Choice)
        {
            _p2Choice = buttonPressed;
            if (ChoiceInvalid(2, _p2Choice))
            {
                Debug.Log("Invalid choice from player 2!");
                _p2Choice = -1;
                return;
            }
            
            _state = BattleState.ExecutingActions;
            StartCoroutine(ExecutePlayerActions());
        }
        else if (_state == BattleState.Player1MidTurnSwitch)
        {
            if (ChoiceInvalid(1, buttonPressed))
            {
                Debug.Log("Invalid mid-switch choice from player 1!");
                return;
            }

            MonsterUnit newMonster = _player1.team[buttonPressed - 4];
            _p1MonA.SwitchMonster(newMonster);
            _state = BattleState.ExecutingActions;
            _forcedSwitchOptionsText.gameObject.SetActive(false);
        }
        else if (_state == BattleState.Player2MidTurnSwitch)
        {
            if (ChoiceInvalid(1, buttonPressed))
            {
                Debug.Log("Invalid mid-switch choice from player 2!");
                return;
            }

            MonsterUnit newMonster = _player2.team[buttonPressed - 4];
            _p2MonA.SwitchMonster(newMonster);
            _state = BattleState.ExecutingActions;
            _forcedSwitchOptionsText.gameObject.SetActive(false);
        }
    }

    private bool ChoiceInvalid(int playerNum, int choice)
    {
        Trainer player = playerNum == 1 ? _player1 : _player2;
        BattlePosition position =  playerNum == 1 ? _p1MonA : _p2MonA;
        MonsterUnit monsterBattling = position.MonsterHere;
        
        switch (choice)
        {
            case 1:
            case 2:
            case 3:
            case 4:
                
                //if this situation is a mid-turn switch (pivot move, faint), then choosing a move does nothing.
                if (_state == BattleState.Player1MidTurnSwitch ||
                    _state == BattleState.Player2MidTurnSwitch) return true;
                
                
                int moveIndex = choice - 1;
                //if the monster doesn't have a move that corresponds to the player choice, then their choice was invalid.
                if (monsterBattling.KnownAttacks[moveIndex] == null) return true;
                return false;
            
            case 5:
            case 6:
            case 7: 
            case 8: 
            case 9:
                int switchIndex = choice - 4;
                //if they don't have a mon there in their party there, or it is fainted, then this choice is invalid
                if (player.team[switchIndex] == null || player.team[switchIndex].Fainted) return true;
                return false;
            case 0:
                //forfeit 
                return false;
            default:
                return true;
        }
    }

    //temp UI solution for prototyping purposes
    private void ShowActionPrompts(int playerNum, MonsterUnit monsterBattling)
    {
        
        HideActionPrompts();
        UpdatePromptText(playerNum, monsterBattling);
        UpdateAttackText(monsterBattling);
        UpdateSwitchText(playerNum, monsterBattling, _switchOptionsText);
    }


    private void HideActionPrompts()
    {
        _actionPrompt.gameObject.SetActive(false);
        _attackOptionsText.gameObject.SetActive(false);
        _switchOptionsText.gameObject.SetActive(false);
        _forcedSwitchOptionsText.gameObject.SetActive(false);
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

    //this method takes the TextMeshPro that needs to show the player's team as a parameter, because we might have to use two 
    //different TextMeshPro objects at different times- one for when the player is choosing to switch on their own volition, and
    //another when they are forced to switch by a situation like fainting, or the secondary effect of a move like U-Turn
    private void UpdateSwitchText(int playerNum, MonsterUnit monsterBattling, TextMeshProUGUI switchOptionsText)
    {
        //reset the options text to the default, so we can replace stuff again
        const string defaultSwitchPromptText = "5- {MONSTER_1}\n6- {MONSTER_2}\n7- {MONSTER_3}\n8- {MONSTER_4}\n9- {MONSTER_5}";
        switchOptionsText.text = defaultSwitchPromptText;
        
        string replaceMeTemplate = "{MONSTER_";
        Trainer trainer = playerNum == 1 ? _player1 : _player2;
        MonsterUnit[] team = trainer.team;
        string newSwitchPromptText = switchOptionsText.text;
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

        switchOptionsText.text = newSwitchPromptText;
        switchOptionsText.gameObject.SetActive(true);
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

            while (TeamContainsSpecies(randomTeam, randomMonster))
            {
                randIndex = Random.Range(0, allSpecies.Length - 1);
                randomMonster = allSpecies[randIndex];
            }
            
            //randomTeam[i] = new MonsterUnit(randomMonster);
            
            /*TODO: remove/comment out this line and uncomment the above.
             using random natures to ensure stat calculation is working, 
             random teams will all have neutral natures normally*/
            randomTeam[i] = new MonsterUnit(randomMonster, NatureHelper.GetRandomNature());
        }
        player.team = randomTeam;
    }

    private static bool TeamContainsSpecies(MonsterUnit[] team, MonsterSpecies species)
    {
        for (int i = 0; i < team.Length; i++)
        {
            if (team[i] == null) continue;

            if (team[i].GetSpecies().GetName() == species.GetName())
            {
                return true;
            }
        }

        return false;
    }
}
