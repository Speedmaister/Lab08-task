using Lab08.Services.Contracts;
using Lab08.Task.ViewModels.VehicleRegistration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Lab08.Task.Controllers
{
    [Authorize]
    [Route("api/vehicle")]
    [ApiController]
    public class VehicleRegistrationController : ControllerBase
    {
        private readonly ILogger<VehicleRegistrationController> logger;
        private readonly IVehicleRegistrationService vehicleRegistrationService;

        public VehicleRegistrationController(ILogger<VehicleRegistrationController> logger,
            IVehicleRegistrationService vehicleRegistrationService)
        {
            this.logger = logger;
            this.vehicleRegistrationService = vehicleRegistrationService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]VehicleRegisterViewModel vehicle)
        {
            try
            {
                if(await vehicleRegistrationService.RegisterVehicleAsync(vehicle.ToModel()))
                {
                    return Ok();
                }

                return BadRequest("Parking lot doesn't have free space.");
            }
            catch(Services.Exceptions.ApplicationException ae)
            {
                logger.LogWarning(ae, "Error occurred.");
                return BadRequest(ae.ToString());
            }
            catch(Exception ex)
            {
                logger.LogWarning(ex, "Error occurred.");
                return BadRequest("Failed to register vehicle.");
            }
        }

        [HttpPut("unregister")]
        public async Task<IActionResult> Unregister([FromBody] VehicleUnregisterViewModel vehicle)
        {
            try
            {
                var amountToPay = await vehicleRegistrationService.UnregisterVehicleAsync(vehicle.RegistrationNumber);
                return Ok(new { AmountToPay = amountToPay });
            }
            catch (Services.Exceptions.ApplicationException ae)
            {
                logger.LogWarning(ae, "Error occurred.");
                return BadRequest(ae.ToString());
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error occurred.");
                return BadRequest("Failed to register vehicle.");
            }
        }
    }
}
