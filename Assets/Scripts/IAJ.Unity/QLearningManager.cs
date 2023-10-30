using Assets.Scripts.IAJ.Unity.DecisionMaking.RL;
using UnityEngine;

public class QLearningManager : MonoBehaviour
{
    public static QLearningManager instance;
    public QLearning qLearning;
    public GameManager gameManager;

    private int currentIteration;

    public void Awake()
    {
        qLearning = new QLearning();
        currentIteration = 0;
        gameManager = GameObject.FindAnyObjectByType<GameManager>();
    }
}