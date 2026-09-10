using System;
using EJLive.Core;
using EJLive.Core.Models;

namespace EJLive.Business
{
    public sealed class StableBusinessRuntime
    {
        public UnifiedSystemConfiguration Configuration { get; }
        public UnifiedRemoteCommandPolicy RemoteCommandPolicy { get; }
        public DatabaseManager Database { get; }

        public StableBusinessRuntime()
            : this(new UnifiedSystemConfiguration(), DatabaseManager.Instance, new UnifiedRemoteCommandPolicy())
        {
        }

        public StableBusinessRuntime(
            UnifiedSystemConfiguration configuration,
            DatabaseManager database,
            UnifiedRemoteCommandPolicy remoteCommandPolicy)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Database = database ?? throw new ArgumentNullException(nameof(database));
            RemoteCommandPolicy = remoteCommandPolicy ?? throw new ArgumentNullException(nameof(remoteCommandPolicy));
        }

        public RemoteCommandPolicyResult ValidateRemoteCommand(RemoteCommandEnvelope command)
        {
            return RemoteCommandPolicy.Validate(command);
        }
    }
}
