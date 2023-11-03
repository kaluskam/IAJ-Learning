using Assets.Scripts.IAJ.Unity.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Game.NPCs;
using System.IO;
using System.Text;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.IAJ.Unity.DecisionMaking.RL;
using Assets.Scripts.Game;
using UnityEngine.SceneManagement;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions;
using System.Text.RegularExpressions;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static class GameConstants
    {
        public const float UPDATE_INTERVAL = 2.0f;
        public const int TIME_LIMIT = 150;
        public const int PICKUP_RANGE = 15;

    }
    public static string Q_TABLES_PATH = "./Assets/Results/QLearning/Tables";
    public static string Q_TABLES_PRETTY_PATH = "./Assets/Results/QLearning/Pretty";

    //public fields, seen by Unity in Editor

    public static AutonomousCharacter Character;

    [Header("UI Objects")]
    public Text HPText;
    public Text ShieldHPText;
    public Text ManaText;
    public Text TimeText;
    public Text XPText;
    public Text LevelText;
    public Text MoneyText;
    public Text DiaryText;
    public GameObject GameEnd;

    [Header("Enemy Settings")]
    public bool SleepingNPCs;
    public bool BehaviourTreeNPCs;
    public bool StochasticWorld = false;

    //fields
    public List<GameObject> chests { get; set; }
    public List<GameObject> skeletons { get; set; }
    public List<GameObject> orcs { get; set; }
    public List<GameObject> dragons { get; set; }
    public List<GameObject> enemies { get; set; }
    public List<GameObject> potions { get; set; }
    public Dictionary<string, List<GameObject>> disposableObjects { get; set; }

    public bool WorldChanged { get; set; }

    private float nextUpdateTime = 0.0f;
    private float enemyAttackCooldown = 0.0f;
    public bool gameEnded { get; set; } = false;
    public Vector3 initialPosition { get; set; }
    private int gameWon;
    public int maxQlearningIterations { get; set; } = 2000;
    public int currentQLearningIteration { get; set; }

    public static QLearning qLearning { get; set; }


    void Awake()
    {
        Instance = this;
        //DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(Instance);
        initializeObjects();
        UpdateDisposableObjects();
        Instance.WorldChanged = true;
        Character = GameObject.FindGameObjectWithTag("Player").GetComponent<AutonomousCharacter>();
        Character.Reward = 0;
        Instance.initialPosition = Character.gameObject.transform.position + Vector3.zero;
        QTablePrinter.ACTIONS = Character.Actions;

    }

    void Start()
    {
        if (qLearning == null)
        {
            qLearning = new QLearning(new Assets.Scripts.Game.CurrentStateWorldModel(this, Character.Actions, Character.Goals));
            QTablePrinter.ACTIONS = Character.Actions;
            if (Directory.GetFiles(Q_TABLES_PATH).Length > 0)
            {
                var lastIteration = Directory.GetFiles(Q_TABLES_PATH)
               .Select(fileName => System.Convert.ToInt16(Regex.Match(fileName, "[0-9]+").Value))
               .Max();
                qLearning.qTable = QTablePrinter.LoadFromFile(Q_TABLES_PATH + "/QTable_" + lastIteration);
                Debug.Log(lastIteration);
                qLearning.CurrentIteration = lastIteration + 1;
            } //TODO           
        }        
    }

    void RestartGame()    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        GameManager.qLearning.CurrentIteration++;
    }

    private void initializeObjects()
    {
        this.chests = GameObject.FindGameObjectsWithTag("Chest").ToList();
        this.skeletons = GameObject.FindGameObjectsWithTag("Skeleton").ToList();
        this.orcs = GameObject.FindGameObjectsWithTag("Orc").ToList();
        this.dragons = GameObject.FindGameObjectsWithTag("Dragon").ToList();
        var healthPotions = GameObject.FindGameObjectsWithTag("HealthPotion").ToList();
        var manaPotions = GameObject.FindGameObjectsWithTag("ManaPotion").ToList();
        this.potions = new List<GameObject>();
        this.potions.AddRange(healthPotions);
        this.potions.AddRange(manaPotions);
    }

    private void ReawakeNPCs()
    {
        foreach(NPC npc in GameObject.FindObjectsOfType<NPC>())
        {            
            npc.ReAwake();
        }
    }

    public void UpdateDisposableObjects()
    {
        this.enemies = new List<GameObject>();
        this.disposableObjects = new Dictionary<string, List<GameObject>>();
        this.enemies.AddRange(this.skeletons);
        this.enemies.AddRange(this.orcs);
        this.enemies.AddRange(this.dragons);

     
        //adds all enemies to the disposable objects collection
        foreach (var enemy in this.enemies)
        {

            if (disposableObjects.ContainsKey(enemy.name))
            {
                this.disposableObjects[enemy.name].Add(enemy);
            }
            else this.disposableObjects.Add(enemy.name, new List<GameObject>() { enemy });
        }
        //add all chests to the disposable objects collection
        foreach (var chest in this.chests)
        {
            if (disposableObjects.ContainsKey(chest.name))
            {
                this.disposableObjects[chest.name].Add(chest);
            }
            else this.disposableObjects.Add(chest.name, new List<GameObject>() { chest });
        }

        //adds all health potions to the disposable objects collection
        foreach (var potion in this.potions)
        {
            if (disposableObjects.ContainsKey(potion.name))
            {
                this.disposableObjects[potion.name].Add(potion);
            }
            else this.disposableObjects.Add(potion.name, new List<GameObject>() { potion });
        }
    }

    public void SetActiveAllObjects()
    {
        UpdateDisposableObjects();
        foreach(var list in this.disposableObjects.Values)
        {
            foreach(var o in list)
            {
                o.SetActive(true);
            }
        }
    }
    
    void FixedUpdate()
    {
        if (!this.gameEnded)
        {

            if (Time.time > this.nextUpdateTime)
            {
                this.nextUpdateTime = Time.time + GameConstants.UPDATE_INTERVAL;
                Character.baseStats.Time += GameConstants.UPDATE_INTERVAL;
            }


            this.HPText.text = "HP: " + Character.baseStats.HP;
            this.XPText.text = "XP: " + Character.baseStats.XP;
            this.ShieldHPText.text = "Shield HP: " + Character.baseStats.ShieldHP;
            this.LevelText.text = "Level: " + Character.baseStats.Level;
            this.TimeText.text = "Time: " + Character.baseStats.Time;
            this.ManaText.text = "Mana: " + Character.baseStats.Mana;
            this.MoneyText.text = "Money: " + Character.baseStats.Money;

            if (Character.baseStats.HP <= 0 || Character.baseStats.Time >= GameConstants.TIME_LIMIT)
            {
                this.GameEnd.SetActive(true);
                this.gameEnded = true;
                this.GameEnd.GetComponentInChildren<Text>().text = "You Died";
                this.gameWon = 0;
                Character.Reward = -100;
                WorldChanged = true;
                GameManager.qLearning.UpdateQTable(Character.OldWorldState, Character.CurrentAction, Character.Reward,
                    RLState.Create(new CurrentStateWorldModel(this, Character.Actions, Character.Goals)));
            }
            else if (Character.baseStats.Money >= 25)
            {
                this.GameEnd.SetActive(true);
                this.gameEnded = true;
                this.GameEnd.GetComponentInChildren<Text>().text = "Victory \n GG EZ";
                this.gameWon = 1;
                Character.Reward = 100;
                WorldChanged = true;
                GameManager.qLearning.UpdateQTable(Character.OldWorldState, Character.CurrentAction, Character.Reward,
                    RLState.Create(new CurrentStateWorldModel(this, Character.Actions, Character.Goals)));
            }
            if (this.gameEnded)
            {
                if (Character.QLearningActive && qLearning.CurrentIteration < this.maxQlearningIterations)
                {
                    if (qLearning.CurrentIteration < 4000)
                    {
                        var QTablePrettyPath = Q_TABLES_PRETTY_PATH + "/QTable_" + qLearning.CurrentIteration;
                        var QTablePath = Q_TABLES_PATH + "/QTable_" + qLearning.CurrentIteration;
                        QTablePrinter.SaveToFile(qLearning.qTable, QTablePrettyPath, QTablePath);
                        RestartGame();

                    }
                    else 
                    {
                        qLearning.learningInProgress = false;
                        Debug.Log(this.gameWon);
                        RestartGame();
                    }
                }
            }
        }
    }

    public void SwordAttack(GameObject enemy)
    {
        int damage = 0;

        Monster.EnemyStats enemyData = enemy.GetComponent<Monster>().enemyStats;
        if (enemy != null && enemy.activeSelf && InMeleeRange(enemy))
        {
            Character.AddToDiary(" I Sword Attacked " + enemy.name);

            if (this.StochasticWorld)
            {
                damage = enemy.GetComponent<Monster>().DmgRoll.Invoke();

                //attack roll = D20 + attack modifier. Using 7 as attack modifier (+4 str modifier, +3 proficiency bonus)
                int attackRoll = RandomHelper.RollD20() + 7;

                if (attackRoll >= enemyData.AC)
                {
                    //there was an hit, enemy is destroyed, gain xp
                    this.enemies.Remove(enemy);
                    this.disposableObjects.Remove(enemy.name);
                    enemy.SetActive(false);
                 
                }
            }
            else
            {
                damage = enemyData.SimpleDamage;
                this.enemies.Remove(enemy);
                this.disposableObjects.Remove(enemy.name);
                enemy.SetActive(false);
       
            }

            Character.baseStats.XP += enemyData.XPvalue;

            int remainingDamage = damage - Character.baseStats.ShieldHP;
            Character.baseStats.ShieldHP = Mathf.Max(0, Character.baseStats.ShieldHP - damage);

            if (remainingDamage > 0)
            {
                Character.baseStats.HP -= remainingDamage;
            }

            this.WorldChanged = true;
        }
    }

    public void EnemyAttack(GameObject enemy)
    {
        if (Time.time > this.enemyAttackCooldown)
        {

            int damage = 0;

            Monster monster = enemy.GetComponent<Monster>();

            if (enemy.activeSelf && monster.InWeaponRange(GameObject.FindGameObjectWithTag("Player")))
            {

                Character.AddToDiary(" I was Attacked by " + enemy.name);
                this.enemyAttackCooldown = Time.time + GameConstants.UPDATE_INTERVAL;

                if (this.StochasticWorld)
                {
                    damage = monster.DmgRoll.Invoke();

                    //attack roll = D20 + attack modifier. Using 7 as attack modifier (+4 str modifier, +3 proficiency bonus)
                    int attackRoll = RandomHelper.RollD20() + 7;

                    if (attackRoll >= monster.enemyStats.AC)
                    {
                        //there was an hit, enemy is destroyed, gain xp
                        this.enemies.Remove(enemy);
                        this.disposableObjects.Remove(enemy.name);
                        enemy.SetActive(false);
          
                    }
                }
                else
                {
                    damage = monster.enemyStats.SimpleDamage;
                    this.enemies.Remove(enemy);
                    this.disposableObjects.Remove(enemy.name);
                    enemy.SetActive(false);
                
                }

                Character.baseStats.XP += monster.enemyStats.XPvalue;

                int remainingDamage = damage - Character.baseStats.ShieldHP;
                Character.baseStats.ShieldHP = Mathf.Max(0, Character.baseStats.ShieldHP - damage);

                if (remainingDamage > 0)
                {
                    Character.baseStats.HP -= remainingDamage;
                    Character.AddToDiary(" I was wounded with " + remainingDamage + " damage");
                }

                this.WorldChanged = true;
            }
        }
    }

    public void DivineSmite(GameObject enemy)
    {
        if (enemy != null && enemy.activeSelf && InDivineSmiteRange(enemy) && Character.baseStats.Mana >= 2)
        {
            if (enemy.tag.Equals("Skeleton"))
            {
                Character.baseStats.XP += 3;
                Character.AddToDiary(" I Smited " + enemy.name);
                this.enemies.Remove(enemy);
                this.disposableObjects.Remove(enemy.name);
                enemy.SetActive(false);
             
            }
            Character.baseStats.Mana -= 2;

            this.WorldChanged = true;
            // add reward
        }
    }

    public void ShieldOfFaith()
    {
        if (Character.baseStats.Mana >= 5)
        {
            Character.baseStats.ShieldHP = 5;
            Character.baseStats.Mana -= 5;
            Character.AddToDiary(" My Shield of Faith will protect me!");
            this.WorldChanged = true;
        }
    }

    public void PickUpChest(GameObject chest)
    {


        if (chest != null && chest.activeSelf && InChestRange(chest))
        {
            Character.AddToDiary(" I opened  " + chest.name);
            this.chests.Remove(chest);
            this.disposableObjects.Remove(chest.name);
            chest.SetActive(false);
            Character.Reward = 30;
            Character.baseStats.Money += 5;
            this.WorldChanged = true;
        }
    }

    public void GetManaPotion(GameObject manaPotion)
    {
        if (manaPotion != null && manaPotion.activeSelf && InPotionRange(manaPotion))
        {
            Character.AddToDiary(" I drank " + manaPotion.name);
            this.disposableObjects.Remove(manaPotion.name);
            manaPotion.SetActive(false);
    
            Character.baseStats.Mana = 10;
            this.WorldChanged = true;
            
        }
    }

    public void GetHealthPotion(GameObject potion)
    {
        if (potion != null && potion.activeSelf && InPotionRange(potion))
        {
            Character.AddToDiary(" I drank " + potion.name);
            this.disposableObjects.Remove(potion.name);
            potion.SetActive(false);
            Character.baseStats.HP = Character.baseStats.MaxHP;
            this.WorldChanged = true;
        }
    }

    public void LevelUp()
    {
        if (Character.baseStats.Level >= 4) return;

        if (Character.baseStats.XP >= Character.baseStats.Level * 10)
        {
            if (!Character.LevelingUp)
            {
                Character.AddToDiary(" I am trying to level up...");
                Character.LevelingUp = true;
                Character.StopTime = Time.time + AutonomousCharacter.LEVELING_INTERVAL;
            }
            else if (Character.StopTime < Time.time)
            { 
                Character.baseStats.Level++;
                Character.baseStats.MaxHP += 10;
                Character.baseStats.XP = 0;
                Character.AddToDiary(" I leveled up to level " + Character.baseStats.Level);
                Character.LevelingUp = false;
                this.WorldChanged = true;
            }
        }
    }

 
    public void Rest()
    {
        if (!Character.Resting)
        {
            Character.AddToDiary(" I am resting");
            Character.Resting = true;
            Character.StopTime = Time.time + AutonomousCharacter.RESTING_INTERVAL;
        }
        else if (Character.StopTime < Time.time)
        {
            Character.baseStats.HP += AutonomousCharacter.REST_HP_RECOVERY;
            Character.baseStats.HP = Mathf.Min(Character.baseStats.HP, Character.baseStats.MaxHP);
            Character.Resting = false;
            this.WorldChanged = true;
        }
    }

    public void Teleport()
    {
        if (Character.baseStats.Level >= 2 && Character.baseStats.Mana >= 5)
        {
            Character.AddToDiary(" With my Mana I teleported away from danger.");
            Character.transform.position = this.initialPosition + Vector3.zero;
            Character.baseStats.Mana -= 5;
            this.WorldChanged = true;
        }

    }


    private bool CheckRange(GameObject obj, float maximumSqrDistance)
    {
        var distance = (obj.transform.position - Character.gameObject.transform.position).sqrMagnitude;
        return distance <= maximumSqrDistance;
    }


    public bool InMeleeRange(GameObject enemy)
    {
        return this.CheckRange(enemy, GameConstants.PICKUP_RANGE);
    }

    public bool InDivineSmiteRange(GameObject enemy)
    {
        return this.CheckRange(enemy, GameConstants.PICKUP_RANGE * 10);
    }

    public bool InChestRange(GameObject chest)
    {

        return this.CheckRange(chest, GameConstants.PICKUP_RANGE);
    }

    public bool InPotionRange(GameObject potion)
    {
        return this.CheckRange(potion, GameConstants.PICKUP_RANGE);
    }

}
