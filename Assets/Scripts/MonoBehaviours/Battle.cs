using System;
using System.Collections;
using System.Collections.Generic;
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
    //private BattlePosition _p1MonB; //only used in double battles, which are not yet implemented. 
    
    private Trainer _player2;
    private BattlePosition _p2MonA;
    //private BattlePosition _p2MonB; //only used in double battles, which are not yet implemented.

    private Transform _actionPrompt;
    private Transform _attackTextObjects;
    private Transform _switchTextObjects;

    private TextMeshProUGUI _actionPromptText;
    private TextMeshProUGUI _attackOptionsText;
    private TextMeshProUGUI _switchOptionsText;
    private TextMeshProUGUI _forcedSwitchOptionsText;
    private TextMeshProUGUI _infoText;

    private BattleState _state;
    private int _p1Choice = -1;
    private int _p2Choice = -1;
    
    //[SerializeField] private bool isDoubleBattle = false;
    
    
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
        /*if (isDoubleBattle)
        {
            _p1MonB = player1Positions.GetChild(1).GetComponent<BattlePosition>();
            _p2MonB = player2Positions.GetChild(1).GetComponent<BattlePosition>();
            _p1MonB.InitializePlayer(_player1);
            _p2MonB.InitializePlayer(_player2);
        }*/
        
        //initialize UI references 
        Transform canvas = GameObject.Find("MainCanvas").transform;
        _actionPrompt = canvas.GetChild(0);
        _attackTextObjects = _actionPrompt.GetChild(0);
        _switchTextObjects = _actionPrompt.GetChild(1);

        _actionPromptText = _actionPrompt.GetComponent<TextMeshProUGUI>();
        _attackOptionsText = _attackTextObjects.GetChild(1).GetComponent<TextMeshProUGUI>();
        _switchOptionsText = _switchTextObjects.GetChild(1).GetComponent<TextMeshProUGUI>();
        _forcedSwitchOptionsText = canvas.GetChild(1).GetComponent<TextMeshProUGUI>();
        _infoText = canvas.GetChild(2).GetComponent<TextMeshProUGUI>();
        
        SFX.Play(SoundEffect.Confirm);
    }

    private void Start()
    {
        List<MonsterSpecies> allSpecies = Resources.LoadAll<MonsterSpecies>("Monster Species").ToList();
        
        GiveRandomTeam(_player1, allSpecies);
        GiveRandomTeam(_player2, allSpecies);

        /*if (isDoubleBattle)
        {
            SendOutMonsters(_player1, _p1MonA, _p1MonB);
            SendOutMonsters(_player2, _p2MonA, _p2MonB);
        }
        else
        {*/
            SendOutMonsters(_player1, _p1MonA);
            SendOutMonsters(_player2, _p2MonA);
        //}

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
        
        Message("Actions Selected. Press Space to see them play out!");
        yield return WaitForInput();
        
        Debug.Log("Before HandleForfeit");
        //executed if anyone selected forfeit
        yield return HandleForfeit(p1Action, p2Action);

        Debug.Log("Before Handling Switch");
        //handle switching for both players
        if (p1Action == PlayerAction.Switch && p2Action == PlayerAction.Switch)
        {        
            Debug.Log("Before Both Switch");
            yield return HandleSwitch( _p1Choice, 1);
            yield return HandleSwitch(_p2Choice, 2);
        }
        else 
        {
            if (p1Action == PlayerAction.Switch)
            {
                Debug.Log("Before P1 Switch");

                yield return HandleSwitch( _p1Choice, 1);
            }
            
            if (p2Action == PlayerAction.Switch)
            {
                Debug.Log("Before P2 Switch");
                yield return HandleSwitch( _p2Choice, 2);
            }
        }
        
        Debug.Log("Before Players Attack");
        yield return PlayersAttack(p1Action, p2Action);
        
        Debug.Log("Before HandleFaint");
        yield return HandleFaint();
        
        Debug.Log("The turn is now over");
        Message("The turn is over. Press space to start the next turn!");
        yield return WaitForInput();

        _state = BattleState.Player1Choice;
        ShowActionPrompts(1, _p1MonA.MonsterHere);
        
    }

    private IEnumerator HandleFaint()
    {

        int winner = CheckForWinner();

        if (winner != -1)
        {
            if (winner == 1)
            {
                Message("Player 2 has no more monsters that can fight!\nPlayer 1 Wins!\nPress Space to play again!");
            }
            else if (winner == 2)
            {
                Message("Player 1 has no more monsters that can fight!\nPlayer 2 Wins!\nPress Space to play again!");
            }
            
            FindObjectOfType<MusicPlayer>().PlayWinMusic();
            yield return WaitForInput();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        
        if (_p1MonA.MonsterHere.Fainted)
        {
            _state = BattleState.Player1MidTurnSwitch;
            UpdateSwitchText(1, _p1MonA.MonsterHere, _forcedSwitchOptionsText);
            yield return new WaitUntil(() => _state != BattleState.Player1MidTurnSwitch && _state != BattleState.Player2MidTurnSwitch);
            yield return WaitForInput();
        }
        else if (_p2MonA.MonsterHere.Fainted)
        {
            _state = BattleState.Player2MidTurnSwitch;
            UpdateSwitchText(2, _p2MonA.MonsterHere, _forcedSwitchOptionsText);
            yield return new WaitUntil(() => _state != BattleState.Player1MidTurnSwitch && _state != BattleState.Player2MidTurnSwitch);
            yield return WaitForInput();
        }

        
    }
    
    private int CheckForWinner()
    {
        if (PlayerLost(_player1)) return 2;
        if (PlayerLost(_player2)) return 1;
        return -1;
    }

    private bool PlayerLost(Trainer player)
    {
        MonsterUnit[] team = player.team;

        foreach (MonsterUnit monster in team)
        {
            if (!monster.Fainted)
            {
                return false;
            }
        }
        return true;
    }

    
    public void Pivot(int playerNum)
    {
        //Debug.Log($"Battlefield Pivot Secondary Effect {playerNum}");
        if (playerNum == 1)
        {
            Debug.Log("Player 1 is Pivoting");
            Message(_infoText.text + "\nPlayer 1 is Switching!");
            _state = BattleState.Player1MidTurnSwitch;
        }
        else if (playerNum == 2)
        {
            Debug.Log("Player 2 is Pivoting");
            Message(_infoText.text + "\nPlayer 2 is Switching!");
            _state = BattleState.Player2MidTurnSwitch;
        }
    }

    private IEnumerator HandlePivot()
    {
        if (_state == BattleState.Player1MidTurnSwitch)
        {
            UpdateSwitchText(1, _p1MonA.MonsterHere, _forcedSwitchOptionsText);
            yield return new WaitUntil(() => _state != BattleState.Player1MidTurnSwitch && _state != BattleState.Player2MidTurnSwitch);
            yield return WaitForInput();
            
        }
        else if (_state == BattleState.Player2MidTurnSwitch)
        {
            UpdateSwitchText(2, _p2MonA.MonsterHere, _forcedSwitchOptionsText);
            yield return new WaitUntil(() => _state != BattleState.Player1MidTurnSwitch && _state != BattleState.Player2MidTurnSwitch);
            yield return WaitForInput();
        }
    }

    private IEnumerator HandleForfeit(PlayerAction p1Action, PlayerAction p2Action)
    {
        //if anyone forfeits, restart the game 
        if (p1Action == PlayerAction.Forfeit || p2Action == PlayerAction.Forfeit)
        {
            if (p1Action == PlayerAction.Forfeit)
            {
                Message("Player 1 Forfeited! Player 2 Wins!\nPress Space to play again!");
            }
            else
            {
                Message("Player 2 Forfeited! Player 1 Wins!\nPress Space to play again!");
            }
            
            FindObjectOfType<MusicPlayer>().PlayWinMusic();
            yield return WaitForInput();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        yield return null;
    }

    private IEnumerator HandleSwitch(int playerChoice, int playerNum)
    {
        Trainer player = playerNum == 1 ? _player1 : _player2;
        BattlePosition position =  playerNum == 1 ? _p1MonA : _p2MonA;
        int switchIndex = playerChoice - 4;
        Debug.Log($"Player {playerNum} is switching to the monster at index {switchIndex}! That monster is {player.team[switchIndex].UnitName}");
        Message($"Player {playerNum} is switching to {player.team[switchIndex].UnitName}");
        position.SwitchMonster(player.team[switchIndex]);
        yield return WaitForInput();
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
                int p1Speed = _p1MonA.MonsterHere.GetStat(Stat.Readiness);
                int p2Speed = _p2MonA.MonsterHere.GetStat(Stat.Readiness);

                if (p1Speed > p2Speed)
                {
                    yield return ActuallyAttack(p1Mon, p1AttackIndex, p2Mon, p2AttackIndex);
                }
                else if (p1Speed < p2Speed)
                {
                    yield return ActuallyAttack(p2Mon, p2AttackIndex, p1Mon, p1AttackIndex);
                }

                //speed tie! whoever attacks first is RANDOM!
                else
                {
                    yield return SpeedTieAttack(p1Mon, p1AttackIndex, p2Mon, p2AttackIndex);
                }
            }
            else
            {
                if (p1Attack.Priority > p2Attack.Priority)
                {
                    yield return ActuallyAttack(p1Mon, p1AttackIndex, p2Mon, p2AttackIndex);
                }
                else if (p1Attack.Priority > p2Attack.Priority)
                {
                    yield return ActuallyAttack(p2Mon, p2AttackIndex, p1Mon, p1AttackIndex);
                }
            }

        }

        //if only one is attacking, just tell it to attack whatever is in front of it 
        else if (p1Action == PlayerAction.Attack)
        {
            int p1AttackIndex = _p1Choice - 1;
            yield return ActuallyAttack(p1Mon, p1AttackIndex);
            //p1Mon.UseAttack(p1AttackIndex, new[]{p2Mon});
        }
        else if (p2Action == PlayerAction.Attack)
        {
            int p2AttackIndex = _p2Choice - 1;
            yield return ActuallyAttack(p2Mon, p2AttackIndex);
            //p2Mon.UseAttack(p2AttackIndex, new[]{p1Mon});
        }
    }

    //first is the monster who moves first. second is the monster who moves second.
    private IEnumerator ActuallyAttack(MonsterUnit first, int firstAttackIndex, MonsterUnit second = null, int secondAttackIndex = -1)
    {
        
        second = ReassignTarget(first);
        
        bool moveFailed1 = CheckFailure(first, firstAttackIndex, second, secondAttackIndex, firstCheck: true);
        
        //we must call reassign to make sure that the monster first is trying to attack is actually on the field and didn't switch out.
        first.UseAttack(firstAttackIndex, new[]{second}, moveFailed1);
        yield return WaitForInput();
        
        if (_state == BattleState.Player1MidTurnSwitch || _state == BattleState.Player2MidTurnSwitch)
        {
            yield return HandlePivot();
        }

        if (second != null && !second.Fainted && secondAttackIndex != -1)
        {
            //make sure first is still there, and not returned to its trainer after a pivot. a mon that isn't on the field cannot be attacked
            first = ReassignTarget(second);


            bool moveFailed2 = CheckFailure(first, firstAttackIndex, second, secondAttackIndex, firstCheck: false);
            second.UseAttack(secondAttackIndex, new[] { first }, moveFailed2);
            yield return WaitForInput();
            if (_state == BattleState.Player1MidTurnSwitch || _state == BattleState.Player2MidTurnSwitch)
            {
                yield return HandlePivot();
            }
        }
    }

    private IEnumerator WaitForInput()
    {
        yield return new WaitForSeconds(0.05f);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        yield return new WaitForSeconds(0.05f);
    }

    //this method will detect if a move being readied should fail because of some precondition that must be checked
    //before attacks can be executed. 
    private bool CheckFailure(MonsterUnit first, int firstAttackIndex, MonsterUnit second, int secondAttackIndex, bool firstCheck)
    {
        return CheckSuckerPunchFail(first, firstAttackIndex, second, secondAttackIndex, firstCheck);
    }

    /*sucker punch (and its variants, like thunderclap) have a unique effect:
      the move has increased priority, but will fail if: 
      - the opponent went first (meaning they also used an increased priority move, like sucker punch or quick attack, 
        and their readiness/speed was higher than yours, 
        or they used a move with higher priority
      - the opponent uses a status move, not an attack that deals direct damage
      - the opponent switched, or did something other than attacking
      
      this means that sucker punch variants will work if and only if 
      the move strikes first, and the opponent is readying an attack that deals direct damage.
     */
    private bool CheckSuckerPunchFail(MonsterUnit first, int firstAttackIndex, MonsterUnit second, int secondAttackIndex, bool firstCheck)
    {
        //if we were moving second, and we used sucker punch, our move will fail.
        if (UsingSuckerPunch(second, secondAttackIndex))
        {
            Debug.Log("Check Fail Case 1");
            return true;

        }

        
        //if we're not using sucker punch, then of course the sucker punch fail won't trigger. duh.
        if (!UsingSuckerPunch(first, firstAttackIndex))
        {
            Debug.Log("Check Fail Case 2");
            return false;
        }

        
        //if the opponent isn't attacking, then sucker punch will fail.
        if (secondAttackIndex == -1)
        {
            Debug.Log("Check Fail Case 3");
            return true;
        }

        //if our target doesn't exist, sucker punch will fail. duh.
        if (second == null)
        {
            Debug.Log("Check Fail Case 4");
            return true;
        }

        //get the opponent's attack that they are readying
        Attack secondAttack = second.KnownAttacks[secondAttackIndex];
        
        //if that move is a status move (meaning it does no damage) then sucker punch will fail
        //this should ONLY happen if this check is occuring for the first mon, hence the and condition.
        if (secondAttack.Category == AttackCategory.Status && firstCheck)
        {
            Debug.Log("Check Fail Case 5");
            return true;

        }

        Debug.Log("Check fail returned false");
        //if all of the above return statements never execute, then sucker punch will succeed.
        return false;
    }

    //this method just checks if the monster is using sucker punch or a variant like thunderclap
    private static bool UsingSuckerPunch(MonsterUnit unit, int attackIndex)
    {
        if (attackIndex < 0 || attackIndex > unit.KnownAttacks.Length - 1) return false;
        
        //get the secondary effects of the move we're checking
        AttackEffect[] effects = unit.KnownAttacks[attackIndex].SecondaryEffects;
        
        //if the move has no secondary effects, then this is not a sucker punch variant
        if (effects == null || effects.Length == 0) return false; 
        
        //the suckerpunch effect must ALWAYS be the first secondary effect, according to this implementation.
        AttackEffect effect = effects[0];


        if (effect is SuckerPunchEffect)
        {
            Debug.Log($"{unit.UnitName} is using sucker punch!!");
            return true;
        }
        else
        {
            Debug.Log($"{unit.UnitName} is NOT using sucker punch!!");
            return false;
        }

        /*if the first effect in the list of effects for the move they are readying is the sucker punch effect,
         then return true. else return false.*/
        //return effect is SuckerPunchEffect;
    }
    
    private MonsterUnit ReassignTarget(MonsterUnit unit)
    {
        int playerNum = unit.PositionInBattle.Player.PlayerNum;
        if (playerNum == 1)
        {
            return _p2MonA.MonsterHere;
        }
        else
        {
            return _p1MonA.MonsterHere;
        }
    }

    private IEnumerator SpeedTieAttack(MonsterUnit p1, int p1Index, MonsterUnit p2, int p2Index)
    {
        int rand = Random.Range(0, 1);

        if (rand == 0)
        {
            yield return ActuallyAttack(p1, p1Index, p2, p2Index);
        }
        else
        {
            yield return ActuallyAttack(p2, p2Index, p1, p1Index);
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
            if (ChoiceInvalid(2, buttonPressed))
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
                if (_state == BattleState.Player1MidTurnSwitch || _state == BattleState.Player2MidTurnSwitch)
                {
                    SFX.Play(SoundEffect.Deny);
                    return true;
                }
                
                int moveIndex = choice - 1;
                //if the monster doesn't have a move that corresponds to the player choice, then their choice was invalid.
                if (monsterBattling.KnownAttacks[moveIndex] == null) 
                {
                    SFX.Play(SoundEffect.Deny);
                    return true;
                }
                
                SFX.Play(SoundEffect.Confirm);
                return false;
            
            case 5:
            case 6:
            case 7: 
            case 8: 
            case 9:
                int switchIndex = choice - 4;
                //if they don't have a mon there in their party there, or it is fainted, then this choice is invalid
                if (player.team[switchIndex] == null || player.team[switchIndex].Fainted)
                {
                    SFX.Play(SoundEffect.Deny);
                    return true;
                }
                SFX.Play(SoundEffect.Confirm);
                return false;
            case 0:
                //forfeit 
                SFX.Play(SoundEffect.Confirm);
                return false;
            default:
                SFX.Play(SoundEffect.Deny);
                return true;
        }
    }

    //temp UI solution for prototyping purposes
    private void ShowActionPrompts(int playerNum, MonsterUnit monsterBattling)
    {
        _infoText.text = "";
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
        
        //clear the main message, because this can block the switch prompts
        Message("");
        
        //reset the options text to the default, so we can replace stuff again
        const string defaultSwitchPromptText = "5- {MONSTER_1}\n6- {MONSTER_2}\n7- {MONSTER_3}\n8- {MONSTER_4}\n9- {MONSTER_5}";
        switchOptionsText.text = defaultSwitchPromptText;
        
        string replaceMeTemplate = "{MONSTER_";
        Trainer trainer = playerNum == 1 ? _player1 : _player2;
        MonsterUnit[] team = trainer.team;
        string newSwitchPromptText = switchOptionsText.text;
        int uiNum = 1;

        //starts at 1 because 0 is in battle 
        for (int i = 1; i < team.Length; i++)
        {
            MonsterUnit mon = team[i];
            string toReplace = replaceMeTemplate + uiNum + "}";
            string monName = mon.UnitName;
            
            if (mon.Fainted)
            {
                monName = $"{monName} (fainted)";
            }
            
            newSwitchPromptText = newSwitchPromptText.Replace(toReplace, monName);
            uiNum++;
        }
        
        switchOptionsText.text = newSwitchPromptText;
        switchOptionsText.gameObject.SetActive(true);
    }
    
    public static void StaticMessage(string message)
    {
        Battle theBattle = FindObjectOfType<Battle>();
        theBattle.Message(message);
    }

    public static string GetCurrentMessage()
    {
        Battle theBattle = FindObjectOfType<Battle>();
        return theBattle._infoText.text;
    }

    private void Message(string message)
    {
        _infoText.text = message;
    }


    private static void SendOutMonsters(Trainer player, BattlePosition spotA, BattlePosition spotB = null)
    {
        spotA.SendMonster(player.team[0]);

        //spotB is null if this is a single battle! spotB has a value if this is a double battle
        if (spotB == null) return;
        spotB.SendMonster(player.team[1]);
    }
    
    private static void GiveRandomTeam(Trainer player, List<MonsterSpecies> allSpecies)
    {

        //don't give a random team to a player that has a team already!
        if (player.team.Length != 0) return;
        
        MonsterUnit[] randomTeam = new MonsterUnit[6];
        for (int i = 0; i < 6; i++)
        {
            int randIndex = Random.Range(0, allSpecies.Count - 1);
            MonsterSpecies randomMonster = allSpecies[randIndex];
            allSpecies.Remove(allSpecies[randIndex]);
            randomTeam[i] = new MonsterUnit(randomMonster, NatureHelper.GetRandomNature());
        }
        player.team = randomTeam;
    }
}
