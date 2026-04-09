// Editor 스크립트: 시간/계절 시스템 SO 에셋 일괄 생성
// 사용: Unity 메뉴 SeedMind → Create Time Season Assets
// -> see docs/mcp/time-season-tasks.md Phase A-2, B-1, B-2, C-2, D-2
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using SeedMind.Core;

namespace SeedMind.Editor
{
    public static class CreateTimeSeasonAssets
    {
        [MenuItem("SeedMind/Create Time Season Assets")]
        public static void CreateAll()
        {
            CreateFolders();
            CreateTimeConfig();
            CreateSeasonData();
            CreateWeatherData();
            CreateFestivalData();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CreateTimeSeasonAssets] 모든 시간/계절 SO 에셋 생성 완료.");
        }

        private static void CreateFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Data/Core"))
                AssetDatabase.CreateFolder("Assets/_Project/Data", "Core");
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Data/Core/Seasons"))
                AssetDatabase.CreateFolder("Assets/_Project/Data/Core", "Seasons");
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Data/Core/Weather"))
                AssetDatabase.CreateFolder("Assets/_Project/Data/Core", "Weather");
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Data/Core/Festivals"))
                AssetDatabase.CreateFolder("Assets/_Project/Data/Core", "Festivals");
        }

        // --- TimeConfig ---
        private static void CreateTimeConfig()
        {
            const string path = "Assets/_Project/Data/Core/SO_TimeConfig.asset";
            if (AssetDatabase.LoadAssetAtPath<TimeConfig>(path) != null)
            {
                Debug.Log("[CreateTimeSeasonAssets] SO_TimeConfig 이미 존재, 건너뜀.");
                return;
            }
            var cfg = ScriptableObject.CreateInstance<TimeConfig>();
            // canonical: docs/systems/time-season-architecture.md 섹션 2.2
            cfg.secondsPerGameHour = 33.33f;
            cfg.dayStartHour = 6;
            cfg.dayEndHour = 24;
            cfg.daysPerSeason = 28;
            cfg.seasonsPerYear = 4;
            cfg.defaultTimeScale = 1.0f;
            cfg.maxTimeScale = 3.0f;
            AssetDatabase.CreateAsset(cfg, path);
            Debug.Log("[CreateTimeSeasonAssets] SO_TimeConfig 생성.");
        }

        // --- SeasonData 4개 ---
        // 수치 canonical: docs/systems/time-season.md 섹션 2.2, 2.3
        private static void CreateSeasonData()
        {
            CreateSeason(
                Season.Spring, "봄",
                new Color(1.00f, 0.98f, 0.93f), 1.0f,
                new Color(0.91f, 0.96f, 0.91f), new Color(0.90f, 0.95f, 0.90f), 0.005f,
                1.0f, 1.0f,
                new Color(0.87f, 0.96f, 0.87f),
                MakeSpringPhases()
            );
            CreateSeason(
                Season.Summer, "여름",
                new Color(1.00f, 1.00f, 0.95f), 1.15f,
                new Color(0.89f, 0.95f, 0.99f), new Color(0.88f, 0.94f, 0.98f), 0.003f,
                1.05f, 1.0f,
                new Color(0.40f, 0.60f, 0.30f),
                MakeSummerPhases()
            );
            CreateSeason(
                Season.Autumn, "가을",
                new Color(1.00f, 0.85f, 0.65f), 0.9f,
                new Color(1.00f, 0.95f, 0.88f), new Color(0.95f, 0.88f, 0.78f), 0.008f,
                1.0f, 1.1f,
                new Color(0.80f, 0.55f, 0.25f),
                MakeAutumnPhases()
            );
            CreateSeason(
                Season.Winter, "겨울",
                new Color(0.85f, 0.90f, 1.00f), 0.7f,
                new Color(0.91f, 0.91f, 0.97f), new Color(0.88f, 0.90f, 0.97f), 0.015f,
                0.0f, 1.0f,
                new Color(0.90f, 0.93f, 1.00f),
                MakeWinterPhases()
            );
        }

        private static DayPhaseVisual[] MakeSpringPhases()
        {
            // canonical: docs/systems/time-season-architecture.md 섹션 1.2 조명 테이블
            return new DayPhaseVisual[]
            {
                new DayPhaseVisual { phase=DayPhase.Dawn,      lightColor=new Color(1.00f,0.89f,0.71f), lightIntensity=0.6f, lightRotation=new Vector3(10f,-30f,0), ambientColor=new Color(0.45f,0.38f,0.33f), transitionDuration=10f },
                new DayPhaseVisual { phase=DayPhase.Morning,   lightColor=new Color(1.00f,0.98f,0.93f), lightIntensity=1.0f, lightRotation=new Vector3(30f,-30f,0), ambientColor=new Color(0.42f,0.44f,0.40f), transitionDuration=8f  },
                new DayPhaseVisual { phase=DayPhase.Afternoon, lightColor=new Color(1.00f,1.00f,1.00f), lightIntensity=1.0f, lightRotation=new Vector3(60f,-30f,0), ambientColor=new Color(0.44f,0.46f,0.42f), transitionDuration=5f  },
                new DayPhaseVisual { phase=DayPhase.Evening,   lightColor=new Color(1.00f,0.70f,0.28f), lightIntensity=0.7f, lightRotation=new Vector3(10f,150f,0),  ambientColor=new Color(0.45f,0.35f,0.27f), transitionDuration=8f  },
                new DayPhaseVisual { phase=DayPhase.Night,     lightColor=new Color(0.29f,0.43f,0.64f), lightIntensity=0.3f, lightRotation=new Vector3(-10f,30f,0),  ambientColor=new Color(0.18f,0.22f,0.32f), transitionDuration=10f },
            };
        }

        private static DayPhaseVisual[] MakeSummerPhases()
        {
            return new DayPhaseVisual[]
            {
                new DayPhaseVisual { phase=DayPhase.Dawn,      lightColor=new Color(1.00f,0.85f,0.60f), lightIntensity=0.65f, lightRotation=new Vector3(10f,-30f,0), ambientColor=new Color(0.46f,0.38f,0.30f), transitionDuration=10f },
                new DayPhaseVisual { phase=DayPhase.Morning,   lightColor=new Color(1.00f,1.00f,0.95f), lightIntensity=1.1f,  lightRotation=new Vector3(35f,-30f,0), ambientColor=new Color(0.45f,0.47f,0.42f), transitionDuration=8f  },
                new DayPhaseVisual { phase=DayPhase.Afternoon, lightColor=new Color(1.00f,1.00f,0.92f), lightIntensity=1.15f, lightRotation=new Vector3(65f,-30f,0), ambientColor=new Color(0.47f,0.49f,0.44f), transitionDuration=5f  },
                new DayPhaseVisual { phase=DayPhase.Evening,   lightColor=new Color(1.00f,0.60f,0.20f), lightIntensity=0.75f, lightRotation=new Vector3(10f,150f,0), ambientColor=new Color(0.46f,0.33f,0.22f), transitionDuration=8f  },
                new DayPhaseVisual { phase=DayPhase.Night,     lightColor=new Color(0.25f,0.38f,0.60f), lightIntensity=0.3f,  lightRotation=new Vector3(-10f,30f,0), ambientColor=new Color(0.16f,0.20f,0.30f), transitionDuration=10f },
            };
        }

        private static DayPhaseVisual[] MakeAutumnPhases()
        {
            return new DayPhaseVisual[]
            {
                new DayPhaseVisual { phase=DayPhase.Dawn,      lightColor=new Color(1.00f,0.80f,0.55f), lightIntensity=0.55f, lightRotation=new Vector3(8f,-30f,0),  ambientColor=new Color(0.43f,0.35f,0.28f), transitionDuration=10f },
                new DayPhaseVisual { phase=DayPhase.Morning,   lightColor=new Color(1.00f,0.90f,0.75f), lightIntensity=0.9f,  lightRotation=new Vector3(28f,-30f,0), ambientColor=new Color(0.44f,0.40f,0.35f), transitionDuration=8f  },
                new DayPhaseVisual { phase=DayPhase.Afternoon, lightColor=new Color(1.00f,0.92f,0.78f), lightIntensity=0.9f,  lightRotation=new Vector3(55f,-30f,0), ambientColor=new Color(0.45f,0.41f,0.36f), transitionDuration=5f  },
                new DayPhaseVisual { phase=DayPhase.Evening,   lightColor=new Color(0.95f,0.55f,0.20f), lightIntensity=0.65f, lightRotation=new Vector3(8f,150f,0),  ambientColor=new Color(0.44f,0.30f,0.20f), transitionDuration=8f  },
                new DayPhaseVisual { phase=DayPhase.Night,     lightColor=new Color(0.25f,0.35f,0.55f), lightIntensity=0.25f, lightRotation=new Vector3(-10f,30f,0), ambientColor=new Color(0.17f,0.20f,0.28f), transitionDuration=10f },
            };
        }

        private static DayPhaseVisual[] MakeWinterPhases()
        {
            return new DayPhaseVisual[]
            {
                new DayPhaseVisual { phase=DayPhase.Dawn,      lightColor=new Color(0.80f,0.85f,1.00f), lightIntensity=0.45f, lightRotation=new Vector3(6f,-30f,0),  ambientColor=new Color(0.33f,0.35f,0.42f), transitionDuration=10f },
                new DayPhaseVisual { phase=DayPhase.Morning,   lightColor=new Color(0.88f,0.92f,1.00f), lightIntensity=0.75f, lightRotation=new Vector3(22f,-30f,0), ambientColor=new Color(0.38f,0.40f,0.48f), transitionDuration=8f  },
                new DayPhaseVisual { phase=DayPhase.Afternoon, lightColor=new Color(0.90f,0.94f,1.00f), lightIntensity=0.7f,  lightRotation=new Vector3(40f,-30f,0), ambientColor=new Color(0.39f,0.41f,0.49f), transitionDuration=5f  },
                new DayPhaseVisual { phase=DayPhase.Evening,   lightColor=new Color(0.70f,0.75f,1.00f), lightIntensity=0.5f,  lightRotation=new Vector3(6f,150f,0),  ambientColor=new Color(0.30f,0.32f,0.42f), transitionDuration=8f  },
                new DayPhaseVisual { phase=DayPhase.Night,     lightColor=new Color(0.20f,0.25f,0.50f), lightIntensity=0.2f,  lightRotation=new Vector3(-10f,30f,0), ambientColor=new Color(0.14f,0.16f,0.28f), transitionDuration=10f },
            };
        }

        private static void CreateSeason(
            Season season, string displayName,
            Color sunColor, float sunIntensity,
            Color ambientColor, Color fogColor, float fogDensity,
            float growthMult, float shopPriceMult,
            Color terrainTint,
            DayPhaseVisual[] phases)
        {
            string name = $"SO_Season_{season}";
            string path = $"Assets/_Project/Data/Core/Seasons/{name}.asset";
            if (AssetDatabase.LoadAssetAtPath<SeasonData>(path) != null)
            {
                Debug.Log($"[CreateTimeSeasonAssets] {name} 이미 존재, 건너뜀.");
                return;
            }
            var sd = ScriptableObject.CreateInstance<SeasonData>();
            sd.season = season;
            sd.displayName = displayName;
            sd.sunColor = sunColor;
            sd.sunIntensity = sunIntensity;
            sd.ambientColor = ambientColor;
            sd.fogColor = fogColor;
            sd.fogDensity = fogDensity;
            sd.growthSpeedMultiplier = growthMult;
            sd.shopPriceMultiplier = shopPriceMult;
            sd.terrainTintColor = terrainTint;
            sd.phaseOverrides = phases;
            AssetDatabase.CreateAsset(sd, path);
            Debug.Log($"[CreateTimeSeasonAssets] {name} 생성.");
        }

        // --- WeatherData 4개 ---
        // 확률 canonical: docs/systems/time-season.md 섹션 3.2
        private static void CreateWeatherData()
        {
            CreateWeather(Season.Spring, 0.40f, 0.20f, 0.25f, 0.10f, 0.05f, 0.00f, 0.00f);
            CreateWeather(Season.Summer, 0.45f, 0.15f, 0.20f, 0.05f, 0.15f, 0.00f, 0.00f);
            CreateWeather(Season.Autumn, 0.30f, 0.20f, 0.25f, 0.15f, 0.10f, 0.00f, 0.00f);
            CreateWeather(Season.Winter, 0.15f, 0.15f, 0.00f, 0.00f, 0.00f, 0.35f, 0.35f);
        }

        private static void CreateWeather(Season season,
            float clear, float cloudy, float rain, float heavy, float storm, float snow, float blizzard)
        {
            string name = $"SO_Weather_{season}";
            string path = $"Assets/_Project/Data/Core/Weather/{name}.asset";
            if (AssetDatabase.LoadAssetAtPath<WeatherData>(path) != null)
            {
                Debug.Log($"[CreateTimeSeasonAssets] {name} 이미 존재, 건너뜀.");
                return;
            }
            var wd = ScriptableObject.CreateInstance<WeatherData>();
            wd.season = season;
            wd.clearChance = clear;
            wd.cloudyChance = cloudy;
            wd.rainChance = rain;
            wd.heavyRainChance = heavy;
            wd.stormChance = storm;
            wd.snowChance = snow;
            wd.blizzardChance = blizzard;
            // 연속 보정: canonical (time-season-architecture.md 섹션 2.4)
            wd.maxConsecutiveSameWeatherDays = 3;
            wd.maxConsecutiveExtremeWeatherDays = 2;
            wd.consecutivePenalty = 0.5f;
            // 날씨 효과: canonical (time-season-architecture.md 섹션 2.4)
            wd.rainGrowthBonus = 0.0f;
            wd.stormCropDamageChance = 0.1f;
            wd.blizzardWitherChance = 0.05f;
            AssetDatabase.CreateAsset(wd, path);
            Debug.Log($"[CreateTimeSeasonAssets] {name} 생성.");
        }

        // --- FestivalData 4개 ---
        // canonical: docs/systems/time-season.md 섹션 4.2
        private static void CreateFestivalData()
        {
            CreateFestival("봄 씨앗 축제",  "festival_spring_seed",      Season.Spring, 13, "봄의 씨앗을 나누는 축제. 희귀 씨앗을 교환할 수 있다.", 0.0f, 1.0f, "dialogue_festival_spring_seed");
            CreateFestival("여름 불꽃 축제", "festival_summer_fireworks", Season.Summer, 21, "여름 밤을 수놓는 불꽃놀이 축제.",                        0.0f, 1.0f, "dialogue_festival_summer_fireworks");
            CreateFestival("수확 축제",      "festival_autumn_harvest",   Season.Autumn, 21, "가을 수확의 기쁨을 나누는 작물 품평회.",                   0.0f, 1.0f, "dialogue_festival_autumn_harvest");
            CreateFestival("겨울 별빛 축제", "festival_winter_starlight", Season.Winter, 25, "겨울 밤 별빛 아래 소중한 이에게 선물을 전하는 축제.",      0.0f, 1.0f, "dialogue_festival_winter_starlight");
        }

        private static void CreateFestival(string displayName, string id, Season season, int day,
            string desc, float discount, float bonus, string dialogueKey)
        {
            string assetName = $"SO_Festival_{id}";
            string path = $"Assets/_Project/Data/Core/Festivals/{assetName}.asset";
            if (AssetDatabase.LoadAssetAtPath<FestivalData>(path) != null)
            {
                Debug.Log($"[CreateTimeSeasonAssets] {assetName} 이미 존재, 건너뜀.");
                return;
            }
            var fd = ScriptableObject.CreateInstance<FestivalData>();
            fd.festivalName = displayName;
            fd.festivalId = id;
            fd.season = season;
            fd.day = day;
            fd.description = desc;
            fd.shopDiscountRate = discount;
            fd.bonusMultiplier = bonus;
            fd.dialogueKey = dialogueKey;
            AssetDatabase.CreateAsset(fd, path);
            Debug.Log($"[CreateTimeSeasonAssets] {assetName} 생성.");
        }
    }
}
#endif
