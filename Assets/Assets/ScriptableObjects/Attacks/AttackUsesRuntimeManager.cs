using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Runtime failsafe component that ensures attack uses are properly reset
/// even if the editor script misses any cases.
/// </summary>
public class AttackUsesRuntimeManager : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    // Store original values for restoration
    private Dictionary<BuffDebuffAttackData, int> originalUsesBackup = new Dictionary<BuffDebuffAttackData, int>();
    
    private void Awake()
    {
        // Find all limited-use attacks and store their original values
        var allAttacks = Resources.FindObjectsOfTypeAll<BuffDebuffAttackData>();
        
        foreach (var attack in allAttacks)
        {
            if (attack.maxUses > 0)
            {
                // Store current value as the "original" for this play session
                originalUsesBackup[attack] = attack.currentUses;
                
                // Reset to 0 for clean gameplay
                if (attack.currentUses > 0)
                {
                    if (showDebugLogs)
                        Debug.Log($"[Runtime] Resetting {attack.attackName} uses: {attack.currentUses} → 0");
                    
                    attack.ResetUses();
                }
            }
        }
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        // Handle mobile/editor pause events
        if (pauseStatus)
        {
            RestoreOriginalUses();
        }
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        // Handle window focus changes (useful for editor)
        if (!hasFocus)
        {
            RestoreOriginalUses();
        }
    }
    
    private void OnDestroy()
    {
        RestoreOriginalUses();
    }
    
    private void RestoreOriginalUses()
    {
        foreach (var kvp in originalUsesBackup)
        {
            BuffDebuffAttackData attack = kvp.Key;
            int originalValue = kvp.Value;
            
            if (attack != null && attack.currentUses != originalValue)
            {
                if (showDebugLogs)
                    Debug.Log($"[Runtime] Restoring {attack.attackName} uses: {attack.currentUses} → {originalValue}");
                
                attack.currentUses = originalValue;
            }
        }
    }
    
    /// <summary>
    /// Manual function to reset all attack uses - useful for testing
    /// </summary>
    [ContextMenu("Manual Reset All Attack Uses")]
    public void ManualResetAllUses()
    {
        var allAttacks = Resources.FindObjectsOfTypeAll<BuffDebuffAttackData>();
        
        foreach (var attack in allAttacks)
        {
            if (attack.maxUses > 0 && attack.currentUses > 0)
            {
                Debug.Log($"[Manual] Resetting {attack.attackName}: {attack.currentUses} → 0");
                attack.ResetUses();
            }
        }
    }
}