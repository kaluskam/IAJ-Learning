﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS;
using Assets.Scripts.IAJ.Unity.DecisionMaking.RL;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions;
using Assets.Scripts.Game;
using Assets.Scripts.Game.NPCs;
using Assets.Scripts.IAJ.Unity.Utils;
using Assets.Scripts.IAJ.Unity;
using System.Runtime.CompilerServices;
using UnityEngine.TextCore.Text;

public class AutonomousCharacter : NPC
{
    //constants
    public const string SURVIVE_GOAL = "Survive";
    public const string GAIN_LEVEL_GOAL = "GainXP";
    public const string BE_QUICK_GOAL = "BeQuick";
    public const string GET_RICH_GOAL = "GetRich";

    public const float DECISION_MAKING_INTERVAL = 40f;
    public const float RESTING_INTERVAL = 5.0f;
    public const float LEVELING_INTERVAL = 10.0f;
    public const float ENEMY_NEAR_CHECK_INTERVAL = 0.5f;
    public const float ENEMY_DETECTION_RADIUS = 10.0f;
    public const int REST_HP_RECOVERY = 2;


    //UI Variables
    private Text SurviveGoalText;
    private Text GainXPGoalText;
    private Text BeQuickGoalText;
    private Text GetRichGoalText;
    private Text DiscontentmentText;
    private Text TotalProcessingTimeText;
    private Text BestDiscontentmentText;
    private Text ProcessedActionsText;
    private Text BestActionText;
    private Text BestActionSequence;
    private Text DiaryText;

    [Header("Character Settings")]
    public bool controlledByPlayer;
    public float Speed = 10.0f;

    [Header("Decision Algorithm Options")]
    public bool QLearningActive;
    public bool GOBActive;
    public bool GOAPActive;
    public bool MCTSActive;
    public bool MCTSBiasedPlayoutActive;
    public bool MCTSWithLimitedDepth;
    public int MCTSNumberOfPlayouts;
    public bool ChangeWhenEnemyNear = true;

    [Header("Character Info")]
    public bool Resting = false;
    public bool LevelingUp = false;
    public float StopTime;

    public string AlgorithmName;
    public Goal BeQuickGoal { get; private set; }
    public Goal SurviveGoal { get; private set; }
    public Goal GetRichGoal { get; private set; }
    public Goal GainLevelGoal { get; private set; }
    public List<Goal> Goals { get; set; }
    public List<Action> Actions { get; set; }
    public Action CurrentAction { get; private set; }
    public GOBDecisionMaking GOBDecisionMaking { get; set; }
    public DepthLimitedGOAPDecisionMaking GOAPDecisionMaking { get; set; }
    public MCTS MCTSDecisionMaking { get; set; }
    //public QLearning QLearningDecisionMaking { get; set; }

    public GameObject nearEnemy { get; private set; }
    public float Reward { get; set; }
    public RLState OldWorldState { get; set; }
    //private fields for internal use only

    private float nextUpdateTime = 0.0f;
    private float lastUpdateTime = 0.0f;
    private float lastEnemyCheckTime = 0.0f;
    private float previousGold = 0.0f;
    private int previousLevel = 1;
    public TextMesh playerText;
    private GameObject closestObject;
    private Stats initialBaseStats;

    // Draw path settings
    private LineRenderer lineRenderer;

    public void Start()
    {
        initializeActions();
        initializeUIText();
        initializeGoals();
        initialBaseStats = this.baseStats.Copy();
        ReStart();
        
    }

