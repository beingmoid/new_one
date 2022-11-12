﻿


using PanoramBackend.Data;
using PanoramBackend.Data.Entities;
using NukesLab.Core.Repository;
using System;
using System.Collections.Generic;
using System.Text;
using PanoramBackend.Data.CatalogDb;

namespace PanoramBackend.Data.CatalogDb.Repos
{
    public class DatabasesRepo : EFRepository<Databases, int>, IDatabasesRepo
    {
        public DatabasesRepo(CatalogDbContext requestScope) : base(requestScope)
        {

        }
    }
    public interface IDatabasesRepo : IEFRepository<Databases, int>
    {

    }
}
