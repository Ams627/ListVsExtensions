using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ListVsExtensions
{
    internal class Program
    {
        private static void Main(string[] args)
        {   
            try
            {
                var config = new VS2017Info.Vs2017SetupConfig();
                foreach (var instance in config.VSInstances)
                {
                    Console.WriteLine($"instance ID:{instance.Id}");
                    Console.WriteLine($"installed path:{instance.InstalledPath}");
                    var vsPath = VS2017Info.VS2017AppData.GetLocalAppDataPath(instance.Version, instance.Id);
                    Console.WriteLine($"local appdata path is {vsPath}");

                    var extensions = VS2017Info.VS2017AppData.GetInstalledExtensions(instance.Version, instance.Id);
                    foreach (var ext in extensions)
                    {
                        Console.WriteLine($"{ext.Key} {ext.Value}");
                    }


                    var hivefile = VS2017Info.VS2017AppData.GetPrivateRegFilename(instance.Version, instance.Id);
                    var hKey = RegistryNativeMethods.RegLoadAppKey(hivefile);

                    var keyVersion = VS2017Info.VS2017AppData.GetVersionForRegistryKey(instance.Version, instance.Id);

                    var keypath = $@"Software\Microsoft\VisualStudio\{keyVersion}\ExtensionManager\EnabledExtensions";
                    using (var safeRegistryHandle = new SafeRegistryHandle(new IntPtr(hKey), true))
                    using (var appKey = RegistryKey.FromHandle(safeRegistryHandle))
                    using (var extensionsKey = appKey.OpenSubKey(keypath, true))
                    {
                        if (extensionsKey == null)
                        {
                            throw new ApplicationException("Specified file is not a well-formed software registry hive: " + hivefile);
                        }

                        //var extensions = extensionsKey.GetValueNames().Select(x=>x.Split(',').FirstOrDefault());
                        //foreach (var extGuid in extensions)
                        //{
                        //    var value = extensionsKey.GetValue(extGuid);
                        //    Console.WriteLine($"extension:{extGuid}: {value}");
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                var codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                var progname = Path.GetFileNameWithoutExtension(codeBase);
                Console.Error.WriteLine(progname + ": Error: " + ex.Message);
            }
        }
    }
}
