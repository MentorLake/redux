namespace MentorLake.Redux.Effects;

/// <summary>
/// Options for an <see cref="Effect"/>.
/// </summary>
public class EffectConfig
{
	/// <summary>
	/// When true, the effect's outputs are dispatched as actions through the full pipeline.
	/// When false (default), outputs are observed only and dropped.
	/// </summary>
	public bool Dispatch { get; set; }
}