    private void initializeActions()
    {
        this.Actions = new List<Action>();

        //First it is necessary to add the actions made available by the elements in the scene
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Skeleton"))
        {
            this.Actions.Add(new SwordAttack(this, enemy));
            //this.Actions.Add(new DivineSmite(this, enemy));
        }

        foreach (var enemy in GameObject.FindGameObjectsWithTag("Orc"))
        {
            this.Actions.Add(new SwordAttack(this, enemy));
        }

        foreach (var enemy in GameObject.FindGameObjectsWithTag("Dragon"))
        {
            this.Actions.Add(new SwordAttack(this, enemy));
        }

        foreach (var chest in GameObject.FindGameObjectsWithTag("Chest"))
        {
            this.Actions.Add(new PickUpChest(this, chest));
        }

        //foreach (var potion in GameObject.FindGameObjectsWithTag("HealthPotion"))
        //{
        //    this.Actions.Add(new GetHealthPotion(this, potion));
        //}

        //foreach (var potion in GameObject.FindGameObjectsWithTag("ManaPotion"))
        //{
        //    this.Actions.Add(new GetManaPotion(this, potion));
        //}

        //Then we have a series of extra actions available to Sir Uthgard
        this.Actions.Add(new LevelUp(this));
        //this.Actions.Add(new ShieldOfFaith(this));
        //this.Actions.Add(new Rest(this));
        //this.Actions.Add(new Teleport(this));
    }

    private void initializeUIText()
    {
        // Initializing UI Text
        this.BeQuickGoalText = GameObject.Find("BeQuickGoal").GetComponent<Text>();
        this.SurviveGoalText = GameObject.Find("SurviveGoal").GetComponent<Text>();
        this.GainXPGoalText = GameObject.Find("GainXP").GetComponent<Text>();
        this.GetRichGoalText = GameObject.Find("GetRichGoal").GetComponent<Text>();
        this.DiscontentmentText = GameObject.Find("Discontentment").GetComponent<Text>();
        this.TotalProcessingTimeText = GameObject.Find("ProcessTime").GetComponent<Text>();
        this.BestDiscontentmentText = GameObject.Find("BestDicont").GetComponent<Text>();
        this.ProcessedActionsText = GameObject.Find("ProcComb").GetComponent<Text>();
        this.BestActionText = GameObject.Find("BestAction").GetComponent<Text>();
        this.BestActionSequence = GameObject.Find("BestActionSequence").GetComponent<Text>();
        DiaryText = GameObject.Find("DiaryText").GetComponent<Text>();
    }

    private void initializeGoals()
    {
        this.SurviveGoal = new Goal(SURVIVE_GOAL, 11f)
        {

        };

        this.GainLevelGoal = new Goal(GAIN_LEVEL_GOAL, 3f)
        {
            InsistenceValue = 10.0f,
            ChangeRate = 0.2f
        };

        this.GetRichGoal = new Goal(GET_RICH_GOAL, 4f)
        {
            InsistenceValue = 5.0f,
            ChangeRate = 0.2f
        };

        this.BeQuickGoal = new Goal(BE_QUICK_GOAL, 0f)
        {
            ChangeRate = 1f,
        };

        this.Goals = new List<Goal>
        {
            this.SurviveGoal,
            this.BeQuickGoal,
            this.GetRichGoal,
            this.GainLevelGoal
        };
    }

    public void ReStart()
    {
        this.baseStats = this.initialBaseStats.Copy();
        lineRenderer = this.GetComponent<LineRenderer>();
        playerText.text = "";
        nearEnemy = null;
        // Initialization of Decision Making Algorithms
        if (!this.controlledByPlayer)
        {
            if (this.GOBActive)
            {
                this.GOBDecisionMaking = new GOBDecisionMaking(this.Actions, this.Goals);
                this.AlgorithmName = "GOB" + " SG: " + this.SurviveGoal.Weight
                    + " GL: " + this.GainLevelGoal.Weight
                    + " BQ: " + this.BeQuickGoal.Weight
                    + " GR: " + this.GetRichGoal.Weight;

            }
            else if (this.GOAPActive)
            {
                // the worldModel is necessary for the GOAP and MCTS algorithms that need to predict action effects on the world...
                var worldModel = new CurrentStateWorldModel(GameManager.Instance, this.Actions, this.Goals);
                this.GOAPDecisionMaking = new DepthLimitedGOAPDecisionMaking(worldModel, this.Actions, this.Goals);
                this.AlgorithmName = "GOAP" + " SG: " + this.SurviveGoal.Weight
                    + " GL: " + this.GainLevelGoal.Weight
                    + " BQ: " + this.BeQuickGoal.Weight
                    + " GR: " + this.GetRichGoal.Weight;
            }
            else if (this.MCTSActive)
            {
                var worldModel = new CurrentStateWorldModel(GameManager.Instance, this.Actions, this.Goals);
                this.AlgorithmName = "MCTS";
                if (this.MCTSBiasedPlayoutActive)
                {
                    this.MCTSDecisionMaking = new MCTSBiasedPlayout(worldModel);
                    this.AlgorithmName += " Biased";
                }
                else
                {
                    this.MCTSDecisionMaking = new MCTS(worldModel);
                }
                this.MCTSDecisionMaking.LimitedDepth = this.MCTSWithLimitedDepth;
                this.MCTSDecisionMaking.NumberOfPlayouts = this.MCTSNumberOfPlayouts;
                if (this.MCTSWithLimitedDepth) this.AlgorithmName += " Limited";
                this.AlgorithmName += " P: " + this.MCTSNumberOfPlayouts + " Iter: " + this.MCTSDecisionMaking.MaxIterations;
            }

            else if (this.QLearningActive)
            {
                //var worldModel = new CurrentStateWorldModel(GameManager.Instance, this.Actions, this.Goals);
                this.AlgorithmName = "QLearning";
                //QLearningDecisionMaking = new QLearning(worldModel);
            }
        }

        DiaryText.text += "My Diary \n I awoke. What a wonderful day to kill Monsters! \n";
    }

