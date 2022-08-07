namespace Overmind
{
    static class PathUtils
    {
        /// <summary>
        /// Checks whether a given path is sub-path of a given base path, for prevention of directory traversal.
        /// </summary>
        /// <param name="pathToCheck">The path to check. This may be a full path or relative path, and it will be evaluated in the same context as a file open operation.</param>
        /// <param name="basePath">The base path. This is usually a directory path. Relative paths will be evaluated in the same context as a file open operation.</param>
        /// <param name="expandEnvironmentVars">Whether or not to first expand environment variables in the path string. This should normally be set to false if the checked path is to be used in a file open operation within .NET, which does not expand environment variables. This should be set to true if the checked path will be used in a context where environment variables would be automatically expanded, e.g. as part of a shell command.</param>
        /// <returns>True if the checked path is within the base path, otherwise false.</returns>
        public static bool IsSubPath(string pathToCheck, string basePath, bool expandEnvironmentVars)
        {
            // expand environment variables in both paths if we were asked to
            string expandedBasePath = basePath;
            string expandedCheckPath = pathToCheck;
            if (expandEnvironmentVars)
            {
                expandedBasePath = Environment.ExpandEnvironmentVariables(basePath);
                expandedCheckPath = Environment.ExpandEnvironmentVariables(pathToCheck);
            }
            // canonicalise the base path
            string canonBase = Path.GetFullPath(expandedBasePath);
            // ensure that the canonical base path ends with a directory separator character
            // we could just add the char unconditionally but this defends against future API changes
            if (!canonBase.EndsWith(Path.DirectorySeparatorChar))
                canonBase += Path.DirectorySeparatorChar;
            // canonicalise the path being checked
            string canonPath = Path.GetFullPath(expandedCheckPath);
            // note that we do not add a trailing slash here because the check path might be a filename

            // ensure that the resulting canonical path starts with the canonical base path
            // (with trailing slash to avoid neighbouring directory traversal)
            // this is done with case-insensitive ordinal case comparison based on .NET docs
            return canonPath.StartsWith(canonBase, StringComparison.OrdinalIgnoreCase);
        }
    }
}