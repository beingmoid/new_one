﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NukesLab.Core.Repository;
using PanoramaBackend.Controllers;
using PanoramaBackend.Data.Entities;
using PanoramaBackend.Services.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PanoramaBackend.Api.Controllers
{

    public class TeamsController : BaseController<Teams,int>
    {
        public TeamsController(RequestScope requestScope,ITeamsService
            service)
            :base(requestScope,service)
        {

        }
    }
}
