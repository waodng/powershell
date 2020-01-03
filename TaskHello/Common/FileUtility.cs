using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

/* ==============================================================================
 * 创建日期：2020/1/3 0:48:24
 * 创 建 者：wgd
 * 功能描述：FileUtility  
 * ==============================================================================*/
namespace PublishTask.Common
{
    /// <summary>
    /// Indicates how to proceed with the move file operation. 
    /// </summary>
    [Flags]
    public enum MoveFileFlag : int
    {
        /// <summary>
        /// Perform a default move funtion.
        /// </summary>
        None                = 0x00000000,
        ///// <summary>
        /// If the target file exists, the move function will replace it.
        /// </summary>
        ReplaceExisting     = 0x00000001,
        //// <summary>
        /// If the file is to be moved to a different volume, 
        /// the function simulates the move by using the CopyFile and DeleteFile functions. 
        /// </summary>
        CopyAllowed         = 0x00000002,
        //// <summary>
        /// The system does not move the file until the operating system is restarted. 
        /// The system moves the file immediately after AUTOCHK is executed, but before 
        /// creating any paging files. Consequently, this parameter enables the function 
        /// to delete paging files from previous startups. 
        /// </summary>
        DelayUntilReboot    = 0x00000004,
        //// <summary>
        /// The function does not return until the file has actually been moved on the disk. 
        /// </summary>
        WriteThrough        = 0x00000008,
        //// <summary>
        /// Reserved for future use.
        /// </summary>
        CreateHardLink      = 0x00000010,
        //// <summary>
        /// The function fails if the source file is a link source, but the file cannot be tracked after the move. This situation can occur if the destination is a volume formatted with the FAT file system.
        /// </summary>
        FailIfNotTrackable    = 0x00000020,
    }

    //// <summary>
    /// Provides certain utilities used by configuration processors, such as correcting file paths.
    /// </summary>
    public sealed class FileUtility
    {
        #region Constructor

        //// <summary>
        /// Default constructor.
        /// </summary>
        private FileUtility()
        {
        }

        #endregion

       #region Public members

        //// <summary>
        /// Returns whether the path is a UNC path.
        /// </summary>
        /// <param name="path">The path string.</param>
        /// <returns><c>true</c> if the path is a UNC path.</returns>
        public static bool IsUncPath( string path )
        {
            //  FIRST, check if this is a URL or a UNC path; do this by attempting to construct uri object from it
            Uri url = new Uri( path );

            if( url.IsUnc )
            {
                //  it is a unc path, return true
                return true;
            }
            else
            {
                return false;
            }
        }

        //// <summary>
        /// Takes a UNC or URL path, determines which it is (NOT hardened against bad strings, assumes one or the other is present)
        /// and returns the path with correct trailing slash: backslash for UNC or
        /// slash mark for URL.
        /// </summary>
        /// <param name="path">The URL or UNC string.</param>
        /// <returns>Path with correct terminal slash.</returns>
        public static string AppendSlashUrlOrUnc( string path )
        {                    
            if( IsUncPath( path ) )
            {
                //  it is a unc path, so decorate the end with a back-slash (to correct misconfigurations, defend against trivial errors)
                return AppendTerminalBackslash( path );
            }
            else
            {
                //  assume URL here
                return AppendTerminalForwardSlash( path );
            }
        }

        //// <summary>
        /// If not present appends terminal backslash to paths.
        /// </summary>
        /// <param name="path">A path string; for example, "C:\AppUpdaterClient".</param>
        /// <returns>A path string with trailing backslash; for example, "C:\AppUpdaterClient\".</returns>
        public static string AppendTerminalBackslash( string path )
        {
            if( path.IndexOf( Path.DirectorySeparatorChar, path.Length - 1 ) == -1 )
            {
                return path + Path.DirectorySeparatorChar;
            }
            else
            {
                return path;
            }
        }

