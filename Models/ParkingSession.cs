using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingApp.Models
{
    public class ParkingSession
    {
        [Key]
        public int SessionId { get; set; }

        [Required]
        public int VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }

        [Required]
        public DateTime EntryTime { get; set; }

        public DateTime? ExitTime { get; set; }

        // Duration in minutes (calculated on exit)
        public int? DurationMinutes { get; set; }

        public string? Reward { get; set; }

        [Required]
        public string Status { get; set; } = "En parqueo"; // or "Fuera del Parqueo"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
