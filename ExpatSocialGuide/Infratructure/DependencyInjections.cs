using Infratructure.Interface;
using Infratructure.Mapper;
using Infratructure.Repository;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infratructure
{
    public static class DependencyInjections
    {
        public static IServiceCollection AddInfratructure(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(AutoMapperProfile));
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IHREmployeeRepository, HREmployeeRepository>();
            return services;
        }
    }
}