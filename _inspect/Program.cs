using System;
using System.Linq;
using System.Reflection;

// ===== V4 =====
Console.WriteLine("##################################################");
Console.WriteLine("# LanguageExt.Core v4.4.9 - NewType types");
Console.WriteLine("##################################################");
var dllV4 = @"p:\packages\.nuget\languageext.core\4.4.9\lib\net7.0\LanguageExt.Core.dll";
try
{
    // Need to use MetadataLoadContext since we can't load both versions
    var resolver = new PathAssemblyResolver(new string[] {
        dllV4,
        typeof(object).Assembly.Location,
        System.IO.Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "System.Runtime.dll")
    });
    using var mlc = new MetadataLoadContext(resolver);
    var asmV4 = mlc.LoadFromAssemblyPath(dllV4);
    
    var ntTypes = asmV4.GetTypes().Where(t => t.Name.Contains("NewType") && !t.Name.Contains("<")).OrderBy(t => t.FullName).ToArray();
    foreach (var t in ntTypes)
    {
        Console.WriteLine($"\nType: {t.FullName}");
        Console.WriteLine($"  GenericArgs: {string.Join(", ", t.GetGenericArguments().Select(g => g.Name))}");
        Console.WriteLine($"  IsAbstract={t.IsAbstract} IsClass={t.IsClass}");
        if (t.BaseType != null)
            Console.WriteLine($"  BaseType: {t.BaseType}");
        
        Console.WriteLine("  Properties:");
        foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            Console.WriteLine($"    {p.PropertyType} {p.Name}");
        }
        
        Console.WriteLine("  Methods (declared):");
        foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            if (m.IsSpecialName) continue;
            var parms = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType} {p.Name}"));
            var access = m.IsPublic ? "public" : m.IsFamily ? "protected" : "private";
            var stat = m.IsStatic ? " static" : "";
            Console.WriteLine($"    {access}{stat} {m.ReturnType} {m.Name}({parms})");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error loading v4: {ex.Message}");
}

// ===== V5 - more detail =====
Console.WriteLine("\n\n##################################################");
Console.WriteLine("# LanguageExt.Core v5.0.0-beta-77 - New interface details");
Console.WriteLine("##################################################");

var dllV5 = @"p:\packages\.nuget\languageext.core\5.0.0-beta-77\lib\net10.0\LanguageExt.Core.dll";
var asmV5 = System.Reflection.Assembly.LoadFrom(dllV5);

// Check if there is a NewTry or Value property anywhere
Console.WriteLine("\n=== Searching for 'NewTry' in all type members ===");
foreach (var t in asmV5.GetTypes().Where(t => !t.Name.Contains("<")))
{
    var methods = t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)
        .Where(m => m.Name.Contains("NewTry")).ToArray();
    if (methods.Length > 0)
    {
        Console.WriteLine($"  Type: {t.FullName}");
        foreach (var m in methods)
            Console.WriteLine($"    {m.ReturnType.Name} {m.Name}({string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"))})");
    }
}

// Check the Identifier type if it exists
Console.WriteLine("\n=== Searching for 'Identifier' types ===");
foreach (var t in asmV5.GetTypes().Where(t => t.Name.Contains("Identifier") && !t.Name.Contains("<")).Take(10))
{
    Console.WriteLine($"  {t.FullName}");
}

// Check for NumType, FloatType
Console.WriteLine("\n=== Searching for 'NumType' or 'FloatType' ===");
foreach (var t in asmV5.GetTypes().Where(t => (t.Name.Contains("NumType") || t.Name.Contains("FloatType")) && !t.Name.Contains("<")).Take(10))
{
    Console.WriteLine($"  {t.FullName}");
}

// Check the DomainType interface more - how is it meant to be used?
// Also check if Fin is still there
Console.WriteLine("\n=== DomainType`2 deeper look ===");
var dt2 = asmV5.GetTypes().First(t => t.Name == "DomainType`2");
Console.WriteLine($"  Full: {dt2}");
Console.WriteLine($"  All interfaces (including inherited):");
foreach (var i in dt2.GetInterfaces())
    Console.WriteLine($"    {i}");

Console.WriteLine("\n=== Checking for Locus/Identifier types (DomainType implementations) ===");
var domainImpls = asmV5.GetTypes().Where(t =>
{
    try
    {
        return t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition().Name == "DomainType`2");
    }
    catch { return false; }
}).Take(20).ToArray();
foreach (var t in domainImpls)
{
    Console.WriteLine($"  {t.FullName} (GenArgs={t.GetGenericArguments().Length})");
}

// Check Fin
Console.WriteLine("\n=== Fin`1 detail ===");
var fin = asmV5.GetTypes().First(t => t.FullName == "LanguageExt.Fin`1");
Console.WriteLine($"  {fin.FullName} IsAbstract={fin.IsAbstract} IsClass={fin.IsClass}");
