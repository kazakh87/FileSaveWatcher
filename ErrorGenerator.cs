//using Microsoft.VisualStudio;
//using Microsoft.VisualStudio.Shell.Interop;
//using System;


//namespace FileSaveWatcher
//  {

//public class ErrorGenerator : IVsSingleFileGenerator
//    {

//    public int DefaultExtension(out string pbstrDefaultExtension)
//      {
//      pbstrDefaultExtension = "";
//      return 0; //  throw new NotImplementedException();
//      }

//    public int Generate(string wszInputFilePath, string bstrInputFileContents, string wszDefaultNamespace, IntPtr[] rgbOutputFileContents, out uint pcbOutput, IVsGeneratorProgress pGenerateProgress)
//      {
//      pcbOutput = (uint)rgbOutputFileContents.Length;
//      //throw new NotImplementedException();
//      pGenerateProgress.GeneratorError(Convert.ToInt32(false), 0, "An error occured", 2, 4);
//      return VSConstants.S_OK;
//      }




//    }

//}
