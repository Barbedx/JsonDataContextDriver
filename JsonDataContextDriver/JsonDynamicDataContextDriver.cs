
//using MessageBox = System.Windows.MessageBox;


using JsonDataContext;

using JsonDataContextDriver.Extensions;
using JsonDataContextDriver.Extensions.Notepad;
using JsonDataContextDriver.Inputs; 
using JsonDataContextDriver.Views;

using LINQPad.Extensibility.DataContext;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

//using LINQPad.Extensibility.DataContext;

using Microsoft.CSharp;

using Newtonsoft.Json;

using System.CodeDom.Compiler;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Web;

namespace JsonDataContextDriver
{
    public class JsonDynamicDataContextDriver : DynamicDataContextDriver
    {
        public override string Name => "JSON DataContext Provider";

        public override string Author => "Ryan Davis";

        public override string GetConnectionDescription(IConnectionInfo cxInfo)
        {
            var deb = File.ReadAllText(@"C:\Users\petruan1\OneDrive - Mars Inc\Desktop\debuger.txt");
            if(deb.ToLower().Contains("on1"))
                Debugger.Launch();
            return String.IsNullOrWhiteSpace(cxInfo.DisplayName) ? "Unnamed JSON Data Context" : cxInfo.DisplayName;

        }

        public override bool ShowConnectionDialog(IConnectionInfo cxInfo, bool isNewConnection)
        {
            var deb = File.ReadAllText(@"C:\Users\petruan1\OneDrive - Mars Inc\Desktop\debuger.txt");
            if (deb.ToLower().Contains("on2"))
                Debugger.Launch();
            var dialog = new ConnectionDialog();
            dialog.SetContext(cxInfo, isNewConnection);

            var result = dialog.ShowDialog();
            return result == true;
        }

        public override IEnumerable<string> GetAssembliesToAdd(IConnectionInfo cxInfo)
        {
            var deb = File.ReadAllText(@"C:\Users\petruan1\OneDrive - Mars Inc\Desktop\debuger.txt");
            if (deb.ToLower().Contains("on3"))
                Debugger.Launch();
            return base.GetAssembliesToAdd(cxInfo)
                .Concat(new[] { typeof(JsonDataContextBase).Assembly.Location, typeof(HttpUtility).Assembly.Location });
        }

        public override IEnumerable<string> GetNamespacesToAdd(IConnectionInfo cxInfo)
        {
            var deb = File.ReadAllText(@"C:\Users\petruan1\OneDrive - Mars Inc\Desktop\debuger.txt");
            if (deb.ToLower().Contains("on4"))
                Debugger.Launch();
            return base.GetNamespacesToAdd(cxInfo)
                .Concat(_nameSpacesToAdd)
                .Distinct();
        }

        private List<string> _nameSpacesToAdd = new List<string>();


        public override List<ExplorerItem> GetSchemaAndBuildAssembly(IConnectionInfo cxInfo,
            AssemblyName assemblyToBuild, ref string nameSpace,
            ref string typeName)
        {

            _nameSpacesToAdd = new List<string>();

            var xInputs = cxInfo.DriverData.Element("inputDefs");
            if (xInputs == null)
                return new List<ExplorerItem>();

            var jss = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            var inputDefs = JsonConvert.DeserializeObject<List<IJsonInput>>(xInputs.Value, jss).ToList();

            var ns = nameSpace;

            // generate class definitions
            var classDefinitions =
                inputDefs
                    .AsParallel()
                    .SelectMany(i =>
                    {
                        i.GenerateClasses(ns);
                        return i.GeneratedClasses;
                    })
                    .ToList();

            // add namespaces
            _nameSpacesToAdd.AddRange(inputDefs.SelectMany(i => i.NamespacesToAdd));
            _nameSpacesToAdd.AddRange(classDefinitions.Select(c => c.Namespace));

            // remove the error'd inputs
            var classGenErrors = inputDefs.SelectMany(i => i.Errors).ToList();

            classDefinitions =
                classDefinitions
                    .Where(c => c.Success)
                    .ToList();

            // resolve duplicates
            classDefinitions
                .GroupBy(c => c.ClassName)
                .Where(c => c.Count() > 1)
                .SelectMany(cs => cs.Select((c, i) => new { Class = c, Index = i + 1 }).Skip(1))
                .ToList()
                .ForEach(c => c.Class.ClassName += "_" + c.Index);

            // create code to compile
            var usings = "using System;\r\n" +
                         "using System.Collections.Generic;\r\n" +
                         "using System.IO;\r\n" +
                         "using Newtonsoft.Json;\r\n" +
                         "using System.Web;\r\n" +
                         "using System.Linq;\r\n" +
                         "using JsonDataContext;\r\n" +
                         "using System.Net;\r\n";

            usings += String.Join("\r\n", classDefinitions.Select(c => String.Format("using {0};", c.Namespace)));




            var contextProperties =
                inputDefs.SelectMany(i => i.ContextProperties);
            var baseCode = Constants.JsonBaseCode;
            var context =
                String.Format("namespace {1} {{\r\n\r\n public class {2} : JsonDataContextBase {{\r\n\r\n\t\t{0}\r\n\r\n}}\r\n\r\n}}",
                    String.Join("\r\n\r\n\t\t", contextProperties), nameSpace, typeName);
            var code = String.Join("\r\n", classDefinitions.Select(c => c.ClassDefinition));

            var contextWithCode = String.Join("\r\n\r\n", usings, baseCode, context, code);

            var deb = File.ReadAllText(@"C:\Users\petruan1\OneDrive - Mars Inc\Desktop\debuger.txt");
            if (deb.ToLower().Contains("on5"))
                Debugger.Launch();
            var codeString = SourceText.From(contextWithCode);
            var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Default);

