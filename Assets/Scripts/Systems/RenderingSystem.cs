using System;
using System.Collections.Generic;

/// <summary>
/// Simple animation state machine.
/// Syncs visual state with entity logic.
/// </summary>
public class AnimationController
{
    private Dictionary<string, string[]> _animations; // animation name -> frames
    private string _currentAnimation = "idle";
    private int _currentFrame = 0;
    private float _frameTime = 0.1f; // seconds per frame
    private float _elapsed = 0f;
    private bool _isLooping = true;

    public string CurrentAnimation => _currentAnimation;
    public int CurrentFrame => _currentFrame;
    public string CurrentFrameData => GetCurrentFrame();

    public AnimationController()
    {
        _animations = new Dictionary<string, string[]>
        {
            { "idle", new[] { "[    ]", "[.   ]" } },
            { "run", new[] { "[->  ]", "[ ->]" } },
            { "jump", new[] { "[^   ]" } },
            { "attack", new[] { "[*->]", "[ * ]" } },
            { "hit", new[] { "[OOO]" } },
            { "dead", new[] { "[XXX]" } }
        };
    }

    /// <summary>
    /// Play a specific animation.
    /// </summary>
    public void PlayAnimation(string name, bool loop = true)
    {
        if (_currentAnimation == name) return;
        
        if (_animations.ContainsKey(name))
        {
            _currentAnimation = name;
            _currentFrame = 0;
            _elapsed = 0f;
            _isLooping = loop;
        }
    }

    /// <summary>
    /// Update animation state.
    /// </summary>
    public void Update(float deltaTime)
    {
        if (!_animations.ContainsKey(_currentAnimation)) return;

        _elapsed += deltaTime;
        var frames = _animations[_currentAnimation];

        if (_elapsed >= _frameTime)
        {
            _elapsed -= _frameTime;
            _currentFrame++;

            if (_currentFrame >= frames.Length)
            {
                if (_isLooping)
                {
                    _currentFrame = 0;
                }
                else
                {
                    _currentFrame = frames.Length - 1;
                }
            }
        }
    }

    /// <summary>
    /// Get current frame's display data.
    /// </summary>
    public string GetCurrentFrame()
    {
        if (!_animations.ContainsKey(_currentAnimation))
            return "[?   ]";

        var frames = _animations[_currentAnimation];
        if (_currentFrame >= frames.Length)
            _currentFrame = 0;

        return frames[_currentFrame];
    }

    /// <summary>
    /// Add custom animation frames.
    /// </summary>
    public void AddAnimation(string name, params string[] frames)
    {
        _animations[name] = frames;
    }

    /// <summary>
    /// Check if animation has finished.
    /// </summary>
    public bool IsFinished()
    {
        if (!_isLooping && _animations.ContainsKey(_currentAnimation))
        {
            return _currentFrame >= _animations[_currentAnimation].Length - 1;
        }
        return false;
    }
}

/// <summary>
/// Rendering system that syncs animations with entity state.
/// </summary>
public class RenderingSystem : IUpdatable
{
    private Dictionary<string, (int x, int y)> _entityPositions = new Dictionary<string, (int, int)>();
    private Dictionary<string, AnimationController> _animations = new Dictionary<string, AnimationController>();

    public void RegisterEntity(string id, AnimationController animation)
    {
        if (!_animations.ContainsKey(id))
            _animations[id] = animation;

        if (!_entityPositions.ContainsKey(id))
            _entityPositions[id] = (0, 0);
    }

    public void UnregisterEntity(string id)
    {
        _animations.Remove(id);
        _entityPositions.Remove(id);
    }

    public void SetEntityPosition(string id, int x, int y)
    {
        if (_entityPositions.ContainsKey(id))
            _entityPositions[id] = (x, y);
    }

    public void Tick(float deltaTime)
    {
        // Update all animations
        foreach (var anim in _animations.Values)
        {
            anim.Update(deltaTime);
        }

        // Render (simple console rendering)
        RenderFrame();
    }

    public void Clear()
    {
        _animations.Clear();
        _entityPositions.Clear();
    }

    private void RenderFrame()
    {
        Console.Clear();
        Console.WriteLine("=== PixelForce Game ===");
        Console.WriteLine();

        // Simple grid rendering
        const int width = 60;
        const int height = 20;
        char[,] grid = new char[height, width];

        // Initialize grid
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                grid[y, x] = '.';
            }
        }

        // Draw entities
        foreach (var kvp in _animations)
        {
            var id = kvp.Key;
            var anim = kvp.Value;

            if (_entityPositions.TryGetValue(id, out var pos))
            {
                var frame = anim.GetCurrentFrame();
                int x = pos.x;
                int y = pos.y;

                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    // Simple rendering of frame
                    grid[y, x] = 'E'; // Placeholder
                }
            }
        }

        // Print grid
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Console.Write(grid[y, x]);
            }
            Console.WriteLine();
        }

        Console.WriteLine();
        Console.WriteLine("A/D: Move | W/Space: Jump | J: Attack | P: Pause | ESC: Quit");
    }
}