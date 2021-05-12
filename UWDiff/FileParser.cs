using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace UWDiff
{
    public class FileParser
    {
        public FileParser()
        {
        }

        public IEnumerable<ClassDeclaration> Parse(string path)
        {
            var num = 0;
            var file = Path.GetFileName(path);
            foreach (var line in  File.ReadLines(path))
            {
                num++;
                var stripped = line.TrimStart();
                if (stripped.StartsWith("public partial class"))
                {
                    yield return new ClassDeclaration
                    {
                        File = file,
                        Line = num,
                        ClassName = stripped.Split(" ", StringSplitOptions.RemoveEmptyEntries).Last()
                    };
                }
            }
        }
    }

    public class ClassDeclaration : IEquatable<ClassDeclaration>
    {
        public string File { get; set; }
        public int Line { get; set; }
        public string ClassName { get; set; }

        public bool Equals([AllowNull] ClassDeclaration other)
        {
            if (other is null) return false;

            return ClassName.Equals(other.ClassName);
        }
    }
}
