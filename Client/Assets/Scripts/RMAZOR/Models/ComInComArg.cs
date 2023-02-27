namespace RMAZOR.Models
{
    public static class ComInComArg
    {
        public const string KeyLevelIndex             = "level_index";
        public const string KeyLevelIndexMainLevels   = "level_index_main";
        public const string KeyLevelIndexPuzzleLevels = "level_index_puzzles";
        public const string KeyLevelIndexBigLevels    = "level_index_big_levels";
        
        public const string KeyDailyChallengeIndex       = "daily_challenge_index";
        public const string KeyIsDailyChallengeSuccess   = "is_daily_challenge_success";
        public const string KeyDailyChallengeRewardMoney = "daily_challenge_reward_money";
        public const string KeyDailyChallengeRewardXp    = "daily_challenge_reward_xp";
        
        public const string KeyGameMode                            = "game_mode";
        public const string KeyLoadFirstLevelInGroup               = "load_first_level_in_group";
        public const string KeyNextLevelType                       = "next_level_type";
        public const string KeyCurrentLevelType                    = "current_level_type";
        public const string KeyLoadShopPanelFromCharacterDiedPanel = "load_shop_panel_from_character_died_panel";
        public const string KeySetBackgroundFromEditor             = "set_background_from_editor";
        public const string KeyDeathPosition                       = "death_position";
        public const string KeySkipLevel                           = "skip_level";
        public const string KeySource                              = "source";
        public const string KeyOnReadyToLoadLevelFinishAction      = "on_ready_to_load_level_finish_action";
        public const string KeyChallengeGoal                       = "level_time";
        public const string KeyRandomLevelSize                     = "random_level_size";
        public const string KeyCharacterId                         = "character_id";
        public const string KeyChallengeType                       = "challenge_type";
        public const string KeyCharacterXp                         = "character_xp";
        public const string KeyAiSimulation                        = "ai_simulation";

        public const string KeyRemoveTrapsFromLevel = "remove_traps_from_level";

        public const string KeyMoneyCount                   = "money_count";
        public const string KeyPassCommandsRecord           = "pass_commands_record";
        public const string KeyShowPuzzleLevelHint          = "show_puzzle_level_hint";
        public const string KeyAdditionalCameraEffectAction = "additional_camera_effect_action";

        public const string ParameterLevelTypeBonus         = "bonus";
        public const string ParameterLevelTypeDefault       = "default";

        public const string ParameterGameModeMain           = "main";
        public const string ParameterGameModeRandom         = "random";
        public const string ParameterGameModeDailyChallenge = "daily_challenge";
        public const string ParameterGameModePuzzles        = "puzzles";
        public const string ParameterGameModeBigLevels      = "big_levels";

        public const string ParameterChallengeTypeTimer = "challenge_type_timer";
        public const string ParameterChallengeTypeSteps = "challenge_type_steps";

        public const string ParameterSourceScreenTap             = "screen_tap";
        public const string ParameterSourceFinishLevelGroupPanel = "finish_levels_group_panel";
        public const string ParameterSourceLevelsPanel           = "levels_panel";
        public const string ParameterSourcePlayBonusLevelPanel   = "play_bonus_level_panel";
        public const string ParameterSourceCharacterDiedPanel    = "character_died_panel";
        public const string ParameterSourceLevelSkipper          = "level_skipper";
        public const string ParameterSourceMainMenu              = "main_menu";
    }
}