using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Hosting;

namespace MapAccounts.Helpers
{
    public static class PathMap
    {
        /// <summary>
        /// Helper function to load a local path to a file
        /// Ex: PathMap.MapPath(@"~/FolderInRoot/File.ext);
        /// </summary>
        /// <param name="seedFile"></param>
        /// <returns></returns>
        public static string MapPath(string seedFile)
        {
            if (HttpContext.Current != null)
                return HostingEnvironment.MapPath(seedFile);

            var absolutePath = new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath;
            var directoryName = Path.GetDirectoryName(absolutePath);
            var path = Path.Combine(directoryName, ".." + seedFile.TrimStart('~').Replace('/', '\\'));

            return path;
        }
    }
}