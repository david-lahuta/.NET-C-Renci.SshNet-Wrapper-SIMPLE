using Renci.SshNet;
using System;
using System.IO;
using System.Net.Sockets;
namespace Core
{
    public class SshNetWrapper : IDisposable
    {
        private readonly string _privateKeyPath;
        private static string _username = "admin";
        private readonly string _serverIp;
        private readonly SftpClient _sftpClient;
        private readonly SshClient _sshClient;
        private const int SftpBufferSize = 30 * 1024; //roughly max

        //  USAGE EXAMPLE
        /*  SshCommand cmdResult;
         *  var runtimeSuccess = true;
         *  using (var con = new SshNetWrapper(privateKeyPath, serverIp))
         *  {
         *     cmdResult = con.ExecuteCommand("sudo....", ref ok);
         *     con.DownloadFile("remotePath", "localPath", ref runtimeSuccess)
         *  }
         *  Console.WriteLine(runtimeSuccess.ToString());
         *  Console.WriteLine(cmdResult.Result);
         */

        public SshNetWrapper(string username, string privateKeyPath, string serverIp) : this(privateKeyPath, serverIp)
        {
            _username = username;
        }

        public SshNetWrapper(string privateKeyPath, string serverIp)
        {
            _privateKeyPath = privateKeyPath; //.pem recommended
            _serverIp = serverIp;

            //inicialize clients
            var connectionInfo = CreateConnectionInfo();
            _sftpClient = new SftpClient(connectionInfo) { BufferSize = SftpBufferSize };
            _sshClient = new SshClient(connectionInfo);
        }

        public void UploadFile(string @sourcePath, ref bool result, string @destinationPath = null)
        {
            ConnectSftp();
            try
            {
                if (destinationPath != null) _sftpClient.ChangeDirectory(@destinationPath);
                using (var uplFileStream = File.OpenRead(@HostingEnvironment.MapPath(@sourcePath)))
                {
                    _sftpClient.UploadFile(uplFileStream, @Path.GetFileName(@sourcePath), true);
                }
            }
            catch (Exception e)
            {
                result = false;
                Error("SshNetWrapper upload file error", e);
            }
        }

        public void DownloadFile(string @remoteFile, string @localPath, ref bool result)
        {
            ConnectSftp();
            try
            {
                _sftpClient.ChangeDirectory(@Path.GetDirectoryName(@remoteFile)?.Replace(@"\", @"/"));
                using (var fs = new FileStream(@HostingEnvironment.MapPath(@localPath + @Path.GetFileName(@remoteFile)), FileMode.Create))
                {
                    _sftpClient.DownloadFile(@Path.GetFileName(@remoteFile), fs);
                    fs.Close();
                }
            }
            catch (Exception e)
            {
                result = false;
                Error("SshNetWrapper download file error", e);
            }
        }

        public SshCommand ExecuteCommand(string @command, ref bool result)
        {
            ConnectSsh();
            try
            {
                var cmd = _sshClient.CreateCommand(@command);
                cmd.Execute();
                return cmd;

            }
            catch (Exception e)
            {
                result = false;
                Error("SshNetWrapper execute command error", e);
                return null;
            }
        }

        //USE ONLY AS STATIC
        //MAKE SURE ALL CONNECTIONS TO REBOOTED SERVER ARE CLOSED!
        //Method disconnect from server after reboot command, however socket exception is OK, because server may not properly respond. 
        public static bool Reboot(string privateKeyPath, string serverIp, ref bool result)
        {
            try
            {
                using (var client = new SshClient(CreateConnectionInfo(privateKeyPath, serverIp)))
                {
                    client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(3);
                    client.KeepAliveInterval = TimeSpan.FromSeconds(3);
                    client.Connect();

                    client.RunCommand(@"sudo reboot");
                    client.Disconnect();
                    return true;
                }
            }
            catch (SocketException)
            {
                return true;
            }
            catch (Exception e)
            {
                result = false;
                Error("SshNetWrapper reboot error", e);
                return false;
            }
        }

        //simpled overloads
        public bool UploadFile(string @sourcePath, string @destinationPath = null)
        {
            var r = true;
            UploadFile(sourcePath, ref r, destinationPath);
            return r;
        }

        public bool DownloadFile(string @remoteFile, string @localPath)
        {
            var r = true;
            DownloadFile(remoteFile, localPath, ref r);
            return r;
        }

        //if someone need it...
        public static ConnectionInfo CreateConnectionInfo(string privateKeyPath, string serverIp)
        {
            var privateKeyFilePath = @HostingEnvironment.MapPath(privateKeyPath);
            ConnectionInfo connectionInfo;

            using (var stream = new FileStream(privateKeyFilePath, FileMode.Open, FileAccess.Read))
            {
                var privateKeyFile = new PrivateKeyFile(stream);
                AuthenticationMethod authenticationMethod = new PrivateKeyAuthenticationMethod(_username, privateKeyFile);

                connectionInfo = new ConnectionInfo(
                    serverIp,
                    _username,
                    authenticationMethod);
            }

            return connectionInfo;
        }

        //private methods
        private ConnectionInfo CreateConnectionInfo()
        {
            return CreateConnectionInfo(_privateKeyPath, _serverIp);
        }

        private void ConnectSftp()
        {
            try
            {
                if (!_sftpClient.IsConnected) _sftpClient.Connect();
            }
            catch (Exception e)
            {
                Error("SshNetWrapper SFTP connect error", e);
            }
        }

        private void ConnectSsh()
        {
            try
            {
                if (!_sshClient.IsConnected) _sshClient.Connect();
            }
            catch (Exception e)
            {
                Error("SshNetWrapper SSH connect error", e);
            }
        }

        private static void Error(string subject, Exception e)
        {
            Console.WriteLine(subject);
            Console.WriteLine(e.ToString());
            //or throw something....
        }

        //Dispose implementation & Destructor
        public void Dispose()
        {
            _sftpClient?.Disconnect();
            _sftpClient?.Dispose();
            _sshClient?.Disconnect();
            _sshClient?.Dispose();
        }

        ~SshNetWrapper()
        {
            Dispose();
        }
    }
}
