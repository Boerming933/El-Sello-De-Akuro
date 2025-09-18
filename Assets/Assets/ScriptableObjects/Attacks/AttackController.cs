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
        else if (Input.GetKeyDown(KeyCode.Escape))
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

        if (hovered == lastHoverTile) return;
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
                        gabiteHUD.PreviewDamage(Mathf.RoundToInt(currentAttack.damage * currentAttack.scalingFactor));                               ///////
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
                Vector2 isoH = new Vector2(0.5f, 0.25f);
                for (int i = -size; i <= size; i++)
                {
                    Vector2 samplePos = origin + isoH * i;
                    var tile = mapTiles.FirstOrDefault(t => Vector2.Distance(t.transform.position, samplePos) < threshold);
                    if (tile != null) area.Add(tile);
                }
                break;
            case AreaShape.LineVertical:
                Vector2 isoV = new Vector2(0.5f, -0.25f);
                for (int i = -size; i <= size; i++)
                {
                    Vector2 samplePos = origin + isoV * i;
                    var tile = mapTiles.FirstOrDefault(t => Vector2.Distance(t.transform.position, samplePos) < threshold);
                    if (tile != null) area.Add(tile);
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
        }
        return area;
    }

    void ConfirmAttack(OverlayTile targetTile)
    {
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
                            gabiteHUD.ApplyDamage(Mathf.RoundToInt(currentAttack.damage * currentAttack.scalingFactor));                                 //////
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
                    u.TakeDamage(Mathf.RoundToInt(currentAttack.damage * currentAttack.scalingFactor));                                                    /////
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
}