using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using DiffMatchPatch;
using FluentFTP;

namespace FTPClient.Commands
{
    public static class DefaultCommands
    { 
        public static string Login(string address, string username="")
        {
            string returnMessage = "";
            string password = "";
            if (username == "" | username == "anonymous")
            {
                username = "anonymous";
                password = "anonymous";
            }
            else
            {
                System.Console.Write("Enter the password: ");
                password = FTPClient.Console.Console.ReadPassword();
                System.Console.Write('\n');
            }
            try
            {
                FtpClient client = new FtpClient(address)
                {
                    Credentials = new NetworkCredential(username, password)
                };

                client.Connect();

                if (client.IsConnected)
                {
                    Client.serverName = address;
                    Client.clientObject = client;
                    Client.viewingRemote = true;
                    FTPClient.Console.Console.readPrompt = "FTP ("+ FTPClient.Client.clientObject.GetWorkingDirectory() + ")> ";
                    returnMessage = "Connected to " + address;
                    new SavedConnection(address, username).saveConnection();
                }
            }
            catch (Exception e)
            {
                returnMessage = "Connection failed with Exception:" + e;
            }

            return returnMessage;
        }

        public static string cd(string filePath)
        {
            FTPClient.Client.clientObject.SetWorkingDirectory(filePath);
            FTPClient.Console.Console.readPrompt = "FTP ("+ FTPClient.Client.clientObject.GetWorkingDirectory() + ")> ";
            return "";
        }

        public static string pwd()
        {
            return FTPClient.Client.clientObject.GetWorkingDirectory();
        }

        public static string lr()
        {
            string returnMessage = "";
            string res = "";
            try
            {
                foreach (FtpListItem item in Client.clientObject.GetListing(Client.clientObject.GetWorkingDirectory()))
                {
                    res += item + "\n";
                }
                returnMessage = res;
            }
            catch (Exception e)
            {
                returnMessage = "Server not connected Or Listing failed with Exception:" + e;
            }
            return returnMessage;
        }

        public static string ls()
        {
            string returnMessage = "";
            try
            {
                returnMessage = "";
                var currentdir = System.IO.Directory.GetCurrentDirectory();
                var directories = System.IO.Directory.GetDirectories(currentdir);
                var files = System.IO.Directory.GetFiles(currentdir);
                foreach (string item in directories)
                {
                    var dir = new DirectoryInfo(item);
                    returnMessage += "DIR\t" + dir.Name + "\t Modified :"+dir.LastWriteTime + '\n';
                }
                foreach (string file in files)
                {
                    var time = File.GetLastWriteTime(file);
                    var size = Path.GetFileName(file).Length;
                    returnMessage += "FILE\t"+ Path.GetFileName(file) + "\t("+size +")bytes" + "\t Modified :" + time +'\n';
                }
            }
            catch (Exception e)
            {
                returnMessage = "Listing of local directories and files failed with Exception: " + e;
            }
            return returnMessage;
        }

        //Uses absoulute path for target and name changes
        public static string mv(string target, string name)
        {
            string returnMessage = "";
            string res = "";
            try
            {
                foreach (FtpListItem item in Client.clientObject.GetListing(Client.clientObject.GetWorkingDirectory()))
                {
                    if(item.FullName == target)
                    {
                        Client.clientObject.Rename(target, name);
                    }
                }
                returnMessage = res;
            }
            catch (Exception e)
            {
                returnMessage = "Rename failed with exception: " + e;
            }
            return returnMessage;
        }

        //Uses absolute path for target and name changes
        public static string mvLocal(string target, string name)
        {
            string returnMessage = "";
            try
            {
                var currentdir = System.IO.Directory.GetCurrentDirectory();
                var directories = System.IO.Directory.GetDirectories(currentdir);
                var files = System.IO.Directory.GetFiles(currentdir);

                string qualifiedTarget = currentdir + "\\" + target;
                string qualifiedName = currentdir + "\\" + name;

                foreach (string item in directories)
                {
                    if (item == qualifiedTarget)
                    {
                        Directory.Move(qualifiedTarget, qualifiedName);
                    }
                }
                foreach (string file in files)
                {
                    if (file == qualifiedTarget)
                    {
                        File.Move(qualifiedTarget, qualifiedName);
                    }
                }
            }
            catch (Exception e)
            {
                Console.Console.WriteToConsole(e.ToString());
                returnMessage = "Rename failed with exception";
            }
            return returnMessage;
        }

