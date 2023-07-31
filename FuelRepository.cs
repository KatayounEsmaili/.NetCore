using Dapper;
using OPP.AspNetCore.ApiCore;
using OPP.AspNetCore.Logger;
using OPP.DomainClass.Common;
using OPP.Entities.PM;
using System;
using System.Threading.Tasks;

namespace OPP.API.PM.DataAccessLayer
{
    public interface IFuelRepository
    {
        Task<long> DeleteAsync(FuelEntity entity);

        //Task<FuelEntity> FindAsync(long id);

        Task<bool> InsertAsync(FuelEntity entity);

        Task<bool> UpdateAsync(FuelEntity entity);

        Task<DataPagingEntity<FuelEntity>> GetAsync(string createdByUsername, int page, int pageSize);

        Task<DataPagingEntity<FuelEntity>> GetAsync(Guid companyId, int page, int pageSize);
    }

    public class FuelRepository : IFuelRepository
    {
        private readonly ISQLConnectionFactory _connectionFactory;
        private readonly ILoggerService _logger;

        public FuelRepository(ISQLConnectionFactory connectionFactory, ILoggerService logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory)); ;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger)); ;
        }

        public async Task<bool> InsertAsync(FuelEntity entity)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                try
                {
                    string command = @"INSERT INTO [dbo].[Fuel] ([CreatedBy] ,[VehicleId] ,[Date] ,[Price] ,[Unit] ,[FuelType],[Odometer])
                                    VALUES (@CreatedBy ,@VehicleId ,@Date ,@Price ,@Unit ,@FuelType,@Odometer)";
                    return await connection.ExecuteAsync(command, entity) > 0;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    return false;
                }
            }
        }

        public async Task<bool> UpdateAsync(FuelEntity entity)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                string command = @"UPDATE [dbo].[Fuel]
                            SET [Price] = @Price ,[Unit] = @Unit ,[FuelType] = @FuelType
                            WHERE VehicleId=@VehicleId AND [Date]=@Date";
                return await connection.ExecuteAsync(command, entity) > 0;
            }
        }

        public async Task<long> DeleteAsync(FuelEntity entity)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                string command = @"Delete FROM [Fuel] WHERE VehicleId=@VehicleId AND [Date]=@Date";
                return await connection.ExecuteAsync(command, new { entity.VehicleId, entity.Date });
            }
        }

        //public async Task<FuelEntity> FindAsync(long id)
        //{
        //    using (var connection = _connectionFactory.CreateConnection())
        //    {
        //        string command = @"SELECT f.*, Vehicle.Caption AS VehicleCaption,Vehicle.Icon as VehicleIcon FROM [Fuel] as f inner join Vehicle as v ON f.VehicleId = v.Id WHERE f.Id=@id";
        //        return await connection.QueryFirstOrDefaultAsync<FuelEntity>(command, new { id });
        //    }
        //}

        public async Task<DataPagingEntity<FuelEntity>> GetAsync(string createdByUsername, int page, int pageSize)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                string query = @"SELECT        Fuel.VehicleId, Fuel.Date, Fuel.Price, Fuel.Unit, Fuel.FuelType, Fuel.CreatedBy, Fuel.Odometer, Vehicle.Caption AS VehicleCaption, Vehicle.CompanyId,Vehicle.Icon as VehicleIcon
                                    FROM   Fuel INNER JOIN
                                    Vehicle ON Fuel.VehicleId = Vehicle.Id
                                    Where  CreatedBy=@createdByUsername
                                    ORDER BY Fuel.Date DESC
                                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                                    Select COUNT(*) As TotalRows FROM   Fuel INNER JOIN
                                    Vehicle ON Fuel.VehicleId = Vehicle.Id
                                    Where  CreatedBy=@createdByUsername";

                DataPagingEntity<FuelEntity> results = new DataPagingEntity<FuelEntity>();

                try
                {
                    using (var gridReader = await connection.QueryMultipleAsync(query, new
                    {
                        Offset = (page - 1) * pageSize,
                        PageSize = pageSize,
                        createdByUsername
                    }))
                    {
                        results.Data = gridReader.Read<FuelEntity>();
                        results.ItemsCount = gridReader.ReadFirst<int>();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
                return results;
            }
        }

        public async Task<DataPagingEntity<FuelEntity>> GetAsync(Guid companyId, int page, int pageSize)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                string query = @"SELECT        Fuel.VehicleId, Fuel.Date, Fuel.Price, Fuel.Unit, Fuel.FuelType, Fuel.CreatedBy, Fuel.Odometer, Vehicle.Caption AS VehicleCaption, Vehicle.CompanyId,Vehicle.Icon as VehicleIcon
                                    FROM   Fuel INNER JOIN
                                    Vehicle ON Fuel.VehicleId = Vehicle.Id
                                    Where  CompanyId=@companyId
                                    ORDER BY Fuel.Date DESC
                                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                                    Select COUNT(*) As TotalRows FROM   Fuel INNER JOIN
                                    Vehicle ON Fuel.VehicleId = Vehicle.Id
                                    Where  CompanyId=@companyId";

                DataPagingEntity<FuelEntity> results = new DataPagingEntity<FuelEntity>();

                try
                {
                    using (var gridReader = await connection.QueryMultipleAsync(query, new
                    {
                        Offset = (page - 1) * pageSize,
                        PageSize = pageSize,
                        companyId
                    }))
                    {
                        results.Data = gridReader.Read<FuelEntity>();
                        results.ItemsCount = gridReader.ReadFirst<int>();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
                return results;
            }
        }
    }
}