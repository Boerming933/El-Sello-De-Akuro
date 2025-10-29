using UnityEngine;
using System.Collections.Generic;

public class Decoraciones : MonoBehaviour
{
    public enum SortGroup { Background = 0, Dynamic = 1, Foreground = 2, Houses = 3 }

    public SortGroup sortGroup = SortGroup.Dynamic;

    private SpriteRenderer _renderer;

    public float yDiff;

    public float yDiff2;

    void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    public float EffectiveY => transform.position.y + yDiff;

    public float EffectiveY2 => transform.position.y + yDiff2;

    public SpriteRenderer Renderer => _renderer;

}
