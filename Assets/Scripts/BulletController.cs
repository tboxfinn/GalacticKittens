using Unity.Netcode;
using UnityEngine;

public class BulletController : NetworkBehaviour
{
    private enum BulletOwner
    {
        enemy,
        player
    };

    public int damage = 1;

    [SerializeField]
    private float m_speed = 12f;

    [Header("Time alive in seconds (s)")]
    [SerializeField]
    private float m_timeToLive = 6f;

    [SerializeField]
    private BulletOwner m_owner;

    [HideInInspector]
    public CharacterDataSO characterData;

    [HideInInspector]
    public Vector3 direction { get; set; } = Vector3.right;

    [HideInInspector]
    public GameObject m_Owner { get; set; } = null;

    private void Start()
    {
        if (m_owner == BulletOwner.player && IsServer)
        {
            ChangeBulletColorClientRpc(characterData.color);
        }
    }

    private void Update()
    {
        if (IsServer)
        {
            m_timeToLive -= Time.deltaTime;
            if(m_timeToLive <= 0f)
                Despawn();

            transform.Translate(direction * m_speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!IsServer)
            return;

        if (collider.TryGetComponent(out IDamagable damagable))
        {
            if (m_owner == BulletOwner.player)
            {
                // For the final score
                characterData.enemiesDestroyed++;
            }

            damagable.Hit(damage);

            Despawn();
        }
    }

    [ClientRpc]
    private void ChangeBulletColorClientRpc(Color newColor)
    {
        GetComponent<SpriteRenderer>().color = newColor;
    }

    public void Despawn()
    {
        if(NetworkObject != null && NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }
}
