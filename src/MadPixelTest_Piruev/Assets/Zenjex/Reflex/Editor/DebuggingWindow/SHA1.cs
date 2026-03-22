// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Text;

namespace Reflex.Editor.DebuggingWindow
{
  internal static class SHA1
  {
    public static string Hash(object obj)
    {
      var input = obj.ToString();

      // SHA1Managed is obsolete in .NET 5+ - use SHA1.Create() instead
      using (var sha1 = System.Security.Cryptography.SHA1.Create())
      {
        var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
          sb.Append(b.ToString("x2"));
        return sb.ToString();
      }
    }

    public static string ShortHash(object obj) => Hash(obj).Substring(0, 7);
  }
}
