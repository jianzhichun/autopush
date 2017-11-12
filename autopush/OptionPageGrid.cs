using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace autopush
{
    class OptionPageGrid : DialogPage
    {
        [Category("autopush")]
        [DisplayName("author")]
        [Description("author")]
        public string author { get; set; }
        [Category("autopush")]
        [DisplayName("email")]
        [Description("email")]
        public string email { get; set; }
        [Category("autopush")]
        [DisplayName("ssh.path")]
        [Description("ssh.path")]
        public string ssh_path { get; set; }
        [Category("autopush")]
        [DisplayName("ssh.public-key")]
        [Description("ssh.public-key")]
        public string public_key { get; set; }
        [Category("autopush")]
        [DisplayName("ssh.private-key")]
        [Description("ssh.private-key")]
        public string private_key { get; set; }
        [Category("autopush")]
        [DisplayName("ssh.passphrase")]
        [Description("ssh.passphrase")]
        public string ssh_passphrase { get; set; }
        [Category("autopush")]
        [DisplayName("libgit2-path")]
        [Description("libgit2-path")]
        public string libgit2_path { get; set; }
        public OptionPageGrid()
        {
            this.author = Environment.GetEnvironmentVariable("USERNAME");
            this.email = this.author + "@imcap.ap.ssmb.com";
            this.ssh_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh");
            this.public_key = "id_rsa.pub";
            this.private_key = "id_rsa";
            this.ssh_passphrase = string.Empty;
            this.libgit2_path = System.IO.Path.GetTempPath() + "//libgit2";
        }

    }
}
