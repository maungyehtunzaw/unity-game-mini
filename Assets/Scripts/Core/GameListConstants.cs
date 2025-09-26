using System.Collections.Generic;
using UnityEngine;

namespace MiniGameHub.Core
{
    /// <summary>
    /// Constants and configuration for game list management.
    /// Controls which games are enabled/disabled and their display order.
    /// </summary>
    public static class GameListConstants
    {
        #region Game Categories
        public const string CATEGORY_PUZZLE = "Puzzle";
        public const string CATEGORY_ACTION = "Action";
        public const string CATEGORY_MEMORY = "Memory";
        public const string CATEGORY_STRATEGY = "Strategy";
        public const string CATEGORY_REFLEX = "Reflex";
        public const string CATEGORY_CASUAL = "Casual";
        public const string CATEGORY_WORD = "Word";
        public const string CATEGORY_MATH = "Math";
        public const string CATEGORY_ALL = "All";
        #endregion

        #region Game Identifiers
        // Core games
        public const string GAME_TIC_TAC_TOE = "tic_tac_toe";
        public const string GAME_MEMORY_MATCH = "memory_match";
        public const string GAME_TAP_TRAINER = "tap_trainer";
        public const string GAME_LANE_RUNNER = "lane_runner";
        
        // Puzzle games
        public const string GAME_SLIDING_PUZZLE = "sliding_puzzle";
        public const string GAME_JIGSAW = "jigsaw";
        public const string GAME_BLOCK_PUZZLE = "block_puzzle";
        public const string GAME_MATCH_THREE = "match_three";
        
        // Action games
        public const string GAME_SNAKE = "snake";
        public const string GAME_BREAKOUT = "breakout";
        public const string GAME_SPACE_SHOOTER = "space_shooter";
        public const string GAME_PLATFORMER = "platformer";
        
        // Memory games
        public const string GAME_CARD_MATCH = "card_match";
        public const string GAME_SEQUENCE_MEMORY = "sequence_memory";
        public const string GAME_NUMBER_MEMORY = "number_memory";
        public const string GAME_COLOR_MEMORY = "color_memory";
        
        // Strategy games
        public const string GAME_CHESS_MINI = "chess_mini";
        public const string GAME_CHECKERS = "checkers";
        public const string GAME_CONNECT_FOUR = "connect_four";
        public const string GAME_REVERSI = "reversi";
        
        // Reflex games
        public const string GAME_REACTION_TIME = "reaction_time";
        public const string GAME_WHACK_MOLE = "whack_mole";
        public const string GAME_SIMON_SAYS = "simon_says";
        public const string GAME_RHYTHM_TAP = "rhythm_tap";
        
        // Casual games
        public const string GAME_BUBBLE_POP = "bubble_pop";
        public const string GAME_INFINITE_JUMPER = "infinite_jumper";
        public const string GAME_FLAPPY_CLONE = "flappy_clone";
        public const string GAME_STACK_BUILDER = "stack_builder";
        
        // Word games
        public const string GAME_WORD_SEARCH = "word_search";
        public const string GAME_ANAGRAM = "anagram";
        public const string GAME_HANGMAN = "hangman";
        public const string GAME_CROSSWORD_MINI = "crossword_mini";
        
        // Math games
        public const string GAME_MATH_QUIZ = "math_quiz";
        public const string GAME_NUMBER_SEQUENCE = "number_sequence";
        public const string GAME_CALCULATOR_RACE = "calculator_race";
        public const string GAME_GEOMETRY_CHALLENGE = "geometry_challenge";
        #endregion

        #region Game Configuration
        /// <summary>
        /// List of games that are currently enabled and available to play
        /// </summary>
        public static readonly HashSet<string> EnabledGames = new HashSet<string>
        {
            // Core games (always enabled)
            GAME_TIC_TAC_TOE,
            GAME_MEMORY_MATCH,
            GAME_TAP_TRAINER,
            GAME_LANE_RUNNER,
            
            // Popular games
            GAME_SLIDING_PUZZLE,
            GAME_SNAKE,
            GAME_CARD_MATCH,
            GAME_CONNECT_FOUR,
            GAME_REACTION_TIME,
            GAME_BUBBLE_POP,
            GAME_WORD_SEARCH,
            GAME_MATH_QUIZ,
            
            // Additional enabled games
            GAME_MATCH_THREE,
            GAME_BREAKOUT,
            GAME_SEQUENCE_MEMORY,
            GAME_WHACK_MOLE,
            GAME_INFINITE_JUMPER,
            GAME_ANAGRAM,
            GAME_NUMBER_SEQUENCE
        };

        /// <summary>
        /// Games that are temporarily disabled (under development, maintenance, etc.)
        /// </summary>
        public static readonly HashSet<string> DisabledGames = new HashSet<string>
        {
            GAME_CHESS_MINI,        // Complex AI needed
            GAME_CROSSWORD_MINI,    // Word database needed
            GAME_GEOMETRY_CHALLENGE // Advanced math engine needed
        };

