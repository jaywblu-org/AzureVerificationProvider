using AzureVerificationProvider.Data.Enttities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureVerificationProvider.Data.Contexts;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<VerificationRequestEntity> VerificationRequests { get; set; }
}
