//------------------------------------------------------------------------------
// <copyright file="SaveWatcher.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FileSaveWatcher
  {
  /// <summary>
  /// This is the class that implements the package exposed by this assembly.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The minimum requirement for a class to be considered a valid package for Visual Studio
  /// is to implement the IVsPackage interface and register itself with the shell.
  /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
  /// to do it: it derives from the Package class that provides the implementation of the
  /// IVsPackage interface and uses the registration attributes defined in the framework to
  /// register itself and its components with the shell. These attributes tell the pkgdef creation
  /// utility what data to put into .pkgdef file.
  /// </para>
  /// <para>
  /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
  /// </para>
  /// </remarks>
  [PackageRegistration(UseManagedResourcesOnly = true)]
  [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 401)] // Info on this package for Help/About
  [Guid(SaveWatcher.PackageGuidString)]
  [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
  // start on complete load of a solution
  [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
  //[ProvideAutoLoad(VSConstants.UICONTEXT.ShellInitialized_string)] // startet nach initialisierung, doch löst keine solution/project events aus
  public sealed class SaveWatcher : Package
    {
    /// <summary>
    /// SaveWatcher OutputWindow Name
    /// </summary>
    public const string OutputWindowName = "SaveFileWatcher";

    /// <summary>
    /// SaveWatcher GUID string.
    /// </summary>
    public const string PackageGuidString = "0a1215a9-a365-471d-a597-ff9fd024f57f";

    /// <summary>
    /// Config File Name
    /// </summary>
    public const string Name = "SaveFileWatcherConfig";

    /// <summary>
    /// Initializes a new instance of the <see cref="SaveWatcher"/> class.
    /// </summary>
    public SaveWatcher()
      {
      // Inside this method you can place any initialization code that does not require
      // any Visual Studio service because at this point the package object is created but
      // not sited yet inside Visual Studio environment. The place to do all the other
      // initialization is the Initialize method.
      }

    #region Package Members

    public static OutputWindow Win;
    private VSEvents VsEv;

    //private ErrorGenerator gen;

    public static bool AutoGenerateConfig = true;
    public static string AutoGenerateConfigPaths = "\\wwwroot\\";

    /// <summary>
    /// Initialization of the package; this method is called right after the package is sited, so this is the place
    /// where you can put all the initialization code that rely on services provided by VisualStudio.
    /// </summary>
    protected override void Initialize()
      {
      base.Initialize();

      Win = new OutputWindow((IVsOutputWindow)GetGlobalService(typeof(SVsOutputWindow)));
      Win.Output("# " + OutputWindowName + " is started #");

      VsEv = new VSEvents((EnvDTE.DTE)GetService(typeof(EnvDTE.DTE)), new DocumentHandler());

      //gen = new ErrorGenerator();

      //ManualTriggerCmd.Initialize(this);
      }

      #endregion

      }
  }
