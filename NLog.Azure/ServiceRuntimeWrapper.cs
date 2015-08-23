using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NLog.Common;

namespace NLog.Azure
{
    internal class ServiceRuntimeWrapper
    {
        private readonly string[] _serviceRuntimeAssemblyNames = {
            // It does not specify a version for backwards compatibility. Behavior is confirmed in only Azure SDK 2.7
            "Microsoft.WindowsAzure.ServiceRuntime, Culture=neutral, PublicKeyToken=31bf3856ad364e35, ProcessorArchitecture=MSIL"
        };

        private const string RoleEnvironmentTypeName = "Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment";
        private const string LocalResourceTypeName = "Microsoft.WindowsAzure.ServiceRuntime.LocalResource";
        private const string RoleEnvironmentExceptionTypeName = "Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironmentException";
        private const string IsAvailablePropertyName = "IsAvailable";
        private const string GetLocalResource = "GetLocalResource";
        private const string RootPathPropertyName = "RootPath";
        private readonly Type _roleEnvironmentExceptionType;
        private readonly Type _localResourceType;
        private readonly MethodInfo _getLocalResource;


        internal ServiceRuntimeWrapper()
        {
            var serviceRuntimeAssembly = GetServiceRuntimeAssembly();

            var type = serviceRuntimeAssembly?.GetType(RoleEnvironmentTypeName, false);
            if (type == null)
                return;

            _localResourceType = serviceRuntimeAssembly.GetType(LocalResourceTypeName, false);
            if (_localResourceType == null)
                return;

            _roleEnvironmentExceptionType = serviceRuntimeAssembly.GetType(RoleEnvironmentExceptionTypeName, false);
            if (_roleEnvironmentExceptionType == null)
                return;

            var property = type.GetProperty(IsAvailablePropertyName);
            try
            {
                IsAvailable = property != null && (bool)property.GetValue(null, null);

                if (!IsAvailable) return;

                _getLocalResource = type.GetMethod(GetLocalResource, BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod);

                InternalLogger.Debug($"Load was successful {serviceRuntimeAssembly.FullName}");
            }
            catch (TargetInvocationException ex)
            {
                if (!(ex.InnerException is TypeInitializationException))
                    throw;
            }
        }

        internal bool IsAvailable { get; }

        internal string GetLocalResouceRootPath(string name)
        {
            var ret = (string)null;
            if (_getLocalResource != null)
            {
                try
                {
                    var localResouce = _getLocalResource.Invoke(null, new object[]
                    {
                        name
                    });
                    if (localResouce != null)
                    {
                        var property = _localResourceType.GetProperty(RootPathPropertyName);
                        ret =  (string) property?.GetValue(localResouce, null);
                    }
 
                }
                catch (TargetInvocationException ex)
                {
                    var type = ex.InnerException.GetType();

                    if((type != _roleEnvironmentExceptionType) || type.IsSubclassOf(_roleEnvironmentExceptionType))
                        throw;
                   else
                        InternalLogger.Error($"maybe resource not found. GetLocalResource(\"{name}\"): {ex.InnerException.GetType()} {ex.InnerException.Message}");
                }
            }
            return ret;
        }
 

        private Assembly GetServiceRuntimeAssembly()
        {
            var assembly = (Assembly) null;

            foreach (var assemblyPath in _serviceRuntimeAssemblyNames.Select(FusionAPI.GetAssemblyPath))
            {
                try
                {
                    if (!string.IsNullOrEmpty(assemblyPath))
                        assembly = Assembly.LoadFrom(assemblyPath);
                }
                catch (Exception ex)
                {
                    if (ex is FileNotFoundException) continue;
                    if (ex is FileLoadException) continue;
                    if (ex is BadImageFormatException) continue;
                    throw;
                }
            }
            return assembly;
        }
    }
}
