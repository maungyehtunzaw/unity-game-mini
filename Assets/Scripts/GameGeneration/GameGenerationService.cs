using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Service responsible for procedurally generating thousands of game variations
/// from base game templates to reach the 10,000 game target efficiently
/// </summary>
public class GameGenerationService
{
    private List<GameTemplate> _baseTemplates = new List<GameTemplate>();
    private Dictionary<string, VariationConfig> _variationConfigs = new Dictionary<string, VariationConfig>();
    private System.Random _random;

    public GameGenerationService()
    {
        _random = new System.Random();
        InitializeBaseTemplates();
        InitializeVariationConfigs();
    }

    private void InitializeBaseTemplates()
    {
        // Define base game templates that can be varied
        _baseTemplates.AddRange(new GameTemplate[]
        {
            // Arcade Games (2000 variations)
            new GameTemplate("tap_trainer", "Tap Trainer", GameCategory.Arcade, 200),
            new GameTemplate("lane_runner", "Lane Runner", GameCategory.Arcade, 200),
            new GameTemplate("bubble_pop", "Bubble Pop", GameCategory.Arcade, 200),
            new GameTemplate("fruit_slice", "Fruit Slice", GameCategory.Arcade, 200),
            new GameTemplate("target_shoot", "Target Shoot", GameCategory.Arcade, 200),
            new GameTemplate("ball_bounce", "Ball Bounce", GameCategory.Arcade, 200),
            new GameTemplate("gem_match", "Gem Match", GameCategory.Arcade, 200),
            new GameTemplate("snake_classic", "Snake Classic", GameCategory.Arcade, 200),
            new GameTemplate("brick_break", "Brick Break", GameCategory.Arcade, 200),
            new GameTemplate("space_dodge", "Space Dodge", GameCategory.Arcade, 200),

            // Puzzle Games (2000 variations)
            new GameTemplate("slide_puzzle", "Slide Puzzle", GameCategory.Puzzle, 200),
            new GameTemplate("match_three", "Match Three", GameCategory.Puzzle, 200),
            new GameTemplate("word_find", "Word Find", GameCategory.Puzzle, 200),
            new GameTemplate("number_link", "Number Link", GameCategory.Puzzle, 200),
            new GameTemplate("color_fill", "Color Fill", GameCategory.Puzzle, 200),
            new GameTemplate("pipe_connect", "Pipe Connect", GameCategory.Puzzle, 200),
            new GameTemplate("block_fit", "Block Fit", GameCategory.Puzzle, 200),
            new GameTemplate("pattern_match", "Pattern Match", GameCategory.Puzzle, 200),
            new GameTemplate("logic_grid", "Logic Grid", GameCategory.Puzzle, 200),
            new GameTemplate("maze_solve", "Maze Solve", GameCategory.Puzzle, 200),

            // Casual Games (2000 variations)
            new GameTemplate("idle_tap", "Idle Tap", GameCategory.Casual, 200),
            new GameTemplate("merge_blocks", "Merge Blocks", GameCategory.Casual, 200),
            new GameTemplate("stack_tower", "Stack Tower", GameCategory.Casual, 200),
            new GameTemplate("draw_line", "Draw Line", GameCategory.Casual, 200),
            new GameTemplate("color_sort", "Color Sort", GameCategory.Casual, 200),
            new GameTemplate("tile_match", "Tile Match", GameCategory.Casual, 200),
            new GameTemplate("chain_reaction", "Chain Reaction", GameCategory.Casual, 200),
            new GameTemplate("rhythm_tap", "Rhythm Tap", GameCategory.Casual, 200),
            new GameTemplate("physics_drop", "Physics Drop", GameCategory.Casual, 200),
            new GameTemplate("memory_cards", "Memory Cards", GameCategory.Casual, 200),

            // Action Games (1500 variations)
            new GameTemplate("quick_reflex", "Quick Reflex", GameCategory.Action, 150),
            new GameTemplate("dodge_obstacles", "Dodge Obstacles", GameCategory.Action, 150),
            new GameTemplate("timing_jump", "Timing Jump", GameCategory.Action, 150),
            new GameTemplate("speed_tap", "Speed Tap", GameCategory.Action, 150),
            new GameTemplate("reaction_test", "Reaction Test", GameCategory.Action, 150),
            new GameTemplate("precision_aim", "Precision Aim", GameCategory.Action, 150),
            new GameTemplate("combo_chain", "Combo Chain", GameCategory.Action, 150),
            new GameTemplate("defense_wave", "Defense Wave", GameCategory.Action, 150),
            new GameTemplate("collect_rush", "Collect Rush", GameCategory.Action, 150),
            new GameTemplate("survival_mode", "Survival Mode", GameCategory.Action, 150),

            // Strategy Games (1000 variations)
            new GameTemplate("tower_defense", "Tower Defense", GameCategory.Strategy, 100),
            new GameTemplate("resource_manage", "Resource Manage", GameCategory.Strategy, 100),
            new GameTemplate("turn_based", "Turn Based", GameCategory.Strategy, 100),
            new GameTemplate("city_build", "City Build", GameCategory.Strategy, 100),
            new GameTemplate("chess_variant", "Chess Variant", GameCategory.Strategy, 100),
            new GameTemplate("card_battle", "Card Battle", GameCategory.Strategy, 100),
            new GameTemplate("territory_control", "Territory Control", GameCategory.Strategy, 100),
            new GameTemplate("auction_bid", "Auction Bid", GameCategory.Strategy, 100),
            new GameTemplate("diplomacy_game", "Diplomacy Game", GameCategory.Strategy, 100),
            new GameTemplate("economic_sim", "Economic Sim", GameCategory.Strategy, 100),

            // Educational Games (500 variations)
            new GameTemplate("math_quiz", "Math Quiz", GameCategory.Educational, 50),
            new GameTemplate("spelling_bee", "Spelling Bee", GameCategory.Educational, 50),
            new GameTemplate("geography_test", "Geography Test", GameCategory.Educational, 50),
            new GameTemplate("science_lab", "Science Lab", GameCategory.Educational, 50),
            new GameTemplate("history_timeline", "History Timeline", GameCategory.Educational, 50),
            new GameTemplate("language_learn", "Language Learn", GameCategory.Educational, 50),
            new GameTemplate("coding_puzzle", "Coding Puzzle", GameCategory.Educational, 50),
            new GameTemplate("art_creation", "Art Creation", GameCategory.Educational, 50),
            new GameTemplate("music_theory", "Music Theory", GameCategory.Educational, 50),
            new GameTemplate("typing_challenge", "Typing Challenge", GameCategory.Educational, 50),

            // Memory Games (500 variations)
            new GameTemplate("simon_says", "Simon Says", GameCategory.Memory, 50),
            new GameTemplate("sequence_repeat", "Sequence Repeat", GameCategory.Memory, 50),
            new GameTemplate("pairs_match", "Pairs Match", GameCategory.Memory, 50),
            new GameTemplate("pattern_recall", "Pattern Recall", GameCategory.Memory, 50),
            new GameTemplate("number_sequence", "Number Sequence", GameCategory.Memory, 50),
            new GameTemplate("color_sequence", "Color Sequence", GameCategory.Memory, 50),
            new GameTemplate("sound_memory", "Sound Memory", GameCategory.Memory, 50),
            new GameTemplate("spatial_memory", "Spatial Memory", GameCategory.Memory, 50),
            new GameTemplate("face_remember", "Face Remember", GameCategory.Memory, 50),
            new GameTemplate("story_recall", "Story Recall", GameCategory.Memory, 50),

            // Reflex Games (500 variations)
            new GameTemplate("whack_mole", "Whack Mole", GameCategory.Reflex, 50),
            new GameTemplate("catch_falling", "Catch Falling", GameCategory.Reflex, 50),
            new GameTemplate("button_mash", "Button Mash", GameCategory.Reflex, 50),
            new GameTemplate("stop_timer", "Stop Timer", GameCategory.Reflex, 50),
            new GameTemplate("follow_cursor", "Follow Cursor", GameCategory.Reflex, 50),
            new GameTemplate("avoid_collision", "Avoid Collision", GameCategory.Reflex, 50),
            new GameTemplate("perfect_timing", "Perfect Timing", GameCategory.Reflex, 50),
            new GameTemplate("multi_target", "Multi Target", GameCategory.Reflex, 50),
            new GameTemplate("sequence_tap", "Sequence Tap", GameCategory.Reflex, 50),
            new GameTemplate("coordination_test", "Coordination Test", GameCategory.Reflex, 50)
        });
    }

