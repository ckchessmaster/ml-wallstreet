using Microsoft.Extensions.Configuration;
using MLWallstreetUI.Data.Enums;
using MLWallstreetUI.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MLWallstreetUI.Services
{
    public class ApiService
    {
        private readonly Dictionary<ServiceApiType, ServiceApi> serviceApis;

        public ApiService(IConfiguration configuration)
        {
            serviceApis = new Dictionary<ServiceApiType, ServiceApi>
            {
                { ServiceApiType.DataManagerApi, new ServiceApi(configuration, configuration.GetValue<string>("DataManagerApi:Url")) },
                { ServiceApiType.MLServiceApi, new ServiceApi(configuration, configuration.GetValue<string>("MLServiceAPI:Url")) }
            };
        }

        public async Task<ServiceApi> GetApi(ServiceApiType api)
        {
            if (!serviceApis[api].IsAuthenticated())
            {
                await serviceApis[api].Authenticate();
            }

            return serviceApis[api];
        }
    }
}