        /// <summary>
        /// Premium games that require purchase or special unlock
        /// </summary>
        public static readonly HashSet<string> PremiumGames = new HashSet<string>
        {
            GAME_SPACE_SHOOTER,
            GAME_PLATFORMER,
            GAME_REVERSI,
            GAME_HANGMAN,
            GAME_CALCULATOR_RACE
        };

        /// <summary>
        /// Featured games to highlight on the home page
        /// </summary>
        public static readonly List<string> FeaturedGames = new List<string>
        {
            GAME_TIC_TAC_TOE,
            GAME_MEMORY_MATCH,
            GAME_SNAKE,
            GAME_MATCH_THREE,
            GAME_REACTION_TIME
        };

        /// <summary>
        /// Game categories and their associated games
        /// </summary>
        public static readonly Dictionary<string, List<string>> GamesByCategory = new Dictionary<string, List<string>>
        {
            {
                CATEGORY_PUZZLE, new List<string>
                {
                    GAME_TIC_TAC_TOE, GAME_SLIDING_PUZZLE, GAME_JIGSAW,
                    GAME_BLOCK_PUZZLE, GAME_MATCH_THREE
                }
            },
            {
                CATEGORY_ACTION, new List<string>
                {
                    GAME_SNAKE, GAME_BREAKOUT, GAME_SPACE_SHOOTER,
                    GAME_PLATFORMER, GAME_LANE_RUNNER
                }
            },
            {
                CATEGORY_MEMORY, new List<string>
                {
                    GAME_MEMORY_MATCH, GAME_CARD_MATCH, GAME_SEQUENCE_MEMORY,
                    GAME_NUMBER_MEMORY, GAME_COLOR_MEMORY
                }
            },
            {
                CATEGORY_STRATEGY, new List<string>
                {
                    GAME_CHESS_MINI, GAME_CHECKERS, GAME_CONNECT_FOUR, GAME_REVERSI
                }
            },
            {
                CATEGORY_REFLEX, new List<string>
                {
                    GAME_REACTION_TIME, GAME_WHACK_MOLE, GAME_SIMON_SAYS,
                    GAME_RHYTHM_TAP, GAME_TAP_TRAINER
                }
            },
            {
                CATEGORY_CASUAL, new List<string>
                {
                    GAME_BUBBLE_POP, GAME_INFINITE_JUMPER, GAME_FLAPPY_CLONE, GAME_STACK_BUILDER
                }
            },
            {
                CATEGORY_WORD, new List<string>
                {
                    GAME_WORD_SEARCH, GAME_ANAGRAM, GAME_HANGMAN, GAME_CROSSWORD_MINI
                }
            },
            {
                CATEGORY_MATH, new List<string>
                {
                    GAME_MATH_QUIZ, GAME_NUMBER_SEQUENCE, GAME_CALCULATOR_RACE, GAME_GEOMETRY_CHALLENGE
                }
            }
        };
        #endregion

        #region Game Display Properties
        /// <summary>
        /// Display names for games (for UI)
        /// </summary>
        public static readonly Dictionary<string, string> GameDisplayNames = new Dictionary<string, string>
        {
            { GAME_TIC_TAC_TOE, "Tic Tac Toe" },
            { GAME_MEMORY_MATCH, "Memory Match" },
            { GAME_TAP_TRAINER, "Tap Trainer" },
            { GAME_LANE_RUNNER, "Lane Runner" },
            { GAME_SLIDING_PUZZLE, "Sliding Puzzle" },
            { GAME_JIGSAW, "Jigsaw Puzzle" },
            { GAME_BLOCK_PUZZLE, "Block Puzzle" },
            { GAME_MATCH_THREE, "Match 3" },
            { GAME_SNAKE, "Snake" },
            { GAME_BREAKOUT, "Breakout" },
            { GAME_SPACE_SHOOTER, "Space Shooter" },
            { GAME_PLATFORMER, "Platformer" },
            { GAME_CARD_MATCH, "Card Match" },
            { GAME_SEQUENCE_MEMORY, "Sequence Memory" },
            { GAME_NUMBER_MEMORY, "Number Memory" },
            { GAME_COLOR_MEMORY, "Color Memory" },
            { GAME_CHESS_MINI, "Mini Chess" },
            { GAME_CHECKERS, "Checkers" },
            { GAME_CONNECT_FOUR, "Connect 4" },
            { GAME_REVERSI, "Reversi" },
            { GAME_REACTION_TIME, "Reaction Time" },
            { GAME_WHACK_MOLE, "Whack-a-Mole" },
            { GAME_SIMON_SAYS, "Simon Says" },
            { GAME_RHYTHM_TAP, "Rhythm Tap" },
            { GAME_BUBBLE_POP, "Bubble Pop" },
            { GAME_INFINITE_JUMPER, "Infinite Jumper" },
            { GAME_FLAPPY_CLONE, "Flappy Bird" },
            { GAME_STACK_BUILDER, "Stack Builder" },
            { GAME_WORD_SEARCH, "Word Search" },
            { GAME_ANAGRAM, "Anagram" },
            { GAME_HANGMAN, "Hangman" },
            { GAME_CROSSWORD_MINI, "Mini Crossword" },
            { GAME_MATH_QUIZ, "Math Quiz" },
            { GAME_NUMBER_SEQUENCE, "Number Sequence" },
            { GAME_CALCULATOR_RACE, "Calculator Race" },
            { GAME_GEOMETRY_CHALLENGE, "Geometry Challenge" }
        };

