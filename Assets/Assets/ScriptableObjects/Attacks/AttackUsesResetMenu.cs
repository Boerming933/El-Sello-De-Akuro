using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public static class AttackUsesResetMenu
{
    [MenuItem("Tools/Combat/Reset All Attack Uses")]
    public static void ResetAllAttackUses()
    {
        string[] guids = AssetDatabase.FindAssets("t:BuffDebuffAttackData");
        int resetCount = 0;
        
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            BuffDebuffAttackData attackData = AssetDatabase.LoadAssetAtPath<BuffDebuffAttackData>(assetPath);
            
            if (attackData != null && attackData.maxUses > 0 && attackData.currentUses > 0)
            {
                Debug.Log($"Manually resetting {attackData.attackName}: {attackData.currentUses} → 0");
                attackData.ResetUses();
                EditorUtility.SetDirty(attackData);
                resetCount++;
            }
        }
        
        if (resetCount > 0)
        {
            AssetDatabase.SaveAssets();
            Debug.Log($"✅ Manually reset {resetCount} attack(s) uses to 0");
        }
        else
        {
            Debug.Log("✅ All attacks already have 0 uses - no reset needed");
        }
    }
    
    [MenuItem("Tools/Combat/Show Attack Uses Status")]
    public static void ShowAttackUsesStatus()
    {
        string[] guids = AssetDatabase.FindAssets("t:BuffDebuffAttackData");
        int limitedUseAttacks = 0;
        
        Debug.Log("=== ATTACK USES STATUS ===");
        
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            BuffDebuffAttackData attackData = AssetDatabase.LoadAssetAtPath<BuffDebuffAttackData>(assetPath);
            
            if (attackData != null && attackData.maxUses > 0)
            {
                limitedUseAttacks++;
                string status = attackData.currentUses >= attackData.maxUses ? " [DEPLETED]" : "";
                Debug.Log($"• {attackData.attackName}: {attackData.currentUses}/{attackData.maxUses}{status}");
            }
        }
        
        if (limitedUseAttacks == 0)
        {
            Debug.Log("No limited-use attacks found in project");
        }
        else
        {
            Debug.Log($"=== Found {limitedUseAttacks} limited-use attack(s) ===");
        }
    }
}
#endif