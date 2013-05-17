using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Virtual.Engine.Camino;
using VideoToolkitLib;

namespace Virtual.Engine.Sockets
{
    public class CommandProcessor
    {
        #region Private Members

        private string _workingDirectory = "";
        private SocketCommand _currentCommand;
        private SocketCommand _returnCommandObject;

        private Output _previewScene = null;
        private Output _outputScene = null;

        #endregion

        #region Constructor

        public CommandProcessor(ref Output outputScene, ref Output previewScene)
        {
            _previewScene = previewScene;
            _outputScene = outputScene;
        }

        #endregion

        public SocketCommand ProcessCommand(SocketCommand commandToProcess)
        {
            _currentCommand = commandToProcess;
            _returnCommandObject = new SocketCommand();

            // extract command from command object 
            switch (commandToProcess.Command)
            {
                case CommandType.Initialize:
                    {
                        VX_DISPLAY_TYPES displayType = VX_DISPLAY_TYPES.VX_DISPLAY_VGA;
                        VX_FORMAT_TYPES formatType = VX_FORMAT_TYPES.VX_FORMAT_720P;

                        if (commandToProcess.Parameters.Count != 0)
                        {
                            CommandParameter displayTypeParameter = commandToProcess.Parameters.Find(delegate(CommandParameter foundList) { return foundList.Name == "DisplayType"; });
                            if (displayTypeParameter != null) { displayType = (VX_DISPLAY_TYPES)displayTypeParameter.Value; }

                            CommandParameter formatTypeParameter = commandToProcess.Parameters.Find(delegate(CommandParameter foundList) { return foundList.Name == "FormatType"; });
                            if (formatTypeParameter != null) { formatType = (VX_FORMAT_TYPES)formatTypeParameter.Value; }
                        }

                        CommandParameter workingDirectory = commandToProcess.Parameters.Find(delegate(CommandParameter foundList) { return foundList.Name == "WorkingDirectory"; });

                        _previewScene.WorkingDirectory = workingDirectory.Value.ToString();
                        _outputScene.WorkingDirectory = workingDirectory.Value.ToString();

                        _previewScene.InitializeScene(VX_DISPLAY_TYPES.VX_DISPLAY_VGA, formatType);
                        _outputScene.InitializeScene(displayType, formatType);

                        _returnCommandObject = null;

                        break;
                    }

                //#region "LoadTemplate"
                //// 
                //// LoadTemplate
                ////
                //case CommandType.LoadTemplate:
                //    {
                //        if (commandToProcess.Parameters.Count != 0)
                //        {
                //            //string templateName = GetParameter("TemplateName", commandToProcess);
                //            string templateDirectory = GetParameter("TemplateDirectory", commandToProcess);

                //            //if ((!string.IsNullOrEmpty(templateDirectory)) && (templateDirectory != _workingDirectory))
                //            //{
                //            //    templateName = templateDirectory + "\\" + templateName;
                //            //}

                //            string templateName = GetTemplateName(commandToProcess);

                //            if (!string.IsNullOrEmpty(templateName))
                //            {
                //                _returnCommandObject = GraphicsPlayer.gfxPlayer.LoadTemplate(templateName);
                //            }
                //            else
                //            {
                //                _returnCommandObject = CreateCommandFailedObject("TemplateName cannot be nothing",
                //                                                                commandToProcess.CommandID);
                //            }
                //            //CommandParameter outputFormatParameter = CommandToProcess.Parameters.Find(delegate(CommandParameter foundList) { return foundList.Name == "TemplateName"; });
                //            //if (outputFormatParameter != null) { outputFormat = outputFormatParameter.Value; }
                //        }
                //        break;

                //    }
                //#endregion

                // didn't really see a difference between ShowPage and UpdatePage commands; combining for now
                case CommandType.ShowPage:
                case CommandType.UpdatePage:
                    {
                        _returnCommandObject = ShowUpdatePage(commandToProcess);
                        break;
                    }

                case CommandType.HidePage:
                    {
                        if (commandToProcess.Parameters.Count != 0)
                        {
                            string templateName = GetParameter("TemplateName", commandToProcess).ToString();
                            string queueCommandString = GetParameter("QueueCommand", commandToProcess).ToString();
                            string destSceneString = GetParameter("DestScene", commandToProcess).ToString();
                            bool queueCommand = false;
                            Output destScene = _previewScene;

                            if (!string.IsNullOrEmpty(queueCommandString))
                            {
                                queueCommand = (queueCommandString == "TRUE");
                            }

                            if (!string.IsNullOrEmpty(destSceneString))
                            {
                                if (destSceneString.ToUpper() == "AIR")
                                {
                                    destScene = _outputScene;
                                }
                                else
                                {
                                    destScene = _previewScene;
                                }
                            }

                            if (!string.IsNullOrEmpty(queueCommandString))
                            {
                                queueCommand = (queueCommandString == "TRUE");
                            }

                            if (templateName != null)
                            {
                                try
                                {
                                    destScene.PageEngine.HidePage(templateName, queueCommand);
                                }
                                catch
                                {
                                    //for some reason HidePage was throwing an exception when a page wasn't visible...eventually figure out what the deal is
                                }
                            }
                            else
                            {
                                _returnCommandObject = CreateCommandFailedObject("TemplateName cannot be nothing",
                                                                                commandToProcess.CommandID);
                            }
                        }
                        break;
                    }
                case CommandType.SetCalibrationFile:
                    {
                        _outputScene.CalibrationFile = GetParameter("CalibrationFile", commandToProcess).ToString();
                        _previewScene.CalibrationFile = GetParameter("CalibrationFile", commandToProcess).ToString();
                        break;
                    }
                case CommandType.SetTelemetryFile:
                    {
                        _outputScene.TelemetryFile = GetParameter("TelemetryFile", commandToProcess).ToString();
                        _previewScene.TelemetryFile = GetParameter("TelemetryFile", commandToProcess).ToString();
                        break;
                    }
                default:
                    {
                        _currentCommand = null;
                        return null;
                    }
            }

            if (_returnCommandObject != null)
            {
                _returnCommandObject.CommandID = commandToProcess.CommandID;

                _currentCommand = null;

                if (_returnCommandObject.Command == CommandType.CommandFailed)
                {
                    //LogAccess.WriteLog("Command Failed. Reason = " + GetParameter("exception", _returnCommandObject), "CommandProcessor");
                }
            }

            //LogAccess.WriteLog("> Done Processing " + commandToProcess.Command, "CommandProcessor");
            return _returnCommandObject;
        }

