using System;
using System.Text;

namespace SwIpToGemp;

static class Extensions
{
    public static StringBuilder AppendXml(this StringBuilder stringBuilder, ReadOnlySpan<char> chars)
    {
        foreach (var c in chars)
        {
            var text = c switch
            {
                '&' => "&amp;",
                '\'' => "&apos;",
                '"' => "&quot;",
                '>' => "&gt;",
                '<' => "&lt;",
                _ => default(string)
            };

            if (text is null)
                stringBuilder.Append(c);
            else
                stringBuilder.Append(text);
        }
        
        return stringBuilder;
    }
}
