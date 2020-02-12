using System;
using System.Collections;
using System.Collections.Generic;
using Destiny.ScrimTracker.Logic.Models;

namespace Destiny.ScrimTracker.Api.Requests
{
    public class CreateMatchRequest
    {
        public IEnumerable<MatchTeam> Teams { get; set; }
        public MatchType MatchType { get; set; }

        public Match ToMatch()
        {
            return new Match()
            {
                Id = $"{ModelIDPrefixes.Match}_{Guid.NewGuid():N}",
                MatchType = this.MatchType
            };
        }
    }
}