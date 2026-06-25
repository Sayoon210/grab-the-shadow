using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// A thing that casts a shadow. For now an axis-aligned box (the character /
    /// test occluder). <see cref="OwnerId"/> tags the resulting shadow so stomp
    /// judgment can later tell whose shadow it is (개발기획서 §2.1, §3).
    /// </summary>
    public readonly struct ShadowCaster
    {
        public readonly Vector2 Center;
        public readonly Vector2 Size;
        public readonly int OwnerId;

        public ShadowCaster(Vector2 center, Vector2 size, int ownerId)
        {
            Center = center;
            Size = size;
            OwnerId = ownerId;
        }
    }
}
