using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace UWDiff
{
    public class ClientExtractor
    {
        readonly IEnumerable<TypeDisagreement> disagreements;

        public ClientExtractor(IEnumerable<TypeDisagreement> disagreements)
        {
            this.disagreements = disagreements;
        }

        public ClientContent Extract(string path)
        {
            var content = new ClientContent();
            var lines = File.ReadAllLines(path);

            for (var i = 0; i < lines.Length; i++)
            {
                var current = lines[i];

                if (content.Namespace is null && current.StartsWith("namespace "))
                {
                    content.Namespace = current;
                    continue;
                }

                var stripped = current.Trim();

                //capture Input Output wrappers
                if (stripped.StartsWith("public partial class") &&
                    stripped.EndsWithEither("Input", "Output"))
                {
                    var prev = lines[i - 1].Trim();
                    if (!prev.Equals("[System.ServiceModel.MessageContractAttribute(IsWrapped=false)]"))
                    {
                        continue;
                    }
                    var (resume, typedef) = ExtractClass(i, lines);
                    i = resume;
                    content.Types.Add(typedef);
                    continue;
                }

                //capture interface
                if (stripped.StartsWith("public interface"))
                {
                    var (resume, typedef) = ExtractInterface(i, lines);
                    i = resume;
                    content.Types.Add(typedef);
                    continue;
                }

                // capture client
                if (stripped.StartsWith("public partial class") &&
                    stripped.Contains("ClientBase<"))
                {
                    var (resume, typedef) = ExtractClass(i, lines);
                    i = resume;
                    content.Types.Add(typedef);
                    continue;
                }
            }

            return content;
        }

        int RewindToStart(int i, string[] lines)
        {
            var rev = i;
            while (true)
            {
                rev--;
                var stripped = lines[rev].Trim();
                if (string.IsNullOrWhiteSpace(stripped) || stripped.Equals("{"))
                {
                    break;
                }
            }
            rev++;
            return rev;
        }

        // Extracts a complete class definition for classes derived from ClientBase
        (int, TypeDefinition) ExtractClass(int i, string[] lines, bool enforcePascal = false)
        {
            var rev = RewindToStart(i, lines);
            var start = rev;

            // at this point rev is lined up on the first attribute line
            var definition = new List<string>();

            // capture lines until we have captured the declaration line
            while (true)
            {
                var trimmed = lines[rev].Trim();
                if (trimmed.StartsWith("///"))
                {
                    definition.Add(lines[rev]);
                    rev++;
                    continue;
                }

                if (trimmed.StartsWith("[") && trimmed.Contains("Attribute("))
                {
                    definition.Add(lines[rev]);
                    rev++;
                    continue;
                }

                if (trimmed.StartsWith("public partial class"))
                {
                    definition.Add(lines[rev]);
                    rev++;
                    break;
                }

                throw new InvalidOperationException($"Unexpected class content at line: {rev}");
            }

            if (!lines[rev].Trim().Equals("{"))
            {
                throw new InvalidOperationException($"Unexpected class content at line: {rev}");
            }
            // ensure the next line is just a brack and capture it, bracket++
            definition.Add(lines[rev]);
            var brackets = 1;
            rev++;

            // capture lines incrementing/decrementing bracket as we go until its back to 0
            while (brackets > 0)
            {
                var trimmed = lines[rev].Trim();
                if (trimmed.Equals("{"))
                {
                    brackets++;
                }
                if (trimmed.Equals("}"))
                {
                    brackets--;
                }

                // if enforcePascal, then issue the correction here first
                if (enforcePascal)
                {
                    //lines[rev] = 
                }

                definition.Add(lines[rev]);
                rev++;
            }

            var end = rev;
            rev++;

            return (rev, new TypeDefinition
            {
                Start = start,
                End = end,
                Lines = definition
            });
        }

        // Extracts a complete definition for public interfaces
        (int, TypeDefinition) ExtractInterface(int i, string[] lines)
        {
            var rev = RewindToStart(i, lines);
            var start = rev;

            // at this point rev is lined up on the first attribute line
            var definition = new List<string>();

            // capture lines until we have captured the declaration line
            while (true)
            {
                var trimmed = lines[rev].Trim();
                if (trimmed.StartsWith("///"))
                {
                    definition.Add(lines[rev]);
                    rev++;
                    continue;
                }

                if (trimmed.StartsWith("[") && trimmed.Contains("Attribute("))
                {
                    definition.Add(lines[rev]);
                    rev++;
                    continue;
                }

                if (trimmed.StartsWith("public interface"))
                {
                    definition.Add(lines[rev]);
                    rev++;
                    break;
                }

                throw new InvalidOperationException($"Unexpected interface content at line: {rev}");
            }

            if (!lines[rev].Trim().Equals("{"))
            {
                throw new InvalidOperationException($"Unexpected interface content at line: {rev}");
            }
            // ensure the next line is just a brack and capture it
            definition.Add(lines[rev]);
            rev++;

            // capture lines incrementing/decrementing bracket as we go until its back to 0
            while (true)
            {
                if (!lines[rev].Trim().Equals("}"))
                {
                    definition.Add(lines[rev]);
                    rev++;
                    continue;
                }

                definition.Add(lines[rev]);
                break;
            }

            var end = rev;
            rev++;

            return (rev, new TypeDefinition
            {
                Start = start,
                End = end,
                Lines = definition
            });
        }
    }

    public class ClientContent
    {
        public string Namespace { get; set; }
        public List<TypeDefinition> Types { get; set; } = new List<TypeDefinition>();

        public override string ToString()
        {
            var b = new StringBuilder(AUTO_GENERATED);

            b.AppendLine(Namespace);
            b.AppendLine("{");

            for (int i = 0; i < Types.Count; i++)
            {
                foreach (var line in Types[i].Lines)
                {
                    b.AppendLine(line);
                }
                if (i < Types.Count - 1)
                {
                    b.AppendLine();
                }
            }
            b.AppendLine("}");

            return b.ToString();
        }

        const string AUTO_GENERATED = @"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

"
;
    }

    public class TypeDefinition
    {
        public int Start { get; set; }
        public int End { get; set; }
        public IEnumerable<string> Lines { get; set; }
    }

    static class StringExtensions
    {
        public static bool EndsWithEither(this string test, params string[] endings)
        {
            return endings.Any(e => test.EndsWith(e));
        }
    }
}