    private void InitializeVariationConfigs()
    {
        // Define how games can be varied
        _variationConfigs = new Dictionary<string, VariationConfig>
        {
            ["difficulty"] = new VariationConfig("Difficulty", new string[] { "Beginner", "Easy", "Normal", "Hard", "Expert", "Insane" }),
            ["speed"] = new VariationConfig("Speed", new string[] { "Slow", "Normal", "Fast", "Lightning", "Turbo" }),
            ["theme"] = new VariationConfig("Theme", new string[] { "Classic", "Neon", "Nature", "Space", "Ocean", "Desert", "Forest", "City", "Medieval", "Futuristic" }),
            ["size"] = new VariationConfig("Size", new string[] { "Mini", "Small", "Normal", "Large", "Mega" }),
            ["mode"] = new VariationConfig("Mode", new string[] { "Normal", "TimeAttack", "Survival", "Endless", "Challenge", "Zen" }),
            ["style"] = new VariationConfig("Style", new string[] { "Minimalist", "Colorful", "Retro", "Modern", "Cartoon", "Realistic" })
        };
    }

    public List<MiniGameDef> GenerateAllGames()
    {
        var generatedGames = new List<MiniGameDef>();
        int totalGenerated = 0;

        foreach (var template in _baseTemplates)
        {
            var variations = GenerateVariationsForTemplate(template);
            generatedGames.AddRange(variations);
            totalGenerated += variations.Count;
            
            Debug.Log($"Generated {variations.Count} variations for {template.baseName}");
        }

        Debug.Log($"Total games generated: {totalGenerated}");
        return generatedGames;
    }

