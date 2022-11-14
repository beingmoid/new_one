﻿using PanoramBackend.Data;
using PanoramBackend.Data.Entities;
using NukesLab.Core.Repository;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace PanoramBackend.Data.Repository
{
    public class StaffOffBoradingRepository : EFRepository<StaffOffBoarding, int>, IStaffOffBoradingRepository
    {
        public StaffOffBoradingRepository(AMFContext requestScope) : base(requestScope)
        {

        }

    }
    public interface IStaffOffBoradingRepository : IEFRepository<StaffOffBoarding, int>
    {

    }

}