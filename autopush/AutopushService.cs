using LibGit2Sharp;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace autopush
{
    class AutopushService
    {
        public OptionPageGrid page { get; set; }
        public IVsUIShell ui_shell { get; set; }
        public string solution_path { get; set; }
        private Action<string> _alert;
        public AutopushService(OptionPageGrid page, IVsUIShell ui_shell, string solution_path)
        {
            string newPath = Environment.GetEnvironmentVariable("PATH") +
                            ";" + page.libgit2_path;
            Environment.SetEnvironmentVariable("PATH", newPath);
            this.page = page;
            this.ui_shell = ui_shell;
            this.solution_path = solution_path;
            Guid __clsid = Guid.Empty;
            int __result;
            this._alert = msg => ui_shell.ShowMessageBox(
                                    0,
                                    ref __clsid,
                                    "autopush",
                                    msg,
                                    string.Empty,
                                    0,
                                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                                    OLEMSGICON.OLEMSGICON_INFO,
                                    0,
                                    out __result);
        }
        public void push()
        {
            try
            {
                using (Repository repo = new Repository(solution_path))
                {
                    var reference_name = repo.Head.CanonicalName;
                    if (!repo.Head.IsTracking)
                    {
                        _alert(string.Format(CultureInfo.CurrentCulture, "this local branch {0} is not connected to a remote one", reference_name));
                        return;
                    }
                    repo.Network.Push(repo.Head.Remote, reference_name, new PushOptions {
                        CredentialsProvider = (_url, usernameFromUrl, types) =>
                                            {
                                                return new SshUserKeyCredentials()
                                                {
                                                    Username = usernameFromUrl,
                                                    Passphrase = page.ssh_passphrase,
                                                    PublicKey = Path.Combine(page.ssh_path, page.public_key),
                                                    PrivateKey = Path.Combine(page.ssh_path, page.private_key),
                                                };
                                            }
                    });
                }
            }
            catch (Exception ex)
            {
                _alert(string.Format(CultureInfo.CurrentCulture, "Push Error {0}", ex.Message));
            }
        }
        public void commit()
        {
            Commit commit = null;
            try { 
                using (Repository repo = new Repository(solution_path))
                {
                    RepositoryStatus status = repo.RetrieveStatus();
                    var author = new Signature(page.author, page.email, DateTimeOffset.Now);
                    if (status.IsDirty)
                    {
                        Func<IEnumerable<StatusEntry>, IEnumerable<string>> get_file_paths = entries => entries.Select(mods => mods.FilePath);
                        List<string> file_paths = get_file_paths(status.Missing)
                                                    .Union(get_file_paths(status.Modified))
                                                    .Union(get_file_paths(status.Removed))
                                                    .Union(get_file_paths(status.RenamedInIndex))
                                                    .Union(get_file_paths(status.RenamedInWorkDir))
                                                    .Union(get_file_paths(status.Untracked))
                                                    .ToList();
                        Commands.Stage(repo, file_paths);
                        commit = repo.Commit(string.Format("auto commit - updated: {0}", string.Join(",", file_paths)), author, author);
                    }
                }
            }
            catch (Exception ex)
            {
                _alert(string.Format(CultureInfo.CurrentCulture, "Commit {0} Error {1}", commit, ex.Message));
            }
        }
    }
}
