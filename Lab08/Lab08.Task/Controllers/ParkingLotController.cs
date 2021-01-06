using Lab08.Services.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab08.Task.Controllers
{
    [Route("api/parkingLot")]
    [ApiController]
    public class ParkingLotController : ControllerBase
    {
        private readonly ILogger<ParkingLotController> logger;
        private readonly IParkingLotService parkingLotService;

        public ParkingLotController(ILogger<ParkingLotController> logger,
              IParkingLotService parkingLotService)
        {
            this.logger = logger;
            this.parkingLotService = parkingLotService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            logger.LogTrace("Getting parking lot description.");
            var parkingLot = await parkingLotService.Get();
            return Ok(new { availableSpace = parkingLot.AvailableSpace });
        }
    }
}
