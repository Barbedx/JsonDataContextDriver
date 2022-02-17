// See https://aka.ms/new-console-template for more information



using JsonDataContext;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Web;
const string longName = "System.Collections, Version=4.1.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

var systemCollectionsAssembly = Assembly.Load(longName);


Console.WriteLine("Hello, World!");
string source = @"using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Web;
using JsonDataContext;
using LINQPad.User.ex4Input;

namespace LINQPad.User {

 public class TypedDataContext : JsonDataContextBase {

		public IEnumerable<LINQPad.User.ex4Input.ex4> ex4 
		{ 
		 get 
		 {
		 return GetTextJsonInput<LINQPad.User.ex4Input.ex4>(""55bef464-6674-41aa-88ba-0a22ce502434""); 
		 }
		}

}

}

namespace LINQPad.User.ex4Input
{

    public class GlossDef
    {
        public string para { get; set; }
        public List<string> GlossSeeAlso { get; set; }
    }

    public class GlossEntry
    {
        public string ID { get; set; }
        public string SortAs { get; set; }
        public string GlossTerm { get; set; }
        public string Acronym { get; set; }
        public string Abbrev { get; set; }
        public GlossDef GlossDef { get; set; }
        public string GlossSee { get; set; }
    }

    public class GlossList
    {
        public GlossEntry GlossEntry { get; set; }
    }

    public class GlossDiv
    {
        public string title { get; set; }
        public GlossList GlossList { get; set; }
    }

    public class Glossary
    {
        public string title { get; set; }
        public GlossDiv GlossDiv { get; set; }
    }

    public class ex4
    {
        public Glossary glossary { get; set; }
    }

}
";


var codeString = SourceText.From(source);
var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp7_3);

var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, options);

var references = new List<MetadataReference>
{
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(JsonDataContextBase).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(JsonConvert).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(UriBuilder).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(HttpUtility).Assembly.Location),

};
Assembly.GetEntryAssembly().GetReferencedAssemblies()
            .ToList()
            .ForEach(a => references.Add(MetadataReference.CreateFromFile(Assembly.Load(a).Location)));
var sharpCompilation = CSharpCompilation.Create("Hello.dll",
    new[] { parsedSyntaxTree },
    references: references,
    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
        optimizationLevel: OptimizationLevel.Release,
        assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));

var assName = "Hello" + "_temp.dll";
using var peStream = new MemoryStream();

var results = sharpCompilation.Emit(peStream);
using var filestream = File.Create(assName);

peStream.Seek(0, SeekOrigin.Begin);
peStream.CopyTo(filestream);
filestream.Close();

//var assemblyVytes = peStream.ToArray();
var asscontext = new AssemblyLoadContext(filestream.Name);
//var asscontext = new MyAsseblyLoadContext(filestream.Name);
var ass = asscontext.LoadFromAssemblyPath(filestream.Name);
//var ass = asscontext.LoadFromAssemblyPath(filestream.Name);

Console.WriteLine(results.Success);


var failures = results.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);
foreach (var diagnostic in failures)
{
    Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
}



