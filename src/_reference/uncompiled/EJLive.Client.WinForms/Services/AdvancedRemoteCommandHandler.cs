using System;
using EJLive.Core.Models;

namespace EJLive.Client.WinForms.Services
{
    /// <summary>
    /// Advanced command facade that reuses the unified RemoteCommandHandler.
    /// </summary>
    public sealed class AdvancedRemoteCommandHandler
    {
        private readonly RemoteCommandHandler _handler;

        public event Action<string>? OnLogMessage;
        public event Action<RemoteCommand>? OnCommandExecuted;

        public AdvancedRemoteCommandHandler(string atmId)
        {
            _handler = new RemoteCommandHandler(atmId);
            _handler.OnLogMessage += message => OnLogMessage?.Invoke(message);
            _handler.OnCommandExecuted += (_, command) => OnCommandExecuted?.Invoke(command);
        }

        public AdvancedRemoteCommandHandler(string atmId, NetworkManager network, Action? forceSyncAction = null)
        {
            _handler = new RemoteCommandHandler(atmId, network, forceSyncAction);
            _handler.OnLogMessage += message => OnLogMessage?.Invoke(message);
            _handler.OnCommandExecuted += (_, command) => OnCommandExecuted?.Invoke(command);
        }

        public bool AllowProcessControl
        {
            get => _handler.AllowProcessControl;
            set => _handler.AllowProcessControl = value;
        }

        public RemoteCommand ExecuteCommand(RemoteCommand command) => _handler.ExecuteCommand(command);

        public void ProcessCommand(string command, string parameters) => _handler.ProcessCommand(command, parameters);
    }
}