        private SocketCommand ShowUpdatePage(SocketCommand commandToProcess)
        {
            SocketCommand returnCommandObject = new SocketCommand();

            if (commandToProcess.Parameters.Count != 0)
            {
                string templateName = GetParameter("TemplateName", commandToProcess).ToString();
                string destSceneString = GetParameter("DestScene", commandToProcess).ToString();
           
                CommandParameter queueCommandParameter = commandToProcess.Parameters.Find(delegate(CommandParameter foundList) { return foundList.Name == "QueueCommand"; });

                bool queueCommand = false;
                Output destScene = _previewScene;

                if (queueCommandParameter != null && !string.IsNullOrEmpty(queueCommandParameter.Value.ToString()))
                {
                    queueCommand = (queueCommandParameter.Value.ToString() == "TRUE");
                }

                if (!string.IsNullOrEmpty(destSceneString))
                {
                    if (destSceneString.ToUpper() == "AIR")
                    {
                        destScene = _outputScene;
                    }
                    else
                    {
                        destScene = _previewScene;
                    }
                }

                if (templateName != null)
                {
                    //see if this is a merge data without transitions
                    if (GetParameter("MergeDataWithoutTransitions", commandToProcess) != null)
                    {
                        destScene.PageEngine.MergeDataWithoutTransitions(templateName, templateName, commandToProcess.TemplateData);
                    }
                    else
                    {
                        // check if there is data to merge...
                        if (commandToProcess.TemplateData != null)
                        {
                            destScene.PageEngine.MergeData(templateName, templateName, commandToProcess.TemplateData);
                        }

                        if (returnCommandObject.Command != CommandType.CommandFailed)
                        {
                            destScene.PageEngine.ShowPage(templateName, false);
                        }
                    }
                }
                else
                {
                    returnCommandObject = CreateCommandFailedObject("TemplateName cannot be nothing", commandToProcess.CommandID);
                }
            }

            return returnCommandObject;
        }


        #region "Helper Functions"

        private bool FileExists(string filename)
        {
            try
            {
                string filepath = FileNameGenerator(filename);
                return File.Exists(filepath);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private string FileNameGenerator(string filename)
        {
            // temporary...
            string workingDirectory = "\\\\merlin\\emt\\SIG\\Applications\\Compressions\\previews\\";

            string workingName = filename;

            if (!workingName.EndsWith(".jpg"))
            {
                workingName = string.Concat(workingName, ".jpg");
            }
            workingName = string.Concat(workingDirectory, workingName);

            return workingName;
        }

        //public delegate void CommandCompleteEventHandler(PlayerCommand Command);
        //static event GFXPlayer.CommandCompleteEventHandler CommandCompleted;
        //static void OnCommandCompleted(PlayerCommand Command)
        //{
        //    //DataArrival(CommandtoRaise);
        //    if (CommandCompleted != null)
        //    {
        //        CommandCompleted(Command);
        //    }
        //}

        //private static void gfxCommandCompleted(PlayerCommand command)
        //{

        //    string filenameToSave = GetParameter("filename", _currentCommand);
        //    string templateName = GetParameter("Template", _currentCommand);

        //    if ((filenameToSave != null) & (templateName != null))
        //    {
        //        filenameToSave = FileNameGenerator(filenameToSave);
        //        _returnCommandObject = GraphicsPlayer.gfxPlayer.CaptureImage(templateName, filenameToSave);
        //    }
        // }

        private SocketCommand CreateCommandFailedObject(string messageDetail, string commandID)
        {
            SocketCommand returnCommandObject = new SocketCommand();

            returnCommandObject.Command = CommandType.CommandFailed;
            returnCommandObject.Parameters = new List<CommandParameter>();
            returnCommandObject.Parameters.Add(new CommandParameter("exception", messageDetail));
            returnCommandObject.CommandID = commandID;

            return returnCommandObject;
        }

        private object GetParameter(string parameterName, SocketCommand commandToSearch)
        {
            try
            {
                CommandParameter outputFormatParameter = commandToSearch.Parameters.Find(delegate(CommandParameter foundList) { return foundList.Name == parameterName; });
                if (outputFormatParameter != null) { return outputFormatParameter.Value; }

            }
            catch (Exception ex)
            {
                return null;
            }

            return null;
        }

        #endregion

    }
}
