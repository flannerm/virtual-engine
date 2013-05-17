using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Virtual.Engine.Sockets
{
    public class CommandParameter // : ISerializable
    {
        public string Name { get; set; }
        public object Value { get; set; }

        public CommandParameter()
        {
            Name = null;
            Value = null;
        }

        public CommandParameter(string parameterName, object parameterValue)
        {
            Name = parameterName;
            Value = parameterValue;
        }
    }

    public enum CommandType
    {
        Unknown = 0,

        Initialize = 1,

        LoadTemplate = 2,
        ShowPage = 3,
        UpdatePage = 4,
        HidePage = 5,
        GeneratePreview = 6,

        CommandSuccessful = 7,
        CommandFailed = 8,

        Heartbeat = 9,
        ReceiptAcknowledgement = 10,

        GetStatus = 11,
        SetTimeout = 12,

        RequestData = 13,

        HitLocation = 14,

        SetCalibrationFile = 15,
        SetTelemetryFile = 16,
    }

    public class SocketCommand
    {
        public string ID { get; set; }
        public string CommandID { get; set; }
        public DateTime Timestamp { get; set; }
        public CommandType Command { get; set; }
        public string TemplateData { get; set; }
        public List<CommandParameter> Parameters { get; set; }
    }
}