        public static string upload(string source, string destination)
        {
            string returnMessage = "";
            try
            {
               if(FTPClient.Client.clientObject.UploadFile(source, destination))

                returnMessage = "Upload Successful";
               else
                    returnMessage = "Upload failed";

            }
            catch (Exception e)
            {
                returnMessage = "Upload failed exception" + e;
            }
            return returnMessage;
        }

        public static string download(string source, string destination)
        {
            string returnMessage = "";

            try
            {
               FTPClient.Client.clientObject.DownloadFile(destination, source);
                returnMessage = "Download succesful";

            }
            catch (Exception e)
            {
                returnMessage = "Download failed" + e;
            }
            return returnMessage;
        }


        public static string findl(string filename)
        {
            string returnMessage = "";
            try
            {
                foreach (string file in Directory.EnumerateFiles(System.IO.Directory.GetCurrentDirectory(), "*.*", SearchOption.AllDirectories))
                {
                    if (filename == Path.GetFileName(file))
                    {
                        var time = File.GetLastWriteTime(file);
                        var size = Path.GetFileName(file).Length;
                        returnMessage += "FILE\t" + Path.GetFileName(file) + "\t(" + size + ")bytes" + "\t Modified :" + time + " FilePath:" + Path.GetFullPath(file) +'\n';

                    }
                }
            }
            catch (Exception e)
            {
                returnMessage = "The File does not exist: " + e;
            }

            return returnMessage;
        }

        //find files on server 
        public static string findr(string filename)
        {
            string returnMessage = "";
            try
            {
                foreach (FtpListItem item in Client.clientObject.GetListing(Client.clientObject.GetWorkingDirectory(), FtpListOption.Recursive))
                {
                    if (filename == item.Name)
                    {
                        returnMessage += item + " FilePath:"+ item.FullName +"\n";
                    }
                }
            }
            catch (Exception e)
            {
             
                returnMessage = "Server not connected or Failed with exception" + e;
            }
            return returnMessage;
        }

        //Create directory
        public static string mkdir(string path)
        {
            string returnMessage = "";
            try
            {
                Client.clientObject.CreateDirectory(path);
                Client.clientObject.SetFilePermissions(path, 755);
                returnMessage = "Created directory: " + path;
            }
            catch (Exception e)
            {
                returnMessage = e.Message;
            }
            return returnMessage;
        }

        //disconnect from server 
        public static string exit()
        {
            string returnMessage = "";
            try
            {
                Client.serverName = null;
                Client.clientObject = null;
                FTPClient.Console.Console.readPrompt = "FTP>";
            }
            catch (Exception e)
            {
                returnMessage = "Server not connected or Failed with exception" + e;
            }
            return returnMessage;
        }

        //diff on files on local
        public static string diffl(string file1, string file2)
        {
            string returnMessage = "";
            try
            {
                string text1 = System.IO.File.ReadAllText(@file1);
                string text2 = System.IO.File.ReadAllText(@file2);

                var dmp = DiffMatchPatchModule.Default;
                List<DiffMatchPatch.Diff> diff = dmp.DiffMain(text1, text2);

                // Result: [(-1, "Hell"), (1, "G"), (0, "o"), (1, "odbye"), (0, " World.")]
                dmp.DiffCleanupSemantic(diff);
                // Result: [(-1, "Hello"), (1, "Goodbye"), (0, " World.")]
                for (int i = 0; i < diff.Count; i++)
                {
                    returnMessage += diff[i] + " ";
                }

            }
            catch (Exception e)
            {
                returnMessage = "Server not connected or Failed with exception" + e;
            }
            return returnMessage;
        }

