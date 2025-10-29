using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class ObstacleManager : MonoBehaviour
{
    public Transform depthContainer;
    public BattleSystem battleSystem;
    public List<GameObject> Characters = new List<GameObject>();
    public List<SpriteRenderer> SRCha = new List<SpriteRenderer>();
    public SpriteRenderer sr;

    public List<DecoracionCombate> _decorations;
    private Vector3[] lastPositions;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => MapManager.Instance != null
                                   && MapManager.Instance.map != null);

        // Clear lists to avoid duplicates
        Characters.Clear();
        SRCha.Clear();

        // Add players with null checking
        for (int i = 0; i < battleSystem.PlayersPrefab.Count; i++)
        {
            var playerObj = battleSystem.PlayersPrefab[i];
            if (playerObj != null)
            {
                var spriteRenderer = playerObj.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    Characters.Add(playerObj);
                    SRCha.Add(spriteRenderer);
                }
            }
        }

        // Add enemies with null checking
        for (int i = 0; i < battleSystem.EnemiesPrefab.Count; i++)
        {
            var enemyObj = battleSystem.EnemiesPrefab[i];
            if (enemyObj != null)
            {
                var spriteRenderer = enemyObj.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    Characters.Add(enemyObj);
                    SRCha.Add(spriteRenderer);
                }
            }
        }

        _decorations = GetComponentsInChildren<DecoracionCombate>().ToList();

        lastPositions = new Vector3[SRCha.Count];
        for (int i = 0; i < SRCha.Count; i++)
            lastPositions[i] = SRCha[i].transform.position;
        
        SortRenderers();
    }

    void LateUpdate()
    {
        SortRenderers();
    }

    void SortRenderers()
    {
        // Clean up null references first
        SRCha.RemoveAll(sr => sr == null);

        // Handle background decorations
        foreach (var dec in _decorations
                 .Where(d => d.sortGroup == DecoracionCombate.SortGroup.Background))
        {
            if (dec.Renderer != null)
                dec.Renderer.sortingOrder = 0;
        }

        // Create dynamic items list similar to DecorationManager
        var dynamicItems = new List<(SpriteRenderer sr, float key)>();

        // Add character renderers
        dynamicItems.AddRange(
            SRCha
                .Where(sr => sr != null) // Extra safety check
                .Select(sr => (sr, sr.transform.position.y))
        );

        // Add dynamic decorations
        dynamicItems.AddRange(
            _decorations
                .Where(d => d.sortGroup == DecoracionCombate.SortGroup.Dynamic && d.Renderer != null)
                .Select(d => (d.Renderer, d.transform.position.y)) // Use transform.position.y like characters
        );

        // Sort by Y position (higher Y = lower sorting order)
        var sorted = dynamicItems
            .OrderByDescending(item => item.key)
            .ToList();

        // Assign sorting orders
        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].sr.sortingOrder = i + 1;
        }

        // Handle foreground decorations if needed (similar to DecorationManager)
        int frontBase = sorted.Count + 1;
        foreach (var dec in _decorations
                 .Where(d => d.sortGroup == DecoracionCombate.SortGroup.Foreground))
        {
            if (dec.Renderer != null)
            {
                dec.Renderer.sortingOrder = frontBase;
                frontBase++;
            }
        }
    }
}