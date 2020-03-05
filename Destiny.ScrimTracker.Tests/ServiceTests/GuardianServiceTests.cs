using System;
using System.Threading.Tasks;
using Destiny.ScrimTracker.Logic.Models;
using Destiny.ScrimTracker.Logic.Repositories;
using Destiny.ScrimTracker.Logic.Services;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace Destiny.ScrimTracker.Tests.ServiceTests
{
    [TestFixture]
    public class GuardianServiceTests
    {
        private Guardian _testGuardian;        
        
        private IGuardianRepository _guardianRepository;
        private IGuardianEloRepository _eloRepository;
        private IGuardianEfficiencyRepository _efficiencyRepository;
        private IGuardianMatchResultsRepository _matchResultsRepository;
        
        private IGuardianService _guardianService;

        [SetUp]
        public void Setup()
        {
            _testGuardian = new Guardian() {GamerTag = "testy_mctesterson", Id = "abc-ez-as-123"};
            _guardianRepository = Substitute.For<IGuardianRepository>();
            _eloRepository = Substitute.For<IGuardianEloRepository>();
            _efficiencyRepository = Substitute.For<IGuardianEfficiencyRepository>();
            _matchResultsRepository = Substitute.For<IGuardianMatchResultsRepository>();

            _guardianRepository.CreateGuardian(Arg.Any<Guardian>()).ReturnsForAnyArgs(_testGuardian.Id);

            _guardianService = new GuardianService(_guardianRepository, _eloRepository, _efficiencyRepository,
                _matchResultsRepository);
        }

        [Test]
        public async Task CreateGuardian_MakesCorrectCalls()
        {
            // Arrange
            var guardianEfficiency = new GuardianEfficiency();
            var guardianElo = new GuardianElo();
            await _efficiencyRepository.UpdateGuardianEfficiency(Arg.Do<GuardianEfficiency>(eff => guardianEfficiency = eff));
            await _eloRepository.UpdateGuardianElo(Arg.Do<GuardianElo>(elo => guardianElo = elo));
            
            // Act
            await _guardianService.CreateGuardian(_testGuardian);

            // Assert
            await _guardianRepository.Received(1).CreateGuardian(_testGuardian);

            guardianEfficiency.GuardianId.Should().Be(_testGuardian.Id);
            guardianEfficiency.Id.Should().NotBeNullOrEmpty();
            guardianEfficiency.NewEfficiency.Should().Be(0.0);
            guardianEfficiency.PreviousEfficiency.Should().Be(0.0);
            
            guardianElo.GuardianId.Should().Be(_testGuardian.Id);
            guardianElo.Id.Should().NotBeNullOrEmpty();
            guardianElo.NewElo.Should().Be(1200);
            guardianElo.PreviousElo.Should().Be(0);
        }

        [Test]
        public void CreateGuardian_Throws_IfRepositoryThrows()
        {
            _guardianRepository.CreateGuardian(Arg.Any<Guardian>()).ThrowsForAnyArgs(new Exception("whoops :("));

            Assert.ThrowsAsync<Exception>(async () => await _guardianService.CreateGuardian(_testGuardian));
        }

        [Test]
        public async Task DeleteGuardian_Deletes_AllRelatedRecords()
        {
            await _guardianService.DeleteGuardian("guardianId");

            await _eloRepository.Received(1).DeleteGuardianElos("guardianId");
            await _efficiencyRepository.Received(1).DeleteGuardianEfficiencies("guardianId");
            await _matchResultsRepository.Received(1).DeleteAllResultsForGuardian("guardianId");
            await _guardianRepository.Received(1).DeleteGuardian("guardianId");
        }
    }
}