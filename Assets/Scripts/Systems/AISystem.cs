using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple state machine for enemy AI.
/// </summary>
public class StateMachine
{
    private Dictionary<AIState, Action> _states = new Dictionary<AIState, Action>();
    private AIState _currentState = AIState.Idle;
    private AIState _previousState = AIState.Idle;

    public AIState CurrentState => _currentState;
    public AIState PreviousState => _previousState;

    public void AddState(AIState state, Action onUpdate)
    {
        _states[state] = onUpdate;
    }

    public void ChangeState(AIState newState)
    {
        if (_currentState == newState) return;

        _previousState = _currentState;
        _currentState = newState;
    }

    public void Update()
    {
        if (_states.TryGetValue(_currentState, out var action))
        {
            action?.Invoke();
        }
    }

    public void Clear()
    {
        _states.Clear();
    }
}

/// <summary>
/// Detection component for enemies (vision radius, line-of-sight).
/// </summary>
public class DetectionComponent
{
    public float VisionRadius { get; set; } = 10f;
    public bool HasLineOfSight { get; set; }
    public PlayerCharacter DetectedPlayer { get; set; }

    public void Update(Vector2 detectorPos, PlayerCharacter player)
    {
        if (player == null || !player.IsActive)
        {
            DetectedPlayer = null;
            HasLineOfSight = false;
            return;
        }

        var distance = Vector2.Distance(detectorPos, player.transform.position);
        
        if (distance <= VisionRadius)
        {
            DetectedPlayer = player;
            // Simplified line-of-sight (just distance for now)
            HasLineOfSight = true;
        }
        else
        {
            DetectedPlayer = null;
            HasLineOfSight = false;
        }
    }
}

/// <summary>
/// Centralized AI system managing all enemy behaviors.
/// </summary>
public class AISystem : IUpdatable
{
    private List<EnemyBase> _enemies = new List<EnemyBase>();
    private PlayerCharacter _player;
    private float _updateTimer = 0f;
    private float _updateInterval = 0.1f; // Update AI every 100ms

    public void RegisterEnemy(EnemyBase enemy)
    {
        if (!_enemies.Contains(enemy))
            _enemies.Add(enemy);
    }

    public void UnregisterEnemy(EnemyBase enemy)
    {
        _enemies.Remove(enemy);
    }

    public void SetPlayer(PlayerCharacter player)
    {
        _player = player;
    }

    public void Tick(float deltaTime)
    {
        _updateTimer += deltaTime;

        if (_updateTimer >= _updateInterval)
        {
            _updateTimer -= _updateInterval;

            foreach (var enemy in _enemies)
            {
                if (enemy.IsActive)
                {
                    if (enemy is Enemy e)
                        e.UpdateAI(_player);
                }
            }
        }
    }

    public void Clear()
    {
        _enemies.Clear();
    }
}