using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;
using System.Linq;
using System.Text;

// make special treatment for typenames with - in it
// dotnet-svcutil just drops the -

namespace UWDiff
{
    public class TypeDisagreementParser
    {
        public TypeDisagreementParser()
        {
        }

        public IEnumerable<TypeDisagreement> Parse(string path)
        {
            var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(path));
            var root = tree.GetRoot();

            var ios = root
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Where(c => c.Identifier.Text.EndsWithEither("RequestType", "ResponseType"));

            var results = new List<TypeDisagreement>();

            foreach (var cl in ios)
            {
                var attrs = cl.AttributeLists.SelectMany(a => a.Attributes);
                var xmlType = attrs.FirstOrDefault(a => a.Name?.ToString() == "System.Xml.Serialization.XmlTypeAttribute");

                if (xmlType != null)
                {
                    var classname = cl.Identifier.Text;
                    var typename = xmlType.ArgumentList.Arguments.First().ToFullString().Replace("\"", "").Replace("-", "");

                    if (!classname.Equals(typename))
                    {
                        results.Add(new TypeDisagreement
                        {
                            ClassName = classname,
                            TypeName = typename
                        });
                    }
                }
            }

            return results;
        }
    }

    public class TypeDisagreement
    {
        public string ClassName { get; set; }
        public string TypeName { get; set; }
    }
}
