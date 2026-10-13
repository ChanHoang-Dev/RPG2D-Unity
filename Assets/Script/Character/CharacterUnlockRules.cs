public static class CharacterUnlockRules
{
    /// <summary>
    /// Returns true when the character's unlock requirements are satisfied
    /// (e.g. enough currency, stage cleared, another character reached a level...).
    /// </summary>
    public static bool CanUnlock(CharacterData character)
    {
        // TODO: implement the unlock condition.
        // Placeholder returns true so the current unlock flow keeps working until the logic is written.
        return true;
    }
}