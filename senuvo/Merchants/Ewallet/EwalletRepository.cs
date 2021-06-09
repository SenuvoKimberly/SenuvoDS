using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using DirectScale.Disco.Extension.Services;
using senuvo.Merchants.Ewallet.Interfaces;
using senuvo.Merchants.Ewallet.Models;

namespace senuvo.Merchants.Ewallet.Ewallet
{
    public class EwalletRepository : IEwalletRepository
    {
        private readonly IDataService _dataService;

        public EwalletRepository(IDataService dataService)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        }


        public EwalletSettings GetSettings()
        {
            using (var dbConnection = new SqlConnection(_dataService.ClientConnectionString.ConnectionString))
            {
                var settingsQuery = "SELECT * FROM Client.Ewallet_Settings";

                return dbConnection.QueryFirstOrDefault<EwalletSettings>(settingsQuery);
            }
        }

        public void UpdateSettings(EwalletSettingsRequest settings)
        {
            var parameters = new
            {
                settings.CompanyId,
                settings.PointAccountId,
                settings.Username,
                settings.Password,
                settings.BackUpMerchantId,
                settings.SplitPayment,
                settings.ApiUrl
            };
            using (var dbConnection = new SqlConnection(_dataService.ClientConnectionString.ConnectionString))
            {
                var query = @"UPDATE Client.Ewallet_Settings SET CompanyId = @CompanyId, PointAccountId = @PointAccountId, Username = @Username, Password = @Password, BackUpMerchantId = @BackUpMerchantId, SplitPayment = @SplitPayment,ApiUrl=@ApiUrl";

                dbConnection.Execute(query, parameters);
            }

        }

        public void ResetSettings()
        {
            try
            {
                var settings = GetSettings();
                var parameters = new
                {
                    Username = "senuvoapiuser",
                    Password = "8svMt8uJtm2C6DnJ",
                    ApiUrl = "https://rpmsapi.wsicloud.net/",
                    CompanyId = "606c0c41a0c4962410c28ce1",
                    PointAccountId = "606c0c41a0c4962410c28ce4"
                };

                using (var dbConnection = new SqlConnection(_dataService.ClientConnectionString.ConnectionString))
                {
                    var query = @"MERGE INTO Client.Ewallet_Settings WITH (HOLDLOCK) AS TARGET 
                USING 
                    (SELECT @Username AS 'Username', @Password AS 'Password', @ApiUrl AS 'ApiUrl', @CompanyId AS 'CompanyId', @PointAccountId AS 'PointAccountId'
                ) AS SOURCE 
                    ON SOURCE.CompanyId = TARGET.CompanyId
                WHEN MATCHED THEN 
                    UPDATE SET TARGET.Username = SOURCE.Username, TARGET.Password = SOURCE.Password, TARGET.ApiUrl = SOURCE.ApiUrl, TARGET.CompanyId = SOURCE.CompanyId
                WHEN NOT MATCHED BY TARGET THEN 
                    INSERT (Username, [Password], ApiUrl, CompanyId,PointAccountId) 
					VALUES (SOURCE.Username, SOURCE.Password, SOURCE.ApiUrl, SOURCE.CompanyId, SOURCE.PointAccountId);";

                    dbConnection.Execute(query, parameters);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
