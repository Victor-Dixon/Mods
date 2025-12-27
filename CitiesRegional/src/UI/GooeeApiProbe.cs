using System;
using System.IO;
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
    private const string GooeePluginTypeName = "Gooee.Plugins.GooeePlugin";

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
                if (string.Equals(name, GooeeAssemblySimpleName, StringComparison.OrdinalIgnoreCase))
                {
                    result.GooeeAssemblyLoaded = true;
                    result.GooeeAssemblyFullName = SafeGetAssemblyFullName(asm);
                    result.GooeeAssemblyVersion = SafeGetAssemblyVersion(asm);
                    result.GooeeAssemblyLocation = SafeGetAssemblyLocation(asm);
                    break;
                }
            }

            // If Gooee is already loaded, locate GooeePlugin type within it.
            if (result.GooeeAssemblyLoaded)
            {
                var gooeeAsm = FindAssemblyBySimpleName(GooeeAssemblySimpleName);
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


