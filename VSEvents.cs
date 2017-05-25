
// main VS events
using EnvDTE;

namespace FileSaveWatcher
  {

  public static class Extensions
    {
    public static string FullPath(this Project me)
      {
      string ProjectPath = me.FullName;
      if (!System.IO.Directory.Exists(ProjectPath))
        foreach (Property ProjectProperty in me.Properties)
          {
          if (ProjectProperty.Name == "FullPath")
            {
            ProjectPath = ProjectProperty.Value.ToString();
            break;
            }
          }
      return ProjectPath;
      }

    }

  public class VSEvents
    {
    private DTE mDte;
    private Events mEv;
    private SolutionEvents mSolutionEv;
    private DocumentEvents mDocEv;

    public string SolutionDir
      {
      get { return System.IO.Path.GetDirectoryName(mDte.Solution.FullName); }
      }

    public DocumentHandler mDocHndl;

    public VSEvents(DTE VsEv, DocumentHandler DocHndl)
      {
      mDocHndl = DocHndl;
      mDte = VsEv;
      mEv = mDte.Events;
      mDocEv = mEv.DocumentEvents;
      mDocEv.DocumentSaved += Config_Saved;
      mSolutionEv = mEv.SolutionEvents;
      BindSolutionEv();
      Solution_Opened(); // trigger the first opening self
      }

    private void Config_Saved(Document Document)
      {
      if (Document.Name == SaveWatcher.Name)
        {
        // reload Setting
        DocumentHandler.Settings ProjectSettings = mDocHndl.ContainsSetting(Document.ProjectItem.ContainingProject.UniqueName);
        if (ProjectSettings != null)
          {
          UnbindDocumentEv();
          ProjectSettings.Read(false);
          BindDocumentEv();
          }
        }
      }

    private bool mSolutionEvBind = false;
    private void BindSolutionEv()
      {
      if (!mSolutionEvBind)
        {
        mSolutionEvBind = true;
        mSolutionEv.Opened += Solution_Opened;
        mSolutionEv.AfterClosing += Solution_Closed;
        mSolutionEv.ProjectAdded += Solution_ProjectAdded;
        mSolutionEv.ProjectRemoved += Solution_ProjectRemoved;
        }
      }

    private bool mSolutionOpen = false;
    private void Solution_Opened()
      {
      if (!mSolutionOpen)
        {
        mSolutionOpen = true;
        SaveWatcher.Win.Output("Solution open: reload all config files.");
        mDocHndl.UnloadSettings();
        foreach (Project Project in mDte.Solution.Projects)
          mDocHndl.LoadSetting(Project.UniqueName, Project.FullPath());
        BindDocumentEv();
        }
      }

    private void Solution_Closed()
      {
      if (mSolutionOpen)
        {
        mSolutionOpen = false;
        SaveWatcher.Win.Output("Solution close: unload all config files.");
        UnbindDocumentEv();
        mDocHndl.UnloadSettings();
        }
      }


    private void Solution_ProjectAdded(Project Project)
      {
      if (mSolutionOpen && mDocHndl.ContainsSetting(Project.UniqueName) == null)
        {
        SaveWatcher.Win.Output("Project added: " + Project.UniqueName);
        UnbindDocumentEv();
        mDocHndl.LoadSetting(Project.UniqueName, Project.FullPath());
        BindDocumentEv();
        }
      }

    private void Solution_ProjectRemoved(Project Project)
      {
      if (mSolutionOpen && mDocHndl.ContainsSetting(Project.UniqueName) != null)
        {
        SaveWatcher.Win.Output("Project removed: " + Project.UniqueName);
        UnbindDocumentEv();
        mDocHndl.RemoveSetting(Project.UniqueName);
        BindDocumentEv();
        }
      }

    private bool mDocumentEvBind = false;
    private void BindDocumentEv()
      {
      lock (this)
        {
        if (!mDocumentEvBind)
          {
          mDocumentEvBind = true;
          if (mDocHndl.NeedOpeningEv) mDocEv.DocumentOpening += Document_Opening;
          if (mDocHndl.NeedOpenedEv) mDocEv.DocumentOpened += Document_Opened;
          if (mDocHndl.NeedSavedEv) mDocEv.DocumentSaved += Document_Saved;
          if (mDocHndl.NeedClosingEv) mDocEv.DocumentClosing += Document_Closing;
          }
        }
      }

    private void UnbindDocumentEv()
      {
      lock (this)
        {
        if (mDocumentEvBind)
          {
          mDocumentEvBind = false;
          mDocEv.DocumentOpening -= Document_Opening;
          mDocEv.DocumentOpened -= Document_Opened;
          mDocEv.DocumentSaved -= Document_Saved;
          mDocEv.DocumentClosing -= Document_Closing;
          }
        }
      }

    private void Document_Opening(string DocumentPath, bool ReadOnly)
      { mDocHndl.OnDocumentOpening(DocumentPath, ReadOnly); }

    private void Document_Opened(Document Document)
      { mDocHndl.OnDocumentOpened(Document); }

    private void Document_Saved(Document Document)
      { mDocHndl.OnDocumentSaved(Document); }

    private void Document_Closing(Document Document)
      { mDocHndl.OnDocumentClosing(Document); }
    }
  }
