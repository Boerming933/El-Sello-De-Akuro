using UnityEngine;

public class AttackBools : MonoBehaviour
{
    [Header("Samurai Attack States")]
    [SerializeField] public bool samuraiAttack1 = false;
    [SerializeField] public bool samuraiAttack2 = false;
    [SerializeField] public bool samuraiAttack3 = false;
    [SerializeField] public bool samuraiAttack4 = false;
    [SerializeField] public bool samuraiAttack5 = false;
    
    [Header("Geisha Attack States")]
    [SerializeField] public bool geishaAttack1 = false;
    [SerializeField] public bool geishaAttack2 = false;
    [SerializeField] public bool geishaAttack3 = false;
    [SerializeField] public bool geishaAttack4 = false;
    [SerializeField] public bool geishaAttack5 = false;
    
    [Header("Ninja Attack States")]
    [SerializeField] public bool ninjaAttack1 = false;
    [SerializeField] public bool ninjaAttack2 = false;
    [SerializeField] public bool ninjaAttack3 = false;
    [SerializeField] public bool ninjaAttack4 = false;
    [SerializeField] public bool ninjaAttack5 = false;
    
    [Header("Direction Detection")]
    [SerializeField] public bool isLookingUp = false;
    
    // ========== SAMURAI ATTACK METHODS (Connect to OnClick()) ==========
    
    public void ActivateSamuraiAttack1()
    {
        samuraiAttack1 = true;
    }
    
    public void ActivateSamuraiAttack2()
    {
        samuraiAttack2 = true;
    }
    
    public void ActivateSamuraiAttack3()
    {
        samuraiAttack3 = true;
    }
    
    public void ActivateSamuraiAttack4()
    {
        samuraiAttack4 = true;
    }
    
    public void ActivateSamuraiAttack5()
    {
        samuraiAttack5 = true;
    }
    
    // ========== GEISHA ATTACK METHODS (Connect to OnClick()) ==========
    
    public void ActivateGeishaAttack1()
    {
        geishaAttack1 = true;
    }
    
    public void ActivateGeishaAttack2()
    {
        geishaAttack2 = true;
    }
    
    public void ActivateGeishaAttack3()
    {
        geishaAttack3 = true;
    }
    
    public void ActivateGeishaAttack4()
    {
        geishaAttack4 = true;
    }
    
    public void ActivateGeishaAttack5()
    {
        geishaAttack5 = true;
    }
    
    // ========== NINJA ATTACK METHODS (Connect to OnClick()) ==========
    
    public void ActivateNinjaAttack1()
    {
        ninjaAttack1 = true;
    }
    
    public void ActivateNinjaAttack2()
    {
        ninjaAttack2 = true;
    }
    
    public void ActivateNinjaAttack3()
    {
        ninjaAttack3 = true;
    }
    
    public void ActivateNinjaAttack4()
    {
        ninjaAttack4 = true;
    }
    
    public void ActivateNinjaAttack5()
    {
        ninjaAttack5 = true;
    }
    
    // ========== RESET METHODS ==========
    
    // Individual reset methods
    public void ResetSamuraiAttack1() => samuraiAttack1 = false;
    public void ResetSamuraiAttack2() => samuraiAttack2 = false;
    public void ResetSamuraiAttack3() => samuraiAttack3 = false;
    public void ResetSamuraiAttack4() => samuraiAttack4 = false;
    public void ResetSamuraiAttack5() => samuraiAttack5 = false;
    
    public void ResetGeishaAttack1() => geishaAttack1 = false;
    public void ResetGeishaAttack2() => geishaAttack2 = false;
    public void ResetGeishaAttack3() => geishaAttack3 = false;
    public void ResetGeishaAttack4() => geishaAttack4 = false;
    public void ResetGeishaAttack5() => geishaAttack5 = false;
    
    public void ResetNinjaAttack1() => ninjaAttack1 = false;
    public void ResetNinjaAttack2() => ninjaAttack2 = false;
    public void ResetNinjaAttack3() => ninjaAttack3 = false;
    public void ResetNinjaAttack4() => ninjaAttack4 = false;
    public void ResetNinjaAttack5() => ninjaAttack5 = false;
    
    // Group reset methods
    public void ResetAllSamuraiAttacks()
    {
        samuraiAttack1 = samuraiAttack2 = samuraiAttack3 = samuraiAttack4 = samuraiAttack5 = false;
    }
    
    public void ResetAllGeishaAttacks()
    {
        geishaAttack1 = geishaAttack2 = geishaAttack3 = geishaAttack4 = geishaAttack5 = false;
    }
    
    public void ResetAllNinjaAttacks()
    {
        ninjaAttack1 = ninjaAttack2 = ninjaAttack3 = ninjaAttack4 = ninjaAttack5 = false;
    }
    
    public void ResetAllAttacks()
    {
        ResetAllSamuraiAttacks();
        ResetAllGeishaAttacks();
        ResetAllNinjaAttacks();
        ResetDirectionStates();
    }
    
    // ========== DIRECTION DETECTION METHODS ==========
    
    /// <summary>
    /// Checks the direction from character to selected tile and updates both
    /// horizontal (isFacingRight) and vertical (isLookingUp) direction booleans.
    /// A tile is considered "above" if (deltaX + deltaY) > 0, "below" if <= 0.
    /// </summary>
    public void CheckTileDirection(OverlayTile characterTile, OverlayTile selectedTile)
    {
        if (characterTile == null || selectedTile == null)
        {
            isLookingUp = false;
            return;
        }
        
        Vector2Int characterPos = characterTile.grid2DLocation;
        Vector2Int selectedPos = selectedTile.grid2DLocation;
        
        // Calculate relative position
        int deltaX = selectedPos.x - characterPos.x;
        int deltaY = selectedPos.y - characterPos.y;
                
        // Vertical direction (up/down) using diagonal sum calculation
        // Above if sum of relative coordinates > 0, below if <= 0
        isLookingUp = (deltaX + deltaY) > 0;
    }
    
    public void ResetDirectionStates()
    {
        isLookingUp = false;
    }
    
}
