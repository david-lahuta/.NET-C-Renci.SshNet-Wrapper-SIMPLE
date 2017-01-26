# .NET-C-Renci.SshNet-Wrapper-SIMPLE
### SSH, SFTP  robust synchronous wrapper designed for SIMPLE use.

Based on **Renci.SshNet** library. You must add this library to your project before use this wrapper.<br/>https://github.com/sshnet/SSH.NET<br /><br/>
For FILE PATHS **I'm using HostingEnvironment.MapPath(...)**, make sure is it OK for you.<br/>
**Error output goes to Console.WrietLine()**, edit Error(...) method to change this behavior. <br/>
**Recommended to use absolute paths for downloading/uploading**

## PUBLIC METHODS:

- ExecuteCommand
- UploadFile
- DownloadFile
- Reboot
- CreateConnectionInfo

## BASIC USAGE

```
using (var con = new SshNetWrapper(privateKeyPath, serverIp))
{
    var cmd = con.ExecuteCommand("sudo...");
    Console.WriteLine(cmd.Result);
}
```

## RECOMMEND USAGE

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