            var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, options);
            const string longName = "System.Collections, Version=4.1.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

            List<string> references = GetReferencesPaths(longName);
           var s= references.Select(x => x.ToString()).ToArray();
            var compileResult = CompileSource(new CompilationInput
            {
                FilePathsToReference = s,
                OutputPath = assemblyToBuild.CodeBase,
                SourceCode = new[] { contextWithCode }
            });



            //var sharpCompilation = CSharpCompilation.Create(assemblyToBuild.Name,
            //    new[] { parsedSyntaxTree },
            //    references: references,
            //    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
            //        optimizationLevel: OptimizationLevel.Release,
            //        assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));

            ////var assName = assemblyToBuild.Name + "_temp.dll";
            ////string assPath; 
            //using var peStream = new MemoryStream();

            //var result = sharpCompilation.Emit(peStream);

            //using var filestream = File.Create(assName);
            //peStream.Seek(0, SeekOrigin.Begin);
            //peStream.CopyTo(filestream);
            //assPath = filestream.Name;
            //}


            //  var provider = new CSharpCodeProvider();
            //  var parameters = new CompilerParameters
            //  {
            //      IncludeDebugInformation = true,
            //      OutputAssembly = assemblyToBuild.CodeBase,
            //      ReferencedAssemblies =
            //{
            //    typeof (JsonDataContextBase).Assembly.Location,
            //    typeof (JsonConvert).Assembly.Location,

            //    typeof (UriBuilder).Assembly.Location,
            //    typeof (HttpUtility).Assembly.Location,
            //    typeof (System.String).Assembly.Location
            //}
            //  };
             

            // var result = provider.CompileAssemblyFromSource(parameters, contextWithCode);

            if (!compileResult.Errors.Any())
            {
                ////var assemblyVytes = peStream.ToArray();
                //var asscontext = new AssemblyLoadContext(assemblyToBuild.Name);
                //peStream.Seek(0, SeekOrigin.Begin);
                //var /*assembly*/ = new AssemblyLoadContext(assemblyToBuild.Name);
                //assembly.LoadFromAssemblyName(assemblyToBuild);
                var assembly= AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyToBuild.CodeBase);

                // Pray to the gods of UX for redemption..
                // We Can Do Better
                if (classGenErrors.Any())
                    MessageBox.Show(String.Format("Couldn't process {0} inputs:\r\n{1}", classGenErrors.Count,
                        String.Join(Environment.NewLine, classGenErrors)));

                return

                    LinqPadSampleCode.GetSchema(assembly.GetType(String.Format("{0}.{1}", nameSpace, typeName)))
                    .Concat(inputDefs.SelectMany(i => i.ExplorerItems ?? new List<ExplorerItem>()))
                    .ToList();
            }
            else
            {
                // compile failed, this is Bad
                var sb = new StringBuilder();
                sb.AppendLine("Could not generate a typed context for the given inputs. The compiler returned the following errors:\r\n");

                //var failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);
                foreach (var err in compileResult.Errors)
                    sb.AppendFormat(" - {0}\r\n", err);

                if (classGenErrors.Any())
                {
                    sb.AppendLine("\r\nThis may have been caused by the following class generation errors:\r\n");
                    sb.AppendLine(String.Join(Environment.NewLine, String.Join(Environment.NewLine, classGenErrors)));
                }

                MessageBox.Show(sb.ToString());

                NotepadHelper.ShowMessage(contextWithCode, "Generated source code");

                throw new Exception("Could not generate a typed context for the given inputs");
            }
        }

        private static List<string> GetReferencesPaths(string longName)
        {
            //var systemCollectionsAssembly = Assembly.Load(longName);
            var references = new List<string>();
            //{
            //     typeof(object).Assembly.Location,
            //     typeof(Console).Assembly.Location,
            //     typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location,
            //     typeof(JsonDataContextBase).Assembly.Location,
            //     typeof(JsonConvert).Assembly.Location,
            //     typeof(UriBuilder).Assembly.Location,
            //     typeof(HttpUtility).Assembly.Location,
            //     typeof(HttpWebRequest).Assembly.Location,
            //     typeof(System.Net.WebRequest).Assembly.Location,
            //     typeof(DecompressionMethods).Assembly.Location,
            //     typeof(WebHeaderCollection).Assembly.Location,
            //     typeof(TextReader).Assembly.Location,
            //     typeof(NameValueCollection).Assembly.Location,
            //     typeof(Enumerable).Assembly.Location,
            //     typeof(List<object>).Assembly.Location,
            //     AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "netstandard").Location,
            //     systemCollectionsAssembly.Location
            //};
            references.AddRange(GetCoreFxReferenceAssemblies());
            Assembly.GetEntryAssembly().GetReferencedAssemblies() //TODO:Remove unused assembly lol
           .ToList()
           .ForEach(a => references.Add(Assembly.Load(a).Location));
            return references;
        }

        public override void InitializeContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
        {
            var deb = File.ReadAllText(@"C:\Users\petruan1\OneDrive - Mars Inc\Desktop\debuger.txt");
            if (deb.ToLower().Contains("on6"))
                Debugger.Launch();
            base.InitializeContext(cxInfo, context, executionManager);
            
            var ctx = (JsonDataContextBase)context;

            var xInputs = cxInfo.DriverData.Element("inputDefs");
            if (xInputs == null)
                return;

            var jss = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            var inputs = JsonConvert.DeserializeObject<List<IJsonInput>>(xInputs.Value, jss).ToList();

            inputs
                .OfType<JsonTextInput>()
                .ToList()
                .ForEach(c => ctx._jsonTextInputs.Add(c.InputGuid, c.Json));
        }
    }

}