        //// <summary>
        /// Appends a terminal slash mark if there is not already one; returns corrected path.
        /// </summary>
        /// <param name="path">The path that may be missing a terminal slash mark.</param>
        /// <returns>The corrected path with terminal slash mark.</returns>
        public static string AppendTerminalForwardSlash( string path )
        {
            if( path.IndexOf( Path.AltDirectorySeparatorChar, path.Length - 1 ) == -1 )
            {
                return path + Path.AltDirectorySeparatorChar;
            }
            else
            {
                return path;
            }
        }

        //// <summary>
        /// Creates a new temporary folder under the system temp folder
        /// and returns its full pathname.
        /// </summary>
        /// <returns>The full temp path string.</returns>
        public static string CreateTemporaryFolder()
        {
            return Path.Combine( Path.GetTempPath(), Path.GetFileNameWithoutExtension( Path.GetTempFileName() ) );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public static void CreateIfNotExists(string path)
        {
            if (!File.Exists(path))
            {
                File.Create(path);
            }
        }

        

        //// <summary>
        /// Copies files from the source to destination directories. Directory.Move is not 
        /// suitable here because the downloader may still have the temporary 
        /// directory locked. 
        /// </summary>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="destinationPath">The destination path.</param>
        public static void CopyTo(string sourcePath, string destinationPath)
        {
            CopyDirectory( sourcePath, destinationPath, true );
        }

        //// <summary>
        /// Copies files from the source to destination directories. Directory.Move is not 
        /// suitable here because the downloader may still have the temporary 
        /// directory locked. 
        /// </summary>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="destinationPath">The destination path.</param>
        /// <param name="overwrite">Indicates whether the destination files should be overwritten.</param>
        public static void CopyDirectory( string sourcePath, string destinationPath, bool overwrite )
        {
            CopyDirRecurse( sourcePath, destinationPath, destinationPath, overwrite );
        }

        //// <summary>
        /// Move a file from a folder to a new one.
        /// </summary>
        /// <param name="existingFileName">The original file name.</param>
        /// <param name="newFileName">The new file name.</param>
        /// <param name="flags">Flags about how to move the files.</param>
        /// <returns>indicates whether the file was moved.</returns>
        public static bool MoveFile( string existingFileName, string newFileName, MoveFileFlag flags)
        {
            return MoveFileEx( existingFileName, newFileName, (int)flags );
        }

        //// <summary>
        /// Deletes a folder. If the folder cannot be deleted at the time this method is called,
        /// the deletion operation is delayed until the next system boot.
        /// </summary>
        /// <param name="folderPath">The directory to be removed</param>
        public static void DestroyFolder( string folderPath )
        {
            try
            {
                if ( Directory.Exists( folderPath) )
                {
                    Directory.Delete( folderPath, true );
                }
            }
            catch( Exception )
            {
                // If we couldn't remove the files, postpone it to the next system reboot
                if ( Directory.Exists( folderPath) )
                {
                    FileUtility.MoveFile(
                        folderPath,
                        null,
                        MoveFileFlag.DelayUntilReboot );
                }
            }
        }

        //// <summary>
        /// Deletes a file. If the file cannot be deleted at the time this method is called,
        /// the deletion operation is delayed until the next system boot.
        /// </summary>
        /// <param name="filePath">The file to be removed</param>
        public static void DestroyFile( string filePath )
        {
            try
            {
                if ( File.Exists( filePath ) )
                {
                    File.Delete( filePath );
                }
            }
            catch
            {
                if ( File.Exists( filePath ) )
                {
                    FileUtility.MoveFile(
                        filePath,
                        null,
                        MoveFileFlag.DelayUntilReboot );
                }
            }
        }

        //// <summary>
        /// Clear up a folder. Delete all sub folders and files in the folder.
        /// </summary>
        /// <param name="folderPath">The directory to be cleared up</param>
        public static void ClearUpFolder( string folderPath )
        {
            // Delete all sub folders
            string[] dirs = Directory.GetDirectories( folderPath );
            for (int i = 0; i < dirs.Length; i++)
            {
                DestroyFolder( dirs[i] );
            }

            // Delete all files
            string[] files = Directory.GetFiles( folderPath );
            for (int i = 0; i < files.Length; i++)
            {
                DestroyFile( files[i] );
            }
        }


        //// <summary>
        /// Returns the path to the newer version of the .NET Framework installed on the system.
        /// </summary>
        /// <returns>A string containig the full path to the newer .Net Framework location</returns>
        public static string GetLatestDotNetFrameworkPath()
        {
            Version latestVersion = null;
            string fwkPath = Path.GetFullPath( Path.Combine( Environment.SystemDirectory, @"..\Microsoft.NET\Framework" ) );
            foreach(string path in Directory.GetDirectories( fwkPath, "v*" ) )
            {
                string candidateVersion = Path.GetFileName( path ).TrimStart( 'v' );
                try
                {
                    Version curVersion = new Version( candidateVersion );
                    if ( latestVersion == null || ( latestVersion != null && latestVersion < curVersion ) )
                    {
                        latestVersion = curVersion;
                    }
                }
                catch {}
            }

            return  Path.Combine( fwkPath, "v" + latestVersion.ToString() );
        }

        #endregion

      #region Private members

        //// <summary>
        /// API declaration of the Win32 function.
        /// </summary>
        /// <param name="lpExistingFileName">Existing file path.</param>
        /// <param name="lpNewFileName">The file path.</param>
        /// <param name="dwFlags">Move file flags.</param>
        /// <returns>Whether the file was moved or not.</returns>
        [DllImport("KERNEL32.DLL")]
        private static extern bool MoveFileEx( 
            string lpExistingFileName, 
            string lpNewFileName, 
            long dwFlags );

        //// <summary>
        /// Utility function that recursively copies directories and files.
        /// Again, we could use Directory.Move but we need to preserve the original.
        /// </summary>
        /// <param name="sourcePath">The source path to copy.</param>
        /// <param name="destinationPath">The destination path to copy to.</param>
        /// <param name="originalDestination">The original dstination path.</param>
        /// <param name="overwrite">Whether the folders should be copied recursively.</param>
        private static void CopyDirRecurse( string sourcePath, string destinationPath, string originalDestination, bool overwrite )
        {
            //  ensure terminal backslash
            sourcePath = FileUtility.AppendTerminalBackslash( sourcePath );
            destinationPath = FileUtility.AppendTerminalBackslash( destinationPath );

            if ( !Directory.Exists( destinationPath ) )
            {
                Directory.CreateDirectory( destinationPath );
            }

            //  get dir info which may be file or dir info object
            DirectoryInfo dirInfo = new DirectoryInfo( sourcePath );

            string destFileName = null;

            foreach( FileSystemInfo fsi in dirInfo.GetFileSystemInfos() )
            {
                if ( fsi is FileInfo )
                {
                    destFileName = Path.Combine( destinationPath, fsi.Name );

                    //  if file object just copy when overwrite is allowed
                    if ( File.Exists( destFileName ) )
                    {
                        if ( overwrite )
                        {
                            File.Copy( fsi.FullName, destFileName, true );
                        }
                    }
                    else
                    {
                        File.Copy( fsi.FullName, destFileName );
                    }
                }
                else
                {
                    // avoid this recursion path, otherwise copying directories as child directories
                    // would be an endless recursion (up to an stack-overflow exception).
                    if ( fsi.FullName != originalDestination )
                    {
                        //  must be a directory, create destination sub-folder and recurse to copy files
                        //Directory.CreateDirectory( destinationPath + fsi.Name );
                        CopyDirRecurse( fsi.FullName, destinationPath + fsi.Name, originalDestination, overwrite );
                    }
                }
            }
        }
        #endregion
    }
}
