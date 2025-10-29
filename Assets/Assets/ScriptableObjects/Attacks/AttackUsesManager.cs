using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Runtime manager to handle attack uses and ensure they reset properly between play sessions.
/// This provides a fallback solution in case the editor script doesn't catch all cases.
/// </summary>
public class AttackUsesManager : MonoBehaviour
{
    [Header("Debug Info")]
    [SerializeField] private bool showDebugLogs = true;
    
    private Dictionary<BuffDebuffAttackData, int> originalUses = new Dictionary<BuffDebuffAttackData, int>();
    
    private void Awake()
    {
        // Find all BuffDebuffAttackData assets and store their current uses
        var allAttacks = Resources.FindObjectsOfTypeAll<BuffDebuffAttackData>();
        
        foreach (var attack in allAttacks)
        {
            if (attack.maxUses > 0)
            {
                originalUses[attack] = attack.currentUses;
                
                // Reset uses at start of play session
                if (attack.currentUses > 0)
                {
                    if (showDebugLogs)
                        Debug.Log($"[AttackUsesManager] Resetting {attack.attackName} uses: {attack.currentUses} → 0");
                    
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
        // Handle window focus changes
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
        foreach (var kvp in originalUses)
        {
            BuffDebuffAttackData attack = kvp.Key;
            int originalValue = kvp.Value;
            
            if (attack != null && attack.currentUses != originalValue)
            {
                if (showDebugLogs)
                    Debug.Log($"[AttackUsesManager] Restoring {attack.attackName} uses: {attack.currentUses} → {originalValue}");
                
                attack.currentUses = originalValue;
            }
        }
    }
    
    /// <summary>
    /// Manual reset function for testing purposes
    /// </summary>
    [ContextMenu("Reset All Attack Uses")]
    public void ManualResetAllUses()
    {
        var allAttacks = Resources.FindObjectsOfTypeAll<BuffDebuffAttackData>();
        
        foreach (var attack in allAttacks)
        {
            if (attack.maxUses > 0 && attack.currentUses > 0)
            {
                Debug.Log($"[Manual Reset] {attack.attackName}: {attack.currentUses} → 0");
                attack.ResetUses();
            }
        }
    }
}