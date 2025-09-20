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

    public List<SpriteRenderer> renderers;
    private Vector3[] lastPositions;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private IEnumerator Start()
    {
        yield return new WaitUntil(() => MapManager.Instance != null
                                   && MapManager.Instance.map != null);

        for (int i = 0; i < battleSystem.PlayersPrefab.Count; i++)
        {
            Characters.Add(battleSystem.PlayersPrefab[i]);
            SRCha.Add(battleSystem.PlayersPrefab[i].GetComponent<SpriteRenderer>());
        }

        for (int i = 0; i < battleSystem.EnemiesPrefab.Count; i++)
        {
            Characters.Add(battleSystem.EnemiesPrefab[i]);
            SRCha.Add(battleSystem.EnemiesPrefab[i].GetComponent<SpriteRenderer>());
        }

        renderers = depthContainer.GetComponentsInChildren<SpriteRenderer>().ToList();

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
        // Combina ambas listas
        int Size = 0;
        var allRenderers = renderers
        .Concat(SRCha)
        .OrderByDescending(r => r.transform.position.y)
        .ToList();

        // Asigna sortingOrder secuencial
        for (int i = 0; i < allRenderers.Count; i++)
        {
            Size++;
        }
        for (int i = 0; i < allRenderers.Count; i++)
        allRenderers[i].sortingOrder = i+1;
    }
}
