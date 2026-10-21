using System;
using UnityEngine;

/// Put one on the Player. Other scripts reach it through PlayerStats.Instance.
public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("Starting Values")]
    [SerializeField, Min(0)] private int money = 0;
    [SerializeField, Min(0)] private int food = 0;   
    [SerializeField, Min(0)] private int drinks = 0;
    [SerializeField, Min(0)] private int reputationLevel = 0;    

    [Header("Reputation")]
    [Tooltip("Reputation points needed to go from level 0 to level 1.")]
    [SerializeField, Min(1)] private int reputationPerLevel = 100;
    [Tooltip("Extra points needed for each level after that. 0 = every level costs the same.")]
    [SerializeField, Min(0)] private int extraPerLevel = 25;

    private int reputationPoints; 
    private int totalReputation;  

    public event Action Changed;

    public int Money => money;
    public int Food => food;
    public int Drinks => drinks;
    public int ReputationLevel => reputationLevel;
    public int ReputationPoints => reputationPoints;
    public int PointsToNextLevel => reputationPerLevel + extraPerLevel * reputationLevel;
    public int TotalReputation => totalReputation;
    public float ReputationProgress => (float)reputationPoints / PointsToNextLevel;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Debug.LogWarning("There's more than one PlayerStats in the scene. Using the newest one.", this);
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void AddMoney(int amount)  => Change(ref money, amount);
    public void AddFood(int amount)   => Change(ref food, amount);
    public void AddDrinks(int amount) => Change(ref drinks, amount);

    public void AddReputation(int points)
    {
        if (points == 0) return;

        reputationPoints += points;
        totalReputation += points;

        while (reputationPoints >= PointsToNextLevel)
        {
            reputationPoints -= PointsToNextLevel;
            reputationLevel++;
        }

        if (reputationPoints < 0)
        {
            totalReputation -= reputationPoints;
            reputationPoints = 0;
        }

        Changed?.Invoke();
    }

    private void Change(ref int stat, int amount)
    {
        if (amount == 0) return;
        stat = Mathf.Max(0, stat + amount);
        Changed?.Invoke();
    }
}