        //Delete directory 
        public static string rmdir(string path)
        {
            string returnMessage = "";
            try 
            {
                Client.clientObject.DeleteDirectory(path);
                returnMessage = "Deleted directory: " + path;
            }
            catch (Exception e)
            {
                returnMessage = e.ToString();
            }
            return returnMessage;
        }

        //Change Permissions
        public static string chmod(string path, int permission)
        {
            string returnMessage = "";
            string npath;
            try
            {
                npath = Path.Combine(Client.clientObject.GetWorkingDirectory(), path);
                Client.clientObject.SetFilePermissions(npath, permission);
                returnMessage = "Permission of file/folder: " + path + " set to :" + permission;
            }
            catch (Exception e)
            {
                returnMessage = e.Message;
            }
            return returnMessage;
        }

        public static string uploadMultiple(string files, string destination)
        {
            try
            {
                List<string> args = FTPClient.Console.Console.SeparateArguments(files);



                int numberOfFiles = FTPClient.Client.clientObject.UploadFiles(args, destination);
                return numberOfFiles + " uploaded.";
            }

            catch (Exception e)
            {
                return "Server not connected or Failed with exception" + e;
            }
        }

        public static string downloadMultiple( string destination, string files)
        {
            try
            {
                List<string> args = FTPClient.Console.Console.SeparateArguments(files);

             

                int numberOfFiles = FTPClient.Client.clientObject.DownloadFiles(destination, args);
                return numberOfFiles + " downloaded.";
            }
            catch(Exception e)
            {
                return "Server not connected or Failed with exception" + e;
            }
        }

        //diff on remote files
        public static string diffr(string file1, string file2)
        {
            string returnMessage = "";
            try
            {
                string name1 = DateTime.Now.ToString("yyyyMMddHHmmssfffff");
                Client.clientObject.DownloadFile(name1, file1);
                string name2 = DateTime.Now.ToString("yyyyMMddHHmmssfffff");
                Client.clientObject.DownloadFile(name2, file2);
                
                string text1 = System.IO.File.ReadAllText(name1);
                string text2 = System.IO.File.ReadAllText(name2);

                var dmp = DiffMatchPatchModule.Default;
                List<DiffMatchPatch.Diff> diff = dmp.DiffMain(text1, text2);

                // Result: [(-1, "Hell"), (1, "G"), (0, "o"), (1, "odbye"), (0, " World.")]
                dmp.DiffCleanupSemantic(diff);
                // Result: [(-1, "Hello"), (1, "Goodbye"), (0, " World.")]
                for (int i = 0; i < diff.Count; i++)
                {
                    returnMessage += diff[i] + " ";
                }

                File.Delete(name1);
                File.Delete(name2);
            }
            catch (Exception e)
            {
                returnMessage = "Server not connected or Failed with exception" + e;
            }
            return returnMessage;
        }

        //diff on file on remote and local
        public static string diff(string file1, string file2)
        {
            string returnMessage = "";
            try
            {
                string name1 = DateTime.Now.ToString("yyyyMMddHHmmssfffff");
                Client.clientObject.DownloadFile(name1, file1);

                string text1 = System.IO.File.ReadAllText(name1);
                string text2 = System.IO.File.ReadAllText(file2);

                var dmp = DiffMatchPatchModule.Default;
                List<DiffMatchPatch.Diff> diff = dmp.DiffMain(text1, text2);

                // Result: [(-1, "Hell"), (1, "G"), (0, "o"), (1, "odbye"), (0, " World.")]
                dmp.DiffCleanupSemantic(diff);
                // Result: [(-1, "Hello"), (1, "Goodbye"), (0, " World.")]
                for (int i = 0; i < diff.Count; i++)
                {
                    returnMessage += diff[i] + " ";
                }

                File.Delete(name1);
            }
            catch (Exception e)
            {
                returnMessage = "Server not connected or Failed with exception" + e;
            }
            return returnMessage;
        }

        //Prints history of command usage
        public static string history()
        {
            string readText = File.ReadAllText("./history.txt");
            return readText;
        }