    private void UpdateQLearning()
    {
        GameManager.qLearning.currentState = new CurrentStateWorldModel(GameManager.Instance, this.Actions, this.Goals);
        var action = GameManager.qLearning.ChooseAction();


        if (action != null)
        {
            this.CurrentAction = action;
        }
        this.OldWorldState = RLState.Create(new CurrentStateWorldModel(GameManager.Instance, this.Actions, this.Goals));
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.gameEnded) return;

        //Agent Perception 
        if (Time.time > this.lastEnemyCheckTime + ENEMY_NEAR_CHECK_INTERVAL)
        {
            GameObject enemy = CheckEnemies(ENEMY_DETECTION_RADIUS);
            if (enemy != null)
            {
                if (this.ChangeWhenEnemyNear)
                {
                    GameManager.Instance.WorldChanged = true;
                }
                //AddToDiary(" There is " + enemy.name + " in front of me!");
                this.nearEnemy = enemy;
            }
            else
            {
                this.nearEnemy = null;
            }
            this.lastEnemyCheckTime = Time.time;
        }

        if (QLearningActive && GameManager.Instance.WorldChanged) 
        {
            
        }
        
        this.nextUpdateTime = Time.time + DECISION_MAKING_INTERVAL;
        var duration = Time.time - this.lastUpdateTime;

        //first step, perceptions
        //update the agent's goals based on the state of the world

        // Max Health minus current Health
        this.SurviveGoal.InsistenceValue = baseStats.MaxHP - baseStats.HP;
        // Normalize it to 0-10
        this.SurviveGoal.InsistenceValue = NormalizeGoalValues(this.SurviveGoal.InsistenceValue, 0, baseStats.Level * 10);

        this.BeQuickGoal.InsistenceValue = baseStats.Time;
        this.BeQuickGoal.InsistenceValue = NormalizeGoalValues(this.BeQuickGoal.InsistenceValue, 0, (float)GameManager.GameConstants.TIME_LIMIT);

        this.GainLevelGoal.InsistenceValue += this.GainLevelGoal.ChangeRate * duration; //increase in goal over time
        if (baseStats.Level > this.previousLevel)
        {
            this.GainLevelGoal.InsistenceValue -= (baseStats.Level - this.previousLevel) * 10;
            this.previousLevel = baseStats.Level;
        }
        this.GainLevelGoal.InsistenceValue = NormalizeGoalValues(this.GainLevelGoal.InsistenceValue, 0, baseStats.Level * 10);

        this.GetRichGoal.InsistenceValue += this.GetRichGoal.ChangeRate * duration - (baseStats.Money - this.previousGold);
        this.GetRichGoal.InsistenceValue = NormalizeGoalValues(this.GetRichGoal.InsistenceValue, 0, 25);
        this.previousGold = baseStats.Money;

