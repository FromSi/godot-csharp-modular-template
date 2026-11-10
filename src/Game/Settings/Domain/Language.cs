namespace Game.Game.Settings.Domain;

/// <summary>
/// A game language: the locale code (<c>en</c>, <c>ru</c>) and its name in that language.
/// </summary>
public record Language(string Code, string Name)
{
    public override string ToString()
    {
        return Name;
    }
}
