using System.Reflection;

namespace Protfi.Common.Reflection
{
    public interface IAssemblyScanner
    {
        /// <summary>
        /// Returns all assemblies in application's folders exporting types inheriting from T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>

        IEnumerable<Assembly> GetAssembliesWithType<T>();
        IEnumerable<Assembly> GetAssembliesWithType(Type type);
        IEnumerable<T> CreateInstancesOfType<T>(List<Type> requiredAttributes = null);
        IEnumerable<object> CreateInstancesOfType(Type requestedType, List<Type> requiredAttributes = null);

        T CreateInstanceOfType<T>(Assembly assembly);

        IEnumerable<Type> GetChildrenOfType<T>(params Type[] requiredAttributes);
    }

    public class AssemblyScanner : IAssemblyScanner
    {
        /// <summary>
        /// Returns all assemblies in application's folders exporting types inheriting from T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<Assembly> GetAssembliesWithType<T>()
        {
            return GetAssembliesWithType(typeof(T));
        }

        public IEnumerable<Assembly> GetAssembliesWithType(Type type)
        {
            var assemblies = GetAssembliesInPath();
            foreach (Assembly assembly in assemblies)
            {
                bool containsType = false;
                try
                {
                    containsType = assembly.GetTypes().Any(type.IsAssignableFrom);

                }
                catch (Exception)
                {
                    // continue;
                }

                if (containsType)
                    yield return assembly;

            }
        }

        public IEnumerable<Type> GetChildrenOfType<T>(params Type[] requiredAttributes)
        {
            var assemblies = GetAssembliesInPath();
            var parentType = typeof(T);

            foreach (var assembly in assemblies)
            {
                IEnumerable<Type> types;
                try
                {
                    types = assembly.GetTypes().Where(parentType.IsAssignableFrom);
                    if (requiredAttributes.Any())
                    {
                        types = types.Where(t => t.GetCustomAttributes().Select(attr => attr.GetType()).ContainsAll(requiredAttributes));
                    }

                }
                catch (Exception)
                {
                    continue;
                }

                foreach (var type in types)
                {
                    yield return type;
                }

            }
        }

        public IEnumerable<T> CreateInstancesOfType<T>(List<Type> requiredAttributes = null)
        {
            return CreateInstancesOfType(typeof(T), requiredAttributes).Cast<T>();
        }

        public IEnumerable<object> CreateInstancesOfType(Type requestedType, List<Type> requiredAttributes = null)
        {

            return GetAssembliesInPath()
                .SelectMany(assembly => assembly.GetTypes().Where(type =>
                    requestedType.IsAssignableFrom(type) && type.HasDefaultConstructor() &&
                    TypeHasAnyAttribute(type, requiredAttributes)))
                .Select(type => type.GetConstructor(Type.EmptyTypes).Invoke(null));

        }

        private static bool TypeHasAnyAttribute(Type type, List<Type> requiredAttributes)
        {
            if (requiredAttributes == null || !requiredAttributes.Any())
                return true;

            return type.GetCustomAttributes().Select(attr => attr.GetType()).Intersect(requiredAttributes).Any();
        }

        public T CreateInstanceOfType<T>(Assembly assembly)
        {
            var baseType = typeof(T);
            var types = assembly.GetTypes().Where(baseType.IsAssignableFrom).ToArray();
            if (!types.Any())
            {
                //throw new InvalidOperationException(string.Format("Could not find any implementations of {0} in assembly", baseType.FullName));
                return default(T);
            }
            if (types.Length > 1)
            {
                //throw new InvalidOperationException(string.Format("Could not determine which implementation of {0} to use. {1} implementations found in assembly.", baseType.FullName, types.Length));
                return default(T);
            }

            return (T)Activator.CreateInstance(types.Single());
        }

        private IEnumerable<Assembly> GetAssembliesInPath(string dllPattern)
        {
            var assemblies = GetDllsInPath();
            var dlls = new List<Assembly>();
            foreach (var dll in assemblies)
            {
                Assembly assembly = null;
                try
                {
                    assembly = Assembly.LoadFile(dll);
                    dlls.Add(assembly);
                }
                catch (Exception)
                {

                }
            }
            return dlls;
        }
        private IEnumerable<Assembly> GetAssembliesInPath()
        {
            var assemblies = GetDllsInPath();
            var systemAssemblies = new List<string>();
            foreach (var assemblyPath in assemblies)
            {
                var fileName = Path.GetFileName(assemblyPath);
                if (fileName.ToLower().StartsWith("protfi"))
                {
                    systemAssemblies.Add(assemblyPath);
                }
            }
            var dlls = new List<Assembly>();
            foreach (var dll in systemAssemblies)
            {
                Assembly assembly = null;
                try
                {
                    assembly = Assembly.LoadFrom(dll);
                    dlls.Add(assembly);
                }
                catch (Exception)
                {

                }
            }
            return dlls;
        }

        private static IEnumerable<string> GetDllsInPath()
        {
            // REMOVED NOT SUPPORTED IN LINUX
            //var foldersToScan = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath.EmptyIfNull().Split(';');
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var allFolders = new List<string>() { basePath };
            //allFolders.AddRange(foldersToScan.Select(folder => Path.Combine(basePath, folder)));
            var allDlls = allFolders.SelectMany(folder => Directory.EnumerateFiles(folder, "*.dll"))
                .Union(allFolders.SelectMany(folder => Directory.EnumerateFiles(folder, "*.exe"))).Distinct();

            return allDlls;
        }
    }
}