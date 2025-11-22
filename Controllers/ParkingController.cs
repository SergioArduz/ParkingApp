using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkingApp.Data;
using ParkingApp.Models;

namespace ParkingApp.Controllers
{
    public class ParkingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ParkingController> _logger;

        public ParkingController(ApplicationDbContext context, ILogger<ParkingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Lista sesiones (abiertas y cerradas)
        public async Task<IActionResult> Index()
        {
            var sessions = await _context.ParkingSessions
                .Include(s => s.Vehicle)
                .OrderByDescending(s => s.EntryTime)
                .ToListAsync();
            return View(sessions);
        }

        // Registrar vehículo (si no existe) y crear sesión de entrada
        [HttpGet]
        public IActionResult CreateEntry()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateEntry(string plate, string? description)
        {
            if (string.IsNullOrWhiteSpace(plate))
            {
                ModelState.AddModelError("", "La placa es obligatoria.");
                return View();
            }

            plate = plate.Trim().ToUpper();

            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Plate == plate);
            if (vehicle == null)
            {
                vehicle = new Vehicle { Plate = plate, Description = description };
                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();
            }

            // Crear sesión de entrada
            var session = new ParkingSession
            {
                VehicleId = vehicle.VehicleId,
                EntryTime = DateTime.UtcNow,
                Status = "En parqueo"
            };
            _context.ParkingSessions.Add(session);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Entrada registrada para {plate} a las {session.EntryTime.ToLocalTime():g}";
            return RedirectToAction(nameof(Index));
        }

        // Vista para cerrar sesión (registrar salida)
        [HttpGet]
        public async Task<IActionResult> EndSession(int id)
        {
            var session = await _context.ParkingSessions.Include(s => s.Vehicle).FirstOrDefaultAsync(s => s.SessionId == id);
            if (session == null) return NotFound();
            return View(session);
        }

        [HttpPost]
        public async Task<IActionResult> EndSessionConfirmed(int id)
        {
            var session = await _context.ParkingSessions.Include(s => s.Vehicle).FirstOrDefaultAsync(s => s.SessionId == id);
            if (session == null) return NotFound();

            if (session.ExitTime != null)
            {
                TempData["Info"] = "La sesión ya fue cerrada.";
                return RedirectToAction(nameof(Index));
            }

            session.ExitTime = DateTime.UtcNow;
            var duration = session.ExitTime.Value - session.EntryTime;
            session.DurationMinutes = (int)duration.TotalMinutes;
            session.Status = "Fuera del Parqueo";

            // Reglas de recompensa: si supera 10 horas (600 minutos)
            session.ExitTime = DateTime.UtcNow;
            session.DurationMinutes = (int)duration.TotalMinutes;
            session.Status = "Fuera del Parqueo";

            // RECOMPENSA POR ACUMULACIÓN
            session.Reward = CalculateAccumulatedReward(session.Vehicle, session.DurationMinutes.Value);

            _context.Vehicles.Update(session.Vehicle);
            _context.ParkingSessions.Update(session);
            await _context.SaveChangesAsync();


            _context.Update(session);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Salida registrada. Duración: {session.DurationMinutes} minutos. Regalo: {session.Reward}";
            return RedirectToAction(nameof(Index));
        }

        // Detalle de sesión
        public async Task<IActionResult> Details(int id)
        {
            var session = await _context.ParkingSessions.Include(s => s.Vehicle).FirstOrDefaultAsync(s => s.SessionId == id);
            if (session == null) return NotFound();
            return View(session);
        }

        // Opcional: vista para crear vehículo sin iniciar sesión
        [HttpGet]
        public IActionResult CreateVehicle()
        {
            return View();
        }
        private string CalculateAccumulatedReward(Vehicle vehicle, int newMinutes)
        {
            vehicle.AccumulatedMinutes += newMinutes;

            if (vehicle.AccumulatedMinutes >= 600) // 10 horas
            {
                vehicle.AccumulatedMinutes -= 600; // se consumen 10 horas

                var rewards = new List<string>
        {
            "Limpiavidrios",
            "Vaselina",
            "Paño de microfibra"
        };

                return rewards[new Random().Next(rewards.Count)];
            }

            return "Aún no aplica";
        }
        public async Task<IActionResult> Vehicles()
        {
            var vehicles = await _context.Vehicles.ToListAsync();
            return View(vehicles);
        }

        [HttpPost]
        public async Task<IActionResult> CreateVehicle(Vehicle vehicle)
        {
            if (!ModelState.IsValid) return View(vehicle);

            vehicle.Plate = vehicle.Plate.Trim().ToUpper();
            var exists = await _context.Vehicles.AnyAsync(v => v.Plate == vehicle.Plate);
            if (exists)
            {
                ModelState.AddModelError("Plate", "La placa ya existe.");
                return View(vehicle);
            }

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
