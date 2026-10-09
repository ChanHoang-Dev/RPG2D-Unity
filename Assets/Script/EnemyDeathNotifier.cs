using System;
using UnityEngine;

public class EnemyDeathNotifier : MonoBehaviour
{
    public event Action OnEnemyDestroyed;

    void OnDestroy()
    {
        OnEnemyDestroyed?.Invoke();
    }
}