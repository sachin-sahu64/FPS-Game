using System.Collections.Generic;
using FPSGame.AI;
using FPSGame.Rounds;
using UnityEngine;
using UnityEngine.AI;

namespace FPSGame.Spawning
{
    [DefaultExecutionOrder(-500)]
    public class OfflineMatchBootstrapper : MonoBehaviour
    {
        [SerializeField] private OfflineMatchSettings settings;
        [SerializeField] private RoundManager roundManager;
        [SerializeField] private GameObject localPlayerPrefab;
        [SerializeField] private GameObject botPrefab;
        [SerializeField] private Transform actorRoot;
        [SerializeField] private TeamSpawnPoint[] spawnPoints;

        private readonly List<SpawnedActor> spawnedActors = new();
        private bool matchSpawned;

        private void Awake()
        {
            if (roundManager == null)
            {
                roundManager = FindFirstObjectByType<RoundManager>();
            }

            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                spawnPoints = FindObjectsByType<TeamSpawnPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            }

            SpawnMatchIfNeeded();
        }

        private void OnEnable()
        {
            if (roundManager != null)
            {
                roundManager.StateChanged += HandleRoundStateChanged;
            }
        }

        private void OnDisable()
        {
            if (roundManager != null)
            {
                roundManager.StateChanged -= HandleRoundStateChanged;
            }
        }

        private void Start()
        {
            roundManager?.StartMatch();
        }

        private void HandleRoundStateChanged(RoundState state, float timeRemaining)
        {
            if (state == RoundState.FreezeTime)
            {
                RespawnAllActors();
            }
        }

        private void SpawnMatchIfNeeded()
        {
            if (matchSpawned || settings == null)
            {
                return;
            }

            int playersPerSide = Mathf.Clamp(settings.PlayersPerSide, 1, 32);
            int terroristBots = playersPerSide;
            int counterTerroristBots = playersPerSide;

            if (settings.IncludeLocalPlayer)
            {
                if (localPlayerPrefab == null)
                {
                    Debug.LogWarning("OfflineMatchBootstrapper: Local player prefab is missing.");
                }
                else
                {
                    TeamSide playerSide = settings.LocalPlayerSide;
                    SpawnedActor playerActor = SpawnActor(localPlayerPrefab, playerSide, false, 0);
                    spawnedActors.Add(playerActor);

                    if (playerSide == TeamSide.Terrorists)
                    {
                        terroristBots = Mathf.Max(0, terroristBots - 1);
                    }
                    else
                    {
                        counterTerroristBots = Mathf.Max(0, counterTerroristBots - 1);
                    }
                }
            }

            for (int index = 0; index < terroristBots; index++)
            {
                if (botPrefab == null)
                {
                    break;
                }

                spawnedActors.Add(SpawnActor(botPrefab, TeamSide.Terrorists, true, settings.IncludeLocalPlayer && settings.LocalPlayerSide == TeamSide.Terrorists ? index + 1 : index));
            }

            for (int index = 0; index < counterTerroristBots; index++)
            {
                if (botPrefab == null)
                {
                    break;
                }

                spawnedActors.Add(SpawnActor(botPrefab, TeamSide.CounterTerrorists, true, settings.IncludeLocalPlayer && settings.LocalPlayerSide == TeamSide.CounterTerrorists ? index + 1 : index));
            }

            matchSpawned = true;
        }

        private SpawnedActor SpawnActor(GameObject prefab, TeamSide side, bool isBot, int slotIndex)
        {
            if (prefab == null)
            {
                return default;
            }

            SpawnPose pose = GetSpawnPose(side, slotIndex);
            GameObject instance = Instantiate(prefab, pose.Position, pose.Rotation, actorRoot);
            instance.name = isBot ? $"{side}_Bot_{slotIndex + 1:00}" : $"{side}_Player";

            TeamMember member = instance.GetComponent<TeamMember>();
            if (member != null)
            {
                member.Configure(side, isBot);
                roundManager?.RegisterTeamMember(member);
            }

            RoundActorLifecycle lifecycle = instance.GetComponent<RoundActorLifecycle>() ?? instance.AddComponent<RoundActorLifecycle>();
            BotController botController = instance.GetComponent<BotController>();

            if (botController != null)
            {
                botController.SetHomeAnchor(pose.Position);
            }

            return new SpawnedActor(instance, lifecycle, botController, side, slotIndex);
        }

        private void RespawnAllActors()
        {
            for (int index = 0; index < spawnedActors.Count; index++)
            {
                SpawnedActor actor = spawnedActors[index];

                if (!actor.IsValid)
                {
                    continue;
                }

                SpawnPose pose = GetSpawnPose(actor.Side, actor.SideSlotIndex);
                actor.Lifecycle.Respawn(pose.Position, pose.Rotation);
                actor.BotController?.SetHomeAnchor(pose.Position);
            }
        }

        private SpawnPose GetSpawnPose(TeamSide side, int slotIndex)
        {
            List<TeamSpawnPoint> sideSpawnPoints = new();

            foreach (TeamSpawnPoint spawnPoint in spawnPoints)
            {
                if (spawnPoint != null && spawnPoint.Side == side)
                {
                    sideSpawnPoints.Add(spawnPoint);
                }
            }

            if (sideSpawnPoints.Count == 0)
            {
                return new SpawnPose(transform.position, transform.rotation);
            }

            TeamSpawnPoint baseSpawn = sideSpawnPoints[slotIndex % sideSpawnPoints.Count];
            int clusterIndex = slotIndex / sideSpawnPoints.Count;
            float radius = settings != null ? settings.SpawnScatterRadius * clusterIndex : clusterIndex;
            float angle = slotIndex * 137.5f;
            Vector3 radialOffset = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * radius;
            Vector3 position = baseSpawn.transform.position + radialOffset;

            if (NavMesh.SamplePosition(position, out NavMeshHit hit, Mathf.Max(2f, radius + 2f), NavMesh.AllAreas))
            {
                position = hit.position;
            }

            return new SpawnPose(position, baseSpawn.transform.rotation);
        }

        private readonly struct SpawnPose
        {
            public SpawnPose(Vector3 position, Quaternion rotation)
            {
                Position = position;
                Rotation = rotation;
            }

            public Vector3 Position { get; }

            public Quaternion Rotation { get; }
        }

        private readonly struct SpawnedActor
        {
            public SpawnedActor(GameObject instance, RoundActorLifecycle lifecycle, BotController botController, TeamSide side, int sideSlotIndex)
            {
                Instance = instance;
                Lifecycle = lifecycle;
                BotController = botController;
                Side = side;
                SideSlotIndex = sideSlotIndex;
            }

            public GameObject Instance { get; }

            public RoundActorLifecycle Lifecycle { get; }

            public BotController BotController { get; }

            public TeamSide Side { get; }

            public int SideSlotIndex { get; }

            public bool IsValid => Instance != null && Lifecycle != null;
        }
    }
}
