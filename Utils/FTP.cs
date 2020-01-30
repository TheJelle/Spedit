using System;
using System.IO;
using System.Net;
using System.Text;
using Renci.SshNet;

namespace Spedit.Utils
{
    public class FTP
    {
        private readonly string _host;
        private readonly string _user;
        private readonly string _pass;

        public FTP(string host, string user, string password) { _host = host; _user = user; _pass = password; }

        public void Upload(string remoteFile, string localFile)
        {
            var requestUri = new UriBuilder(_host) { Path = remoteFile }.Uri;

            if (requestUri.Scheme == "sftp")
            {
                var connectionInfo = new ConnectionInfo(requestUri.Host, requestUri.Port == -1 ? 22 : requestUri.Port, _user, new PasswordAuthenticationMethod(_user, _pass));
                using (var sftp = new SftpClient(connectionInfo))
                {
                    sftp.Connect();

                    using (var stream = File.OpenRead(localFile))
                    {
                        sftp.UploadFile(stream, remoteFile, true);
                    }

                    sftp.Disconnect();
                }
            }
            else
            {
                using (var client = new WebClient())
                {
                    client.Credentials = new NetworkCredential(_user, _pass);
                    client.UploadFile(requestUri, WebRequestMethods.Ftp.UploadFile, localFile);
                }
            }
        }
    }
}
