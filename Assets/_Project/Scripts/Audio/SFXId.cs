namespace SeedMind.Audio
{
    /// <summary>
    /// 효과음 식별자. 전체 목록은 docs/systems/sound-design.md canonical.
    /// </summary>
    public enum SFXId
    {
        // 경작
        HoeTillBasic, HoeTillReinforced, HoeTillLegendary,
        WaterBasic, WaterReinforced, WaterLegendary,
        ScytheBasic, ScytheReinforced, ScytheLegendary,
        SeedPlant, Harvest, Fertilize,

        // 작물
        CropGrow, CropWither, CropGolden, CropGiant,

        // 도구
        ToolEquip, ToolUpgradeStart, ToolUpgradeComplete, ToolBreak,

        // 시설/건설
        ConstructStart, ConstructComplete, ConstructUpgrade,
        FacilityActivate, FacilityIdle,

        // 가공
        ProcessStart, ProcessComplete,
        MillRunning, FermentBubble, BakeryOven, CheeseChurn,

        // 목축
        AnimalFeed, AnimalMilk, AnimalShear, EggCollect,
        ChickenCluck, CowMoo, SheepBaa, GoatBleat,
        AnimalHappy, AnimalSick,

        // 낚시
        FishCastLine, FishNibble, FishBite, FishReelIn,
        FishStruggle, FishCatchNormal, FishCatchRare, FishEscape, FishSplash,

        // NPC/상점
        ShopOpen, ShopClose, Purchase, Sale,
        DialogueStart, DialogueAdvance, DialogueChoice, AffinityUp, ShippingBin,

        // 퀘스트/업적
        QuestAccept, QuestProgress, QuestComplete, QuestReward, AchievementToast,

        // UI
        UIClick, UIHover, UITab,
        InventoryOpen, InventoryClose, ItemMove,
        Notification, UIError, UIConfirm, UICancel,

        // 환경/날씨 Ambient
        AmbRainLight, AmbRainHeavy, AmbSnow, AmbWindLight,
        AmbBirdsDay, AmbStorm, Thunder, LightningFlash, AmbBlizzard,
        AmbCicada, AmbCricket, AmbInsectsNight, AmbWaves,
        FootstepDirt, FootstepGrass, FootstepWood, FootstepSnow, FootstepStone,

        // 진행/레벨
        LevelUp, XPGain, GoldGain, GoldSpend,
        EnergyWarning, EnergyDepleted,

        // 시간/계절
        MorningChime, EveningBell, MidnightWarning,
        PassOut, Sleep, SeasonTransition, DaySummary
    }
}
