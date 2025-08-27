
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Xml.Linq;
using Datagrams;
using Newtonsoft.Json;

namespace DITch
{
    class ByteArrayComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[]? x, byte[]? y)
        {
            if (x == y) return true;
            if (x == null || y == null) return false;
            if (x.Length != y.Length) return false;

            return x.SequenceEqual(y);
        }

        public int GetHashCode(byte[] obj)
        {
            ArgumentNullException.ThrowIfNull(obj);

            unchecked
            {
                int hash = 17;
                foreach (var b in obj)
                    hash = hash * 31 + b;
                return hash;
            }
        }
    }

    public class DITch
    {

        public static Dictionary<byte[], string> CheckForChange(FileInfo[] files, List<byte[]> previousHashes, HashingTool hashingTool)
        {
            var newFiles = new Dictionary<byte[], string>(new ByteArrayComparer());

            foreach (FileInfo fInfo in files)
            {
                try
                {
                    byte[] hashValue = hashingTool.Hash(File.ReadAllBytes(fInfo.FullName));

                    // Check if this hash is not already in previousHashes
                    if (!previousHashes.Any(existing => hashValue.SequenceEqual(existing)))
                    {
                        Console.WriteLine($"File {fInfo.Name}/{HashingTool.ToBase64Url(hashValue)} has been added or changed");

                        newFiles[hashValue] = fInfo.FullName;
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine($"I/O Exception: {e.Message}");
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine($"Access Exception: {e.Message}");
                }
            }

            return newFiles;
        }

        public static Dictionary<byte[], string> CheckForDeletions(FileInfo[] files, List<byte[]> previous_hashes, HashingTool hashingTool)
        {
            // Use a custom comparer so byte[] keys compare by content
            Dictionary<byte[], string> newHashes = [];

            foreach (FileInfo fInfo in files)
            {
                try
                {
                    byte[] hashValue = hashingTool.Hash(File.ReadAllBytes(fInfo.FullName));
                    newHashes[hashValue] = fInfo.FullName;
                }
                catch (IOException e)
                {
                    Console.WriteLine($"I/O Exception: {e.Message}");
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine($"Access Exception: {e.Message}");
                }
            }

            // Now find hashes in previous_hashes that are NOT in newHashes
            Dictionary<byte[], string> removed_hashes = [];

            foreach (var oldHash in previous_hashes) if (!newHashes.ContainsKey(oldHash)) removed_hashes.Remove(oldHash);

            return removed_hashes;
        }

        public static void Main(string[] args)
        {
            FileManager fileManager = new FileManager();
            HashingTool hashingTool = new HashingTool(true);
            NetworkManager networkManager;

            string path = "";

            bool running = true;

            string? branch = null;

            bool success;

            //Populate the above from the settings file

            byte[] data = fileManager.ReadEncryptedFile(path + "\\..\\.dit\\settings.secure");

            byte[] decrypted_data = fileManager.Decrypt(data, "David"); //just use this for now

            var settings_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path + "\\..\\.dit\\settings.secure"));

            int server_port = int.Parse(settings_dict["Server_Port"]);
            string server_address = settings_dict["Server_Address"];
            int communication_protocol = int.Parse(settings_dict["Protocol"]);
            string password = settings_dict["User_Password"];
            string user_ID = settings_dict["User_ID"];
            string user_password = settings_dict["User_Password"];
            string repository_master_password = settings_dict["Repository_Master_Password"];

            new NetworkManager(server_address, (uint)server_port, server_address, (uint)server_port, communication_protocol);

            while (running)
            {
                Console.WriteLine("\n\nDIT : DITch Git, store it with DIT\n");
                Console.WriteLine($"Server: {server_address}:{server_port}");
                Console.WriteLine($"Repository: {path}\n");
                Console.WriteLine($"Branch: {branch ?? "HEAD"}");
                Console.WriteLine("Please select from the following options:\n");
                Console.WriteLine("1. Select Repo"); //done
                Console.WriteLine("2. Connect to server"); 
                Console.WriteLine("3. Make a commit"); //done
                Console.WriteLine("4. Push to server");
                Console.WriteLine("5. Pull from server");
                Console.WriteLine("6. Sync with server");
                Console.WriteLine("7. Initialise new repo"); //done
                Console.WriteLine("8. Remove DIT from a repo");
                Console.WriteLine("9. Quit\n"); //done
                Console.WriteLine("10. Test NetworkManager\n"); //done
                Console.WriteLine("11. Create a new branch\n"); //done
                Console.WriteLine("12. Switch to branch\n"); //done
                Console.WriteLine("13. Merge branches\n");
                Console.Write("> ");
                string? input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        Console.Write("New repo path: ");
                        string? new_path = Console.ReadLine();
                        if (new_path == null)
                        {
                            Console.WriteLine("User gave empty path");
                            break;
                        }
                        if (!Directory.Exists(new_path + "\\..\\.dit"))
                        {
                            Console.WriteLine($"DIT repo at {new_path} could not be found, try initialising this location instead");
                            break;
                        } else
                        {
                            path = new_path;
                        }
                        break;

                    case "2":
                        Console.Write("Server address: ");
                        string? new_address = Console.ReadLine();
                        server_address = new_address ?? "";
                        Console.Write("Server port: ");
                        server_port = Convert.ToInt32(Console.ReadLine());
                        Console.Write("0 - UDP, 1 - TCP: ");
                        string? protocol = Console.ReadLine();
                        int protocol_choice = protocol == "0" ? 0 : 1;
                        Console.WriteLine($"New server: {server_address}:{server_port}");
                        networkManager = new NetworkManager(server_address, (uint)server_port, server_address, (uint)server_port, protocol_choice);

                        break;

                    case "3":
                        string path_to_head;
                        if (branch is null)
                        {
                            path_to_head = "\\..\\.dit\\refs\\heads";
                        } else
                        {
                            path_to_head = "\\..\\.dit\\refs\\heads\\" + branch;
                        }
                        //check repo has a commit
                        if (Directory.GetFiles(path + path_to_head, "*.xml", System.IO.SearchOption.TopDirectoryOnly).Length < 1)
                        {
                            Console.WriteLine("Repo has no known commits, repo likely requires initialisation");
                            break;
                        } 

                        Console.Write("Commit message: ");
                        string? message = Console.ReadLine();

                        DirectoryInfo dir = new DirectoryInfo(path);
                        FileInfo[] files = dir.GetFiles();
                        //Get the repos most recent snapshot
                        //pull all the repo hashes off this via GetAllFileHashes()

                        XElement xml;
                        if (branch is null)
                        {
                            xml = XElement.Load(Directory.GetFiles(path + "\\..\\.dit\\refs\\heads", "*.xml", System.IO.SearchOption.TopDirectoryOnly)[0]);
                        }
                        else
                        {
                            xml = XElement.Load(Directory.GetFiles(path + "\\..\\.dit\\refs\\heads\\" + branch, "*.xml", System.IO.SearchOption.TopDirectoryOnly)[0]);
                        }
                        CommitSnapshot previous_commit = CommitSnapshot.SnapshotFromXML(xml);
                        List<byte[]> previous_commit_hashes = previous_commit.GetAllFileHashes();

                        //put this in to the change checkers to get back who's new and who's gone

                        Dictionary<byte[], string> deleted_files = CheckForDeletions(files, previous_commit_hashes, hashingTool);
                        Dictionary<byte[], string> added_files = CheckForChange(files, previous_commit_hashes, hashingTool);

                        //construct commit from this by:
                        //index the new/changed files

                        byte[] commit_file_data = [];

                        foreach (var filePath in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                        {
                            byte[] hash_name = hashingTool.Hash(File.ReadAllBytes(filePath));
                            byte[] compressed_file = fileManager.CompressFile(filePath);

                            byte[] newData = new byte[commit_file_data.Length + compressed_file.Length];
                            Buffer.BlockCopy(commit_file_data, 0, newData, 0, commit_file_data.Length);
                            Buffer.BlockCopy(compressed_file, 0, newData, commit_file_data.Length, compressed_file.Length);
                            commit_file_data = newData;

                            if (added_files.ContainsValue(filePath))
                            {
                                byte[] encrypted_compressed_file = fileManager.Encrypt(compressed_file, user_password);
                                success = fileManager.WriteEncryptedFile(encrypted_compressed_file, path + "\\..\\.dit\\objects\\" + HashingTool.ToBase64Url(hash_name) + ".secure");

                                string meta_data = fileManager.GetMetaData(filePath);
                                success = fileManager.SaveMetaDataFile(meta_data, path + "\\..\\.dit\\index\\", hash_name);
                            }
                        }

                        //then we have all the details for each file and what to do with it
                        //sequentially construct the commit object from the changes, not the whole repo

                        byte[] commit_hash = hashingTool.Hash(commit_file_data);
                        DateTime dateTime = DateTime.Now;
                        CommitSnapshot commit_snapshot = new CommitSnapshot(message, commit_hash, dateTime);
                        commit_snapshot.SetCommitSnapshotFileTree(commit_snapshot.BuildFileTreeFromFolder(path, hashingTool));
                        XElement xml_data = commit_snapshot.GenerateXML();
                        xml_data.Save(path + "\\..\\.dit\\objects\\" + HashingTool.ToBase64Url(commit_hash) + ".xml");

                        fileManager.CopyFile(@path + "\\..\\.dit\\objects\\" + HashingTool.ToBase64Url(commit_hash) + ".xml", @path + "\\..\\.dit\\refs\\heads\\" + HashingTool.ToBase64Url(commit_hash) + ".xml");

                        //boom
                        break;

                    case "7": //Initialise new repo
                        Console.Write("New repo path: ");
                        string? new_repo_path = Console.ReadLine();
                        if (new_repo_path == null) {
                            Console.WriteLine("User gave empty path");
                            break;
                        }
                        if (Directory.Exists(new_repo_path + "\\..\\.dit"))
                        {
                            Console.WriteLine($"Repo {new_repo_path} has already been initialised, Select the repo instead");
                            break;
                        }

                        DirectoryInfo ret = Directory.CreateDirectory(new_repo_path + "\\..\\.dit");
                        Directory.CreateDirectory(new_repo_path + "\\..\\.dit\\index");
                        Directory.CreateDirectory(new_repo_path + "\\..\\.dit\\objects");
                        Directory.CreateDirectory(new_repo_path + "\\..\\.dit\\refs");
                        Directory.CreateDirectory(new_repo_path + "\\..\\.dit\\refs\\heads");

                        Console.WriteLine($"Created .dit for new repo at {ret.FullName}");
                        path = new_repo_path;

                        string? commit_message = "Initial Commit";
                        if (commit_message == null) break;

                        byte[] file_data = [];

                        foreach (var filePath in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                        {
                            byte[] hash_name = hashingTool.Hash(File.ReadAllBytes(filePath));
                            byte[] compressed_file = fileManager.CompressFile(filePath);

                            byte[] newData = new byte[file_data.Length + compressed_file.Length];
                            Buffer.BlockCopy(file_data, 0, newData, 0, file_data.Length);
                            Buffer.BlockCopy(compressed_file, 0, newData, file_data.Length, compressed_file.Length);
                            file_data = newData;

                            byte[] encrypted_compressed_file = fileManager.Encrypt(compressed_file, user_password);
                            success = fileManager.WriteEncryptedFile(encrypted_compressed_file, path + "\\..\\.dit\\objects\\" + HashingTool.ToBase64Url(hash_name) + ".secure");

                            if (success)
                            {
                                Console.WriteLine($"file {filePath} saved successfully");
                            } else
                            {
                                Console.WriteLine($"file {filePath} failed to save");
                            }

                            string meta_data = fileManager.GetMetaData(filePath);
                            success = fileManager.SaveMetaDataFile(meta_data, path + "\\..\\.dit\\index\\", hash_name);
                        }

                        byte[] init_hash = hashingTool.Hash(file_data);
                        CommitSnapshot init_snapshot = new CommitSnapshot(commit_message, init_hash, DateTime.Now);
                        init_snapshot.SetCommitSnapshotFileTree(init_snapshot.BuildFileTreeFromFolder(path, hashingTool));
                        XElement init_xml_data = init_snapshot.GenerateXML();
                        init_xml_data.Save(path + "\\..\\.dit\\objects\\" + HashingTool.ToBase64Url(init_hash) + ".xml");

                        DirectoryInfo dInfo = new DirectoryInfo(@path + "\\..\\.dit\\refs\\heads");
                        foreach (FileInfo finfo in dInfo.GetFiles())
                        {
                            File.Delete(finfo.FullName);
                        }
                        fileManager.CopyFile(@path + "\\..\\.dit\\objects\\" + HashingTool.ToBase64Url(init_hash) + ".xml", @path + "\\..\\.dit\\refs\\heads\\" + HashingTool.ToBase64Url(init_hash) + ".xml");

                        //settings JSON needs creating in the code
                        //then encrypted and the original deleted
                        //then tell the user it exists and if they want to change it - option 13

                        var dict = new Dictionary<string, string>
                            {
                            { "Server_Port", "8000" },
                            { "Server_Address", "127.0.0.1" },
                            { "Protocol", "UDP" },
                            { "User_ID", "" },
                            { "User_Password", "" },
                            { "Repository_Master_Password", "" }
                            };

                        byte[] encrypted_settings_file = fileManager.Encrypt(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dict, Formatting.Indented)), user_password);
                        fileManager.WriteEncryptedFile(encrypted_settings_file, path + "\\..\\.dit\\Settings.secure");

                        Console.WriteLine("A default settings file has been created, you will need to fill it in in order to access more complex DITch features (Option 13)");

                        break;
                    case "9": //quit
                        running = false;

                        break;
                    case "10":
                        /*networkManager = new NetworkManager("127.0.0.1", 8000, 0);
                        success = networkManager.ConnectToServer();
                        success = networkManager.SendData(0x1111, null);

                        Payload0x1222 response = (Payload0x1222)networkManager.ReceiveData();

                        UInt32 new_port = response.port_number;
                        networkManager.SetPort(new_port);

                        success = networkManager.SendData(0x1111, null);*/
                        break;
                    case "11": //make a new branch
                        DirectoryInfo _dir = new DirectoryInfo(path);
                        FileInfo[] _files = _dir.GetFiles();
                        //Get the repos most recent snapshot
                        //pull all the repo hashes off this via GetAllFileHashes()

                        XElement _xml;
                        if (branch is null)
                        {
                            _xml = XElement.Load(Directory.GetFiles(path + "\\..\\.dit\\refs\\heads", "*.xml", System.IO.SearchOption.TopDirectoryOnly)[0]);
                        } else
                        {
                            _xml = XElement.Load(Directory.GetFiles(path + "\\..\\.dit\\refs\\heads\\" + branch, "*.xml", System.IO.SearchOption.TopDirectoryOnly)[0]);
                        }
                        CommitSnapshot _previous_commit = CommitSnapshot.SnapshotFromXML(_xml);
                        List<byte[]> _previous_commit_hashes = _previous_commit.GetAllFileHashes();

                        //put this in to the change checkers to get back who's new and who's gone

                        Dictionary<byte[], string> _deleted_files = CheckForDeletions(_files, _previous_commit_hashes, hashingTool);
                        Dictionary<byte[], string> _added_files = CheckForChange(_files, _previous_commit_hashes, hashingTool);

                        if (_deleted_files.Count > 0 || _added_files.Count > 0)
                        {
                            Console.WriteLine("There are pending changes in the repo, commit these then try again");
                            break;
                        } else
                        {
                            Console.WriteLine($"Current branch: {branch ?? "HEAD"}");
                            Console.WriteLine("If you are looking to branch off another branch, type the original branch followed by \\\\ and the new branch name, e.g. feat\\\\login");
                            Console.Write("New branch: ");
                            string? new_branch = Console.ReadLine();
                            if (new_branch == null)
                            {
                                Console.WriteLine("User entered null branch name, aborting");
                                break;
                            } else
                            {
                                branch = new_branch;
                                FileInfo head = new DirectoryInfo(@path + "\\..\\.dit\\refs\\heads").GetFiles()[0];
                                fileManager.CopyFile(head.FullName, path + "\\..\\.dit\\refs\\heads\\new_branch");

                                Console.WriteLine($"New branch has been crated, switching to {branch}");
                            }
                        }

                        break;
                    case "12": //switch to branch
                        DirectoryInfo changing_repos_dir = new DirectoryInfo(path);
                        FileInfo[] changing_repos_files = changing_repos_dir.GetFiles();
                        //Get the repos most recent snapshot
                        //pull all the repo hashes off this via GetAllFileHashes()

                        XElement changing_repos_xml;
                        if (branch is null)
                        {
                            changing_repos_xml = XElement.Load(Directory.GetFiles(path + "\\..\\.dit\\refs\\heads", "*.xml", System.IO.SearchOption.TopDirectoryOnly)[0]);
                        }
                        else
                        {
                            changing_repos_xml = XElement.Load(Directory.GetFiles(path + "\\..\\.dit\\refs\\heads\\" + branch, "*.xml", System.IO.SearchOption.TopDirectoryOnly)[0]);
                        }
                        CommitSnapshot changing_repos_previous_commit = CommitSnapshot.SnapshotFromXML(changing_repos_xml);
                        List<byte[]> changing_repos_previous_commit_hashes = changing_repos_previous_commit.GetAllFileHashes();

                        //put this in to the change checkers to get back who's new and who's gone

                        Dictionary<byte[], string> changing_repos_deleted_files = CheckForDeletions(changing_repos_files, changing_repos_previous_commit_hashes, hashingTool);
                        Dictionary<byte[], string> changing_repos_added_files = CheckForChange(changing_repos_files, changing_repos_previous_commit_hashes, hashingTool);

                        if (changing_repos_deleted_files.Count > 0 || changing_repos_added_files.Count > 0)
                        {
                            Console.WriteLine("There are pending changes in the repo, commit these then try again");
                            break;
                        }
                        else
                        {
                            Console.WriteLine($"Current branch: {branch ?? "HEAD"}\n");

                            DirectoryInfo directoryInfo = new DirectoryInfo(path + "\\..\\refs\\heads");

                            string fileStructure = fileManager.GetDirectoryStructure(directoryInfo.FullName);
                            Console.WriteLine(fileStructure);

                            Console.Write("branch: ");
                            string? new_branch = Console.ReadLine();
                            if (new_branch == null)
                            {
                                Console.WriteLine("User entered null branch name, aborting");
                                break;
                            }
                            else
                            {
                                if (fileStructure.Contains(new_branch)) //Valid branch
                                {
                                    //Get commit from the branch we want
                                    //clear working directory
                                    //Unpack the file tree from the commit to rebuild the working directory
                                    XElement new_repo_xml = XElement.Load(Directory.GetFiles(path + "\\..\\.dit\\refs\\heads\\" + new_branch, "*.xml", System.IO.SearchOption.TopDirectoryOnly)[0]);
                                    CommitSnapshot new_repo_previous_commit = CommitSnapshot.SnapshotFromXML(new_repo_xml);

                                    Directory.Delete(path);
                                    Directory.CreateDirectory(path);

                                    new_repo_previous_commit.UnpackCommitNode(new_repo_previous_commit.commitSnapshotFileTree.GetRoot(), path, true);

                                } else
                                {
                                    Console.WriteLine($"User supplied branch {new_branch} does not exist, perhaps it needs creating");
                                    break;
                                }
                            }
                        }

                        break;
                    case "13":
                        //get password off user
                        //decrypt settings file then parse the JSON in to a dictionary
                        //print out the content here
                        //ask user what they want to change, then change it
                        //keep asking till they're done
                        //rewrite the encrypted file

                        Console.Write("Password: ");
                        string? _password = Console.ReadLine();
                        string attempted_password = _password ?? "";
                        byte[] decrypted_json_data = [];
                        try
                        {
                            byte[] json_data = fileManager.ReadEncryptedFile(path + "\\..\\.dit\\settings.secure");
                            decrypted_json_data = fileManager.Decrypt(json_data, attempted_password);
                        } catch (Exception e)
                        {
                            Console.WriteLine("User gave a bad password");
                            break;
                        }

                        if (decrypted_json_data.Length == 0)
                        {
                            Console.WriteLine("Error reading settings file");
                            break;
                        } else
                        {
                            string jsonString = File.ReadAllText(path + "\\..\\.dit\\settings.secure");

                            var new_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);

                            Console.WriteLine(JsonConvert.SerializeObject(new_dict, Formatting.Indented));

                            while (true)
                            {
                                Console.Write("Which key do you wish to enter or type done: ");
                                string? option = Console.ReadLine();
                                string _option = option ?? "";

                                if (option == "done")
                                {
                                    break;
                                }
                                else if (option == "Server_Port" || option == "Server_Address" || option == "Protocol" || option == "User_ID" || option == "User_Password" || option == "Repository_Master_Password")
                                {
                                    object? value = new_dict?.TryGetValue(option, out _);
                                    Console.WriteLine($"Previous value: {value ?? ""}");
                                    Console.Write("New value: ");
                                    string new_value = Console.ReadLine() ?? "";
                                    new_dict[option] = new_value;
                                } else
                                {
                                    Console.WriteLine("User chose unknown option");
                                    break;
                                }
                            }

                            byte[] new_encrypted_settings_file = fileManager.Encrypt(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new_dict, Formatting.Indented)), user_password);
                            fileManager.WriteEncryptedFile(new_encrypted_settings_file, path + "\\..\\.dit\\Settings.secure");

                            Console.WriteLine("Settings file updated");
                        }

                        break;
                    default:
                        break;
                }

                //Console.Clear();
            }
        }
    }
}