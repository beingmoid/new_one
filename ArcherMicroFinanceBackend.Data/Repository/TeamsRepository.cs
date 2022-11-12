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
    public class TeamsRepository : EFRepository<Teams, int>, ITeamsRepository
    {
        public TeamsRepository(AMFContext requestScope) : base(requestScope)
        {

        }

    }
    public interface ITeamsRepository : IEFRepository<Teams, int>
    {

    }

}
