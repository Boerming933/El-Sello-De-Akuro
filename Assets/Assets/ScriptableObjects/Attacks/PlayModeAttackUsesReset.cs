using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#if UNITY_EDITOR
[InitializeOnLoad]
public static class PlayModeAttackUsesReset
{
    // Store original uses values to restore them when exiting play mode
    private static Dictionary<string, int> originalUsesValues = new Dictionary<string, int>();
    
    static PlayModeAttackUsesReset()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }
    
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        switch (state)
        {
            case PlayModeStateChange.EnteredPlayMode:
                ResetAllAttackUses();
                break;
            case PlayModeStateChange.ExitingPlayMode:
                RestoreOriginalUses();
                break;
            case PlayModeStateChange.EnteredEditMode:
                // Additional safety - restore when entering edit mode
                RestoreOriginalUses();
                break;
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
                    Debug.Log($"[PlayMode] Resetting {attackData.attackName} uses: {attackData.currentUses} → 0");
                    attackData.ResetUses();
                    EditorUtility.SetDirty(attackData);
                }
            }
        }
        
        if (originalUsesValues.Count > 0)
        {
            AssetDatabase.SaveAssets();
        }
    }
    
    private static void RestoreOriginalUses()
    {
        foreach (var kvp in originalUsesValues)
        {
            string assetPath = kvp.Key;
            int originalUses = kvp.Value;
            
            BuffDebuffAttackData attackData = AssetDatabase.LoadAssetAtPath<BuffDebuffAttackData>(assetPath);
            if (attackData != null && attackData.currentUses != originalUses)
            {
                Debug.Log($"[PlayMode] Restoring {attackData.attackName} uses: {attackData.currentUses} → {originalUses}");
                attackData.currentUses = originalUses;
                EditorUtility.SetDirty(attackData);
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