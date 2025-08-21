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

            return services;
        }
    }
}