        //Deletes file from remote server
        public static string rmr(string fileToDelete)
        {
            string returnMessage = "";
            try
            {
                Client.clientObject.DeleteFile(fileToDelete);
                returnMessage = "Deleted file: " + fileToDelete;
            }
            catch (Exception e)
            {
                returnMessage = e.Message;
            }

            return returnMessage;
        }
      
        public static string copyDir(string source)
         {
            string returnMessage = "";

            try
            {
                foreach (string file in Directory.EnumerateFiles(Path.GetFullPath(source), "*.*", SearchOption.AllDirectories))
                {

                    string relativePath = Path.GetRelativePath(source, file);
                    System.Console.WriteLine("Copying  "+ file);
                    FTPClient.Client.clientObject.UploadFile(file, relativePath, createRemoteDir: true);
                }
                returnMessage = "Copy succesful";

            }
            catch (Exception e)
            {
                System.Console.WriteLine("Exception " + e);
            }
             return returnMessage;
        }

        public static string help()
        {
            string returnMessage = "";
            string helpstr = "Login <ftpserver> <username> when prompted <password>                        --  To login into ftpserver using the username and password\n" +
            "cd  <directory name>                                                         --  To change directory on remote\n" +
            "pwd                                                                          --  To display path of working directory\n" +
            "ls                                                                           --  To display files and directories in current directory of local\n" +
            "lr                                                                           --  To display files and directories in current directory of remote\n" +
            "findl <filename>                                                             --  To find a file with given filename on local \n" +
            "findr <filename>                                                             --  To find a file with given filename on remote \n" +
            "mvLocal <currentfile> <newfile>                                              --  Takes newfile name and replaces currentifle with it - this is for local\n" +
            "mv <currentfile> <newfile>                                                   --  Takes newfile name and replaces currentfile with it - this is for remote\n" +
            "mkdir <path>                                                                 --  Creates a directory on the given path\n" +
            "rm <path>                                                                    --  Delete a directory on the given path\n" +
            "chmod <path> <permissions>                                                   --  Sets permissions       \n" +
            "upload <local path with filename> <remote path with filename>                --  Uploads a file from local to remote with the name as filename\n" +
            "download <local path with filename> <remote path with filename>              --  Downloads a file from remote to local with the name as filename\n" +
            "uploadMultiple <\"file1 file2 'filename with spaces' file4\"> <destination>    --  Uploads multiple files given as args on remote at the destination location provided by user\n" +
            "downloadMultiple <destination> <\"file1 file2 'filename with spaces' file4\">  --  Downloads multiple files given as args from remote, to the destination location on local\n" +
            "copyDir <source path>                                                        --  Copies the directory from the provided source path to current working directory on remote \n" +
            "diffl <filepath1> <filepath2>                                                --  Difference of files on local\n" +
            "diffr <filepath1> <filepath2>                                                --  Difference of files on remote\n" +
            "diff  <filepath1> <filepath2>                                                --  Difference of files on remote and local\n" +
            "history                                                                      --  Prints history of command lines\n" +
            "rmr <filepath>                                                               --  Removes file at filepath location\n" +
            "exit                                                                         --  To disconnect from server\n";
            try
            {
                System.Console.WriteLine(@helpstr);
            }
            catch (Exception e)
            {
                returnMessage = e.ToString();
            }
            return returnMessage;
        }

        public static string saved()
        {
            List<SavedConnection> savedConns = SavedConnection.getSavedConnections();

            if (savedConns.Count > 0)
            {
                int counter = 1;
                foreach (var connection in savedConns)
                {
                    System.Console.WriteLine("[" + counter + "] " + connection.ToString());
                    counter += 1;
                }

                System.Console.Write("Enter the saved connection you want to use: ");
                int index = Convert.ToInt32(System.Console.ReadLine());
                System.Console.WriteLine("Logging into: " + savedConns[index-1]);
                return Login(savedConns[index - 1].address, savedConns[index - 1].username);
            }
            else
            {
                return "There is no saved information. Please use the Login command to connect to a server.";
            }
        }
    }
}