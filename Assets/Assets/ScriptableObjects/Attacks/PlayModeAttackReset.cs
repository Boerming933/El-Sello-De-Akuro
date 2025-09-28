using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#if UNITY_EDITOR
[InitializeOnLoad]
public static class PlayModeAttackReset
{
    // Store the original uses values to restore on exit
    private static Dictionary<string, int> originalUsesValues = new Dictionary<string, int>();
    
    static PlayModeAttackReset()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }
    
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            ResetAllAttackUses();
        }
        else if (state == PlayModeStateChange.ExitingPlayMode)
        {
            RestoreOriginalUses();
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            // Additional safety - restore on entering edit mode
            RestoreOriginalUses();
        }
    }
    
    private static void ResetAllAttackUses()
    {
        originalUsesValues.Clear();
        
        // Find all BuffDebuffAttackData assets in the project
        string[] guids = AssetDatabase.FindAssets("t:BuffDebuffAttackData");
        
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            BuffDebuffAttackData attackData = AssetDatabase.LoadAssetAtPath<BuffDebuffAttackData>(assetPath);
            
            if (attackData != null && attackData.maxUses > 0)
            {
                // Store original value before resetting
                originalUsesValues[assetPath] = attackData.currentUses;
                
                if (attackData.currentUses > 0)
                {
                    Debug.Log($"Resetting uses for attack: {attackData.attackName} (was {attackData.currentUses}, now 0)");
                    attackData.ResetUses();
                }
            }
        }
        
        // Save the changes to disk
        AssetDatabase.SaveAssets();
    }
    
    private static void RestoreOriginalUses()
    {
        foreach (var kvp in originalUsesValues)
        {
            string assetPath = kvp.Key;
            int originalUses = kvp.Value;
            
            BuffDebuffAttackData attackData = AssetDatabase.LoadAssetAtPath<BuffDebuffAttackData>(assetPath);
            if (attackData != null)
            {
                if (attackData.currentUses != originalUses)
                {
                    Debug.Log($"Restoring original uses for {attackData.attackName}: {attackData.currentUses} → {originalUses}");
                    attackData.currentUses = originalUses;
                }
            }
        }
        
        if (originalUsesValues.Count > 0)
        {
            AssetDatabase.SaveAssets();
            originalUsesValues.Clear();
        }
    }
}
#endif