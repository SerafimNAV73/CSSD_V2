using System.Collections.Generic;
using UnityEngine;

namespace CSSD
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class PickUpItem : MonoBehaviour
    {
        [SerializeField] private List<MonoBehaviour> _effects;

        private void OnValidate()
        {
            CheckEffects();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.TryGetComponent<NetworkPlayer>(out var player))
            {
                PickUp(player);
            }
            else 
                return;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.TryGetComponent<NetworkPlayer>(out var player))
            {
                PickUp(player);
            }
            else
                return;
        }

        private void PickUp(NetworkPlayer player)
        {
            for (int i = 0; i < _effects.Count; i++)
            {
                if (_effects[i] != null)
                    if (_effects[i] is IEffect effect)
                    {
                        effect.Effect(player);
                        player.UpBonusCount();
                    }
                    else
                        Debug.LogError("Lost IEffect inheritance");
                else
                    Debug.Log("Lost behaviour");
            }
            Destruction();
        }

        private void CheckEffects()
        {
            if (_effects.Count > 0)
            {
                for (int i = 0; i < _effects.Count; i++)
                {
                    if (_effects[i] == null)
                    {
                        Debug.LogError("Lost effect");
                        return;
                    }
                    else
                        if (_effects[i] is IEffect effect)
                        continue;
                    else
                    {
                        Debug.LogError("Lost IEffect inheritance");
                        return;
                    }
                }
            }
            else
            {
                Debug.LogError("Lost effect");
                return;
            }
        }

        private void Destruction()
        {
            Destroy(gameObject);
        }
    }
}