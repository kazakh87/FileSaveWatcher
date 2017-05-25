
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace FileSaveWatcher
  {
  public class OutputWindow
    {
    private IVsOutputWindowPane mGeneralPane;

    public OutputWindow(IVsOutputWindow vsOutputWindow)
      {
      Guid generalPaneGuid = Microsoft.VisualStudio.VSConstants.GUID_OutWindowGeneralPane; // P.S. There's also the GUID_OutWindowDebugPane available.
      bool visible = true;
      bool clearWithSolution = false;
      vsOutputWindow.CreatePane(ref generalPaneGuid, SaveWatcher.OutputWindowName, Convert.ToInt32(visible), Convert.ToInt32(clearWithSolution));
      vsOutputWindow.GetPane(ref generalPaneGuid, out mGeneralPane);
      }

    public void Output(string Text)
      {
      mGeneralPane.OutputStringThreadSafe(Text + "\r\n");
      }

    public void Activate()
      {
      mGeneralPane.Activate();
      }
    }
  }
