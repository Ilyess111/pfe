using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;

namespace Soroubat.Api.Controllers
{
    /// <summary>
    /// Alertes intelligentes générées à partir des données BC.
    /// Chaque endpoint analyse un domaine métier et retourne les anomalies détectées.
    /// L'endpoint /all parallélise les 6 domaines pour minimiser le temps de réponse.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AlertsController : ControllerBase
    {
        private readonly IAlertService _alertService;

        private string UserProjectNo => User.FindFirst("projectNo")?.Value ?? string.Empty;

        public AlertsController(IAlertService alertService)
        {
            _alertService = alertService;
        }

        // ─── PAR DOMAINE ──────────────────────────────────────────────────────────

        /// <summary>Alertes sur les tâches projet (retard, blocage, non démarrage, dépassement budget).</summary>
        [HttpGet("site-management")]
        [ProducesResponseType(typeof(List<AlertDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<AlertDto>>> GetSiteManagementAlerts()
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                return Ok(await _alertService.GetSiteManagementAlertsAsync(UserProjectNo));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>Alertes sur les demandes d'achat (rejet, attente, échéance, demande vide).</summary>
        [HttpGet("purchase-requests")]
        [ProducesResponseType(typeof(List<AlertDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<AlertDto>>> GetPurchaseRequestAlerts()
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                return Ok(await _alertService.GetPurchaseRequestAlertsAsync(UserProjectNo));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>Alertes sur les ordres de transfert (transit bloqué, non expédié, réception partielle).</summary>
        [HttpGet("transfers")]
        [ProducesResponseType(typeof(List<AlertDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<AlertDto>>> GetTransferAlerts()
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                return Ok(await _alertService.GetTransferAlertsAsync(UserProjectNo));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>Alertes sur le stock chantier (négatif, critique, dormant).</summary>
        [HttpGet("stock")]
        [ProducesResponseType(typeof(List<AlertDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<AlertDto>>> GetStockAlerts()
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                return Ok(await _alertService.GetStockAlertsAsync(UserProjectNo));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>Alertes sur les pointages véhicules (non validé, surutilisation, index, panne, carburant).</summary>
        [HttpGet("vehicules")]
        [ProducesResponseType(typeof(List<AlertDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<AlertDto>>> GetVehiculeAlerts()
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                return Ok(await _alertService.GetVehiculeAlertsAsync(UserProjectNo));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>Alertes sur les fiches gasoil (non validée, index, consommation, véhicule manquant).</summary>
        [HttpGet("gasoil")]
        [ProducesResponseType(typeof(List<AlertDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<AlertDto>>> GetGasoilAlerts()
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                return Ok(await _alertService.GetGasoilAlertsAsync(UserProjectNo));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        // ─── AGRÉGATEUR GLOBAL ────────────────────────────────────────────────────

        /// <summary>
        /// Retourne toutes les alertes de tous les domaines en une seule requête.
        /// Les 6 domaines sont interrogés en parallèle (Task.WhenAll) pour minimiser
        /// le temps de réponse. Les alertes Critical apparaissent en premier.
        /// </summary>
        [HttpGet("all")]
        [ProducesResponseType(typeof(List<AlertDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<AlertDto>>> GetAllAlerts()
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                // Exécution parallèle des 6 domaines — réduit le temps de réponse
                // de (T1+T2+T3+T4+T5+T6) à max(T1, T2, T3, T4, T5, T6)
                var results = await Task.WhenAll(
                    _alertService.GetSiteManagementAlertsAsync(UserProjectNo),
                    _alertService.GetPurchaseRequestAlertsAsync(UserProjectNo),
                    _alertService.GetTransferAlertsAsync(UserProjectNo),
                    _alertService.GetStockAlertsAsync(UserProjectNo),
                    _alertService.GetVehiculeAlertsAsync(UserProjectNo),
                    _alertService.GetGasoilAlertsAsync(UserProjectNo)
                );

                var all = results
                    .SelectMany(r => r)
                    .OrderBy(a => a.Severity == "Critical" ? 0 : 1)
                    .ThenBy(a => a.DetectedAt)
                    .ToList();

                return Ok(all);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
    }
}