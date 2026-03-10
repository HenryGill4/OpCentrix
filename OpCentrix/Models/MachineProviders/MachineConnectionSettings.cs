using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models.MachineProviders
{
    /// <summary>
    /// Per-machine connectivity configuration stored in the database.
    /// Separates "how to connect" from the machine capability model (SlsMachine).
    /// </summary>
    public class MachineConnectionSettings
    {
        public int Id { get; set; }

        /// <summary>Matches SlsMachine.MachineId (e.g. "TI1", "INC1").</summary>
        [Required]
        [StringLength(50)]
        public string MachineId { get; set; } = string.Empty;

        /// <summary>
        /// Selects which IMachineProvider implementation to use.
        /// Values: "Mock", "EOS"
        /// </summary>
        [Required]
        [StringLength(50)]
        public string ProviderType { get; set; } = "Mock";

        #region EOS EOSCONNECT REST API

        /// <summary>Base URL for the EOSCONNECT Web API, e.g. http://192.168.1.10/gui/webapi</summary>
        [StringLength(300)]
        public string? RestApiBaseUrl { get; set; }

        /// <summary>OAuth2 client ID issued via the printer's web interface.</summary>
        [StringLength(200)]
        public string? OAuthClientId { get; set; }

        /// <summary>OAuth2 client secret — stored encrypted at rest.</summary>
        [StringLength(500)]
        public string? OAuthClientSecretEncrypted { get; set; }

        #endregion

        #region OPC UA

        /// <summary>OPC UA server endpoint, e.g. opc.tcp://192.168.1.10:4840</summary>
        [StringLength(300)]
        public string? OpcUaEndpointUrl { get; set; }

        [StringLength(100)]
        public string? OpcUaUsername { get; set; }

        /// <summary>Hashed OPC UA password.</summary>
        [StringLength(200)]
        public string? OpcUaPasswordHash { get; set; }

        /// <summary>
        /// Whether job control commands (start/pause/resume) are enabled.
        /// Requires the EOS OPC UA Control license.
        /// </summary>
        public bool JobControlEnabled { get; set; } = false;

        #endregion

        #region Sync Behaviour

        [Range(5, 3600)]
        public int PollIntervalSeconds { get; set; } = 30;

        public bool IsEnabled { get; set; } = true;

        #endregion

        #region Sync Health

        public DateTime? LastSuccessfulSync { get; set; }

        [StringLength(500)]
        public string? LastSyncError { get; set; }

        public int ConsecutiveFailures { get; set; } = 0;

        #endregion

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
    }
}
