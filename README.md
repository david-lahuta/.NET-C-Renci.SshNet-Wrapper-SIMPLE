# .NET-C-Renci.SshNet-Wrapper-SIMPLE
### SSH, SFTP  robust synchronous wrapper designed for SIMPLE use.

Based on **Renci.SshNet** library. You must add this library to your project before use this wrapper (https://github.com/sshnet/SSH.NET).<br />
For FILE PATHS **I'm using HostingEnvironment.MapPath(...)**, make sure is it OK for you.

## PUBLIC METHODS:

- ExecuteCommand
- UploadFile
- DownloadFile
- Reboot
- CreateConnectionInfo

## RECOMMENDED USAGE EXAMPLE

```
SshCommand cmdResult;
var runtimeSuccess = true;

using (var con = new SshNetWrapper(privateKeyPath, serverIp))
{
   cmdResult = con.ExecuteCommand("sudo....", ref ok);
   con.DownloadFile("remotePath", "localPath", ref runtimeSuccess)
}

Console.WriteLine(runtimeSuccess.ToString());
Console.WriteLine(cmdResult.Result);
```
