using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Soroubat.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AlertsController : ControllerBase
    {
        private readonly IAlertService _alertService;

        public AlertsController(IAlertService alertService)
        {
            _alertService = alertService;
        }

        private string UserProjectNo => User.FindFirst("projectNo")?.Value ?? string.Empty;

        // ── PARTIE 1 : SiteManagement (inchangé) ────────────────────────────────

        [HttpGet("site-management")]
        [ProducesResponseType(typeof(List<AlertDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<AlertDto>>> GetSiteManagementAlerts()
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return BadRequest("Aucun projet assigné dans votre profil.");
            try
            {
                return Ok(await _alertService.GetSiteManagementAlertsAsync(UserProjectNo));
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        // ── PARTIE 2 : PurchaseRequest (NOUVEAU) ────────────────────────────────

        /// <summary>
        /// Retourne les alertes liées aux demandes d'achat du projet connecté.
        /// </summary>
        [HttpGet("purchase-requests")]
        [ProducesResponseType(typeof(List<AlertDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<AlertDto>>> GetPurchaseRequestAlerts()
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return BadRequest("Aucun projet assigné dans votre profil.");
            try
            {
                return Ok(await _alertService.GetPurchaseRequestAlertsAsync(UserProjectNo));
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        // ── PARTIE 3 : Transfer Order (NOUVEAU) ─────────────────────────────────

  
        [HttpGet("transfers")]
        [ProducesResponseType(typeof(List<AlertDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<AlertDto>>> GetTransferAlerts()
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return BadRequest("Aucun projet assigné dans votre profil.");
            try
            {
                return Ok(await _alertService.GetTransferAlertsAsync(UserProjectNo));
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        // ── PARTIE 4 : Stock (NOUVEAU) ───────────────────────────────────────────

        [HttpGet("stock")]
        [ProducesResponseType(typeof(List<AlertDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<AlertDto>>> GetStockAlerts()
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return BadRequest("Aucun projet assigné dans votre profil.");
            try
            {
                return Ok(await _alertService.GetStockAlertsAsync(UserProjectNo));
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        // ── PARTIE 5 : Véhicule (NOUVEAU) ───────────────────────────────────────

        [HttpGet("vehicules")]
        [ProducesResponseType(typeof(List<AlertDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<AlertDto>>> GetVehiculeAlerts()
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return BadRequest("Aucun projet assigné dans votre profil.");
            try
            {
                return Ok(await _alertService.GetVehiculeAlertsAsync(UserProjectNo));
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        // ── PARTIE 6 : Gasoil (NOUVEAU) ──────────────────────────────────────────

        [HttpGet("gasoil")]
        [ProducesResponseType(typeof(List<AlertDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<AlertDto>>> GetGasoilAlerts()
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return BadRequest("Aucun projet assigné dans votre profil.");
            try
            {
                return Ok(await _alertService.GetGasoilAlertsAsync(UserProjectNo));
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        // ── AGRÉGATEUR GLOBAL — version finale complète (toutes parties) ─────────

        [HttpGet("all")]
        [ProducesResponseType(typeof(List<AlertDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<AlertDto>>> GetAllAlerts()
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return BadRequest("Aucun projet assigné dans votre profil.");
            try
            {
                var all = new List<AlertDto>();

                all.AddRange(await _alertService.GetSiteManagementAlertsAsync(UserProjectNo));
                all.AddRange(await _alertService.GetPurchaseRequestAlertsAsync(UserProjectNo));
                all.AddRange(await _alertService.GetTransferAlertsAsync(UserProjectNo));
                all.AddRange(await _alertService.GetStockAlertsAsync(UserProjectNo));
                all.AddRange(await _alertService.GetVehiculeAlertsAsync(UserProjectNo));
                all.AddRange(await _alertService.GetGasoilAlertsAsync(UserProjectNo)); // ← NOUVEAU

                return Ok(all.OrderBy(a => a.Severity == "Critical" ? 0 : 1).ToList());
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }
    }
}