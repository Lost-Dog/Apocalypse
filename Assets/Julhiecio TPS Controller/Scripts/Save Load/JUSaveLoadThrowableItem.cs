using JUTPS.ItemSystem;
using UnityEngine;

namespace JU.SaveLoad
{
    /// <inheritdoc/>
    [RequireComponent(typeof(ThrowableItem))]
    [AddComponentMenu("JU TPS/Save Load/JU Save Load Throwable Item")]
    public class JUSaveLoadThrowableItem : JUSaveLoadItem<ThrowableItem>
    {
    }
}