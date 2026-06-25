namespace Game.Core
{
    /// <summary>
    /// Something that casts a shadow. Abstracting the caster here is the extension
    /// seam for "more shadow-casting objects later": the engine computes one shadow
    /// per caster (it never assumes a single caster), so scaling to N is just
    /// looping over N <see cref="IShadowCaster"/>s — no engine change. A neutral
    /// object can use OwnerId = -1 so stomp judgment ignores it.
    /// </summary>
    public interface IShadowCaster
    {
        /// <summary>Caster silhouette (box) read fresh, with its owner id.</summary>
        CasterShape Shape { get; }

        /// <summary>Display the computed shadow for this caster.</summary>
        void Render(in ShadowPolygon poly);
    }
}
