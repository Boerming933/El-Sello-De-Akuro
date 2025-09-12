using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    [Header("Referencias")]
    public AttackSelectionUI attackUI;
    public MouseControler    mouseController;

    [Header("Overlay Colors")]
    public Color rangeColor    = new Color(1f, 1f, 0f, 0.5f);
    public Color previewColor  = new Color(0f, 1f, 1f, 0.5f);
    public Color impactColor   = new Color(1f, 0f, 0f, 0.5f);

    // Estado interno
    private bool inAttackMode      = false;
    private Unit currentUnit;
    private AttackData currentAttack;
    private List<OverlayTile> validTiles   = new();
    private List<OverlayTile> previewTiles = new();
    private OverlayTile lastHoverTile;

    private RangeFinder rangeFinder = new RangeFinder();
    private bool attackExecuted;

    void OnEnable()
    {
        attackUI.OnAttackChosen += EnterAttackMode;
    }

    void OnDisable()
    {
        attackUI.OnAttackChosen -= EnterAttackMode;
    }

    /// <summary>
    /// Llamar desde BattleSystem para definir la unidad activa
    /// </summary>
    public void SetCurrentUnit(Unit u)
    {
        currentUnit = u;
    }

    /// <summary>
    /// Inicia el modo ataque: pinta el rango de selección
    /// </summary>
    void EnterAttackMode(AttackData atk)
    {
        if (attackExecuted) return;
        attackExecuted = false;

        MapManager.Instance.HideAllTiles();
        validTiles.Clear();
        ClearPreview();

        currentAttack = atk;
        if (currentUnit == null) return;

        if (mouseController != null)
            mouseController.enabled = false;

        // Encuentra el tile bajo el personaje
        float threshold = 0.1f;
        var centerTile = MapManager.Instance.map.Values
            .FirstOrDefault(t =>
                Vector2.Distance(
                    (Vector2)t.transform.position,
                    (Vector2)currentUnit.transform.position
                ) < threshold
            );

        if (centerTile == null)
        {
            Debug.LogError($"No se encontró OverlayTile bajo {currentUnit.name}");
            return;
        }

        // 1) Calcula y pinta el rango de selección (círculo)
        validTiles = rangeFinder.GetTilesInRange(centerTile, currentAttack.selectionRange);
        validTiles.ForEach(t => t.ShowOverlay(rangeColor));

        inAttackMode      = true;
        lastHoverTile     = null;
    }

    void Update()
    {
        if (!inAttackMode) return;

        // 2) Preview dinámico bajo cursor
        UpdatePreviewUnderCursor();

        // 3) Confirmar ataque
        if (Input.GetMouseButtonDown(0))
        {
            var hovered = GetTileUnderCursor();
            if (hovered != null && validTiles.Contains(hovered))
                ConfirmAttack(hovered);
        }
    }

    /// <summary>
    /// Pinta/despinta preview según mouse sobre validTiles
    /// </summary>
    void UpdatePreviewUnderCursor()
    {
        var hovered = GetTileUnderCursor();

        if (hovered == null || !validTiles.Contains(hovered))
        {
            ClearPreview();
            lastHoverTile = null;
            return;
        }

        if (hovered == lastHoverTile) return;

        ClearPreview();
        var area = GetEffectArea(hovered);
        previewTiles = area;
        previewTiles.ForEach(t => t.ShowOverlay(previewColor));
        lastHoverTile = hovered;
    }

    /// <summary>
    /// Raycast sencillo para obtener el OverlayTile bajo el mouse
    /// </summary>
    OverlayTile GetTileUnderCursor()
    {
        Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var hit = Physics2D.Raycast((Vector2)wp, Vector2.zero);
        return hit.collider?.GetComponent<OverlayTile>();
    }

    /// <summary>
    /// Limpia cualquier overlay de preview
    /// </summary>
    void ClearPreview()
    {
        // 1) Quita la capa de preview de cada tile
        foreach (var t in previewTiles)
            t.HideTile();

        // 2) Vuelve a pintarles el overlay de rango
        foreach (var t in validTiles)
            t.ShowOverlay(rangeColor);

        // 3) Limpia la lista de preview
        previewTiles.Clear();
    }


    /// <summary>
    /// Devuelve la lista de tiles del área de efecto según forma y tamaño
    /// </summary>
    List<OverlayTile> GetEffectArea(OverlayTile center)
    {
        var mapTiles  = MapManager.Instance.map.Values;
        var area      = new List<OverlayTile>();
        Vector2 origin    = center.transform.position;
        float  threshold = 0.1f;
        int    size      = currentAttack.areaSize;

        switch (currentAttack.effectShape)
        {
            case AreaShape.Circle:
                // Igual que antes: círculo de radios “size”
                area = rangeFinder.GetTilesInRange(center, size);
                break;

            case AreaShape.LineHorizontal:
                // En isométrico la “horizontal” va NE–SW:
                // offset por 1 tile = (0.5, 0.25)
                Vector2 isoH = new Vector2(0.5f, 0.25f);
                for (int i = -size; i <= size; i++)
                {
                    Vector2 samplePos = origin + isoH * i;
                    var tile = mapTiles
                        .FirstOrDefault(t => Vector2.Distance(t.transform.position, samplePos) < threshold);
                    if (tile != null) area.Add(tile);
                }
                break;

            case AreaShape.LineVertical:
                // En isométrico la “vertical” va SE–NW:
                // offset por 1 tile = (0.5, -0.25)
                Vector2 isoV = new Vector2(0.5f, -0.25f);
                for (int i = -size; i <= size; i++)
                {
                    Vector2 samplePos = origin + isoV * i;
                    var tile = mapTiles
                        .FirstOrDefault(t => Vector2.Distance(t.transform.position, samplePos) < threshold);
                    if (tile != null) area.Add(tile);
                }
                break;

            case AreaShape.Cross:
                // Solo ejes puros de mundo: X=(1,0), Y=(0,0.5)
                area.Add(center);
                Vector2 pureX = new Vector2(1f, 0f);
                Vector2 pureY = new Vector2(0f, 0.5f);
                for (int d = 1; d <= size; d++)
                {
                    var leftPos  = origin - pureX * d;
                    var rightPos = origin + pureX * d;
                    var upPos    = origin + pureY * d;
                    var downPos  = origin - pureY * d;

                    var candidates = new[] { leftPos, rightPos, upPos, downPos };
                    foreach (var pos in candidates)
                    {
                        var tile = mapTiles
                            .FirstOrDefault(t => Vector2.Distance(t.transform.position, pos) < threshold);
                        if (tile != null && !area.Contains(tile))
                            area.Add(tile);
                    }
                }
                break;
        }

        return area;
    }


    /// <summary>
    /// Al confirmar un tile seleccionado, ejecuta el área de impacto
    /// </summary>
    void ConfirmAttack(OverlayTile targetTile)
    {
        if (attackExecuted) return;
        attackExecuted = true;

        attackUI.SetButtonsInteractable(false);

        // Limpia rango y preview
        validTiles .ForEach(t => t.HideTile());
        ClearPreview();
        inAttackMode = false;

        // Oculta todos los PanelAcciones
        var panels = Object.FindObjectsByType<PanelAcciones>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
        foreach (var p in panels)
            p.Hide();

        // Calcula área de impacto y comienza la coroutine
        var area = GetEffectArea(targetTile);
        StartCoroutine(ShowImpactAndFinish(area));
    }

    IEnumerator ShowImpactAndFinish(List<OverlayTile> area)
    {
        // 1) Pinta zona de impacto
        area.ForEach(t => t.ShowOverlay(impactColor));

        // 2) Aplica daño a cualquier Unit en esas casillas
        try
        {
            foreach (var tile in area)
            {
                Vector2 center = tile.transform.position;
                Collider2D[] hits = Physics2D.OverlapCircleAll(center, 0.2f);
                foreach (var col in hits)
                {
                    if (col.TryGetComponent<Unit>(out Unit u) && u.CompareTag("Enemy"))
                        u.TakeDamage(currentAttack.damage);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error al aplicar daño: {ex.Message}");
        }

        // 3) Espera con la zona marcada
        yield return new WaitForSeconds(3f);

        // 4) Limpia TODOS los overlays (rango, preview e impacto)
        MapManager.Instance.HideAllTiles();

        // 5) Restaura input del MouseController
        if (mouseController != null)
            mouseController.enabled = true;

        // 6) Comprueba si aún le quedan movimientos al personaje
        var ci = currentUnit.GetComponent<CharacterInfo>();
        bool hasMovesLeft = ci.tilesMoved < ci.maxTiles;

        if (hasMovesLeft)
        {
            // A) Reactiva movimiento y HUD de acciones
            mouseController.canMove = true;
            mouseController.canAttack = false;
            mouseController.showPanelAcciones = true;

            // B) Fuerza la reapertura de su PanelAcciones
            var panel = Object.FindObjectsByType<PanelAcciones>(
                                FindObjectsInactive.Include,
                                FindObjectsSortMode.None
                            )
                            .FirstOrDefault(p => p.ownerCharacter == ci);
            if (panel != null)
            {
                panel.Show();
                panel.panelBatalla.SetActive(false); // cierra sub-panel de ataques
            }

            // C) Rehabilita los botones de ataque para futuros ataques
            attackUI.SetButtonsInteractable(true);
            attackExecuted = false;

            // Salimos sin terminar el turno
            yield break;
        }

        // 7) Si no quedan movimientos, termina el turno
        mouseController.DeselectCharacter();
        attackUI.SetButtonsInteractable(true);
        attackExecuted = false;
    }


    
    /// <summary>
    /// Llamar desde un botón de HUD para iniciar un ataque con este SO.
    /// </summary>
    public void StartAttack(AttackData atk)
    {
        // Esto oculta el panel de selección de ataques si quieres
        if (attackUI != null)
            attackUI.gameObject.SetActive(false);

        EnterAttackMode(atk);
    }

}
