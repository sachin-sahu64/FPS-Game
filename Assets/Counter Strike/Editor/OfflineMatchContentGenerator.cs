using System;
using System.IO;
using FPSGame.AI;
using FPSGame.Combat;
using FPSGame.Economy;
using FPSGame.Input;
using FPSGame.Movement;
using FPSGame.Objectives;
using FPSGame.Rounds;
using FPSGame.Spawning;
using FPSGame.Weapons;
using FPSGame.World;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace FPSGame.Editor
{
    public static class OfflineMatchContentGenerator
    {
        private const string RootFolder = "Assets/Counter Strike";
        private const string ConfigFolder = RootFolder + "/Configs";
        private const string MaterialFolder = RootFolder + "/Materials";
        private const string PrefabFolder = RootFolder + "/Prefabs";
        private const string SceneFolder = RootFolder + "/Scenes";
        private const string AutoRunFlagPath = RootFolder + "/Editor/GenerateOffline32v32.flag";

        [InitializeOnLoadMethod]
        private static void AutoRunWhenFlagged()
        {
            string absoluteFlagPath = Path.Combine(Application.dataPath, "Counter Strike/Editor/GenerateOffline32v32.flag");
            if (!File.Exists(absoluteFlagPath))
            {
                return;
            }

            try
            {
                File.Delete(absoluteFlagPath);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            EditorApplication.delayCall += GenerateOffline32v32Content;
        }

        [MenuItem("Tools/FPS Game/Generate Offline 32v32 Content")]
        public static void GenerateOffline32v32Content()
        {
            GenerateInternal(exitOnCompletion: false);
        }

        public static void GenerateOffline32v32ContentBatch()
        {
            GenerateInternal(exitOnCompletion: true);
        }

        private static void GenerateInternal(bool exitOnCompletion)
        {
            try
            {
                EnsureFolders();

                PlayerMovementSettings movementSettings = CreateMovementSettings();
                RecoilPattern recoilPattern = CreateRecoilPattern();
                WeaponDefinition weaponDefinition = CreateWeaponDefinition(recoilPattern);
                EconomyTable economyTable = CreateEconomyTable();
                RoundSettings roundSettings = CreateRoundSettings();
                OfflineMatchSettings offlineMatchSettings = CreateOfflineMatchSettings();

                Material floorMaterial = CreateMaterial("MAT_Floor", new Color(0.29f, 0.31f, 0.34f));
                Material wallMaterial = CreateMaterial("MAT_Wall", new Color(0.55f, 0.57f, 0.62f));
                Material coverMaterial = CreateMaterial("MAT_Cover", new Color(0.36f, 0.24f, 0.14f));
                Material bombSiteMaterial = CreateMaterial("MAT_BombSite", new Color(0.72f, 0.18f, 0.12f));
                Material botBodyMaterial = CreateMaterial("MAT_BotBody", new Color(0.15f, 0.41f, 0.26f));
                Material gunMaterial = CreateMaterial("MAT_Gun", new Color(0.11f, 0.12f, 0.13f));

                GameObject localPlayerPrefab = CreateLocalPlayerPrefab(movementSettings, weaponDefinition, gunMaterial);
                GameObject botPrefab = CreateBotPrefab(movementSettings, weaponDefinition, botBodyMaterial, gunMaterial);

                CreateOfflineTestScene(
                    localPlayerPrefab,
                    botPrefab,
                    offlineMatchSettings,
                    roundSettings,
                    economyTable,
                    floorMaterial,
                    wallMaterial,
                    coverMaterial,
                    bombSiteMaterial);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                if (exitOnCompletion)
                {
                    UnityEditor.EditorApplication.Exit(0);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);

                if (exitOnCompletion)
                {
                    UnityEditor.EditorApplication.Exit(1);
                }
                else
                {
                    throw;
                }
            }
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "Counter Strike");
            EnsureFolder(RootFolder, "Configs");
            EnsureFolder(ConfigFolder, "Movement");
            EnsureFolder(ConfigFolder, "Weapons");
            EnsureFolder(ConfigFolder, "Economy");
            EnsureFolder(ConfigFolder, "Rounds");
            EnsureFolder(ConfigFolder, "Spawning");
            EnsureFolder(RootFolder, "Materials");
            EnsureFolder(RootFolder, "Prefabs");
            EnsureFolder(PrefabFolder, "Actors");
            EnsureFolder(RootFolder, "Scenes");
            EnsureFolder(RootFolder, "Editor");
        }

        private static void EnsureFolder(string parent, string child)
        {
            string fullPath = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(fullPath))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static PlayerMovementSettings CreateMovementSettings()
        {
            return CreateAsset(
                ConfigFolder + "/Movement/DefaultPlayerMovementSettings.asset",
                () => ScriptableObject.CreateInstance<PlayerMovementSettings>(),
                serialized =>
                {
                    serialized.FindProperty("walkSpeed").floatValue = 3.6f;
                    serialized.FindProperty("runSpeed").floatValue = 5.9f;
                    serialized.FindProperty("crouchSpeed").floatValue = 2.15f;
                    serialized.FindProperty("acceleration").floatValue = 20f;
                    serialized.FindProperty("groundFriction").floatValue = 13f;
                    serialized.FindProperty("airAcceleration").floatValue = 6f;
                    serialized.FindProperty("gravity").floatValue = -24f;
                    serialized.FindProperty("jumpHeight").floatValue = 1.05f;
                    serialized.FindProperty("standingHeight").floatValue = 1.8f;
                    serialized.FindProperty("crouchedHeight").floatValue = 1.15f;
                    serialized.FindProperty("crouchTransitionSpeed").floatValue = 12f;
                });
        }

        private static RecoilPattern CreateRecoilPattern()
        {
            return CreateAsset(
                ConfigFolder + "/Weapons/DefaultRifleRecoil.asset",
                () => ScriptableObject.CreateInstance<RecoilPattern>(),
                serialized =>
                {
                    SerializedProperty offsets = serialized.FindProperty("sprayOffsets");
                    Vector2[] values =
                    {
                        new(0.15f, 0.6f),
                        new(-0.1f, 0.72f),
                        new(0.18f, 0.85f),
                        new(-0.2f, 1.05f),
                        new(0.24f, 1.12f),
                        new(-0.18f, 1.18f),
                        new(0.12f, 1.25f)
                    };

                    offsets.arraySize = values.Length;
                    for (int index = 0; index < values.Length; index++)
                    {
                        offsets.GetArrayElementAtIndex(index).vector2Value = values[index];
                    }

                    serialized.FindProperty("resetDelay").floatValue = 0.32f;
                });
        }

        private static WeaponDefinition CreateWeaponDefinition(RecoilPattern recoilPattern)
        {
            return CreateAsset(
                ConfigFolder + "/Weapons/DefaultRifle.asset",
                () => ScriptableObject.CreateInstance<WeaponDefinition>(),
                serialized =>
                {
                    serialized.FindProperty("weaponId").stringValue = "rifle_default";
                    serialized.FindProperty("automatic").boolValue = true;
                    serialized.FindProperty("magazineSize").intValue = 30;
                    serialized.FindProperty("reserveAmmo").intValue = 120;
                    serialized.FindProperty("reloadTime").floatValue = 2.2f;
                    serialized.FindProperty("fireRate").floatValue = 9f;
                    serialized.FindProperty("damage").floatValue = 34f;
                    serialized.FindProperty("range").floatValue = 90f;
                    serialized.FindProperty("baseSpread").floatValue = 0.006f;
                    serialized.FindProperty("movementSpreadMultiplier").floatValue = 0.045f;
                    serialized.FindProperty("penetrationPower").floatValue = 1.8f;
                    serialized.FindProperty("penetrationProbeDistance").floatValue = 2.2f;
                    serialized.FindProperty("damageFalloffPerMeter").floatValue = 0.92f;
                    serialized.FindProperty("hitMask").intValue = ~0;
                    serialized.FindProperty("recoilPattern").objectReferenceValue = recoilPattern;
                });
        }

        private static EconomyTable CreateEconomyTable()
        {
            return CreateAsset(
                ConfigFolder + "/Economy/DefaultEconomyTable.asset",
                () => ScriptableObject.CreateInstance<EconomyTable>(),
                _ => { });
        }

        private static RoundSettings CreateRoundSettings()
        {
            return CreateAsset(
                ConfigFolder + "/Rounds/DefaultRoundSettings.asset",
                () => ScriptableObject.CreateInstance<RoundSettings>(),
                serialized =>
                {
                    serialized.FindProperty("freezeTime").floatValue = 6f;
                    serialized.FindProperty("roundTime").floatValue = 100f;
                    serialized.FindProperty("bombTime").floatValue = 35f;
                    serialized.FindProperty("buyTime").floatValue = 12f;
                    serialized.FindProperty("maxRounds").intValue = 24;
                    serialized.FindProperty("startingCash").intValue = 800;
                });
        }

        private static OfflineMatchSettings CreateOfflineMatchSettings()
        {
            return CreateAsset(
                ConfigFolder + "/Spawning/Offline32v32Settings.asset",
                () => ScriptableObject.CreateInstance<OfflineMatchSettings>(),
                serialized =>
                {
                    serialized.FindProperty("playersPerSide").intValue = 32;
                    serialized.FindProperty("includeLocalPlayer").boolValue = true;
                    serialized.FindProperty("localPlayerSide").enumValueIndex = (int)TeamSide.CounterTerrorists;
                    serialized.FindProperty("spawnScatterRadius").floatValue = 1.65f;
                });
        }

        private static Material CreateMaterial(string assetName, Color baseColor)
        {
            string path = MaterialFolder + "/" + assetName + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            material.color = baseColor;
            material.SetFloat("_Smoothness", 0.05f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static GameObject CreateLocalPlayerPrefab(PlayerMovementSettings movementSettings, WeaponDefinition weaponDefinition, Material gunMaterial)
        {
            GameObject root = new("LocalPlayer");
            CharacterController controller = root.AddComponent<CharacterController>();
            controller.radius = 0.35f;
            controller.height = 1.8f;
            controller.center = new Vector3(0f, 0.9f, 0f);

            root.AddComponent<KeyboardMouseInputSource>();
            PlayerMotor motor = root.AddComponent<PlayerMotor>();
            FirstPersonLook look = root.AddComponent<FirstPersonLook>();
            root.AddComponent<Health>();
            root.AddComponent<PlayerEconomy>();
            TeamMember teamMember = root.AddComponent<TeamMember>();
            AudioSource audioSource = root.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f;
            audioSource.playOnAwake = false;
            root.AddComponent<FootstepAudioController>();
            root.AddComponent<RoundActorLifecycle>();

            GameObject lookPivot = CreateChild(root, "LookPivot", new Vector3(0f, 1.6f, 0f));
            Camera camera = lookPivot.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.nearClipPlane = 0.01f;
            camera.fieldOfView = 78f;
            lookPivot.AddComponent<AudioListener>();

            GameObject gunRoot = CreateChild(lookPivot, "WeaponRoot", new Vector3(0.22f, -0.22f, 0.48f));
            GameObject gunMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gunMesh.name = "GunMesh";
            gunMesh.transform.SetParent(gunRoot.transform, false);
            gunMesh.transform.localScale = new Vector3(0.18f, 0.12f, 0.75f);
            AssignMaterial(gunMesh, gunMaterial);
            RemoveCollider(gunMesh);

            GameObject muzzle = CreateChild(gunRoot, "Muzzle", new Vector3(0f, 0f, 0.45f));
            HitscanWeapon weapon = gunRoot.AddComponent<HitscanWeapon>();

            teamMember.Configure(TeamSide.CounterTerrorists, false);

            SerializedObject motorSerialized = new(motor);
            motorSerialized.FindProperty("settings").objectReferenceValue = movementSettings;
            motorSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject lookSerialized = new(look);
            lookSerialized.FindProperty("yawRoot").objectReferenceValue = root.transform;
            lookSerialized.FindProperty("pitchRoot").objectReferenceValue = lookPivot.transform;
            lookSerialized.FindProperty("sensitivity").floatValue = 0.08f;
            lookSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject weaponSerialized = new(weapon);
            weaponSerialized.FindProperty("definition").objectReferenceValue = weaponDefinition;
            weaponSerialized.FindProperty("aimCamera").objectReferenceValue = camera;
            weaponSerialized.FindProperty("muzzle").objectReferenceValue = muzzle.transform;
            weaponSerialized.FindProperty("motor").objectReferenceValue = motor;
            weaponSerialized.FindProperty("look").objectReferenceValue = look;
            weaponSerialized.FindProperty("drawHumanTracers").boolValue = true;
            weaponSerialized.FindProperty("drawBotTracers").boolValue = false;
            weaponSerialized.FindProperty("humanTracerDuration").floatValue = 0.08f;
            weaponSerialized.FindProperty("botTracerDuration").floatValue = 0.18f;
            weaponSerialized.FindProperty("humanTracerWidth").floatValue = 0.02f;
            weaponSerialized.FindProperty("botTracerWidth").floatValue = 0.06f;
            weaponSerialized.ApplyModifiedPropertiesWithoutUndo();

            string prefabPath = PrefabFolder + "/Actors/LocalPlayer.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject CreateBotPrefab(PlayerMovementSettings movementSettings, WeaponDefinition weaponDefinition, Material bodyMaterial, Material gunMaterial)
        {
            GameObject root = new("BotSoldier");
            CharacterController controller = root.AddComponent<CharacterController>();
            controller.radius = 0.35f;
            controller.height = 1.8f;
            controller.center = new Vector3(0f, 0.9f, 0f);

            NavMeshAgent navMeshAgent = root.AddComponent<NavMeshAgent>();
            navMeshAgent.enabled = false;
            navMeshAgent.radius = 0.35f;
            navMeshAgent.height = 1.8f;
            navMeshAgent.speed = 5.8f;
            navMeshAgent.angularSpeed = 720f;
            navMeshAgent.acceleration = 18f;
            navMeshAgent.stoppingDistance = 12f;

            root.AddComponent<BotInputSource>();
            PlayerMotor motor = root.AddComponent<PlayerMotor>();
            root.AddComponent<Health>();
            root.AddComponent<PlayerEconomy>();
            TeamMember teamMember = root.AddComponent<TeamMember>();
            AudioSource audioSource = root.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.playOnAwake = false;
            root.AddComponent<FootstepAudioController>();
            root.AddComponent<RoundActorLifecycle>();

            GameObject bodyMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bodyMesh.name = "BodyMesh";
            bodyMesh.transform.SetParent(root.transform, false);
            bodyMesh.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            bodyMesh.transform.localScale = new Vector3(0.7f, 0.9f, 0.7f);
            AssignMaterial(bodyMesh, bodyMaterial);
            RemoveCollider(bodyMesh);

            GameObject pitchPivot = CreateChild(root, "PitchPivot", new Vector3(0f, 1.45f, 0f));
            GameObject gunRoot = CreateChild(pitchPivot, "WeaponRoot", new Vector3(0f, -0.02f, 0.3f));
            GameObject gunMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gunMesh.name = "GunMesh";
            gunMesh.transform.SetParent(gunRoot.transform, false);
            gunMesh.transform.localScale = new Vector3(0.16f, 0.1f, 0.65f);
            AssignMaterial(gunMesh, gunMaterial);
            RemoveCollider(gunMesh);

            GameObject muzzle = CreateChild(gunRoot, "Muzzle", new Vector3(0f, 0f, 0.4f));
            HitscanWeapon weapon = gunRoot.AddComponent<HitscanWeapon>();
            BotController botController = root.AddComponent<BotController>();

            teamMember.Configure(TeamSide.Terrorists, true);

            SerializedObject motorSerialized = new(motor);
            motorSerialized.FindProperty("settings").objectReferenceValue = movementSettings;
            motorSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject weaponSerialized = new(weapon);
            weaponSerialized.FindProperty("definition").objectReferenceValue = weaponDefinition;
            weaponSerialized.FindProperty("muzzle").objectReferenceValue = muzzle.transform;
            weaponSerialized.FindProperty("motor").objectReferenceValue = motor;
            weaponSerialized.FindProperty("drawHumanTracers").boolValue = false;
            weaponSerialized.FindProperty("drawBotTracers").boolValue = true;
            weaponSerialized.FindProperty("humanTracerDuration").floatValue = 0.08f;
            weaponSerialized.FindProperty("botTracerDuration").floatValue = 0.18f;
            weaponSerialized.FindProperty("humanTracerWidth").floatValue = 0.02f;
            weaponSerialized.FindProperty("botTracerWidth").floatValue = 0.06f;
            weaponSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject botSerialized = new(botController);
            botSerialized.FindProperty("teamMember").objectReferenceValue = teamMember;
            botSerialized.FindProperty("navMeshAgent").objectReferenceValue = navMeshAgent;
            botSerialized.FindProperty("primaryWeapon").objectReferenceValue = weapon;
            botSerialized.FindProperty("yawRoot").objectReferenceValue = root.transform;
            botSerialized.FindProperty("pitchRoot").objectReferenceValue = pitchPivot.transform;
            botSerialized.FindProperty("eyePoint").objectReferenceValue = pitchPivot.transform;
            botSerialized.FindProperty("engageDistance").floatValue = 90f;
            botSerialized.FindProperty("stopDistance").floatValue = 16f;
            botSerialized.FindProperty("targetLossDistance").floatValue = 200f;
            botSerialized.FindProperty("fireConeAngle").floatValue = 8f;
            botSerialized.FindProperty("separationRadius").floatValue = 2.4f;
            botSerialized.FindProperty("separationStrength").floatValue = 2.2f;
            botSerialized.ApplyModifiedPropertiesWithoutUndo();

            string prefabPath = PrefabFolder + "/Actors/BotSoldier.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static void CreateOfflineTestScene(
            GameObject localPlayerPrefab,
            GameObject botPrefab,
            OfflineMatchSettings offlineMatchSettings,
            RoundSettings roundSettings,
            EconomyTable economyTable,
            Material floorMaterial,
            Material wallMaterial,
            Material coverMaterial,
            Material bombSiteMaterial)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject environment = new("Environment");
            NavMeshSurface navMeshSurface = environment.AddComponent<NavMeshSurface>();
            navMeshSurface.collectObjects = CollectObjects.All;
            navMeshSurface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
            navMeshSurface.layerMask = ~0;

            GameObject floor = CreatePrimitive(
                environment.transform,
                "Floor",
                PrimitiveType.Cube,
                new Vector3(0f, -0.5f, 0f),
                new Vector3(80f, 1f, 80f),
                floorMaterial);
            ConfigureSurface(floor, SurfaceType.Concrete, 1.2f);

            CreatePrimitive(environment.transform, "NorthWall", PrimitiveType.Cube, new Vector3(0f, 3f, 40f), new Vector3(80f, 6f, 1f), wallMaterial);
            CreatePrimitive(environment.transform, "SouthWall", PrimitiveType.Cube, new Vector3(0f, 3f, -40f), new Vector3(80f, 6f, 1f), wallMaterial);
            CreatePrimitive(environment.transform, "EastWall", PrimitiveType.Cube, new Vector3(40f, 3f, 0f), new Vector3(1f, 6f, 80f), wallMaterial);
            CreatePrimitive(environment.transform, "WestWall", PrimitiveType.Cube, new Vector3(-40f, 3f, 0f), new Vector3(1f, 6f, 80f), wallMaterial);

            CreatePrimitive(environment.transform, "MidWallA", PrimitiveType.Cube, new Vector3(-8f, 1.5f, 0f), new Vector3(2f, 3f, 16f), wallMaterial);
            CreatePrimitive(environment.transform, "MidWallB", PrimitiveType.Cube, new Vector3(8f, 1.5f, 0f), new Vector3(2f, 3f, 16f), wallMaterial);
            CreatePrimitive(environment.transform, "SiteACoverA", PrimitiveType.Cube, new Vector3(16f, 1f, 16f), new Vector3(4f, 2f, 4f), coverMaterial);
            CreatePrimitive(environment.transform, "SiteACoverB", PrimitiveType.Cube, new Vector3(24f, 1f, 10f), new Vector3(6f, 2f, 3f), coverMaterial);
            CreatePrimitive(environment.transform, "SiteBCoverA", PrimitiveType.Cube, new Vector3(-16f, 1f, -16f), new Vector3(4f, 2f, 4f), coverMaterial);
            CreatePrimitive(environment.transform, "SiteBCoverB", PrimitiveType.Cube, new Vector3(-24f, 1f, -10f), new Vector3(6f, 2f, 3f), coverMaterial);
            CreatePrimitive(environment.transform, "CenterCrate", PrimitiveType.Cube, new Vector3(0f, 1f, 0f), new Vector3(3f, 2f, 3f), coverMaterial);

            GameObject systems = new("MatchSystems");
            RoundManager roundManager = systems.AddComponent<RoundManager>();
            BombObjective bombObjective = systems.AddComponent<BombObjective>();
            OfflineMatchBootstrapper bootstrapper = systems.AddComponent<OfflineMatchBootstrapper>();

            SerializedObject roundSerialized = new(roundManager);
            roundSerialized.FindProperty("settings").objectReferenceValue = roundSettings;
            roundSerialized.FindProperty("economyTable").objectReferenceValue = economyTable;
            roundSerialized.FindProperty("startOnAwake").boolValue = false;
            roundSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject bombSerialized = new(bombObjective);
            bombSerialized.FindProperty("roundManager").objectReferenceValue = roundManager;
            bombSerialized.FindProperty("armedDuration").floatValue = roundSettings != null ? roundSettings.BombTime : 35f;
            bombSerialized.ApplyModifiedPropertiesWithoutUndo();

            GameObject bombVisual = CreatePrimitive(systems.transform, "BombVisual", PrimitiveType.Cube, new Vector3(0f, 0.12f, 0f), new Vector3(0.3f, 0.24f, 0.2f), coverMaterial);
            RemoveCollider(bombVisual);

            GameObject actorRoot = new("Actors");

            TeamSpawnPoint[] spawnPoints = CreateSpawnPoints();
            GameObject bombSiteA = CreateBombSite("BombSite_A", new Vector3(24f, 0f, 20f), bombSiteMaterial);
            GameObject bombSiteB = CreateBombSite("BombSite_B", new Vector3(-24f, 0f, -20f), bombSiteMaterial);

            SerializedObject bootstrapSerialized = new(bootstrapper);
            bootstrapSerialized.FindProperty("settings").objectReferenceValue = offlineMatchSettings;
            bootstrapSerialized.FindProperty("roundManager").objectReferenceValue = roundManager;
            bootstrapSerialized.FindProperty("localPlayerPrefab").objectReferenceValue = localPlayerPrefab;
            bootstrapSerialized.FindProperty("botPrefab").objectReferenceValue = botPrefab;
            bootstrapSerialized.FindProperty("actorRoot").objectReferenceValue = actorRoot.transform;

            SerializedProperty spawnPointsProperty = bootstrapSerialized.FindProperty("spawnPoints");
            spawnPointsProperty.arraySize = spawnPoints.Length;
            for (int index = 0; index < spawnPoints.Length; index++)
            {
                spawnPointsProperty.GetArrayElementAtIndex(index).objectReferenceValue = spawnPoints[index];
            }

            bootstrapSerialized.ApplyModifiedPropertiesWithoutUndo();

            GameObject directionalLightObject = new("Directional Light");
            Light directionalLight = directionalLightObject.AddComponent<Light>();
            directionalLight.type = LightType.Directional;
            directionalLight.intensity = 1.1f;
            directionalLightObject.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            Lightmapping.lightingSettings = null;

            UnityEditor.AI.NavMeshBuilder.ClearAllNavMeshes();
            navMeshSurface.BuildNavMesh();

            string scenePath = SceneFolder + "/Offline32v32Test.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static TeamSpawnPoint[] CreateSpawnPoints()
        {
            TeamSpawnPoint[] spawnPoints = new TeamSpawnPoint[16];
            Vector3[] ctPositions =
            {
                new(-30f, 0f, 28f),
                new(-24f, 0f, 28f),
                new(-18f, 0f, 28f),
                new(-12f, 0f, 28f),
                new(-30f, 0f, 22f),
                new(-24f, 0f, 22f),
                new(-18f, 0f, 22f),
                new(-12f, 0f, 22f)
            };

            Vector3[] tPositions =
            {
                new(30f, 0f, -28f),
                new(24f, 0f, -28f),
                new(18f, 0f, -28f),
                new(12f, 0f, -28f),
                new(30f, 0f, -22f),
                new(24f, 0f, -22f),
                new(18f, 0f, -22f),
                new(12f, 0f, -22f)
            };

            for (int index = 0; index < ctPositions.Length; index++)
            {
                spawnPoints[index] = CreateSpawnPoint($"CT_Spawn_{index + 1:00}", TeamSide.CounterTerrorists, ctPositions[index], Quaternion.Euler(0f, 90f, 0f));
            }

            for (int index = 0; index < tPositions.Length; index++)
            {
                spawnPoints[ctPositions.Length + index] = CreateSpawnPoint($"T_Spawn_{index + 1:00}", TeamSide.Terrorists, tPositions[index], Quaternion.Euler(0f, -90f, 0f));
            }

            return spawnPoints;
        }

        private static TeamSpawnPoint CreateSpawnPoint(string name, TeamSide side, Vector3 position, Quaternion rotation)
        {
            GameObject root = new(name);
            root.transform.position = position;
            root.transform.rotation = rotation;
            TeamSpawnPoint spawnPoint = root.AddComponent<TeamSpawnPoint>();

            SerializedObject serialized = new(spawnPoint);
            serialized.FindProperty("side").enumValueIndex = (int)side;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return spawnPoint;
        }

        private static GameObject CreateBombSite(string name, Vector3 position, Material material)
        {
            GameObject root = new(name);
            root.transform.position = position;
            BombSite bombSite = root.AddComponent<BombSite>();

            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "Marker";
            marker.transform.SetParent(root.transform, false);
            marker.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            marker.transform.localScale = new Vector3(2.4f, 0.05f, 2.4f);
            AssignMaterial(marker, material);
            RemoveCollider(marker);
            return root;
        }

        private static void ConfigureSurface(GameObject target, SurfaceType surfaceType, float penetrationResistance)
        {
            SurfaceMaterial surface = target.GetComponent<SurfaceMaterial>() ?? target.AddComponent<SurfaceMaterial>();
            SerializedObject serialized = new(surface);
            serialized.FindProperty("surfaceType").enumValueIndex = (int)surfaceType;
            serialized.FindProperty("penetrationResistance").floatValue = penetrationResistance;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreatePrimitive(Transform parent, string name, PrimitiveType primitiveType, Vector3 position, Vector3 scale, Material material)
        {
            GameObject gameObject = GameObject.CreatePrimitive(primitiveType);
            gameObject.name = name;
            gameObject.transform.SetParent(parent, false);
            gameObject.transform.localPosition = position;
            gameObject.transform.localScale = scale;
            AssignMaterial(gameObject, material);
            return gameObject;
        }

        private static void AssignMaterial(GameObject target, Material material)
        {
            if (material != null && target.TryGetComponent(out Renderer renderer))
            {
                renderer.sharedMaterial = material;
            }
        }

        private static void RemoveCollider(GameObject target)
        {
            Collider collider = target.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }
        }

        private static GameObject CreateChild(GameObject parent, string name, Vector3 localPosition)
        {
            GameObject child = new(name);
            child.transform.SetParent(parent.transform, false);
            child.transform.localPosition = localPosition;
            return child;
        }

        private static T CreateAsset<T>(string path, Func<T> factory, Action<SerializedObject> configure) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = factory();
                AssetDatabase.CreateAsset(asset, path);
            }

            SerializedObject serialized = new(asset);
            configure(serialized);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            return asset;
        }
    }
}
