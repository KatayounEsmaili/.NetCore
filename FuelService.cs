using OPP.API.PM.DataAccessLayer;
using OPP.AspNetCore.ApiCore;
using OPP.AspNetCore.Logger;
using OPP.DomainClass.Common;
//using OPP.DomainClass.Common;
using OPP.Entities.PM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPP.API.PM.Services
{
    public interface IFuelService
    {
        Task<DataPagingEntity<FuelEntity>> GetByCompanyIdAsync(Guid companyId, int page, int pageSize);

        Task<DataPagingEntity<FuelEntity>> GetByCreatedUserAsync(string createdByUsername, int page, int pageSize);

        Task<bool> Insert(Guid companyId, FuelEntity entity);

        Task<bool> Update(Guid companyId, FuelEntity entity);

        bool Validate(FuelEntity entity);
    }

    public class FuelService : IFuelService
    {
        private readonly IFuelRepository _fuelRepo;
        private readonly IOdometerService _odometerService;

        private IModelstateService _validation;
        private readonly ILoggerService _logger;

        public FuelService(IFuelRepository fuelRepository,
                           ILoggerService logger,
                           IOdometerService odometerService,
                           IModelstateService validation)
        {
            _fuelRepo = fuelRepository;
            _odometerService = odometerService;
            _validation = validation;
            _logger = logger;
        }

        public bool Validate(FuelEntity entity)
        {
            if (entity.VehicleId == 0)
            {
                _validation.Add(nameof(entity.VehicleId), "انتخاب خودرو الزامی است");
            }

            var today = DateTime.UtcNow.Date;

            if (entity.Date.Date > today.Date)
            {
                _validation.Add(nameof(entity.Date), " تاریخ سوخت گیری نمی تواند تاریخی در آینده باشد");
            }

            return _validation.IsValid;
        }

        public async Task<bool> Insert(Guid companyId, FuelEntity entity)
        {
            bool result = false;
            try
            {
                if (Validate(entity))
                {
                    OdometerEntity newOdo = null;

                    if (entity.Odometer != null)
                    {
                        newOdo = new OdometerEntity()
                        {
                            Date = entity.Date,
                            KiloMeter = entity.Odometer.Value,
                            VehicleId = entity.VehicleId,
                            Void = false,
                            SourceType = SourceTable.Fuel,
                            CompanyId = companyId
                        };
                    }

                    if (await _odometerService.ValidateAsync(newOdo))
                    {
                        result = await _fuelRepo.InsertAsync(entity);
                        if (result && entity.Odometer != null)
                        {
                            result = await _odometerService.InsertAsync(newOdo);
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex); ;
                return result;
            }
        }

        public async Task<bool> Update(Guid companyId, FuelEntity entity)
        {
            bool result = false;
            try
            {
                if (Validate(entity))
                {
                    result = await _fuelRepo.UpdateAsync(entity);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex); ;
                return result;
            }
        }


        public async Task<DataPagingEntity<FuelEntity>> GetByCompanyIdAsync(Guid companyId, int page, int pageSize)
        {
            var result = await _fuelRepo.GetAsync(companyId, page, pageSize);
            return new DataPagingEntity<FuelEntity>()
            {
                Data = result.Data != null && result.Data.Any() ? result.Data.ToList() : new List<FuelEntity>(),
                ItemsCount = result.ItemsCount
            };
        }

        public async Task<DataPagingEntity<FuelEntity>> GetByCreatedUserAsync(string createdByUsername, int page, int pageSize)
        {
            var result = await _fuelRepo.GetAsync(createdByUsername, page, pageSize);
            return new DataPagingEntity<FuelEntity>()
            {
                Data = result.Data != null && result.Data.Any() ? result.Data.ToList() : new List<FuelEntity>(),
                ItemsCount = result.ItemsCount
            };
        }
    }
}