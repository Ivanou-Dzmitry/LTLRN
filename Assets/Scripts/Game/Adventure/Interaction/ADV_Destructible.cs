// ADV_Destructible.cs
using UnityEngine;

public class ADV_Destructible : ADV_InteractionBase
{
    protected override void Die()
    {
        PlayParticle(0);

        if (_animator != null)
            _animator.SetTrigger("destr");

        SaveState(ObjectState.Destroyed);
    }
}