        this.SurviveGoalText.text = "Survive: " + this.SurviveGoal.InsistenceValue + " (" + this.SurviveGoal.Weight + ")";
        this.GainXPGoalText.text = "Gain Level: " + this.GainLevelGoal.InsistenceValue.ToString("F1") + " (" + this.GainLevelGoal.Weight + ")"; ;
        this.BeQuickGoalText.text = "Be Quick: " + this.BeQuickGoal.InsistenceValue.ToString("F1") + " (" + this.BeQuickGoal.Weight + ")"; ;
        this.GetRichGoalText.text = "GetRich: " + this.GetRichGoal.InsistenceValue.ToString("F1") + " (" + this.GetRichGoal.Weight + ")"; ;
        this.DiscontentmentText.text = "Discontentment: " + this.CalculateDiscontentment().ToString("F1");

        this.lastUpdateTime = Time.time;

        //To have a new decision lets initialize Decision Making Proccess

        if (GOBActive)
        {
            this.GOBDecisionMaking.InProgress = true;
        }
        else if (GOAPActive)  //Add here other Algorithms...
        {
            this.GOAPDecisionMaking.InitializeDecisionMakingProcess();
        }
        else if (MCTSActive)
        {
            this.MCTSDecisionMaking.InitializeMCTSearch();
        }

        else if (QLearningActive)
        { 


        }
        //else if (QLearningActive)
        //{
        //    //IMPORTANT
        //    if (GameManager.Instance.WorldChanged)
        //    {
        //        var newState = RLState.Create(new CurrentStateWorldModel(GameManager.Instance, this.Actions, this.Goals));
        //        this.QLearningDecisionMaking.UpdateQTable(this.OldWorldState, this.CurrentAction, this.Reward, newState);
        //    }
        //    this.QLearningDecisionMaking.Initialize();
        //}

    

        if (this.controlledByPlayer)
        {
            //Using the old Input System
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                this.transform.position += new Vector3(0.0f, 0.0f, 0.1f) * this.Speed;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                this.transform.position += new Vector3(0.0f, 0.0f, -0.1f) * this.Speed;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                this.transform.position += new Vector3(-0.1f, 0.0f, 0.0f) * this.Speed;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                this.transform.position += new Vector3(0.1f, 0.0f, 0.0f) * this.Speed;
            if (Input.GetKey(KeyCode.F))
                if (closestObject != null)
                {
                    //Simple way of checking which object is closest to Sir Uthgard
                    var s = playerText.text.ToString();
                    if (s.Contains("Health Potion"))
                        PickUpHealthPotion();
                    else if (s.Contains("Mana Potion"))
                        PickUpManaPotion();
                    else if (s.Contains("Chest"))
                        PickUpChest();
                    else if (s.Contains("Enemy"))
                        AttackEnemy();
                }
        }

 

        else if (this.QLearningActive && GameManager.qLearning.learningInProgress) {

            if (GameManager.Instance.WorldChanged)
            {
                this.UpdateQLearning();

                this.AddToDiary(" I am going to " + this.CurrentAction.Name + "\n");
                var newState = RLState.Create(new CurrentStateWorldModel(GameManager.Instance, this.Actions, this.Goals));

                GameManager.qLearning.UpdateQTable(this.OldWorldState, this.CurrentAction, this.Reward, newState);
                GameManager.Instance.WorldChanged = false;
                this.Reward = 0;
            }

            if (this.CurrentAction != null )
            {
                this.CurrentAction.Execute();
                //AddToDiary("Executing " + this.CurrentAction.Name + "\n");
            }
        }
        else if (this.QLearningActive && !GameManager.qLearning.learningInProgress) {
            if (GameManager.Instance.WorldChanged)
            {
                this.UpdateQLearning();

                this.AddToDiary(" I am going to " + this.CurrentAction.Name + "\n");

                GameManager.Instance.WorldChanged = false;

            }

            if (this.CurrentAction != null)
            {
                this.CurrentAction.Execute();

            }
        }

        

