using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class DecorationManager : MonoBehaviour
{
    public List<SpriteRenderer> characterRenderers;

    private List<Decoraciones> _decorations;

    void Start()
    {
        _decorations = GetComponentsInChildren<Decoraciones>().ToList();
    }

    void LateUpdate()
    {
        foreach (var dec in _decorations
                 .Where(d => d.sortGroup == Decoraciones.SortGroup.Background))
        {
            dec.Renderer.sortingOrder = 0;
        }

        var dynamicItems = new List<(SpriteRenderer sr, float key)>();

        dynamicItems.AddRange(
            characterRenderers
                .Select(sr => (sr, sr.transform.position.y))
        );

        dynamicItems.AddRange(
            _decorations
                .Where(d => d.sortGroup == Decoraciones.SortGroup.Dynamic)
                .Select(d => (d.Renderer, d.EffectiveY))
        );

        var sorted = dynamicItems
            .OrderByDescending(item => item.key)
            .ToList();

        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].sr.sortingOrder = i + 1;
        }

        int frontBase = sorted.Count + 1;

        foreach (var dec in _decorations
                 .Where(d => d.sortGroup == Decoraciones.SortGroup.Foreground))
        {
            dec.Renderer.sortingOrder = frontBase;
            frontBase++;
        }
    }
}
