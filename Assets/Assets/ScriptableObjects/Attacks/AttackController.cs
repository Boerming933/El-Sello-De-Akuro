using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AttackController : MonoBehaviour
{
    [Header("Referencias")]
    public Turnero turnero;                 // tu gestor de turnos
    public AttackSelectionUI attackUI;      // donde generas botones
    public MouseControler mouseController;  // para apagar movimiento
    public OverlayTile overlayTile;

    [Header("Overlay Colors")]
    public Color rangeColor  = new Color(0f, 1f, 1f, 0.5f);
    public Color impactColor = new Color(1f, 0f, 0f, 0.5f);

    private bool inAttackMode = false;
    private Unit currentUnit;
    private AttackData currentAttack;
    private List<OverlayTile> validTiles = new();
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

    void EnterAttackMode(AttackData atk)
    {
        if (attackExecuted) return;
        attackExecuted = false;
        MapManager.Instance.HideAllTiles();
        validTiles.Clear();
        attackExecuted = false;
        // 1) Empieza un nuevo modo ataque
        currentAttack = atk;
        currentUnit   = GetActiveUnit();
        if (currentUnit == null) return;

        // 2) Bloquea movimiento
        if (mouseController != null)
            mouseController.enabled = false;

        // 3) Limpia overlays previos
        //overlayTile.HideTile2();
        //overlayTile.HideTile();

        // 4) Encuentra tile central según la posición world del personaje
        float threshold = 0.1f;
        OverlayTile centerTile = MapManager.Instance
            .map.Values
            .FirstOrDefault(t =>
                Vector2.Distance(
                    new Vector2(t.transform.position.x, t.transform.position.y),
                    new Vector2(currentUnit.transform.position.x, currentUnit.transform.position.y)
                ) < threshold
            );

        if (centerTile == null)
        {
            Debug.LogError($"No se encontró OverlayTile bajo {currentUnit.name}.");
            return;
        }

        // 5) Calcula y pinta rango de ataque
        validTiles = rangeFinder.GetTilesInRange(centerTile, currentAttack.radius);
        validTiles.ForEach(t => t.ShowOverlay(rangeColor));

        inAttackMode = true;
    }

    void Update()
    {
        if (!inAttackMode) return;

        // Detecta click sobre un overlay válido
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(
                new Vector2(wp.x, wp.y), Vector2.zero
            );
            if (hit.collider == null) return;

            var ot = hit.collider.GetComponent<OverlayTile>();
            if (ot != null && validTiles.Contains(ot))
                ConfirmAttack(ot);
        }
    }

    void ConfirmAttack(OverlayTile targetTile)
    {
        if (attackExecuted) return;
        attackExecuted = true;
        attackUI.SetButtonsInteractable(false);
        // 1) Deja de mostrar el rango
        validTiles.ForEach(t => t.HideTile());
        inAttackMode = false;

        // 2) Obtiene área de impacto
        var area = rangeFinder.GetTilesInRange(targetTile, currentAttack.radius);

        // 3) Inicia coroutine para pintar impacto, aplicar daño y limpiar
        StartCoroutine(ShowImpactAndFinish(area));
    }

    IEnumerator ShowImpactAndFinish(List<OverlayTile> area)
    {
        // Pinta zona de impacto
        area.ForEach(t => t.ShowOverlay(impactColor));

        // Aplica daño a cualquier Unit en esas casillas
        foreach (var tile in area)
        {
            Vector2 center = tile.transform.position;
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, 0.2f);

            foreach (var col in hits)
            {
                if (col.TryGetComponent<Unit>(out Unit enemy) && enemy.CompareTag("Enemy"))
                {
                    enemy.TakeDamage(currentAttack.damage);
                }
            }
        }

        // Espera 5 segundos con la zona marcada
        yield return new WaitForSeconds(5f);

        // Limpia overlays de impacto
        MapManager.Instance.HideAllTiles();

        // 4) Restaura movimiento y cierra turno
        if (mouseController != null)
            mouseController.enabled = true;

        // Deselecciona personaje para que no pueda moverse/atacar más
        mouseController.DeselectCharacter();
        attackUI.SetButtonsInteractable(true);
        attackExecuted = false;

        // Pasa el turno
        turnero.EndTurn();
    }

    // Helper para saber qué unidad (Unit) está activa según tu Turnero
    Unit GetActiveUnit()
    {
        if (turnero.vaporeon.vaporeonTurn)
            return turnero.vaporeon.GetComponent<Unit>();
        if (turnero.umbreon.umbreonTurn)
            return turnero.umbreon.GetComponent<Unit>();
        if (turnero.leafeon.leafeonTurn)
            return turnero.leafeon.GetComponent<Unit>();
        return null;
    }
}
