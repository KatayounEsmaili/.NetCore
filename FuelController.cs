using Microsoft.AspNetCore.Mvc;
using OPP.API.PM.Models;
using OPP.API.PM.Services;
using OPP.AspNetCore.ApiCore;
using OPP.DomainClass.Common;
using OPP.Entities.PM;
using System;
using System.Net;
using System.Threading.Tasks;

namespace OPP.API.PM.Controllers
{
    [Route("api/[controller]")]
    [AuthorizeApi]
    public class FuelController : BaseCoreApiController
    {
        private readonly IFuelService _fuelService;

        public FuelController(IFuelService fuelService)
        {
            _fuelService = fuelService ?? throw new ArgumentNullException(nameof(fuelService)); ;
        }

        [HttpPost]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Post(AddFuelViewModel model)
        {
            if (model == null)
            {
                return BadRequest_NullModel();
            }

            var username = this.GetCurrentUsername();
            var companyId = this.GetCurrentCompanyId();

            var fuel = model.ToEntity(username);

            var insRes = await _fuelService.Insert(companyId, fuel);
            if (insRes)
            {
                return Ok(true);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest_Validations();
            }

            return OperationFailure();
        }

        [HttpPut]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Put(AddFuelViewModel model)
        {
            if (model == null)
            {
                return BadRequest_NullModel();
            }

            var username = this.GetCurrentUsername();
            var companyId = this.GetCurrentCompanyId();

            var fuel = model.ToEntity(username);

            var insRes = await _fuelService.Update(companyId, fuel);
            if (insRes)
            {
                return Ok(true);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest_Validations();
            }

            return OperationFailure();
        }
        [HttpGet("ByCreator")]
        [ProducesResponseType(typeof(DataPagingEntity<FuelEntity>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetByCreateor(int pageindex = 1, int pageSize = _pagesize)
        {
            var username = this.GetCurrentUsername();

            var model = await _fuelService.GetByCreatedUserAsync(username, pageindex, pageSize);
            if (model == null)
            {
                return NotFound();
            }

            return Ok(model);
        }

        [HttpGet("ByCompany")]
        [ProducesResponseType(typeof(DataPagingEntity<FuelEntity>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetByCompany(int pageindex = 1, int pageSize = _pagesize)
        {
            var companyId = this.GetCurrentCompanyId();

            var model = await _fuelService.GetByCompanyIdAsync(companyId, pageindex, pageSize);
            if (model == null)
            {
                return NotFound();
            }

            return Ok(model);
        }
    }
}