    private List<MiniGameDef> GenerateVariationsForTemplate(GameTemplate template)
    {
        var variations = new List<MiniGameDef>();
        var usedCombinations = new HashSet<string>();

        // Generate the target number of variations for this template
        for (int i = 0; i < template.targetVariations; i++)
        {
            var variation = CreateVariation(template, i, usedCombinations);
            if (variation != null)
            {
                variations.Add(variation);
            }
        }

        return variations;
    }

    private MiniGameDef CreateVariation(GameTemplate template, int index, HashSet<string> usedCombinations)
    {
        int maxAttempts = 100;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            var combination = GenerateRandomCombination();
            string combinationKey = string.Join("|", combination.Values);

            if (!usedCombinations.Contains(combinationKey))
            {
                usedCombinations.Add(combinationKey);
                return CreateGameDefFromCombination(template, index, combination);
            }

            attempts++;
        }

        // If we can't find a unique combination, create a basic variation
        return CreateBasicVariation(template, index);
    }

    private Dictionary<string, string> GenerateRandomCombination()
    {
        var combination = new Dictionary<string, string>();
        
        // Randomly select 2-4 variation aspects for each game
        var selectedAspects = _variationConfigs.Keys.OrderBy(x => _random.Next()).Take(_random.Next(2, 5));
        
        foreach (var aspect in selectedAspects)
        {
            var config = _variationConfigs[aspect];
            var randomValue = config.values[_random.Next(config.values.Length)];
            combination[aspect] = randomValue;
        }

        return combination;
    }

    private MiniGameDef CreateGameDefFromCombination(GameTemplate template, int index, Dictionary<string, string> combination)
    {
        var gameDef = ScriptableObject.CreateInstance<MiniGameDef>();
        
        // Create unique ID and name
        string suffix = string.Join("_", combination.Values.Select(v => v.ToLower().Replace(" ", "")));
        gameDef.gameId = $"{template.baseId}_{index:000}_{suffix}";
        
        // Create descriptive name
        string variationDesc = string.Join(" ", combination.Values);
        gameDef.displayName = $"{template.baseName} ({variationDesc})";
        gameDef.description = $"A {variationDesc.ToLower()} variation of the classic {template.baseName} game.";
        
        // Set basic properties
        gameDef.category = template.category;
        gameDef.prefabKey = template.baseId; // All variations use the same base prefab
        gameDef.useAddressables = true;
        
        // Set difficulty based on combination
        if (combination.ContainsKey("difficulty"))
        {
            gameDef.baseDifficulty = ParseDifficulty(combination["difficulty"]);
        }
        
        // Adjust rewards based on difficulty and complexity
        gameDef.baseCoinReward = CalculateReward(template, combination);
        gameDef.estimatedPlayTime = CalculatePlayTime(template, combination);
        
        // Set unlock requirements
        gameDef.unlockedByDefault = index < 10; // First 10 variations are unlocked
        gameDef.requiredLevel = Mathf.Max(1, index / 20); // Every 20 games require next level
        
        // Add tags based on variations
        gameDef.tags = GenerateTags(template, combination);
        
        // Set priority for sorting
        gameDef.priority = _random.Next(0, 1000);
        
        return gameDef;
    }

    private MiniGameDef CreateBasicVariation(GameTemplate template, int index)
    {
        var gameDef = ScriptableObject.CreateInstance<MiniGameDef>();
        
        gameDef.gameId = $"{template.baseId}_{index:000}";
        gameDef.displayName = $"{template.baseName} #{index + 1}";
        gameDef.description = $"Variation {index + 1} of {template.baseName}.";
        gameDef.category = template.category;
        gameDef.prefabKey = template.baseId;
        gameDef.baseCoinReward = 10;
        gameDef.unlockedByDefault = index == 0;
        gameDef.priority = index;
        
        return gameDef;
    }

    private GameDifficulty ParseDifficulty(string difficultyStr)
    {
        switch (difficultyStr.ToLower())
        {
            case "beginner": return GameDifficulty.VeryEasy;
            case "easy": return GameDifficulty.Easy;
            case "normal": return GameDifficulty.Medium;
            case "hard": return GameDifficulty.Hard;
            case "expert": return GameDifficulty.VeryHard;
            case "insane": return GameDifficulty.Expert;
            default: return GameDifficulty.Medium;
        }
    }

    private int CalculateReward(GameTemplate template, Dictionary<string, string> combination)
    {
        int baseReward = 10;
        
        // Adjust based on difficulty
        if (combination.ContainsKey("difficulty"))
        {
            switch (combination["difficulty"].ToLower())
            {
                case "beginner": baseReward = 5; break;
                case "easy": baseReward = 8; break;
                case "normal": baseReward = 10; break;
                case "hard": baseReward = 15; break;
                case "expert": baseReward = 20; break;
                case "insane": baseReward = 25; break;
            }
        }
        
        // Adjust based on game mode
        if (combination.ContainsKey("mode"))
        {
            switch (combination["mode"].ToLower())
            {
                case "survival": baseReward = (int)(baseReward * 1.5f); break;
                case "challenge": baseReward = (int)(baseReward * 1.3f); break;
                case "timeattack": baseReward = (int)(baseReward * 1.2f); break;
            }
        }
        
        return baseReward;
    }

    private float CalculatePlayTime(GameTemplate template, Dictionary<string, string> combination)
    {
        float baseTime = 3f; // 3 minutes base
        
        if (combination.ContainsKey("mode"))
        {
            switch (combination["mode"].ToLower())
            {
                case "endless": return 10f;
                case "survival": return 7f;
                case "timeattack": return 2f;
                case "zen": return 15f;
            }
        }
        
        return baseTime;
    }

    private string[] GenerateTags(GameTemplate template, Dictionary<string, string> combination)
    {
        var tags = new List<string> { template.baseId };
        
        foreach (var kvp in combination)
        {
            tags.Add(kvp.Value.ToLower());
        }
        
        return tags.ToArray();
    }

    public void Cleanup()
    {
        _baseTemplates.Clear();
        _variationConfigs.Clear();
    }
}

[System.Serializable]
public class GameTemplate
{
    public string baseId;
    public string baseName;
    public GameCategory category;
    public int targetVariations;

    public GameTemplate(string id, string name, GameCategory cat, int variations)
    {
        baseId = id;
        baseName = name;
        category = cat;
        targetVariations = variations;
    }
}

[System.Serializable]
public class VariationConfig
{
    public string name;
    public string[] values;

    public VariationConfig(string configName, string[] configValues)
    {
        name = configName;
        values = configValues;
    }
}