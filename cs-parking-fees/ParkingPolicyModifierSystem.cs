using System;
using Unity.Entities;
using Unity.Collections;
using Game.Buildings;
using Game.Prefabs;
using Game.Policies;
using Game.UI;
using Game.Common;

namespace ParkingFeeControl
{
    /// <summary>
    /// System to modify parking policies on buildings (parking lots, facilities).
    /// Policies control the actual parking fees shown in UI.
    /// Uses periodic update to ensure configured fees are applied.
    /// </summary>
    public partial class ParkingPolicyModifierSystem : SystemBase
    {
        private EntityQuery m_BuildingQuery;
        private float m_TimeSinceLastUpdate = 0f;
        private bool m_SystemInitialized = false;
        private PrefabSystem m_PrefabSystem;
        private NameSystem m_NameSystem;
        
        // Cache of parking fee policy entity (obtained from prefab)
        private Entity m_ParkingFeePolicyEntity = Entity.Null;
        
        // Known policy prefab name for parking fees
        private const string PARKING_FEE_POLICY_NAME = "Lot Parking Fee";

        protected override void OnCreate()
        {
            base.OnCreate();
            
            // Query for buildings with parking facilities
            m_BuildingQuery = GetEntityQuery(
                ComponentType.ReadWrite<Building>(),
                ComponentType.ReadOnly<PrefabRef>()
            );
            
            // Get PrefabSystem reference to access prefab names
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_NameSystem = World.GetOrCreateSystemManaged<NameSystem>();
            
            ModLogger.Info("ParkingPolicyModifierSystem created");
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            if (!m_SystemInitialized)
            {
                ModLogger.Info("ParkingPolicyModifierSystem started running (periodic update mode)");
                
                // Find the parking fee policy prefab entity directly
                m_ParkingFeePolicyEntity = GetParkingFeePolicyFromPrefab();
                
                if (m_ParkingFeePolicyEntity != Entity.Null)
                {
                    ModLogger.Info($"✓ Found parking fee policy prefab entity: {m_ParkingFeePolicyEntity}");
                }
                else
                {
                    ModLogger.Warn("⚠ Could not find parking fee policy prefab - will retry on next update");
                }
                
                m_SystemInitialized = true;
            }
        }

        /// <summary>
        /// Get the parking fee policy entity directly from the PrefabSystem.
        /// Uses PolicySliderPrefab type with the known policy name.
        /// </summary>
        private Entity GetParkingFeePolicyFromPrefab()
        {
            try
            {
                var prefabId = new PrefabID("PolicySliderPrefab", PARKING_FEE_POLICY_NAME);
                
                // if (Mod.Config.DebugLogging)
                // {
                //     Mod.Log?.Info($"TryGetPrefab: Type='PolicySliderPrefab', Name='{PARKING_FEE_POLICY_NAME}'");
                // }
                
                if (m_PrefabSystem.TryGetPrefab(prefabId, out var prefabBase))
                {
                    if (m_PrefabSystem.TryGetEntity(prefabBase, out Entity policyEntity))
                    {
                        // if (Mod.Config.DebugLogging)
                        // {
                        //     Mod.Log?.Info($"✓ Found parking fee policy: Entity {policyEntity}");
                        // }
                        return policyEntity;
                    }
                }
                
                ModLogger.Warn($"Could not find parking fee policy prefab '{PARKING_FEE_POLICY_NAME}'");
            }
            catch (Exception ex)
            {
                ModLogger.Error($"Error finding parking fee policy prefab: {ex.Message}");
            }
            
            return Entity.Null;
        }

        // Removed deprecated fallback method GetParkingFeePolicyFallback - kept out to reduce unused code.

        protected override void OnUpdate()
        {
            if (!Mod.Settings.Enabled)
                return;

            m_TimeSinceLastUpdate += World.Time.DeltaTime;
            
            int freqSeconds = Mod.Settings.GetUpdateFrequencySeconds();
            if (m_TimeSinceLastUpdate < freqSeconds)
                return;

            m_TimeSinceLastUpdate = 0f;

            // Retry finding policy if not found yet
            if (m_ParkingFeePolicyEntity == Entity.Null)
            {
                m_ParkingFeePolicyEntity = GetParkingFeePolicyFromPrefab();
            }

            ModifyBuildingPolicies();
        }

        public void ApplyNow(bool resetTimer = true)
        {
            if (!Mod.Settings.Enabled)
                return;

            if (resetTimer)
            {
                m_TimeSinceLastUpdate = 0f;
            }

            if (m_ParkingFeePolicyEntity == Entity.Null)
            {
                m_ParkingFeePolicyEntity = GetParkingFeePolicyFromPrefab();
            }

            ModifyBuildingPolicies();
        }

