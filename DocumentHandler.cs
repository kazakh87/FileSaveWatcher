

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System;

namespace FileSaveWatcher
  {
  public class DocumentHandler
    {

    public class Settings
      {
      public class ConfigPart
        {
        public string Cmd;
        public Regex FileRegex;
        public bool Sync;
        public bool Notify;

        public ConfigPart(string Cmd, string Pattern, bool Sync, bool Notify)
          {
          this.Cmd = Cmd;
          if (Pattern.StartsWith("/") && Pattern.EndsWith("/"))
            FileRegex = new Regex(Pattern.Substring(1, Pattern.Length - 2));
          else
            FileRegex = new Regex(Pattern.Replace(".", "[.]").Replace("*", ".*").Replace('?', '.'));
          this.Sync = Sync;
          this.Notify = Notify;
          }

        public bool isOkay()
          {
          return !string.IsNullOrWhiteSpace(Cmd);
          }
        }

      public List<ConfigPart> OnOpeningConfigs;
      public List<ConfigPart> OnOpenedConfigs;
      public List<ConfigPart> OnSavedConfigs;
      public List<ConfigPart> OnClosingConfigs;
      public List<ConfigPart> GetConfigs(string evType)
        {
        if (evType == "Saved") return OnSavedConfigs;
        if (evType == "Opening") return OnOpeningConfigs;
        if (evType == "Opened") return OnOpenedConfigs;
        if (evType == "Closing") return OnClosingConfigs;
        return null;
        }

      public string Name;
      public string Path;
      public string WorkingDir;
      public Settings(string Name, string Path)
        {
        this.Name = Name.ToLower();
        this.Path = Path.ToLower();
        this.WorkingDir = this.Path;
        }

      public void Read(bool createConfig = true)
        {
        OnOpeningConfigs = new List<ConfigPart>();
        OnOpenedConfigs = new List<ConfigPart>();
        OnSavedConfigs = new List<ConfigPart>();
        OnClosingConfigs = new List<ConfigPart>();
        
        string ConfigPath = Path + "\\" + SaveWatcher.Name;

        if (File.Exists(ConfigPath))
          {
          List<ConfigPart> evList = null;
          foreach (var line in File.ReadAllLines(ConfigPath))
            {
            string workingLine = line.TrimStart();
            if (workingLine.StartsWith("#") || workingLine.Length == 0) continue; // ignore comment or empty line

            if (workingLine.StartsWith("workingdir "))
              {
              string newWorkingDir = workingLine.Remove(0, "workingdir ".Length).Trim();
              if (Directory.Exists(newWorkingDir))
                WorkingDir = newWorkingDir.ToLower();
              continue;
              }

            if (workingLine.StartsWith("event="))
              {
              evList = null;
              int startPos = "event=".Length;
              int endPos = System.Math.Max(startPos, workingLine.IndexOf(';', startPos));
              switch (workingLine.Substring(startPos, endPos - startPos))
                {
                case "onOpening":
                  evList = OnOpeningConfigs;
                  break;
                case "onOpened":
                  evList = OnOpenedConfigs;
                  break;
                case "onSaved":
                  evList = OnSavedConfigs;
                  break;
                case "onClosing":
                  evList = OnClosingConfigs;
                  break;
                }

              if (evList != null)
                {
                startPos = endPos;
                string filter = string.Empty;
                int filterStart = workingLine.IndexOf("filter=", startPos);
                if (filterStart > 0)
                  {
                  startPos = filterStart + "filter=".Length;
                  endPos = System.Math.Max(startPos, workingLine.IndexOf(';', startPos));
                  filter = workingLine.Substring(startPos, endPos - startPos);
                  startPos = endPos;
                  }
                bool sync = false;
                int syncStart = workingLine.IndexOf("sync=", startPos);
                if (syncStart > 0)
                  {
                  startPos = syncStart + "sync=".Length;
                  endPos = System.Math.Max(startPos, workingLine.IndexOf(';', startPos));
                  sync = workingLine.Substring(startPos, endPos - startPos) == "1";
                  startPos = endPos;
                  }
                bool notify = false;
                int notifyStart = workingLine.IndexOf("notify=", startPos);
                if (notifyStart > 0)
                  {
                  startPos = notifyStart + "notify=".Length;
                  endPos = System.Math.Max(startPos, workingLine.IndexOf(';', startPos));
                  notify = workingLine.Substring(startPos, endPos - startPos) == "1";
                  startPos = endPos;
                  }
                string cmd = string.Empty;
                int cmdStart = workingLine.IndexOf("cmd=", startPos);
                if (cmdStart > 0)
                  {
                  startPos = cmdStart + "cmd=".Length;
                  cmd = workingLine.Substring(startPos);
                  }
                if (string.IsNullOrWhiteSpace(filter))
                  filter = "*";
                ConfigPart part = new ConfigPart(cmd, filter, sync, notify);
                if (part.isOkay()) evList.Add(part);
                }
              }
            }
          SaveWatcher.Win.Output("Readed configfile: " + ConfigPath);
          if (OnOpeningConfigs.Count == 0 && OnOpenedConfigs.Count == 0 && OnSavedConfigs.Count==0 && OnClosingConfigs.Count==0)
            SaveWatcher.Win.Output("config has no events");
          }
        else if (SaveWatcher.AutoGenerateConfig)
          {
          bool Generate = false;
          string[] AllowedPaths = SaveWatcher.AutoGenerateConfigPaths.Split(';');
          if (AllowedPaths.Length > 0)
            {
            foreach (var AllowedPath in AllowedPaths)
              if (ConfigPath.Contains(AllowedPath)) Generate = true;
            }
          else
            Generate = true;
          if (Generate)
            {
            // todo schöner wäre es als resource file
            string DemoFile = "#######################################################################\r\n" +
                              "# This is a config file for SaveFilWatcher extension of Visual Studio #\r\n" +
                              "#######################################################################\r\n" +
                              "# the '#' on start is a comment line\r\n" +
                              "\r\n" +
                              "# uncomment the following line to change the workingdirectory for the script\r\n" +
                              "#workingdir C:\\temp_dir\r\n" +
                              "\r\n" +
                              "# description of entrys\r\n" +
                              "# first event   = [event option of changing the file] !must be the first param!\r\n" +
                              "#                 keys => onOpening|onOpened|onSaved|onClosing\r\n" +
                              "# second filter = [filtering the files, to trigger the command(cmd)] (can be a part, default is '*')\r\n" +
                              "#                 filter is using only the files in the project directory, (for regex use '/' > '/[.]cs/')\r\n" +
                              "# next sync     = [run cmd in shell synchron] (optional, default '0')\r\n" +
                              "#                 1 => true or 0 => false \r\n" +
                              "# next notify   = [notify after run cmd in output window] (optional, default '0')\r\n" +
                              "#                 1 => true or 0 => false \r\n" +
                              "# last cmd      = [cmd is the last param and reads to end of line] !must be the last param!\r\n" +
                              "#                 the content after 'cmd=' (ends with \\r\\n) is piped direct shell\r\n" +
                              "#                 params: {file_name}/{file_path}/{file_dir}/{file_ext}/{file_name_without_ext}\r\n" +
                              "#                         {settings_path}/{settings_name}/{settings_dir}\r\n" +
                              "\r\n" +
                              "# uncomment the following line to activete there, or make a new one\r\n" +
                              "#event=onSaved;filter=*.cs;sync=1;notify=1;cmd=copy \"{file_path}\" \"c:\\temp\"\r\n";
            File.WriteAllText(ConfigPath, DemoFile);
            SaveWatcher.Win.Output("Generate new configfile in: " + ConfigPath);
            }
          else
            {
            SaveWatcher.Win.Output("Generate new configfile is not allowed");
            }
          }
        }
      }

    public void RemoveSetting(string ProjectName)
      {
      Settings set = ContainsSetting(ProjectName);
      if (set != null) mSettings.Remove(set);
      }

    public Settings ContainsSetting(string ProjectName)
      {
      ProjectName = ProjectName.ToLower();
      foreach (var setting in mSettings)
        {
        if (setting.Name == ProjectName)
          return setting;
        }
      return null;
      }

    private List<Settings> mSettings;
    public DocumentHandler()
      { mSettings = new List<Settings>(); }

    public void LoadSetting(string ProjectName, string ProjectPath)
      {
      if (ContainsSetting(ProjectName) == null)
        {
        if (File.Exists(ProjectPath))
          ProjectPath = Path.GetDirectoryName(ProjectPath);

        if (Directory.Exists(ProjectPath))
          {
          Settings set = new Settings(ProjectName, ProjectPath);
          set.Read();
          mSettings.Add(set);
          }
        }
      }

    public void UnloadSettings()
      { mSettings.Clear(); }

    public bool NeedOpeningEv
      {
      get
        {
        bool needOnOpening = false;
        foreach (var setting in mSettings)
          {
          if (setting.OnOpeningConfigs.Count > 0)
            {
            needOnOpening = true;
            break;
            }
          }
        return needOnOpening;
        }
      }

    public bool NeedOpenedEv
      {
      get
        {
        bool needOnOpened = false;
        foreach (var setting in mSettings)
          {
          if (setting.OnOpenedConfigs.Count > 0)
            {
            needOnOpened = true;
            break;
            }
          }
        return needOnOpened;
        }
      }

    public bool NeedSavedEv
      {
      get
        {
        bool needOnSaved = false;
        foreach (var setting in mSettings)
          {
          if (setting.OnSavedConfigs.Count > 0)
            {
            needOnSaved = true;
            break;
            }
          }
        return needOnSaved;
        }
      }

    public bool NeedClosingEv
      {
      get
        {
        bool needOnClosing = false;
        foreach (var setting in mSettings)
          {
          if (setting.OnClosingConfigs.Count > 0)
            {
            needOnClosing = true;
            break;
            }
          }
        return needOnClosing;
        }
      }

    #region "Helper"

    private Dictionary<Settings,List<Settings.ConfigPart>> getMatchedConfigs(string docPath, string evType)
      {
      docPath = docPath.ToLower();
      List<Settings.ConfigPart> temp = null;
      var matchList = new Dictionary<Settings, List<Settings.ConfigPart>>();
      foreach (var setting in mSettings)
        {
        if (docPath.StartsWith(setting.Path))
          {
          foreach (var conf in setting.GetConfigs(evType))
            {
            if (conf.FileRegex.IsMatch(docPath))
              {
              if (!matchList.TryGetValue(setting, out temp))
                {
                temp = new List<Settings.ConfigPart>();
                matchList.Add(setting, temp);
                }
              temp.Add(conf);
              }
            }
          }
        }
      return matchList;
      }
    
    public void OnDocumentOpening(string docPath, bool readOnly)
      {
      var matchDic = getMatchedConfigs(docPath, "Opening");
      ExecuteCmds(docPath, matchDic);
      }

    public void OnDocumentOpened(EnvDTE.Document document)
      {
      string docPath = document.FullName;
      var matchDic = getMatchedConfigs(docPath, "Opened");
      ExecuteCmds(docPath, matchDic);
      }

    public void OnDocumentClosing(EnvDTE.Document document)
      {
      string docPath = document.FullName;
      var matchDic = getMatchedConfigs(docPath, "Closing");
      ExecuteCmds(docPath, matchDic);
      }

    public void OnDocumentSaved(EnvDTE.Document document)
      {
      string docPath = document.FullName; // is SaveFileWatcherConfig reload the Settings
      var matchDic = getMatchedConfigs(docPath, "Saved");
      ExecuteCmds(docPath, matchDic);
      }

    private void ExecuteCmds(string docPath, Dictionary<Settings, List<Settings.ConfigPart>> matchDic)
      {
      foreach (var settingPair in matchDic)
        {
        foreach (var conf in settingPair.Value)
          {
          try
            {
            string executeCmd = ReplacePlaceholder(conf.Cmd, docPath, settingPair.Key);

            var process = new System.Diagnostics.Process();
            process.StartInfo = new System.Diagnostics.ProcessStartInfo();
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/C " + executeCmd;
            process.StartInfo.WorkingDirectory = settingPair.Key.WorkingDir;
            process.EnableRaisingEvents = true;
            process.Exited += Process_Exited;
            lock ("wait for process")
              {
              process.Start();
              process.PriorityClass = System.Diagnostics.ProcessPriorityClass.Normal;
              if (conf.Notify) process.PriorityClass = System.Diagnostics.ProcessPriorityClass.AboveNormal;
              }
            if (conf.Sync) process.WaitForExit();
            }
          catch (Exception ex)
            {
            SaveWatcher.Win.Output("Cmd has an exception: " + ex.Message);
            }
          }
        }
      }

    private void Process_Exited(object sender, EventArgs e)
      {
      lock("wait for process")
        {
        System.Diagnostics.Process process = (System.Diagnostics.Process)sender;
        SaveWatcher.Win.Output("Cmd done with code: " + process.ExitCode + " > " + process.StartInfo.Arguments.Substring(3));
        if (process.PriorityClass == System.Diagnostics.ProcessPriorityClass.AboveNormal)
          SaveWatcher.Win.Activate();
        }
      }

    private string ReplacePlaceholder(string cmd, string docPath, Settings setting)
      {
      int startPos = 0;
      while ((startPos = cmd.IndexOf('{', startPos)) >= 0)
        {
        int endPos = Math.Max(startPos, cmd.IndexOf('}')) + 1;
        string Placeholder = cmd.Substring(startPos, endPos - startPos);
        string Value = string.Empty;
        switch (Placeholder)
          {
          case "{file_path}":
            Value = docPath;
            break;
          case "{file_name}":
            Value = Path.GetFileName(docPath);
            break;
          case "{file_name_without_ext}":
            Value = Path.GetFileNameWithoutExtension(docPath);
            break;
          case "{file_dir}":
            Value = Path.GetDirectoryName(docPath);
            break;
          case "{file_ext}":
            Value = Path.GetExtension(docPath);
            break;
          case "{settings_path}":
            Value = setting.Path + "\\" + SaveWatcher.Name;
            break;
          case "{settings_name}":
            Value = SaveWatcher.Name;
            break;
          case "{settings_dir}":
            Value = setting.Path;
            break;
          default:
            Value = null;
            break;
          }
        if (Value != null)
          {
          endPos = startPos + Value.Length;
          cmd = cmd.Replace(Placeholder, Value);
          }
        // else ignore unkown placeholder
        startPos = endPos;
        }

      return cmd;
      }

    #endregion
    }
  }
