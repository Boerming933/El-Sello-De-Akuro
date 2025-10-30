using System.Collections.Generic; //
using System.Linq; //
using UnityEngine; //

[CreateAssetMenu(fileName = "MovimientoRelampagoAttack", menuName = "Attacks/Movimiento Relampago")] //
public class MovimientoRelampagoAttackData : BuffDebuffAttackData //
{ //
    [Header("Movimiento Relámpago Settings")] //
    public int extraMovementTiles = 4; //
    public int baseDamage = 10; //
    public float damageScalingPerHit = 0.10f; //
    
    private class ActivationState //
    { //
        public Unit unit; //
        public CharacterInfo character; //
        public int originalMoveCount; //
        public int originalMaxTiles; //
        public HashSet<int> hitEnemiesThisActivation = new HashSet<int>(); //
        public int hitsSoFar = 0; //
        public bool isActive = false; //
    } //
    
    private Dictionary<Unit, ActivationState> activeStates = new Dictionary<Unit, ActivationState>(); //
    
    public void ActivateSkill(Unit caster) //
    { //
        if (caster == null) //
        { //
            Debug.LogWarning("[Movimiento Relámpago] Cannot activate: caster is null"); //
            return; //
        } //
        
        var character = caster.GetComponent<CharacterInfo>(); //
        if (character == null) //
        { //
            Debug.LogWarning("[Movimiento Relámpago] Cannot activate: CharacterInfo not found"); //
            return; //
        } //
        
        if (activeStates.ContainsKey(caster) && activeStates[caster].isActive) //
        { //
            Debug.LogWarning($"[Movimiento Relámpago] Skill already active for {caster.Name}. Ignoring duplicate activation."); //
            return; //
        } //
        
        var state = new ActivationState //
        { //
            unit = caster, //
            character = character, //
            originalMoveCount = character.tilesMoved, //
            originalMaxTiles = character.maxTiles, //
            isActive = true //
        }; //
        
        character.maxTiles += extraMovementTiles; //
        caster.MovimientoRelampagoCaminata = true; //
        
        activeStates[caster] = state; //
        
        // ANIM_HOOK: MovementLightning_Start //
        Debug.Log($"[Movimiento Relámpago] Activated for {caster.Name}! Original maxTiles: {state.originalMaxTiles}, New maxTiles: {character.maxTiles}"); //
        
        bool hadNoMovement = state.originalMoveCount >= state.originalMaxTiles; //
        if (hadNoMovement) //
        { //
            var turnable = character.GetComponent<Turnable>(); //
            if (turnable != null && turnable.btnMoverse != null) //
            { //
                turnable.btnMoverse.interactable = true; //
                Debug.Log("[Movimiento Relámpago] Re-enabled movement button (movement was exhausted)"); //
            } //
        } //
    } //
    
