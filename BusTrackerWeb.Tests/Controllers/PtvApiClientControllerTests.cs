﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using BusTrackerWeb.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusTrackerWeb.Models;

namespace BusTrackerWeb.Controllers.Models.Tests
{
    [TestClass()]
    public class PtvApiClientControllerTests
    {
        [TestMethod()]
        public async Task GetRoutesAsyncTest()
        {
            PtvApiClientController apiControl = new PtvApiClientController();

            PtvApiRouteResponse response = await apiControl.GetRoutesAsync();

            PtvApiRoute testRoute =
                response.Routes.Find(r => r.route_name.Contains("Geelong Station") &&
                r.route_name.Contains("Deakin University") &&
                r.route_number == "41");

            Assert.IsTrue(testRoute.route_id == 10846);
            Assert.IsTrue(response.Status.Health == 1);
        }

        [TestMethod()]
        public async Task GetRouteRunsAsyncTest()
        {
            PtvApiClientController apiControl = new PtvApiClientController();

            PtvApiRunResponse response = await apiControl.GetRouteRunsAsync(10846);

            PtvApiRun testRun = response.Runs.OrderByDescending(r => r.run_id).First();

            Assert.IsNotNull(testRun);
            Assert.IsTrue(response.Status.Health == 1);
        }

        [TestMethod()]
        public async Task GetRoutePatternAsyncTest()
        {
            PtvApiClientController apiControl = new PtvApiClientController();

            PtvApiStoppingPattern response = await apiControl.GetRoutePatternAsync(80843);

            Assert.IsTrue(response.Departures.Count == 36);
            Assert.IsTrue(response.Status.Health == 1);
        }

        [TestMethod()]
        public async Task GetRouteStopsAsyncTest()
        {
            PtvApiClientController apiControl = new PtvApiClientController();

            PtvApiStopOnRouteResponse response = await apiControl.GetRouteStopsAsync(10846);

            Assert.IsTrue(response.Stops.Count == 52);
            Assert.IsTrue(response.Status.Health == 1);
        }
    }
}