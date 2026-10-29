using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;

internal static class Program
{
    private const string Repo = "https://api.github.com/repos/hsynbkr-bit/DMTPRO-Updates/releases?per_page=30";
    private const string AssetName = "DMTPRO_update_win_x64.zip";
    private const string ProgramName = "DMTPRO.exe";
    private const string LauncherName = "DMTPROLauncher.exe";
    private static readonly string[] UpdatedFiles = { ProgramName, LauncherName, "DMTPRO_Icon.ico", "version.txt" };
    private static readonly HashSet<string> AllowedZipFiles = new(UpdatedFiles.Append("version.json"), StringComparer.Ordinal);
    private static readonly string LogFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DMTPRO", "updater.log");

    [STAThread]
    private static int Main(string[] args)
    {
        Application.EnableVisualStyles();
        try
        {
            if (args.Length >= 1 && args[0] == "--self-test")
                return SelfTest(args.Length > 1 ? args[1] : null);
            if (args.Length == 4 && args[0] == "--apply")
                return ApplyWorker(args[1], args[2], int.Parse(args[3]));
            if (args.Length == 2 && args[0] == "--after-update")
                return LaunchApplication(args[1]);
            if (args.Length != 0)
                return 2;

            var dir = AppContext.BaseDirectory;
            try
            {
                if (CheckAndStartUpdate(dir))
                    return 0;
            }
            catch (Exception error)
            {
                Log("Online update skipped: " + error.GetType().Name + ": " + error.Message);
            }
            return LaunchApplication(null);
        }
        catch (Exception error)
        {
            Log("Launcher error: " + error);
            MessageBox.Show("DMTPRO başlatılamadı: " + error.Message, "DMTPRO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 1;
        }
    }

    private static int LaunchApplication(string? markerDirectory)
    {
        var path = Path.Combine(AppContext.BaseDirectory, ProgramName);
        if (!File.Exists(path))
            throw new FileNotFoundException("DMTPRO.exe bulunamadı.", path);
        var process = Process.Start(new ProcessStartInfo(path)
        {
            WorkingDirectory = AppContext.BaseDirectory,
            UseShellExecute = true
        }) ?? throw new InvalidOperationException("Program başlatılamadı.");
        if (markerDirectory != null)
        {
            Thread.Sleep(1500);
            if (process.HasExited && process.ExitCode != 0)
                throw new InvalidOperationException("Güncelleme sonrası uygulama açılışı başarısız.");
            File.WriteAllText(Path.Combine(markerDirectory, "started.ok"), DateTime.UtcNow.ToString("O"));
        }
        return 0;
    }

    private sealed record Release(Version Version, string Tag, string Url, string Digest, long Size);

    private static bool CheckAndStartUpdate(string installDir)
    {
        var installed = ReadInstalledVersion(installDir);
        using var client = NewClient(TimeSpan.FromSeconds(9));
        var response = client.GetStringAsync(Repo).GetAwaiter().GetResult();
        var newest = SelectRelease(response, installed);
        if (newest == null)
            return false;

        var yes = MessageBox.Show(
            "DMTPRO " + newest.Version + " güncellemesi hazır.\n\n" +
            "Yeni sürüm indirilsin ve güvenli şekilde kurulsun mu?\n" +
            "Hesabınız, cihaz izniniz ve proje dosyalarınız korunur.",
            "DMTPRO — Yeni sürüm", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
        if (yes != DialogResult.Yes)
            return false;

        var stage = Path.Combine(Path.GetTempPath(), "DMTPRO-Update-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(stage);
        try
        {
            var zip = Path.Combine(stage, "download.zip");
            using (var download = NewClient(TimeSpan.FromMinutes(5)))
            using (var request = download.GetAsync(newest.Url, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult())
            {
                request.EnsureSuccessStatusCode();
                if (request.Content.Headers.ContentLength > 250_000_000)
                    throw new InvalidDataException("Paket çok büyük.");
                using var input = request.Content.ReadAsStream();
                using var output = File.Create(zip);
                input.CopyTo(output);
            }
            if (new FileInfo(zip).Length != newest.Size)
                throw new InvalidDataException("İndirilen paketin boyutu uyuşmuyor.");
            VerifyAndExtract(zip, newest.Digest, newest.Version.ToString(), newest.Tag, stage);

            // Updater itself may be replaced only by a separate worker running from Temp.
            var helper = Path.Combine(stage, "DMTPROUpdateWorker.exe");
            File.Copy(Path.Combine(installDir, LauncherName), helper, true);
            var start = new ProcessStartInfo(helper)
            {
                UseShellExecute = false,
                WorkingDirectory = stage
            };
            start.ArgumentList.Add("--apply");
            start.ArgumentList.Add(stage);
            start.ArgumentList.Add(installDir);
            start.ArgumentList.Add(Environment.ProcessId.ToString());
            Process.Start(start);
            return true;
        }
        catch (Exception error)
        {
            Log("Update failed: " + error);
            MessageBox.Show("Güncelleme kurulamadı. Eski sürüm açılacak.\n\n" + error.Message,
                "DMTPRO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            try { Directory.Delete(stage, true); } catch { }
            return false;
        }
    }

    private static HttpClient NewClient(TimeSpan timeout)
    {
        var client = new HttpClient { Timeout = timeout };
        client.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("DMTPRO-Official-Updater", "2.0.1"));
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        return client;
    }

    private static Version ReadInstalledVersion(string dir)
    {
        var path = Path.Combine(dir, "version.txt");
        if (!File.Exists(path) || !Version.TryParse(File.ReadAllText(path).Trim(), out var v))
            return new Version(2, 0, 1);
        return v;
    }

    private static Release? SelectRelease(string response, Version current)
    {
        using var document = JsonDocument.Parse(response);
        Release? best = null;
        foreach (var item in document.RootElement.EnumerateArray())
        {
            if (item.GetProperty("draft").GetBoolean() || item.GetProperty("prerelease").GetBoolean())
                continue;
            var tag = item.GetProperty("tag_name").GetString() ?? "";
            var match = Regex.Match(tag, @"^dmtpro-v(2)\.(\d+)\.(\d+)$", RegexOptions.CultureInvariant);
            if (!match.Success || !Version.TryParse(match.Groups[1].Value + "." +
                match.Groups[2].Value + "." + match.Groups[3].Value, out var v) ||
                v <= current || (best != null && v <= best.Version))
                continue;
            foreach (var asset in item.GetProperty("assets").EnumerateArray())
            {
                if (asset.GetProperty("name").GetString() != AssetName)
                    continue;
                var digest = asset.TryGetProperty("digest", out var value) ? value.GetString() ?? "" : "";
                var url = asset.GetProperty("browser_download_url").GetString() ?? "";
                var size = asset.GetProperty("size").GetInt64();
                var expectedPrefix = "https://github.com/hsynbkr-bit/DMTPRO-Updates/releases/download/" + tag + "/";
                if (!Regex.IsMatch(digest, "^sha256:[0-9a-fA-F]{64}$") ||
                    url != expectedPrefix + AssetName || size < 100 || size > 250_000_000)
                    continue;
                best = new Release(v, tag, url, digest, size);
            }
        }
        return best;
    }

    private static string Sha256(string filename)
    {
        using var stream = File.OpenRead(filename);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static void VerifyAndExtract(string zipPath, string expectedDigest,
        string expectedVersion, string expectedTag, string stage)
    {
        if (!Regex.IsMatch(expectedDigest, "^sha256:[0-9a-fA-F]{64}$") ||
            !Sha256(zipPath).Equals(expectedDigest.Substring(7), StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("ZIP SHA-256 bütünlük doğrulaması başarısız.");
        var payload = Path.Combine(stage, "payload");
        Directory.CreateDirectory(payload);
        using (var archive = ZipFile.OpenRead(zipPath))
        {
            var names = archive.Entries.Select(x => x.FullName).ToArray();
            if (names.Length != AllowedZipFiles.Count ||
                names.Distinct(StringComparer.Ordinal).Count() != names.Length ||
                names.Any(n => !AllowedZipFiles.Contains(n)))
                throw new InvalidDataException("Paket içeriği beklenen güvenli dosyalardan oluşmuyor.");
            long total = 0;
            foreach (var entry in archive.Entries)
            {
                if (entry.Length > 250_000_000 || (total += entry.Length) > 350_000_000)
                    throw new InvalidDataException("Paket dosya boyutu sınırı aşıldı.");
                entry.ExtractToFile(Path.Combine(payload, entry.FullName), false);
            }
        }
        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(payload, "version.json")));
        if (manifest.RootElement.GetProperty("version").GetString() != expectedVersion ||
            manifest.RootElement.GetProperty("releaseTag").GetString() != expectedTag ||
            File.ReadAllText(Path.Combine(payload, "version.txt")).Trim() != expectedVersion)
            throw new InvalidDataException("Paket sürüm bilgileri uyuşmuyor.");
        var files = manifest.RootElement.GetProperty("files");
        foreach (var name in UpdatedFiles)
        {
            var expectedHash = files.GetProperty(name).GetString() ?? "";
            if (!Regex.IsMatch(expectedHash, "^[0-9a-fA-F]{64}$") ||
                !Sha256(Path.Combine(payload, name)).Equals(expectedHash, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(name + " dosyası doğrulanamadı.");
        }
    }

    private static void ApplyFiles(string stage, string dir, bool simulateFailure = false)
    {
        var backup = Path.Combine(stage, "backup");
        Directory.CreateDirectory(backup);
        var originals = new HashSet<string>(StringComparer.Ordinal);
        foreach (var file in UpdatedFiles)
        {
            var dest = Path.Combine(dir, file);
            if (!File.Exists(dest)) continue;
            File.Copy(dest, Path.Combine(backup, file), true);
            originals.Add(file);
        }
        try
        {
            for (int index = 0; index < UpdatedFiles.Length; index++)
            {
                var file = UpdatedFiles[index];
                File.Copy(Path.Combine(stage, "payload", file), Path.Combine(dir, file), true);
                if (simulateFailure && index == 1)
                    throw new IOException("Simulated interruption");
            }
        }
        catch
        {
            foreach (var file in UpdatedFiles)
            {
                var dest = Path.Combine(dir, file);
                if (originals.Contains(file))
                    File.Copy(Path.Combine(backup, file), dest, true);
                else if (File.Exists(dest))
                    File.Delete(dest);
            }
            throw;
        }
    }

    private static void RestoreFiles(string stage, string dir)
    {
        var backup = Path.Combine(stage, "backup");
        foreach (var name in UpdatedFiles)
        {
            var src = Path.Combine(backup, name);
            var dest = Path.Combine(dir, name);
            if (File.Exists(src))
                File.Copy(src, dest, true);
            else if (File.Exists(dest))
                File.Delete(dest);
        }
    }

    private static int ApplyWorker(string stage, string installDir, int parentId)
    {
        try
        {
            try { Process.GetProcessById(parentId).WaitForExit(25000); }
            catch (ArgumentException) { }
            if (Process.GetProcessesByName("DMTPRO").Any(p =>
                    !p.HasExited && string.Equals(p.MainModule?.FileName,
                    Path.Combine(installDir, ProgramName), StringComparison.OrdinalIgnoreCase)))
                throw new IOException("DMTPRO çalışırken uygulama dosyası değiştirilemez.");

            ApplyFiles(stage, installDir);
            var start = new ProcessStartInfo(Path.Combine(installDir, LauncherName))
            {
                UseShellExecute = false,
                WorkingDirectory = installDir
            };
            start.ArgumentList.Add("--after-update");
            start.ArgumentList.Add(stage);
            var child = Process.Start(start) ?? throw new IOException("Yeni başlatıcı açılamadı.");
            var marker = Path.Combine(stage, "started.ok");
            for (var i = 0; i < 24 && !File.Exists(marker); i++)
            {
                if (child.HasExited && child.ExitCode != 0) break;
                Thread.Sleep(500);
            }
            if (!File.Exists(marker))
            {
                try { if (!child.HasExited) child.Kill(entireProcessTree: true); } catch { }
                throw new IOException("Güncelleme sonrası başlatma doğrulanamadı.");
            }
            Log("Updated successfully to " + File.ReadAllText(Path.Combine(installDir, "version.txt")).Trim());
            return 0;
        }
        catch (Exception error)
        {
            Log("Rollback: " + error);
            try { RestoreFiles(stage, installDir); } catch (Exception ex) { Log("Rollback error: " + ex); }
            try { LaunchFromWorker(installDir); } catch (Exception ex) { Log("Old app launch error: " + ex); }
            MessageBox.Show("Güncelleme uygulanamadı; önceki sürüme dönüldü.\n" +
                error.Message, "DMTPRO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return 1;
        }
    }

    private static void LaunchFromWorker(string dir)
    {
        Process.Start(new ProcessStartInfo(Path.Combine(dir, LauncherName))
        {
            WorkingDirectory = dir, UseShellExecute = true
        });
    }

    private static void Log(string entry)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogFile)!);
            File.AppendAllText(LogFile, DateTime.UtcNow.ToString("O") + " " + entry + Environment.NewLine);
        }
        catch { }
    }

    private static int SelfTest(string? resultFile)
    {
        var dir = Path.Combine(Path.GetTempPath(), "DMTPRO-updater-selftest-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(dir);
            var install = Path.Combine(dir, "install");
            var source = Path.Combine(dir, "source");
            Directory.CreateDirectory(install);
            Directory.CreateDirectory(source);
            foreach (var name in UpdatedFiles)
            {
                File.WriteAllText(Path.Combine(install, name), "old " + name);
                File.WriteAllText(Path.Combine(source, name), name == "version.txt" ? "2.0.2" : "new " + name);
            }
            var hashes = UpdatedFiles.ToDictionary(n => n, n => Sha256(Path.Combine(source, n)));
            File.WriteAllText(Path.Combine(source, "version.json"),
                JsonSerializer.Serialize(new { version = "2.0.2", releaseTag = "dmtpro-v2.0.2", files = hashes }));
            var zip = Path.Combine(dir, "release.zip");
            ZipFile.CreateFromDirectory(source, zip);
            var stage = Path.Combine(dir, "stage");
            VerifyAndExtract(zip, "sha256:" + Sha256(zip), "2.0.2", "dmtpro-v2.0.2", stage);
            bool checksumRejected = false;
            try { VerifyAndExtract(zip, "sha256:" + new string('0', 64), "2.0.2", "dmtpro-v2.0.2", Path.Combine(dir, "bad")); }
            catch (InvalidDataException) { checksumRejected = true; }
            if (!checksumRejected) throw new Exception("Damaged checksum accepted");
            bool rollbackTriggered = false;
            try { ApplyFiles(stage, install, true); }
            catch (IOException) { rollbackTriggered = true; }
            if (!rollbackTriggered || UpdatedFiles.Any(n =>
                    File.ReadAllText(Path.Combine(install, n)) != "old " + n))
                throw new Exception("Rollback test failed");
            ApplyFiles(stage, install);
            if (UpdatedFiles.Any(n => Sha256(Path.Combine(install, n)) != hashes[n]))
                throw new Exception("Apply test failed");
            var releases = "[" +
                "{\"draft\":false,\"prerelease\":false,\"tag_name\":\"v1.0.6\",\"assets\":[]}," +
                "{\"draft\":false,\"prerelease\":true,\"tag_name\":\"dmtpro-v2.0.9-beta.1\",\"assets\":[]}," +
                "{\"draft\":false,\"prerelease\":false,\"tag_name\":\"dmtpro-v2.0.2\",\"assets\":[{\"name\":\"" + AssetName +
                "\",\"size\":400,\"digest\":\"sha256:" + new string('a',64) +
                "\",\"browser_download_url\":\"https://github.com/hsynbkr-bit/DMTPRO-Updates/releases/download/dmtpro-v2.0.2/" + AssetName + "\"}]}]";
            if (SelectRelease(releases, new Version(2, 0, 1))?.Version != new Version(2, 0, 2))
                throw new Exception("Release selection failed");
            var report = "{\"checksum_rejection\":true,\"rollback\":true,\"apply\":true,\"stable_2x_filter\":true}";
            if (resultFile != null) File.WriteAllText(resultFile, report);
            return 0;
        }
        catch (Exception error)
        {
            if (resultFile != null) File.WriteAllText(resultFile, error.ToString());
            return 1;
        }
        finally
        {
            try { Directory.Delete(dir, true); } catch { }
        }
    }
}
