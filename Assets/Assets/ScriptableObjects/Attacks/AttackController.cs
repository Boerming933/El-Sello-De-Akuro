using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    [Header("Referencias")]
    public AttackSelectionUI attackUI;
    public MouseControler mouseController;

    [Header("Overlay Colors")]
    public Color rangeColor = new Color(1f, 1f, 0f, 0.5f);
    public Color previewColor = new Color(0f, 1f, 1f, 0.5f);
    public Color impactColor = new Color(1f, 0f, 0f, 0.5f);

    private bool inAttackMode = false;
    private Unit currentUnit;
    private AttackData currentAttack;
    private List<OverlayTile> validTiles = new();
    private List<OverlayTile> previewTiles = new();
    private OverlayTile lastHoverTile;
    private RangeFinder rangeFinder = new RangeFinder();
    public BattleSystem battleSystem;
    private bool attackExecuted;
    private List<BattleHUD> hudsToReset = new();

    // --- Nuevo: 4 "coordenadas"/facings que pediste ---
    public enum Facing4
    {
        ArribaIzq = 0, // "y"
        ArribaDer = 1, // "x"
        AbajoDer = 2, // "-y"
        AbajoIzq = 3  // "-x"
    }

    void OnEnable() => attackUI.OnAttackChosen += EnterAttackMode;
    void OnDisable() => attackUI.OnAttackChosen -= EnterAttackMode;

    public void SetCurrentUnit(Unit u) => currentUnit = u;

    void EnterAttackMode(AttackData atk)
    {
        if (attackExecuted) return;
        attackExecuted = false;
        MapManager.Instance.HideAllTiles();
        validTiles.Clear();
        ClearPreview();
        currentAttack = atk;
        if (currentUnit == null) return;
        if (mouseController != null) mouseController.enabled = false;

        float threshold = 0.1f;
        var centerTile = MapManager.Instance.map.Values
            .FirstOrDefault(t => Vector2.Distance((Vector2)t.transform.position, (Vector2)currentUnit.transform.position) < threshold);

        if (centerTile == null)
        {
            Debug.LogError($"No se encontró OverlayTile bajo {currentUnit.name}");
            return;
        }

        validTiles = rangeFinder.GetTilesInRange(centerTile, currentAttack.selectionRange);
        validTiles.ForEach(t => t.ShowOverlay(rangeColor));
        inAttackMode = true;
        lastHoverTile = null;
    }

    void Update()
    {
        if (!inAttackMode) return;
        UpdatePreviewUnderCursor();

        if (Input.GetMouseButtonDown(0))
        {
            var hovered = GetTileUnderCursor();
            if (hovered != null && validTiles.Contains(hovered))
                ConfirmAttack(hovered);
        }
        else if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Joystick1Button1))
        {
            ExitAttackMode();
        }
    }

    void UpdatePreviewUnderCursor()
    {
        var hovered = GetTileUnderCursor();
        if (hovered == null || !validTiles.Contains(hovered))
        {
            ClearPreview();
            lastHoverTile = null;
            return;
        }

        // Si cambió el tile bajo el cursor, actualizamos facing del personaje (según la tile bajo el pj)
        if (hovered != lastHoverTile)
        {
            // Determinar la tile bajo el jugador
            float originThreshold = 0.1f;
            var playerTile = MapManager.Instance.map.Values
                .FirstOrDefault(t => Vector2.Distance((Vector2)t.transform.position, (Vector2)currentUnit.transform.position) < originThreshold);

            if (playerTile != null)
            {
                // Calcular facing usando las 4 coordenadas que pediste
                var facing = DetermineFacingFromTiles(playerTile, hovered);
                ApplyFacingToUnit(currentUnit, facing);
            }

            ClearPreview();

            var area = GetEffectArea(hovered);
            previewTiles = area;
            previewTiles.ForEach(t => t.ShowOverlay(previewColor));
            lastHoverTile = hovered;

            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemyObj in enemies)
            {
                var unit = enemyObj.GetComponent<Unit>();
                if (unit == null) continue;

                bool isInArea = previewTiles.Any(tile =>
                    Vector2.Distance(tile.transform.position, enemyObj.transform.position) < 0.2f
                );

                var gabiteHUDObj = enemyObj.transform.Find("HUDSecundaria/GabiteHUD");
                if (gabiteHUDObj != null)
                {
                    var gabiteHUD = gabiteHUDObj.GetComponent<BattleHUD>();
                    if (gabiteHUD != null)
                    {
                        if (isInArea)
                        {
                            gabiteHUD.SetHUD(unit);
                            gabiteHUD.PreviewDamage(Mathf.RoundToInt(currentAttack.damage + currentUnit.Fue * currentAttack.scalingFactor));                               ///////
                            gabiteHUD.Show();
                            if (!hudsToReset.Contains(gabiteHUD)) hudsToReset.Add(gabiteHUD);
                        }
                        else
                        {
                            gabiteHUD.ResetPreview();
                            gabiteHUD.Hide();
                        }
                    }
                }

                var hitMarker = enemyObj.transform.Find("HitMarker");
                if (hitMarker != null)
                    hitMarker.gameObject.SetActive(isInArea);
            }
        }
    }

    OverlayTile GetTileUnderCursor()
    {
        Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var hit = Physics2D.Raycast((Vector2)wp, Vector2.zero);
        return hit.collider?.GetComponent<OverlayTile>();
    }

    void ClearPreview()
    {
        foreach (var t in previewTiles)
            t.HideTile();
        foreach (var t in validTiles)
            t.ShowOverlay(rangeColor);
        previewTiles.Clear();

        foreach (var hud in hudsToReset)
            hud.ResetPreview();
    }

    List<OverlayTile> GetEffectArea(OverlayTile center)
    {
        var mapTiles = MapManager.Instance.map.Values;
        var area = new List<OverlayTile>();
        Vector2 origin = center.transform.position;
        float threshold = 0.1f;
        int size = currentAttack.areaSize;

        switch (currentAttack.effectShape)
        {
            case AreaShape.Circle:
                area = rangeFinder.GetTilesInRange(center, size);
                break;
            case AreaShape.LineHorizontal:
                // 1) Encontramos la casilla donde está el jugador
                float originThreshold = 0.1f;
                var playerTileH = MapManager.Instance.map.Values.FirstOrDefault(t => Vector2.Distance((Vector2)t.transform.position, (Vector2)currentUnit.transform.position) < originThreshold);
                if (playerTileH != null)
                {
                    // 2) Determinamos el facing según la tile hover (que llegó como 'center')
                    var facingH = DetermineFacingFromTiles(playerTileH, center);
                    // 3) Generamos el área dinámica
                    area = GetDirectionalHorizontal(playerTileH, facingH, currentAttack.areaSize);
                }
                break;
            case AreaShape.LineVertical:
               float originThresholdV = 0.1f;
               var playerTileV = MapManager.Instance.map.Values
                   .FirstOrDefault(t => Vector2.Distance((Vector2)t.transform.position, (Vector2)currentUnit.transform.position) < originThresholdV);
               if (playerTileV != null)
               {
                   var facingV = DetermineFacingFromTiles(playerTileV, center);
                   area = GetDirectionalVertical(playerTileV, facingV, currentAttack.areaSize);
               }
                break;
            case AreaShape.Cross:
                area.Add(center);
                Vector2 pureX = new Vector2(1f, 0f);
                Vector2 pureY = new Vector2(0f, 0.5f);
                for (int d = 1; d <= size; d++)
                {
                    var positions = new[]
                    {
                        origin - pureX * d,
                        origin + pureX * d,
                        origin + pureY * d,
                        origin - pureY * d
                    };
                    foreach (var pos in positions)
                    {
                        var tile = mapTiles.FirstOrDefault(t => Vector2.Distance(t.transform.position, pos) < threshold);
                        if (tile != null && !area.Contains(tile))
                            area.Add(tile);
                    }
                }
                break;
            case AreaShape.Boomerang:
                float OriginThreshold = 0.1f;
                var playerTile = MapManager.Instance.map.Values
                    .FirstOrDefault(t => Vector2.Distance((Vector2)t.transform.position, (Vector2)currentUnit.transform.position) < OriginThreshold);
                area = GetBoomerangArea(playerTile, center);
                break;

        }
        return area;
    }

    void ConfirmAttack(OverlayTile targetTile)
    {
        mouseController.canPocion = false;
        if (attackExecuted) return;
        attackExecuted = true;
        attackUI.SetButtonsInteractable(false);

        validTiles.ForEach(t => t.HideTile());
        ClearPreview();
        inAttackMode = false;

        var panels = Object.FindObjectsByType<PanelAcciones>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
        foreach (var p in panels) p.Hide();

        var area = GetEffectArea(targetTile);

        hudsToReset.Clear();

        foreach (var tile in area)
        {
            Vector2 center = tile.transform.position;
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, 0.2f);
            foreach (var col in hits)
            {
                if (col.CompareTag("Enemy"))
                {
                    var u = col.GetComponent<Unit>();
                    var gabiteHUDObj = col.transform.Find("HUDSecundaria/GabiteHUD");
                    if (gabiteHUDObj != null)
                    {
                        var gabiteHUD = gabiteHUDObj.GetComponent<BattleHUD>();
                        if (gabiteHUD != null && !hudsToReset.Contains(gabiteHUD))
                        {

                            gabiteHUD.SetHUD(u);
                            gabiteHUD.ApplyDamage(Mathf.RoundToInt(currentAttack.damage + currentUnit.Fue * currentAttack.scalingFactor));                                 /////
                            gabiteHUD.Show();
                            hudsToReset.Add(gabiteHUD);
                        }
                    }
                }
            }
        }

        foreach (var tile in area)
        {
            Vector2 center = tile.transform.position;
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, 0.2f);
            foreach (var col in hits)
            {
                if (col.TryGetComponent<Unit>(out Unit u) && u.CompareTag("Enemy"))
                    u.TakeDamage(Mathf.RoundToInt(currentAttack.damage + currentUnit.Fue * currentAttack.scalingFactor));                                                    ///
            }
        }

        StartCoroutine(ShowImpactAndFinish(area));
    }

    IEnumerator ShowImpactAndFinish(List<OverlayTile> area)
    {
        area.ForEach(t => t.ShowOverlay(impactColor));

        if (currentAttack.initiativeBonus != 0)
        {
            var im = FindAnyObjectByType<InitiativeManager>();
            im.ApplyInitiativeBuff(
                currentUnit,
                currentAttack.initiativeBonus,
                currentAttack.initiativeDuration
            );
        }

        yield return new WaitForSeconds(3f);
        MapManager.Instance.HideAllTiles();

        if (mouseController != null) mouseController.enabled = true;

        foreach (var enemyObj in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            var hitMarker = enemyObj.transform.Find("HitMarker");
            if (hitMarker != null)
                hitMarker.gameObject.SetActive(false);
        }

        var ci = currentUnit.GetComponent<CharacterInfo>();
        bool hasMovesLeft = ci.tilesMoved < ci.maxTiles;

        if (hasMovesLeft)
        {
            mouseController.canMove = true;
            mouseController.canAttack = false;
            mouseController.showPanelAcciones = true;

            var panel = Object.FindObjectsByType<PanelAcciones>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            ).FirstOrDefault(p => p.ownerCharacter == ci);

            if (panel != null)
            {
                panel.Show();
                panel.panelBatalla.SetActive(false);
            }

            attackUI.SetButtonsInteractable(true);
            attackExecuted = false;

            foreach (var hud in hudsToReset)
                hud?.Hide();
            hudsToReset.Clear();

            yield break;
        }

        mouseController.DeselectCharacter();
        attackUI.SetButtonsInteractable(true);
        attackExecuted = false;

        foreach (var hud in hudsToReset)
            hud?.Hide();
        hudsToReset.Clear();
    }

    public void StartAttack(AttackData atk)
    {
        if (attackUI != null)
            attackUI.gameObject.SetActive(false);
        EnterAttackMode(atk);
    }

    public void ExitAttackMode()
    {
        if (!inAttackMode) return;

        inAttackMode = false;
        currentAttack = null;

        validTiles.ForEach(t => t.HideTile());
        previewTiles.ForEach(t => t.HideTile());
        previewTiles.Clear();
        validTiles.Clear();

        if (mouseController != null)
        {
            mouseController.enabled = true;
            mouseController.canAttack = true;
            mouseController.showPanelAcciones = true;
        }

        var panels = Object.FindObjectsByType<PanelAcciones>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
        foreach (var p in panels)
        {
            p.Show();
            p.panelBatalla.SetActive(true);

            var turnable = p.ownerCharacter.GetComponent<Turnable>();
            if (turnable != null && turnable.btnBatalla != null)
                turnable.btnBatalla.interactable = true;
        }

        var ataques = Object.FindObjectsByType<AttackButtonProxy>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
        foreach (var a in ataques)
        {
            if (a.panelBatallaGeneral != null)
                a.panelBatallaGeneral.SetActive(true);
        }

        attackUI.SetButtonsInteractable(true);

        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            var gabiteHUDObj = enemy.transform.Find("HUDSecundaria/GabiteHUD");
            if (gabiteHUDObj != null)
            {
                var gabiteHUD = gabiteHUDObj.GetComponent<BattleHUD>();
                if (gabiteHUD != null)
                    gabiteHUD.Hide();
            }

            var hitMarker = enemy.transform.Find("HitMarker");
            if (hitMarker != null)
                hitMarker.gameObject.SetActive(false);
        }
    }

    // --- GetBoomerangArea ahora usa DetermineFacingFromTiles con las 4 coordenadas pedidas ---
    List<OverlayTile> GetBoomerangArea(OverlayTile center, OverlayTile target)
    {
        var result = new List<OverlayTile>();
        if (center == null) return result;
        if (MapManager.Instance == null || MapManager.Instance.map == null) return result;

        // Offsets base para la dirección RIGHT (referencia)
        var baseOffsets = new List<Vector2Int>
        {
            new Vector2Int(1,0),
            new Vector2Int(2,0),
            new Vector2Int(2,1),
            new Vector2Int(3,1),
            new Vector2Int(4,1),
            new Vector2Int(4,0),
            new Vector2Int(4,-1),
            new Vector2Int(3,-1),
            new Vector2Int(2,-1)
        };

        // Determinar facing usando la función común
        var facing = DetermineFacingFromTiles(center, target);

        List<Vector2Int> chosenOffsets;

        switch (facing)
        {
            case Facing4.ArribaDer: // "x"  -> RIGHT
                chosenOffsets = baseOffsets;
                break;
            case Facing4.AbajoIzq: // "-x" -> LEFT
                chosenOffsets = baseOffsets.Select(o => new Vector2Int(-o.x, -o.y)).ToList();
                break;
            case Facing4.ArribaIzq: // "y" -> UP
                chosenOffsets = baseOffsets.Select(o => new Vector2Int(-o.y, o.x)).ToList();
                break;
            case Facing4.AbajoDer: // "-y" -> DOWN
                chosenOffsets = baseOffsets.Select(o => new Vector2Int(o.y, -o.x)).ToList();
                break;
            default:
                chosenOffsets = baseOffsets;
                break;
        }

        // Convertir offsets a tiles consultando el map por grid2DLocation
        Vector2Int c = center.grid2DLocation;
        foreach (var off in chosenOffsets)
        {
            var key = c + off;
            if (MapManager.Instance.map.TryGetValue(key, out var tile) && tile != null)
                result.Add(tile);
        }

        return result;
    }

    // --- NUEVO: determina facing usando las 4 coordenadas que pediste ---
    Facing4 DetermineFacingFromTiles(OverlayTile playerTile, OverlayTile targetTile)
    {
        if (playerTile == null) return Facing4.ArribaDer; // default

        Vector2Int c = playerTile.grid2DLocation;
        Vector2Int t = (targetTile != null) ? targetTile.grid2DLocation : c + Vector2Int.right;
        Vector2Int delta = new Vector2Int(t.x - c.x, t.y - c.y);

        if (delta == Vector2Int.zero) delta = Vector2Int.right;

        // Según tu especificación:
        // ArribaIzq = y (up)
        // ArribaDer = x (right)
        // AbajoDer  = -y (down)
        // AbajoIzq  = -x (left)

        if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
        {
            // Dominante horizontal -> "x" o "-x"
            return delta.x > 0 ? Facing4.ArribaDer : Facing4.AbajoIzq;
        }
        else
        {
            // Dominante vertical -> "y" o "-y"
            return delta.y > 0 ? Facing4.ArribaIzq : Facing4.AbajoDer;
        }
    }

    // --- NUEVO: aplica el facing al personaje (intento seguro: Animator int + Sprite flipX fallback)
    void ApplyFacingToUnit(Unit unit, Facing4 facing)
    {
        if (unit == null) return;

        // 1) Animator: buscamos un parámetro int "FacingIndex" (si existe lo seteamos)
        var anim = unit.GetComponentInChildren<Animator>();
        if (anim != null)
        {
            // intentamos setear, si no existe el parámetro no pasa nada visible
            try { anim.SetInteger("FacingIndex", (int)facing); }
            catch { /* harmless if param missing */ }
        }

        // 2) SpriteRenderer fallback: flip horizontal para diferenciar las dos mitades
        var sr = unit.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            // definimos que ArribaDer (x) y AbajoDer ( -y ) usen flipX = false
            // y ArribaIzq / AbajoIzq usen flipX = true -- ajustalo segun tus sprites
            if (facing == Facing4.ArribaDer || facing == Facing4.AbajoDer)
                sr.flipX = true;
            else
                sr.flipX = false;
        }

        // 3) Si tu clase Unit tiene una propiedad para el facing, podríamos setearla (si existe).
        //    Para no depender de la implementación exacta dejamos esto opcional.
    }

    /// <summary>
    /// Convierte un Facing4 a un vector de desplazamiento en grid2D.
    /// </summary>
    Vector2Int FacingToVector(Facing4 f)
    {
        switch (f)
        {
            case Facing4.ArribaIzq: return new Vector2Int(0, +1);
            case Facing4.ArribaDer: return new Vector2Int(+1, 0);
            case Facing4.AbajoDer:  return new Vector2Int(0, -1);
            case Facing4.AbajoIzq:  return new Vector2Int(-1, 0);
            default:                return new Vector2Int(+1, 0);
        }
    }

    /// <summary>
    /// Eje perpendicular al facing (rotación -90°): perp = (facing.y, -facing.x)
    /// </summary>
    Vector2Int Perpendicular(Vector2Int facing)
    {
        return new Vector2Int(facing.y, -facing.x);
    }

    /// <summary>
    /// Área de un ataque vertical *direccional*: desde el jugador hacia adelante (1..size).
    /// </summary>
    List<OverlayTile> GetDirectionalVertical(OverlayTile playerTile, Facing4 facing, int size)
    {
        var grid = MapManager.Instance.map;
        var origin = playerTile.grid2DLocation;
        var dir = FacingToVector(facing);

        var area = new List<OverlayTile>();
        for (int i = 1; i <= size; i++)
        {
            var key = origin + dir * i;
            if (grid.TryGetValue(key, out var t) && t != null)
                area.Add(t);
        }
        return area;
    }

    /// <summary>
    /// Área de un ataque horizontal *direccional*: una barra perpendicular
    /// al facing, desplazada 1 casilla adelante.
    /// </summary>
    List<OverlayTile> GetDirectionalHorizontal(OverlayTile playerTile, Facing4 facing, int size)
    {
        var grid = MapManager.Instance.map;
        var origin = playerTile.grid2DLocation;
        var dir = FacingToVector(facing);
        var perp = Perpendicular(dir);

        var area = new List<OverlayTile>();
        // desplazamiento 1 adelante + barra de -size..+size
        for (int i = -size; i <= size; i++)
        {
            var key = origin + dir + perp * i;
            if (grid.TryGetValue(key, out var t) && t != null)
                area.Add(t);
        }
        return area;
    }


}


