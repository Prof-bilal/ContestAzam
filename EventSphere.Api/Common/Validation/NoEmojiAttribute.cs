using System.ComponentModel.DataAnnotations;

namespace EventSphere.Api.Common.Validation;

/// <summary>
/// Rejects emoji characters while allowing legitimate Unicode letters
/// (Arabic, Chinese, Japanese, etc.). Blocks characters in supplementary
/// planes (U+10000+) where most emoji live, plus known BMP emoji ranges.
/// </summary>
public class NoEmojiAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is not string text || string.IsNullOrEmpty(text))
            return ValidationResult.Success;

        foreach (var rune in text.EnumerateRunes())
        {
            // Supplementary planes (U+10000+) — covers most emoji:
            // U+1F000-U+1FFFF (Miscellaneous Symbols and Pictographs, Emoticons, etc.)
            // U+20000-U+2A6DF (CJK Extension B, rare but mostly not names)
            // U+2A700-U+2B73F, U+2B740-U+2B81F, U+2B820-U+2CEAF (more CJK extensions)
            // U+2F800-U+2FA1F (CJK Compatibility Supplement)
            // U+F0000-U+FFFFF (Supplementary Private Use Area)
            // U+100000-U+10FFFF (Supplementary Private Use Area-A/B)
            if (rune.Value >= 0x10000)
                return new ValidationResult(ErrorMessage ?? "Emoji are not allowed in this field.");

            // Known BMP emoji/symbol ranges
            if (IsBmpEmoji(rune.Value))
                return new ValidationResult(ErrorMessage ?? "Emoji are not allowed in this field.");
        }

        return ValidationResult.Success;
    }

    private static bool IsBmpEmoji(int codePoint)
    {
        // Misc Symbols (U+2600–U+26FF)
        if (codePoint >= 0x2600 && codePoint <= 0x26FF) return true;
        // Dingbats (U+2700–U+27BF)
        if (codePoint >= 0x2700 && codePoint <= 0x27BF) return true;
        // Misc Technical (U+2300–U+23FF) — includes ⌛, ⏰, etc.
        if (codePoint >= 0x2300 && codePoint <= 0x23FF) return true;
        // Arrows (U+2190–U+21FF) — mostly not emoji, skip
        // Enclosed Alphanumerics (U+2460–U+24FF) — ① etc., skip (legitimate)
        // Box Drawing (U+2500–U+257F) — skip (legitimate)
        // Block Elements (U+2580–U+259F) — skip
        // Geometric Shapes (U+25A0–U+25FF) — some emoji here
        if (codePoint >= 0x25A0 && codePoint <= 0x25FF) return true;
        // Miscellaneous Symbols and Arrows (U+2B00–U+2BFF)
        if (codePoint >= 0x2B00 && codePoint <= 0x2BFF) return true;
        // Copyright (U+00A9), Registered (U+00AE) — allow in org names
        // Telecom symbols (U+2100–U+214F) — include ™
        if (codePoint == 0x2122) return true; // ™

        return false;
    }
}
