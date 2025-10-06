using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class DecoracionCombate : MonoBehaviour
{
    public OverlayTile Active;
    
    public enum SortGroup { Background = 0, Dynamic = 1, Foreground = 2 }
    
    public SortGroup sortGroup = SortGroup.Dynamic;
    
    private SpriteRenderer _renderer;
    
    public bool blocked = true;

    public SpriteRenderer Renderer => _renderer;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => MapManager.Instance != null
                                   && MapManager.Instance.map != null);

        var activeTile = ActiveTile();
        Active = activeTile.Value.collider.GetComponent<OverlayTile>();
        if (blocked)
        {
            Active.isBlocked = true;
        }
        _renderer = GetComponent<SpriteRenderer>();
    }
    
    public RaycastHit2D? ActiveTile()
    {
        Vector2 origin = new Vector2(transform.position.x, transform.position.y);

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.zero);

        if (hits.Length > 0)
        {
            return hits.OrderByDescending(i => i.collider.transform.position.z).First();
        }
        return null;
    }
}