    public void CheckAdjacentEnemiesOnMove(Unit caster, Vector2Int currentGridPos) //
    { //
        if (!activeStates.ContainsKey(caster) || !activeStates[caster].isActive) //
            return; //
        
        var state = activeStates[caster]; //
        string casterName = caster.Name; //
        Vector2 casterWorldPos = caster.transform.position; //
        
        Debug.LogError($"[MR] Generando área para {attackName} desde origen {currentGridPos} (world: {casterWorldPos})"); //
        
        Vector2Int[] adjacentOffsets = new Vector2Int[] //
        { //
            new Vector2Int(0, 1),   // N //
            new Vector2Int(0, -1),  // S //
            new Vector2Int(1, 0),   // E //
            new Vector2Int(-1, 0),  // W //
            new Vector2Int(1, 1),   // NE //
            new Vector2Int(-1, 1),  // NW //
            new Vector2Int(1, -1),  // SE //
            new Vector2Int(-1, -1)  // SW //
        }; //
        
        List<string> areaTiles = new List<string>(); //
        foreach (var offset in adjacentOffsets) //
        { //
            Vector2Int neighborPos = currentGridPos + offset; //
            areaTiles.Add($"{neighborPos.x},{neighborPos.y}"); //
        } //
        Debug.LogError($"[MR] Tiles generados: {string.Join("; ", areaTiles)}"); //
        
        foreach (var offset in adjacentOffsets) //
        { //
            Vector2Int neighborPos = currentGridPos + offset; //
            
            bool isInArea = MapManager.Instance.map.TryGetValue(neighborPos, out OverlayTile neighborTile); //
            
            if (isInArea) //
            { //
                Vector2 neighborWorldPos = neighborTile.transform.position; //
                Collider2D[] hits = Physics2D.OverlapCircleAll(neighborWorldPos, 0.1f); //
                
                string entityNames = hits.Length > 0 ? string.Join(", ", hits.Select(h => h.name)) : "null"; //
                Debug.LogError($"[MR] Verificando vecino {neighborPos} (world: {neighborWorldPos}) alrededor de {casterName} en {currentGridPos} | EstáEnÁrea: {isInArea} | Entities: {entityNames}"); //
                
                foreach (var hit in hits) //
                { //
                    if (hit.isTrigger) continue; //
                    if (hit.CompareTag("Enemy")) //
                    { //
                        Unit enemyUnit = hit.GetComponent<Unit>(); //
                        if (enemyUnit != null && enemyUnit != caster) //
                        { //
                            int enemyID = enemyUnit.GetInstanceID(); //
                            string targetName = enemyUnit.Name; //
                            Vector2 targetPos = enemyUnit.transform.position; //
                            bool isAlly = false; //
                            bool isDead = enemyUnit.currentHP <= 0; //
                            bool alreadyHit = state.hitEnemiesThisActivation.Contains(enemyID); //
                            
                            if (alreadyHit) //
                            { //
                                Debug.LogError($"[MR] Saltando objetivo ID={enemyID} Nombre={targetName} Razón=alreadyHit"); //
                                continue; //
                            } //
                            
                            if (isDead) //
                            { //
                                Debug.LogError($"[MR] Saltando objetivo ID={enemyID} Nombre={targetName} Razón=dead"); //
                                continue; //
                            } //
                            
                            int computedDamage = Mathf.RoundToInt(baseDamage * (1 + damageScalingPerHit * state.hitsSoFar)); //
                            
                            Debug.LogError($"[MR] Intentando aplicar daño a ID={enemyID} Nombre={targetName} Pos={targetPos} DamageCalculado={computedDamage} hitsSoFar={state.hitsSoFar}"); //
                            
                            enemyUnit.TakeDamage(computedDamage, caster); //
                            
                            int targetHpAfter = enemyUnit.currentHP; //
                            Debug.LogError($"[MR] Resultado daño para ID={enemyID}: aplicado=true HP_After={targetHpAfter}"); //
                            
                            state.hitEnemiesThisActivation.Add(enemyID); //
                            state.hitsSoFar++; //
                        } //
                    } //
                } //
            } //
            else //
            { //
                Debug.LogError($"[MR] Verificando vecino {neighborPos} alrededor de {casterName} en {currentGridPos} | EstáEnÁrea: false | Entity: null"); //
            } //
        } //
    } //
    
    public void OnTurnEnd(Unit caster) //
    { //
        if (!activeStates.ContainsKey(caster) || !activeStates[caster].isActive) //
            return; //
        
        var state = activeStates[caster]; //
        
        state.character.maxTiles = state.originalMaxTiles; //
        state.character.tilesMoved = Mathf.Min(state.character.tilesMoved, state.originalMaxTiles); //
        caster.MovimientoRelampagoCaminata = false; //
        
        // ANIM_HOOK: MovementLightning_End //
        Debug.Log($"[Movimiento Relámpago] Deactivated for {caster.Name}. Total enemies hit: {state.hitsSoFar}. Movement restored to: {state.originalMaxTiles}"); //
        
        state.hitEnemiesThisActivation.Clear(); //
        state.hitsSoFar = 0; //
        state.isActive = false; //
        
        activeStates.Remove(caster); //
        
        var turnable = state.character.GetComponent<Turnable>(); //
        if (turnable != null && turnable.btnMoverse != null) //
        { //
            turnable.btnMoverse.interactable = (state.character.tilesMoved < state.character.maxTiles); //
        } //
    } //
    
    public bool IsActive(Unit caster) //
    { //
        return activeStates.ContainsKey(caster) && activeStates[caster].isActive; //
    } //
} //
