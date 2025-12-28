using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CitiesRegional.UI;

/// <summary>
/// Runtime probe for Gooee API presence/shape.
/// This intentionally avoids compile-time dependency on Gooee types by using reflection only.
/// </summary>
internal static class GooeeApiProbe
{
    internal const string Marker = "[GooeeProbe]";
    private const string GooeeAssemblySimpleName = "Gooee";
        private const string GooeePluginTypeName = "Gooee.Plugin";  // Updated: Actual type name is Gooee.Plugin

    internal static GooeeApiProbeResult Run()
    {
        var result = new GooeeApiProbeResult
        {
            ProbedAtUtc = DateTime.UtcNow,
        };

        try
        {
            // Disk probe: confirm Gooee.dll location we expect for CS2+BepInEx installs.
            // This doesn't load the DLL; it simply checks for presence to make UI-001 troubleshooting faster.
            var cs2Install = Environment.GetEnvironmentVariable("CS2_INSTALL") ?? "";
            if (!string.IsNullOrWhiteSpace(cs2Install))
            {
                var gooeePath = Path.Combine(cs2Install, "BepInEx", "plugins", "Gooee.dll");
                result.GooeeDllExpectedPath = gooeePath;
                result.GooeeDllExists = File.Exists(gooeePath);
            }

            var bepinexPlugins = Environment.GetEnvironmentVariable("BEPINEX_PLUGINS") ?? "";
            if (!string.IsNullOrWhiteSpace(bepinexPlugins))
            {
                var gooeePath = Path.Combine(bepinexPlugins, "Gooee.dll");
                result.GooeeDllAlternatePath = gooeePath;
                if (File.Exists(gooeePath))
                {
                    result.GooeeDllExists = true;
                    // Prefer the discovered alternate path if present.
                    result.GooeeDllExpectedPath = gooeePath;
                }
            }

            // Prefer checking already-loaded assemblies to avoid triggering loads or dependency cascades.
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (var i = 0; i < assemblies.Length; i++)
            {
                var asm = assemblies[i];
                var name = SafeGetAssemblyName(asm);
                var fullName = SafeGetAssemblyFullName(asm);
                
                // Check for exact match or if name/fullName contains "Gooee"
                bool exactMatch = string.Equals(name, GooeeAssemblySimpleName, StringComparison.OrdinalIgnoreCase);
                bool nameContains = name != null && name.Contains("Gooee", StringComparison.OrdinalIgnoreCase);
                bool fullNameContains = fullName != null && fullName.Contains("Gooee", StringComparison.OrdinalIgnoreCase);
                bool isGooee = exactMatch || nameContains || fullNameContains;
                
                if (isGooee)
                {
                    result.GooeeAssemblyLoaded = true;
                    result.GooeeAssemblyFullName = fullName ?? "";
                    result.GooeeAssemblyVersion = SafeGetAssemblyVersion(asm);
                    result.GooeeAssemblyLocation = SafeGetAssemblyLocation(asm);
                    Logging.LogInfo($"{Marker} Detection: Found Gooee assembly - name='{name}', fullName='{fullName}'");
                    break;
                }
            }
            
            if (!result.GooeeAssemblyLoaded)
            {
                Logging.LogDebug($"{Marker} Detection: Scanned {assemblies.Length} assemblies, Gooee not found");
            }

            // If Gooee is already loaded, locate GooeePlugin type within it.
            if (result.GooeeAssemblyLoaded)
            {
                // Find the Gooee assembly we detected (reuse assemblies from above)
                Assembly? gooeeAsm = null;
                for (var i = 0; i < assemblies.Length; i++)
                {
                    var asm = assemblies[i];
                    var name = SafeGetAssemblyName(asm);
                    var fullName = SafeGetAssemblyFullName(asm);
                    if (string.Equals(name, GooeeAssemblySimpleName, StringComparison.OrdinalIgnoreCase) ||
                        (name != null && name.Contains("Gooee", StringComparison.OrdinalIgnoreCase)) ||
                        (fullName != null && fullName.Contains("Gooee", StringComparison.OrdinalIgnoreCase)))
                    {
                        gooeeAsm = asm;
                        break;
                    }
                }
                
                if (gooeeAsm != null)
                {
                    var pluginType = gooeeAsm.GetType(GooeePluginTypeName, throwOnError: false);
                    if (pluginType != null)
                    {
                        result.GooeePluginTypeFound = true;
                        result.GooeePluginTypeFullName = pluginType.FullName ?? "Gooee.Plugins.GooeePlugin";
                        result.GooeePluginBaseType = pluginType.BaseType?.FullName ?? "";
                        
                        result.NamePropertyFound = pluginType.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance) != null;
                        result.OnSetupMethodFound = pluginType.GetMethod("OnSetup", BindingFlags.Public | BindingFlags.Instance) != null;
                        result.OnUpdateMethodFound = pluginType.GetMethod("OnUpdate", BindingFlags.Public | BindingFlags.Instance) != null;
                    }
                }
            }
            else
            {
                // As a fallback, try resolving the type without forcing hard errors.
                // This may still return null if Gooee isn't present.
                var t = Type.GetType($"{GooeePluginTypeName}, {GooeeAssemblySimpleName}", throwOnError: false);
                if (t != null)
                {
                    result.GooeePluginTypeFound = true;
                    result.GooeePluginTypeFullName = t.FullName ?? "Gooee.Plugins.GooeePlugin";
                    result.GooeePluginBaseType = t.BaseType?.FullName ?? "";
                    result.NamePropertyFound = t.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance) != null;
                    result.OnSetupMethodFound = t.GetMethod("OnSetup", BindingFlags.Public | BindingFlags.Instance) != null;
                    result.OnUpdateMethodFound = t.GetMethod("OnUpdate", BindingFlags.Public | BindingFlags.Instance) != null;
                }
            }
        }
        catch (Exception ex)
        {
            result.Error = ex.ToString();
        }

        return result;
    }

    internal static void Log(GooeeApiProbeResult r)
    {
        Logging.LogInfo($"{Marker} start (utc={r.ProbedAtUtc:O})");
        
        // Debug: Log all loaded assemblies for troubleshooting
        // Also re-check if Gooee wasn't detected initially (it may have loaded between Run() and Log())
        try
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var gooeeRelated = new System.Collections.Generic.List<string>();
            Assembly? foundGooeeAsm = null;
            
            foreach (var asm in assemblies)
            {
                var name = SafeGetAssemblyName(asm);
                var fullName = SafeGetAssemblyFullName(asm);
                bool isGooee = (name != null && (name.Contains("Gooee", StringComparison.OrdinalIgnoreCase) || name.Contains("Cities2Modding", StringComparison.OrdinalIgnoreCase))) ||
                               (fullName != null && (fullName.Contains("Gooee", StringComparison.OrdinalIgnoreCase) || fullName.Contains("Cities2Modding", StringComparison.OrdinalIgnoreCase)));
                
                if (isGooee)
                {
                    gooeeRelated.Add($"  - {name} ({fullName})");
                    if (foundGooeeAsm == null && string.Equals(name, "Gooee", StringComparison.OrdinalIgnoreCase))
                    {
                        foundGooeeAsm = asm;
                    }
                }
            }
            
            if (gooeeRelated.Count > 0)
            {
                Logging.LogInfo($"{Marker} Found {gooeeRelated.Count} Gooee-related assemblies:");
                foreach (var item in gooeeRelated)
                {
                    Logging.LogInfo($"{Marker}{item}");
                }
            }
            
            // If Gooee wasn't detected in Run() but we found it now, update the result
            if (!r.GooeeAssemblyLoaded && foundGooeeAsm != null)
            {
                r.GooeeAssemblyLoaded = true;
                r.GooeeAssemblyFullName = SafeGetAssemblyFullName(foundGooeeAsm) ?? "";
                r.GooeeAssemblyVersion = SafeGetAssemblyVersion(foundGooeeAsm);
                r.GooeeAssemblyLocation = SafeGetAssemblyLocation(foundGooeeAsm);
                
                // Try multiple possible type names
                string[] possibleTypeNames = {
                    "Gooee.Plugin",  // Found! This is the correct type name
                    "Gooee.Plugins.GooeePlugin",
                    "Gooee.GooeePlugin",
                    "GooeePlugin",
                    "Gooee.Plugins.Plugin"
                };
                
                Type? pluginType = null;
                foreach (var typeName in possibleTypeNames)
                {
                    pluginType = foundGooeeAsm.GetType(typeName, throwOnError: false);
                    if (pluginType != null)
                    {
                        Logging.LogInfo($"{Marker} Found GooeePlugin type: {typeName}");
                        break;
                    }
                }
                
                // Also try Type.GetType as fallback
                if (pluginType == null)
                {
                    foreach (var typeName in possibleTypeNames)
                    {
                        pluginType = Type.GetType($"{typeName}, Gooee", throwOnError: false);
                        if (pluginType != null)
                        {
                            Logging.LogInfo($"{Marker} Found GooeePlugin type via Type.GetType: {typeName}");
                            break;
                        }
                    }
                }
                
                if (pluginType != null)
                {
                    r.GooeePluginTypeFound = true;
                    r.GooeePluginTypeFullName = pluginType.FullName ?? "Gooee.Plugin";
                    r.GooeePluginBaseType = pluginType.BaseType?.FullName ?? "";
                    r.NamePropertyFound = pluginType.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance) != null;
                    r.OnSetupMethodFound = pluginType.GetMethod("OnSetup", BindingFlags.Public | BindingFlags.Instance) != null;
                    r.OnUpdateMethodFound = pluginType.GetMethod("OnUpdate", BindingFlags.Public | BindingFlags.Instance) != null;
                    
                    // List all public members to understand the API
                    try
                    {
                        var properties = pluginType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                        var methods = pluginType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                            .Where(m => !m.IsSpecialName && !m.Name.StartsWith("get_") && !m.Name.StartsWith("set_"))
                            .ToList();
                        
                        Logging.LogInfo($"{Marker} Gooee.Plugin public properties ({properties.Length}):");
                        foreach (var prop in properties.Take(10))
                        {
                            Logging.LogInfo($"{Marker}  - {prop.Name} ({prop.PropertyType.Name})");
                        }
                        
                        Logging.LogInfo($"{Marker} Gooee.Plugin public methods ({methods.Count}):");
                        foreach (var method in methods.Take(15))
                        {
                            var paramsStr = string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                            Logging.LogInfo($"{Marker}  - {method.Name}({paramsStr})");
                        }
                        
                        // Look for panel-related methods
                        var panelMethods = methods.Where(m => 
                            m.Name.Contains("Panel", StringComparison.OrdinalIgnoreCase) ||
                            m.Name.Contains("Register", StringComparison.OrdinalIgnoreCase) ||
                            m.Name.Contains("Create", StringComparison.OrdinalIgnoreCase) ||
                            m.Name.Contains("Show", StringComparison.OrdinalIgnoreCase) ||
                            m.Name.Contains("Display", StringComparison.OrdinalIgnoreCase)
                        ).ToList();
                        
                        if (panelMethods.Count > 0)
                        {
                            Logging.LogInfo($"{Marker} Found {panelMethods.Count} panel-related methods:");
                            foreach (var method in panelMethods)
                            {
                                var paramsStr = string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                                Logging.LogInfo($"{Marker}  *** {method.Name}({paramsStr})");
                            }
                        }
                        
                        // Also check for static methods on Gooee.Plugin or other Gooee types
                        try
                        {
                            var staticMethods = pluginType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                                .Where(m => m.Name.Contains("Panel", StringComparison.OrdinalIgnoreCase) ||
                                           m.Name.Contains("Register", StringComparison.OrdinalIgnoreCase))
                                .ToList();
                            if (staticMethods.Count > 0)
                            {
                                Logging.LogInfo($"{Marker} Found {staticMethods.Count} static panel-related methods:");
                                foreach (var method in staticMethods)
                                {
                                    var paramsStr = string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                                    Logging.LogInfo($"{Marker}  *** STATIC {method.Name}({paramsStr})");
                                }
                            }
                        }
                        catch { }
                    }
                    catch (Exception ex)
                    {
                        Logging.LogDebug($"{Marker} Could not enumerate members: {ex.Message}");
                    }
                }
                else
                {
                    // Debug: List all types that might be plugin base classes
                    try
                    {
                        var allTypes = foundGooeeAsm.GetTypes();
                        var pluginTypes = new System.Collections.Generic.List<string>();
                        var foundTypeNames = new System.Collections.Generic.HashSet<string>();
                        
                        // Find types that inherit from BaseUnityPlugin
                        foreach (var t in allTypes)
                        {
                            var baseType = t.BaseType;
                            while (baseType != null)
                            {
                                if (baseType.Name.Contains("BaseUnityPlugin", StringComparison.OrdinalIgnoreCase))
                                {
                                    var typeName = t.FullName ?? t.Name;
                                    if (!foundTypeNames.Contains(typeName))
                                    {
                                        pluginTypes.Add($"  - {typeName} (base: {baseType.FullName})");
                                        foundTypeNames.Add(typeName);
                                    }
                                    break;
                                }
                                baseType = baseType.BaseType;
                            }
                        }
                        
                        // Also find types with "Plugin" in name
                        foreach (var t in allTypes)
                        {
                            var typeName = t.FullName ?? t.Name;
                            if (!foundTypeNames.Contains(typeName) && 
                                (t.Name.Contains("Plugin", StringComparison.OrdinalIgnoreCase) || 
                                 (t.Namespace != null && t.Namespace.Contains("Gooee", StringComparison.OrdinalIgnoreCase))))
                            {
                                pluginTypes.Add($"  - {typeName} (base: {t.BaseType?.FullName ?? "none"})");
                                foundTypeNames.Add(typeName);
                            }
                        }
                        
                        if (pluginTypes.Count > 0)
                        {
                            Logging.LogInfo($"{Marker} Found {pluginTypes.Count} potential plugin types in Gooee:");
                            int count = 0;
                            foreach (var item in pluginTypes)
                            {
                                if (count++ >= 20) break; // Limit to first 20
                                Logging.LogInfo($"{Marker}{item}");
                            }
                        }
                        else
                        {
                            Logging.LogWarn($"{Marker} No plugin base class found - Gooee may not use a plugin base class");
                            
                            // List ALL public classes/types in Gooee to understand the API structure
                            try
                            {
                                var publicTypes = allTypes.Where(t => t.IsPublic && t.IsClass && !t.IsAbstract).Take(30).ToList();
                                if (publicTypes.Count > 0)
                                {
                                    Logging.LogInfo($"{Marker} Sample of public classes in Gooee (first 30):");
                                    foreach (var t in publicTypes)
                                    {
                                        var methods = t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                                            .Where(m => !m.Name.StartsWith("get_") && !m.Name.StartsWith("set_"))
                                            .Take(5)
                                            .Select(m => m.Name)
                                            .ToList();
                                        var methodList = methods.Count > 0 ? $" [methods: {string.Join(", ", methods)}]" : "";
                                        Logging.LogInfo($"{Marker}  - {t.FullName} (base: {t.BaseType?.Name ?? "none"}){methodList}");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Logging.LogDebug($"{Marker} Could not list public types: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.LogDebug($"{Marker} Could not enumerate Gooee types: {ex.Message}");
                    }
                }
            }
        }
        catch { /* ignore debug logging errors */ }

        if (!string.IsNullOrWhiteSpace(r.Error))
        {
            Logging.LogError($"{Marker} probe failed: {r.Error}");
            return;
        }

        // Disk presence checks first: most common failure is simply missing Gooee.dll.
        if (!string.IsNullOrWhiteSpace(r.GooeeDllExpectedPath))
        {
            Logging.LogInfo($"{Marker} Gooee.dll expected path: {r.GooeeDllExpectedPath} (exists={r.GooeeDllExists})");
        }
        else if (!string.IsNullOrWhiteSpace(r.GooeeDllAlternatePath))
        {
            Logging.LogInfo($"{Marker} Gooee.dll alternate path: {r.GooeeDllAlternatePath} (exists={r.GooeeDllExists})");
        }
        else
        {
            Logging.LogWarn($"{Marker} Gooee.dll path not probed (CS2_INSTALL/BEPINEX_PLUGINS env vars not set).");
        }

        if (r.GooeeAssemblyLoaded)
        {
            Logging.LogInfo($"{Marker} Gooee assembly loaded: {r.GooeeAssemblyFullName}");
            if (!string.IsNullOrWhiteSpace(r.GooeeAssemblyVersion))
                Logging.LogInfo($"{Marker} Gooee version: {r.GooeeAssemblyVersion}");
            if (!string.IsNullOrWhiteSpace(r.GooeeAssemblyLocation))
                Logging.LogInfo($"{Marker} Gooee location: {r.GooeeAssemblyLocation}");
        }
        else
        {
            Logging.LogWarn($"{Marker} Gooee assembly NOT detected among loaded assemblies.");
        }

        if (r.GooeePluginTypeFound)
        {
            Logging.LogInfo($"{Marker} GooeePlugin type FOUND: {r.GooeePluginTypeFullName}");
            if (!string.IsNullOrWhiteSpace(r.GooeePluginBaseType))
                Logging.LogInfo($"{Marker} GooeePlugin base type: {r.GooeePluginBaseType}");

            Logging.LogInfo($"{Marker} GooeePlugin members: NameProperty={r.NamePropertyFound}, OnSetup={r.OnSetupMethodFound}, OnUpdate={r.OnUpdateMethodFound}");
        }
        else
        {
            Logging.LogWarn($"{Marker} GooeePlugin type NOT found (expected: Gooee.Plugins.GooeePlugin).");
        }
    }

    private static Assembly? FindAssemblyBySimpleName(string simpleName)
    {
        try
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (var i = 0; i < assemblies.Length; i++)
            {
                var asm = assemblies[i];
                var name = SafeGetAssemblyName(asm);
                if (string.Equals(name, simpleName, StringComparison.OrdinalIgnoreCase))
                    return asm;
            }
        }
        catch
        {
            // ignore
        }

        return null;
    }

    private static string SafeGetAssemblyName(Assembly asm)
    {
        try { return asm.GetName().Name ?? ""; }
        catch { return ""; }
    }

    private static string SafeGetAssemblyFullName(Assembly asm)
    {
        try { return asm.FullName ?? ""; }
        catch { return ""; }
    }

    private static string SafeGetAssemblyVersion(Assembly asm)
    {
        try { return asm.GetName().Version?.ToString() ?? ""; }
        catch { return ""; }
    }

    private static string SafeGetAssemblyLocation(Assembly asm)
    {
        try { return asm.Location ?? ""; }
        catch { return ""; }
    }
}

internal sealed class GooeeApiProbeResult
{
    internal DateTime ProbedAtUtc { get; set; }

    internal bool GooeeDllExists { get; set; }
    internal string GooeeDllExpectedPath { get; set; } = "";
    internal string GooeeDllAlternatePath { get; set; } = "";

    internal bool GooeeAssemblyLoaded { get; set; }
    internal string GooeeAssemblyFullName { get; set; } = "";
    internal string GooeeAssemblyVersion { get; set; } = "";
    internal string GooeeAssemblyLocation { get; set; } = "";

    internal bool GooeePluginTypeFound { get; set; }
    internal string GooeePluginTypeFullName { get; set; } = "";
    internal string GooeePluginBaseType { get; set; } = "";
    internal bool NamePropertyFound { get; set; }
    internal bool OnSetupMethodFound { get; set; }
    internal bool OnUpdateMethodFound { get; set; }

    internal string Error { get; set; } = "";
}