        if (navMeshAgent.hasPath) {DrawPath();}
    }

    private GameObject CheckEnemies(float detectionRadius)
    {
        foreach (var enemy in GameManager.Instance.enemies)
        {
            Transform characterTransform = this.navMeshAgent.GetComponent<Transform>();
            Vector3 enemyDirection = enemy.GetComponent<Transform>().position - characterTransform.position;
            Vector3 characterDirection = navMeshAgent.velocity.normalized;
            //actually it checks if the enemy is in front of the character... it returns the first one...
            if (enemyDirection.sqrMagnitude < detectionRadius*detectionRadius &&
                Mathf.Cos(MathHelper.ConvertVectorToOrientation(characterDirection) - MathHelper.ConvertVectorToOrientation(enemyDirection.normalized)) > 0)
            {
                return enemy;
            }
        }
        return null;
    }

    public void AddToDiary(string s)
    {
        DiaryText.text += Time.time + s + "\n";

        if (DiaryText.text.Length > 600)
            DiaryText.text = DiaryText.text.Substring(500);
    }

    

    void DrawPath()
    {
       
        lineRenderer.positionCount = navMeshAgent.path.corners.Length;
        lineRenderer.SetPosition(0, this.transform.position);

        if (navMeshAgent.path.corners.Length < 2)
        {
            return;
        }

        for (int i = 1; i < navMeshAgent.path.corners.Length; i++)
        {
            Vector3 pointPosition = new Vector3(navMeshAgent.path.corners[i].x, navMeshAgent.path.corners[i].y, navMeshAgent.path.corners[i].z);
            lineRenderer.SetPosition(i, pointPosition);
        }

    }


    public float CalculateDiscontentment()
    {
        var discontentment = 0.0f;

        foreach (var goal in this.Goals)
        {
            discontentment += goal.GetDiscontentment();
        }
        return discontentment;
    }

    //Functions designed for when the Player has control of the character
    void OnTriggerEnter(Collider col)
    {
        if (this.controlledByPlayer)
        {
            if (col.gameObject.tag.ToString().Contains("HealthPotion"))
            {
                playerText.text = "Pickup Health Potion";
                closestObject = col.gameObject;
            }
            else if (col.gameObject.tag.ToString().Contains("ManaPotion"))
            {
                playerText.text = "Pickup Mana Potion";
                closestObject = col.gameObject;
            }
            else if (col.gameObject.tag.ToString().Contains("Chest"))
            {
                playerText.text = "Pickup Chest";
                closestObject = col.gameObject;
            }
            else if (col.gameObject.tag.ToString().Contains("Orc") || col.gameObject.tag.ToString().Contains("Skeleton"))
            {
                playerText.text = "Attack Enemy";
                closestObject = col.gameObject;
            }
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag.ToString() != "")
            playerText.text = "";
    }


    //Actions designed for when the Player has control of the character
    void PickUpHealthPotion()
    {
        if (closestObject != null)
            if (GameManager.Instance.InPotionRange(closestObject))
            {
                GameManager.Instance.GetHealthPotion(closestObject);
                closestObject = null;
                playerText.text = "";
            }
    }

    void PickUpManaPotion()
    {
        if (closestObject != null)
            if (GameManager.Instance.InPotionRange(closestObject))
            {
                GameManager.Instance.GetManaPotion(closestObject);
                closestObject = null;
                playerText.text = "";
            }
    }


    void PickUpChest()
    {
        if (closestObject != null)
            if (GameManager.Instance.InChestRange(closestObject))
            {
                GameManager.Instance.PickUpChest(closestObject);
                closestObject = null;
                playerText.text = "";
            }
    }

    void AttackEnemy()

    {
        if (closestObject != null)
            if (GameManager.Instance.InMeleeRange(closestObject))
            {
                GameManager.Instance.SwordAttack(closestObject);
                closestObject = null;
                playerText.text = "";
            }
    }


    // Normalize different goal values to 0-10 ranges according to their max
    float NormalizeGoalValues(float value, float min, float max)
    {
        if (value < 0) value = 0.0f;
        // Normalizing to 0-1
        var x = (value - min) / (max - min);

        // Multiplying it by 10
        x *= 10;

        return x;
    }

}
