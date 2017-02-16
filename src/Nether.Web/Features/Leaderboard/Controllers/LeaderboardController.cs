// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Nether.Data.Leaderboard;
using Nether.Integration.Analytics;
using Nether.Web.Features.Leaderboard.Configuration;
using Nether.Web.Utilities;
using System.Net;
using Microsoft.Extensions.Logging;
using Nether.Analytics.GameEvents;
using Nether.Web.Features.Leaderboard.Models.Leaderboard;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Nether.Web.Features.Leaderboard
{
    /// <summary>
    /// Leaderboard management
    /// </summary>
    [Route("leaderboards")]
    public class LeaderboardController : Controller
    {
        private readonly ILeaderboardStore _store;
        private readonly ILogger<LeaderboardController> _logger;
        private readonly ILeaderboardConfiguration _leaderboardConfiguration;

        public LeaderboardController(
            ILeaderboardStore store,
            ILogger<LeaderboardController> logger,
            ILeaderboardConfiguration leaderboardConfiguration
            )
        {
            _store = store;
            _logger = logger;
            _leaderboardConfiguration = leaderboardConfiguration;
        }

        /// <summary>
        /// Gets leaderboard by leaderboard configured name
        /// </summary>
        /// <param name="name">Name of the leaderboard</param>
        /// <returns>List of scores and gametags</returns>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(LeaderboardListResponseModel))]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [Authorize(Roles = RoleNames.PlayerOrAdmin)]
        [HttpGet("")]
        public IActionResult GetAll()
        {
            var gamerTag = User.GetGamerTag();

            var leaderboards = _leaderboardConfiguration.GetAll();

            return Ok(new LeaderboardListResponseModel
            {
                Leaderboards = leaderboards
                                .Select(l => new LeaderboardListResponseModel.LeaderboardSummaryModel
                                {
                                    Name = l.Name,
                                    _Link = Url.RouteUrl(nameof(Get), new { name = l.Name})
                                })
                                .ToList()
            });
        }

        /// <summary>
        /// Gets leaderboard by leaderboard configured name
        /// </summary>
        /// <param name="name">Name of the leaderboard</param>
        /// <returns>List of scores and gametags</returns>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(LeaderboardGetResponseModel))]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [Authorize(Roles = RoleNames.PlayerOrAdmin)]
        [HttpGet("{name}", Name = nameof(Get))]
        public async Task<IActionResult> Get(string name)
        {
            var gamerTag = User.GetGamerTag();

            LeaderboardConfig config = _leaderboardConfiguration.GetByName(name);
            if (config == null)
            {
                return NotFound();
            }
            LeaderboardType type = config.Type;
            List<GameScore> scores;
            switch (type)
            {
                case LeaderboardType.AroundMe:
                    scores = await _store.GetScoresAroundMeAsync(gamerTag, config.Radius);
                    break;
                case LeaderboardType.Top:
                    scores = await _store.GetTopHighScoresAsync(config.Top);
                    break;
                default:
                    scores = await _store.GetAllHighScoresAsync();
                    break;
            }

            // Format response model
            var resultModel = new LeaderboardGetResponseModel
            {
                Entries = scores == null ? null : scores.Select(s => (LeaderboardGetResponseModel.LeaderboardEntry)s).ToList()
            };

            // Return result
            return Ok(resultModel);
        }
    }
}

