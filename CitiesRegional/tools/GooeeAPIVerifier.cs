using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;

namespace CitiesRegional.Tools;

/// <summary>
/// Tool I wished I had: Gooee API Verifier
/// 
/// This tool would help verify Gooee API availability and structure
/// without requiring full game launch. It inspects Gooee.dll and reports:
/// - Available types (GooeePlugin, panels, components)
/// - Method signatures (OnSetup, OnUpdate, etc.)
/// - Dependencies and requirements
/// - API compatibility status
/// 
/// Usage: Run this tool to verify Gooee API before implementing UI panels
/// </summary>
public class GooeeAPIVerifier
{
    private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("GooeeAPIVerifier");
    
    /// <summary>
    /// Verifies Gooee API availability and structure
    /// </summary>
    public static VerificationResult VerifyGooeeAPI(string gooeeDllPath)
    {
        var result = new VerificationResult
        {
            DllPath = gooeeDllPath,
            DllExists = System.IO.File.Exists(gooeeDllPath),
            VerifiedAt = DateTime.UtcNow
        };
        
        if (!result.DllExists)
        {
            result.Status = "ERROR";
            result.Message = $"Gooee.dll not found at: {gooeeDllPath}";
            return result;
        }
        
        try
        {
            // Try to load the assembly
            var assembly = Assembly.LoadFrom(gooeeDllPath);
            result.AssemblyLoaded = true;
            result.AssemblyName = assembly.GetName().Name;
            result.AssemblyVersion = assembly.GetName().Version?.ToString() ?? "Unknown";
            
            // Find GooeePlugin type
            var gooeePluginType = assembly.GetType("Gooee.Plugins.GooeePlugin");
            if (gooeePluginType != null)
            {
                result.GooeePluginFound = true;
                result.GooeePluginBaseType = gooeePluginType.BaseType?.FullName ?? "Unknown";
                
                // Check for expected methods
                var onSetupMethod = gooeePluginType.GetMethod("OnSetup", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                var onUpdateMethod = gooeePluginType.GetMethod("OnUpdate", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                var nameProperty = gooeePluginType.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
                
                result.OnSetupMethodFound = onSetupMethod != null;
                result.OnUpdateMethodFound = onUpdateMethod != null;
                result.NamePropertyFound = nameProperty != null;
                
                // Get method signatures
                if (onSetupMethod != null)
                {
                    result.OnSetupSignature = GetMethodSignature(onSetupMethod);
                }
                if (onUpdateMethod != null)
                {
                    result.OnUpdateSignature = GetMethodSignature(onUpdateMethod);
                }
            }
            
            // Find other important types
            var types = assembly.GetExportedTypes();
            result.ExportedTypesCount = types.Length;
            
            foreach (var type in types)
            {
                if (type.Name.Contains("Panel") || type.Name.Contains("Component"))
                {
                    result.RelevantTypes.Add(type.FullName ?? type.Name);
                }
            }
            
            result.Status = "SUCCESS";
            result.Message = "Gooee API structure verified successfully";
        }
        catch (Exception ex)
        {
            result.Status = "ERROR";
            result.Message = $"Failed to load Gooee.dll: {ex.Message}";
            result.ErrorDetails = ex.ToString();
            
            // Check if it's a dependency issue
            if (ex.Message.Contains("Game.dll") || ex.Message.Contains("Colossal"))
            {
                result.Status = "PARTIAL";
                result.Message = "Gooee.dll found but requires game dependencies. API verification requires in-game testing.";
            }
        }
        
        return result;
    }
    
    private static string GetMethodSignature(MethodInfo method)
    {
        var parameters = method.GetParameters();
        var paramStrings = new System.Collections.Generic.List<string>();
        
        foreach (var param in parameters)
        {
            paramStrings.Add($"{param.ParameterType.Name} {param.Name}");
        }
        
        return $"{method.ReturnType.Name} {method.Name}({string.Join(", ", paramStrings)})";
    }
    
    /// <summary>
    /// Prints verification results to console/log
    /// </summary>
    public static void PrintResults(VerificationResult result)
    {
        Logger.LogInfo("=== Gooee API Verification Results ===");
        Logger.LogInfo($"Status: {result.Status}");
        Logger.LogInfo($"DLL Path: {result.DllPath}");
        Logger.LogInfo($"DLL Exists: {result.DllExists}");
        Logger.LogInfo($"Assembly Loaded: {result.AssemblyLoaded}");
        
        if (result.AssemblyLoaded)
        {
            Logger.LogInfo($"Assembly: {result.AssemblyName} v{result.AssemblyVersion}");
            Logger.LogInfo($"GooeePlugin Found: {result.GooeePluginFound}");
            
            if (result.GooeePluginFound)
            {
                Logger.LogInfo($"  Base Type: {result.GooeePluginBaseType}");
                Logger.LogInfo($"  OnSetup Method: {result.OnSetupMethodFound} - {result.OnSetupSignature}");
                Logger.LogInfo($"  OnUpdate Method: {result.OnUpdateMethodFound} - {result.OnUpdateSignature}");
                Logger.LogInfo($"  Name Property: {result.NamePropertyFound}");
            }
            
            Logger.LogInfo($"Exported Types: {result.ExportedTypesCount}");
            if (result.RelevantTypes.Count > 0)
            {
                Logger.LogInfo("Relevant Types:");
                foreach (var type in result.RelevantTypes)
                {
                    Logger.LogInfo($"  - {type}");
                }
            }
        }
        
        Logger.LogInfo($"Message: {result.Message}");
        if (!string.IsNullOrEmpty(result.ErrorDetails))
        {
            Logger.LogWarn($"Error Details: {result.ErrorDetails}");
        }
    }
    
    public class VerificationResult
    {
        public string Status { get; set; } = "UNKNOWN";
        public string DllPath { get; set; } = "";
        public bool DllExists { get; set; }
        public bool AssemblyLoaded { get; set; }
        public string AssemblyName { get; set; } = "";
        public string AssemblyVersion { get; set; } = "";
        public bool GooeePluginFound { get; set; }
        public string GooeePluginBaseType { get; set; } = "";
        public bool OnSetupMethodFound { get; set; }
        public string OnSetupSignature { get; set; } = "";
        public bool OnUpdateMethodFound { get; set; }
        public string OnUpdateSignature { get; set; } = "";
        public bool NamePropertyFound { get; set; }
        public int ExportedTypesCount { get; set; }
        public System.Collections.Generic.List<string> RelevantTypes { get; set; } = new();
        public string Message { get; set; } = "";
        public string ErrorDetails { get; set; } = "";
        public DateTime VerifiedAt { get; set; }
    }
}

