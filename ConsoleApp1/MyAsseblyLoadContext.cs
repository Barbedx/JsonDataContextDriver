// See https://aka.ms/new-console-template for more information

using System.Reflection;
using System.Runtime.Loader;

internal class MyAsseblyLoadContext : AssemblyLoadContext
{
    private AssemblyDependencyResolver _resolver;
    public MyAsseblyLoadContext(string assemblyLocation)
    {
        _resolver = new AssemblyDependencyResolver(assemblyLocation);
    }
    protected override Assembly Load(AssemblyName assemblyName)
    {
        string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }
}