        /// <summary>
        /// Game descriptions for UI tooltips/details
        /// </summary>
        public static readonly Dictionary<string, string> GameDescriptions = new Dictionary<string, string>
        {
            { GAME_TIC_TAC_TOE, "Classic 3x3 grid strategy game" },
            { GAME_MEMORY_MATCH, "Match pairs of cards to clear the board" },
            { GAME_TAP_TRAINER, "Tap targets as fast as possible" },
            { GAME_LANE_RUNNER, "Navigate through lanes avoiding obstacles" },
            { GAME_SLIDING_PUZZLE, "Slide tiles to complete the picture" },
            { GAME_SNAKE, "Eat food and grow while avoiding walls" },
            { GAME_MATCH_THREE, "Match 3 or more gems in a row" },
            { GAME_BREAKOUT, "Bounce ball to break all the bricks" },
            { GAME_CONNECT_FOUR, "Connect four pieces in a row to win" },
            { GAME_REACTION_TIME, "Test how fast you can react" },
            { GAME_BUBBLE_POP, "Pop colorful bubbles for points" },
            { GAME_WORD_SEARCH, "Find hidden words in the grid" },
            { GAME_MATH_QUIZ, "Solve math problems quickly" },
            { GAME_WHACK_MOLE, "Hit the moles as they pop up" },
            { GAME_SEQUENCE_MEMORY, "Remember and repeat sequences" },
            { GAME_ANAGRAM, "Unscramble letters to form words" }
        };

        /// <summary>
        /// Difficulty levels for games
        /// </summary>
        public enum GameDifficulty
        {
            Easy = 1,
            Medium = 2,
            Hard = 3,
            Expert = 4
        }

        /// <summary>
        /// Default difficulty settings for games
        /// </summary>
        public static readonly Dictionary<string, GameDifficulty> GameDifficulties = new Dictionary<string, GameDifficulty>
        {
            { GAME_TIC_TAC_TOE, GameDifficulty.Easy },
            { GAME_MEMORY_MATCH, GameDifficulty.Medium },
            { GAME_TAP_TRAINER, GameDifficulty.Easy },
            { GAME_SNAKE, GameDifficulty.Medium },
            { GAME_MATCH_THREE, GameDifficulty.Medium },
            { GAME_REACTION_TIME, GameDifficulty.Easy },
            { GAME_CONNECT_FOUR, GameDifficulty.Hard },
            { GAME_MATH_QUIZ, GameDifficulty.Medium }
        };
        #endregion

        #region Utility Methods
        /// <summary>
        /// Check if a game is enabled and available to play
        /// </summary>
        public static bool IsGameEnabled(string gameId)
        {
            return EnabledGames.Contains(gameId) && !DisabledGames.Contains(gameId);
        }

        /// <summary>
        /// Check if a game requires premium access
        /// </summary>
        public static bool IsGamePremium(string gameId)
        {
            return PremiumGames.Contains(gameId);
        }

        /// <summary>
        /// Check if a game is featured
        /// </summary>
        public static bool IsGameFeatured(string gameId)
        {
            return FeaturedGames.Contains(gameId);
        }

        /// <summary>
        /// Get display name for a game
        /// </summary>
        public static string GetGameDisplayName(string gameId)
        {
            return GameDisplayNames.TryGetValue(gameId, out string displayName) ? displayName : gameId;
        }

        /// <summary>
        /// Get description for a game
        /// </summary>
        public static string GetGameDescription(string gameId)
        {
            return GameDescriptions.TryGetValue(gameId, out string description) ? description : "Fun mini-game";
        }

        /// <summary>
        /// Get all games in a specific category that are enabled
        /// </summary>
        public static List<string> GetEnabledGamesInCategory(string category)
        {
            var allGames = new List<string>();
            
            if (category == CATEGORY_ALL)
            {
                allGames.AddRange(EnabledGames);
            }
            else if (GamesByCategory.TryGetValue(category, out List<string> categoryGames))
            {
                foreach (string game in categoryGames)
                {
                    if (IsGameEnabled(game))
                    {
                        allGames.Add(game);
                    }
                }
            }
            
            return allGames;
        }

        /// <summary>
        /// Get all available categories that have enabled games
        /// </summary>
        public static List<string> GetAvailableCategories()
        {
            var availableCategories = new List<string> { CATEGORY_ALL };
            
            foreach (var category in GamesByCategory.Keys)
            {
                if (GetEnabledGamesInCategory(category).Count > 0)
                {
                    availableCategories.Add(category);
                }
            }
            
            return availableCategories;
        }
        #endregion
    }
}