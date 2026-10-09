using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyAttackRangeDetector : MonoBehaviour
{
    private FlyingEnemyProperties owner;

    void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;

        owner = GetComponentInParent<FlyingEnemyProperties>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if(other.CompareTag("Player") || other.transform.root.CompareTag("Player"))
        {
            owner.NotifyPlayerInRange();
        }
    }
}