        /// <summary>
        /// Modify parking policies on all parking buildings.
        /// Applies configured fees periodically.
        /// </summary>
        private void ModifyBuildingPolicies()
        {
            var buildings = m_BuildingQuery.ToEntityArray(Allocator.Temp);
            int parkingBuildingsFound = 0;
            int modifiedPolicies = 0;
            int buildingsWithPolicies = 0;
            
            try
            {
                foreach (var buildingEntity in buildings)
                {
                    // Check if building has ParkingFacility component
                    if (!EntityManager.HasComponent<Game.Buildings.ParkingFacility>(buildingEntity))
                        continue;
                    
                    parkingBuildingsFound++;
                    
                    // Get the building's prefab
                    if (!EntityManager.HasComponent<PrefabRef>(buildingEntity))
                        continue;
                        
                    var prefabRef = EntityManager.GetComponentData<PrefabRef>(buildingEntity);
                    var prefabData = EntityManager.GetComponentData<PrefabData>(prefabRef.m_Prefab);
                    string prefabName = "Unknown";
                    
                    try
                    {
                        var prefabBase = m_PrefabSystem.GetPrefab<PrefabBase>(prefabData);
                        if (prefabBase != null)
                        {
                            prefabName = prefabBase.name ?? "Unnamed";
                        }
                    }
                    catch
                    {
                        prefabName = $"Prefab#{prefabData.m_Index}";
                    }
                    
                    // Determine fee based on prefab name matching
                    int targetFee = Mod.Config.GetParkingFeeForPrefab(prefabName);
                    bool disablePolicy = targetFee <= 0;
                    int effectiveFee = disablePolicy ? targetFee : Math.Min(50, Math.Max(1, targetFee));

                    // Check custom name for ignore tag
                    string customName = null;
                    try
                    {
                        if (m_NameSystem != null && m_NameSystem.TryGetCustomName(buildingEntity, out var cn))
                        {
                            customName = cn;
                        }
                    }
                    catch
                    {
                        customName = null;
                    }

                    if (!string.IsNullOrEmpty(customName) && Mod.Settings.ShouldIgnoreByName(customName))
                    {
                        if (parkingBuildingsFound <= 5)
                        {
                            ModLogger.Debug($"  Skipping building #{buildingEntity.Index} (custom name contains ignore tag): '{customName}'");
                        }
                        continue;
                    }
                    
                    if (parkingBuildingsFound <= 5)
                    {
                        ModLogger.Debug($"  Building #{buildingEntity.Index} - Prefab: '{prefabName}' -> Fee: ${effectiveFee}");
                    }
                    
                    // Check if building has Policy buffer
                    if (!EntityManager.HasBuffer<Policy>(buildingEntity))
                    {
                        if (parkingBuildingsFound <= 3)
                        {
                            ModLogger.Debug("    Building has NO Policy buffer");
                        }
                        continue;
                    }
                    
                    buildingsWithPolicies++;
                    
                    // Get the policy buffer
                    var policyBuffer = EntityManager.GetBuffer<Policy>(buildingEntity);
                    
                    if (buildingsWithPolicies <= 3)
                    {
                        ModLogger.Debug($"    Policy buffer length: {policyBuffer.Length}");
                    }
                    
                    // If buffer is empty, add policy using cached entity
                    if (policyBuffer.Length == 0)
                    {
                        if (m_ParkingFeePolicyEntity != Entity.Null)
                        {
                            PolicyFlags flags = disablePolicy ? 0 : PolicyFlags.Active;
                            policyBuffer.Add(new Policy(m_ParkingFeePolicyEntity, flags, effectiveFee));
                            modifiedPolicies++;
                            
                            if (buildingsWithPolicies <= 3)
                            {
                                ModLogger.Debug($"    Added policy with fee: ${effectiveFee}");
                            }
                        }
                        continue;
                    }
                    
                    // Iterate through policies and modify parking fee policies
                    for (int i = 0; i < policyBuffer.Length; i++)
                    {
                        var policy = policyBuffer[i];
                        bool isActive = (policy.m_Flags & PolicyFlags.Active) != 0;
                        // If configured <=0, disable policy if active
                        if (disablePolicy)
                        {
                            if (isActive)
                            {
                                policy.m_Flags &= ~PolicyFlags.Active;
                                policyBuffer[i] = policy;
                                modifiedPolicies++;
                                if (buildingsWithPolicies <= 3)
                                {
                                    ModLogger.Debug("    Disabling policy (fee <= 0)");
                                }
                            }
                            continue;
                        }

                        // If configured >0, possibly enable and apply fee.
                        // New rule: automatically enable policy only if effectiveFee > 1.
                        bool enableNeeded = effectiveFee > 1;
                        bool wasInactive = !isActive;
                        bool modified = false;

                        if (enableNeeded && !isActive)
                        {
                            policy.m_Flags |= PolicyFlags.Active;
                            isActive = true;
                            modified = true;
                        }

                        // Apply fee only if policy is active (or we just enabled it)
                        if (isActive)
                        {
                            if (policy.m_Adjustment != effectiveFee)
                            {
                                policy.m_Adjustment = effectiveFee;
                                modified = true;
                            }
                            
                            if (modified)
                            {
                                policyBuffer[i] = policy;
                                modifiedPolicies++;

                                if (buildingsWithPolicies <= 3)
                                {
                                    ModLogger.Debug($"    {(wasInactive ? "Enabled and applied" : "Applied")} fee: ${effectiveFee}");
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                buildings.Dispose();
            }

            ModLogger.Debug($"Found {parkingBuildingsFound} parking buildings, {buildingsWithPolicies} with policies, modified {modifiedPolicies} policies");
        }
    }
}
