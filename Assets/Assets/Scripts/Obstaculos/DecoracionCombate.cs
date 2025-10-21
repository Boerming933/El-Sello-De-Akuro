using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class DecoracionCombate : MonoBehaviour
{
    public OverlayTile Active;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => MapManager.Instance != null
                                   && MapManager.Instance.map != null);

        var activeTile = ActiveTile();
        Active = activeTile.Value.collider.GetComponent<OverlayTile>();
        Active.isBlocked = true;
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
