﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RdtClient.Data.Models.Data;
using RdtClient.Service.Models;
using RdtClient.Service.Services;

namespace RdtClient.Web.Controllers
{
    [Authorize]
    [Route("Api/Settings")]
    public class SettingsController : Controller
    {
        private readonly ISettings _settings;
        private readonly ITorrents _torrents;

        public SettingsController(ISettings settings, ITorrents torrents)
        {
            _settings = settings;
            _torrents = torrents;
        }

        [HttpGet]
        [Route("")]
        public async Task<ActionResult<IList<Setting>>> Get()
        {
            var result = await _settings.GetAll();
            return Ok(result);
        }

        [HttpPut]
        [Route("")]
        public async Task<ActionResult> Update([FromBody] SettingsControllerUpdateRequest request)
        {
            await _settings.Update(request.Settings);
            _torrents.Reset();

            return Ok();
        }

        [HttpGet]
        [Route("Profile")]
        public async Task<ActionResult<Profile>> Profile()
        {
            var profile = await _torrents.GetProfile();
            return Ok(profile);
        }
        
        [HttpPost]
        [Route("TestFolder")]
        public async Task<ActionResult<Profile>> TestFolder([FromBody] SettingsControllerTestFolderRequest request)
        {
            await _settings.TestFolder(request.Folder);

            return Ok();
        }
    }

    public class SettingsControllerUpdateRequest
    {
        public IList<Setting> Settings { get; set; }
    }

    public class SettingsControllerTestFolderRequest
    {
        public String Folder { get; set; }
    }
}