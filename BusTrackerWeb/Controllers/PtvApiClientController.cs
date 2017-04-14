﻿using BusTrackerWeb.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace BusTrackerWeb.Controllers
{
    /// <summary>
    /// Controls all Bus Tracker PTV API requests.
    /// </summary>
    public class PtvApiClientController
    {
        const string PTV_API_BASE_URL = "http://timetableapi.ptv.vic.gov.au";

        HttpClient Client { get; set; }

        PtvApiSignerModel ApiSigner { get; set; }

        /// <summary>
        /// Initialise the clent controller.
        /// </summary>
        public PtvApiClientController()
        {
            Client = new HttpClient();
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            ApiSigner = new Models.PtvApiSignerModel(Properties.Settings.Default.PtvApiDeveloperKey,
                Properties.Settings.Default.PtvApiDeveloperId);
        }

        /// <summary>
        /// Get all bus routes from the PTV API.
        /// </summary>
        /// <returns>Bus Route collection.</returns>
        public async Task<List<RouteModel>> GetRoutesAsync()
        {
            List<RouteModel> routes = new List<RouteModel>();

            // Get all bus type routes.
            string getRoutesRequest = string.Format("/v3/routes?route_types=2");
            PtvApiRoutesResponse routeResponse = 
                await GetPtvApiResponse<PtvApiRoutesResponse>(getRoutesRequest);

            // If the response is healthy try to convert the API response to a route collection.
            if(routeResponse.Status.Health == 1)
            {
                foreach(PtvApiRoute apiRoute in routeResponse.Routes)
                {
                    try
                    {
                        routes.Add(new RouteModel
                        {
                            RouteId = apiRoute.route_id,
                            RouteName = apiRoute.route_name,
                            RouteNumber = apiRoute.route_number
                        });
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Trace.TraceError("GetRoutesAsync Exception: {0}", e.Message);
                    }
                }
            }

            // Order by route name.
            routes = routes.OrderBy(r => r.RouteName).ToList();
                
            return routes;
        }

        public async Task<RouteModel> GetRouteAsync(int routeId)
        {
            RouteModel route = new RouteModel();

            // Get all bus type routes.
            string getRouteRequest = string.Format("/v3/routes/{0}", routeId);
            PtvApiRouteResponse routeResponse =
                await GetPtvApiResponse<PtvApiRouteResponse>(getRouteRequest);

            // If the response is healthy try to convert the API response to a route.
            if (routeResponse.Status.Health == 1)
            {
                try
                {
                    route = new RouteModel
                    {
                        RouteId = routeResponse.Route.route_id,
                        RouteName = routeResponse.Route.route_name,
                        RouteNumber = routeResponse.Route.route_number,
                        RouteType = routeResponse.Route.route_type
                    };
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.TraceError("GetRouteAsync Exception: {0}", e.Message);
                }
            }

            return route;
        }

        public async Task<DirectionModel> GetDirectionAsync(int directionId, RouteModel route)
        {
            DirectionModel direction = new DirectionModel();

            // Get all bus type routes.
            string getDirectionsRequest = string.Format("/v3/directions/{0}/route_type/2", directionId);
            PtvApiDirectionsResponse directionsResponse =
                await GetPtvApiResponse<PtvApiDirectionsResponse>(getDirectionsRequest);

            // If the response is healthy try to convert the API response to a direction.
            if (directionsResponse.Status.Health == 1)
            {
                try
                {
                    PtvApiDirection apiDirection = directionsResponse.Directions.First(d => d.route_id == route.RouteId);

                    direction.DirectionId = apiDirection.direction_id;
                    direction.DirectionName = apiDirection.direction_name;
                    direction.Route = route;
                    direction.RouteType = apiDirection.route_type;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.TraceError("GetRouteDirectionAsync Exception: {0}", e.Message);
                }

            }

            return direction;
        }

        /// <summary>
        /// Get all bus routes from the PTV API.
        /// </summary>
        /// <returns>Bus Route collection.</returns>
        public async Task<List<DirectionModel>> GetRouteDirectionsAsync(RouteModel route)
        {
            List<DirectionModel> directions = new List<DirectionModel>();

            // Get all route directions.
            string getDirectionsRequest = string.Format("/v3/directions/route/{0}", route.RouteId);
            PtvApiDirectionsResponse directionsResponse =
                await GetPtvApiResponse<PtvApiDirectionsResponse>(getDirectionsRequest);

            // If the response is healthy try to convert the API response to a direction collection.
            if (directionsResponse.Status.Health == 1)
            {
                foreach (PtvApiDirection apiDirection in directionsResponse.Directions)
                {
                    try
                    {
                        directions.Add(new DirectionModel
                        {
                            DirectionId = apiDirection.direction_id,
                            DirectionName = apiDirection.direction_name,
                            Route = route,
                            RouteType = apiDirection.route_type
                        });
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Trace.TraceError("GetRouteDirectionsAsync Exception: {0}", e.Message);
                    }
                }
            }

            // Order by route name.
            directions = directions.OrderBy(r => r.DirectionName).ToList();

            return directions;
        }


        /// <summary>
        /// Get the stops for a route from the PTV API.
        /// </summary>
        /// <returns>PTV API Stopping Pattern.</returns>
        public async Task<List<StopModel>> GetRouteStopsAsync(RouteModel route)
        {
            // Get all bus type routes.
            string getStopsRequest = string.Format("/v3/stops/route/{0}/route_type/2", route.RouteId);

            PtvApiStopOnRouteResponse stopsResponse =
                await GetPtvApiResponse<PtvApiStopOnRouteResponse>(getStopsRequest);

            // If the response is healthy try to convert the API response to a route collection.
            List<StopModel> stops = new List<StopModel>();
            if (stopsResponse.Status.Health == 1)
            {
                foreach (PtvApiStopOnRoute apiStop in stopsResponse.Stops)
                {
                    try
                    {
                        stops.Add(new StopModel
                        {
                            StopId = apiStop.stop_id,
                            StopName = apiStop.stop_name,
                            StopLatitude = apiStop.stop_latitude,
                            StopLongitude = apiStop.stop_longitude
                        });
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Trace.TraceError("GetRouteStopsAsync Exception: {0}", e.Message);
                    }
                }
            }

            return stops;
        }

        /// <summary>
        /// Get all route runs from the PTV API.
        /// </summary>
        /// <returns>PTV API Runs Response.</returns>
        public async Task<List<RunModel>> GetRouteRunsAsync(RouteModel route)
        {
            const int MAX_RUNS_TAKEN = 48; 

            // Get all stops on a run.
            List<StopModel> stops = await GetRouteStopsAsync(route);
            
            // Get all route runs.
            string getRunsRequest = string.Format("/v3/runs/route/{0}", route.RouteId);
            PtvApiRunResponse runResponse = await GetPtvApiResponse<PtvApiRunResponse>(getRunsRequest);

            // If the response is healthy try to convert the API response to a run collection.
            List<RunModel> runs = new List<RunModel>();
            if (runResponse.Status.Health == 1)
            {
                foreach (PtvApiRun apiRun in runResponse.Runs)
                {
                    try
                    {
                        runs.Add(new RunModel
                        {
                            RunId = apiRun.run_id,
                            Route = route,
                            FinalStop = stops.First(s => s.StopId == apiRun.final_stop_id)
                        });
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Trace.TraceError("GetRouteRunsAsync Exception: {0}", e.Message);
                    }
                }

                runs = runs.OrderByDescending(r => r.RunId).Take(MAX_RUNS_TAKEN).ToList();
            }

            return runs;
        }
        
        /// <summary>
        /// Get the departures for a specific run from the PTV API.
        /// </summary>
        /// <returns>PTV API Stopping Pattern.</returns>
        public async Task<StoppingPatternModel> GetStoppingPatternAsync(RunModel run)
        {
            StoppingPatternModel stoppingPattern = new StoppingPatternModel();

            // Get all stops on a run.
            List<StopModel> stops = await GetRouteStopsAsync(run.Route);

            // Get all bus type routes.
            string getPatternRequest = string.Format("/v3/pattern/run/{0}/route_type/2", run.RunId);

            PtvApiStoppingPattern patternResponse = 
                await GetPtvApiResponse<PtvApiStoppingPattern>(getPatternRequest);

            // If the response is healthy try to convert the API response to a pattern collections.
            if (patternResponse.Status.Health == 1)
            {
                // Convert depature objects.
                List<DepartureModel> departures = new List<DepartureModel>();
                
                // Convert disruption objects.
                List<DisruptionModel> disruptions = new List<DisruptionModel>();

                try
                {
                    foreach(PtvApiDeparture apiDeparture in patternResponse.Departures)
                    {
                        departures.Add(new DepartureModel {
                            Stop = new StopModel { StopId = apiDeparture.stop_id },
                            Route = new RouteModel { RouteId = apiDeparture.route_id },
                            RunId = apiDeparture.run_id,
                            Direction = new DirectionModel { DirectionId = apiDeparture.direction_id },
                            Disruptions = apiDeparture.disruption_ids,
                            ScheduledDeparture = DateTime.Parse(apiDeparture.scheduled_departure_utc, null, DateTimeStyles.AssumeLocal),
                            AtPlatform = apiDeparture.at_platform,
                            PlatformNumber = apiDeparture.platform_number,
                            Flags = apiDeparture.flags
                        });
                    }

                    foreach(PtvApiDisruption apiDisruption in patternResponse.Disruptions)
                    {
                        disruptions.Add(new DisruptionModel {
                            DisruptionId = apiDisruption.disruption_id,
                            Title = apiDisruption.title,
                            Url = apiDisruption.url,
                            Description = apiDisruption.description,
                            DisruptionStatus = apiDisruption.disruption_status,
                            DisruptionType = apiDisruption.disruption_type,
                            PublishedOn = apiDisruption.published_on,
                            LastUpdated = apiDisruption.last_updated,
                            FromDate = apiDisruption.from_date,
                            ToDate = apiDisruption.to_date,
                        });
                    }

                    stoppingPattern.Departures = departures;
                    stoppingPattern.Disruptions = disruptions;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.TraceError("GetRunPatternAsync Exception: {0}", e.Message);
                }
            }

            return stoppingPattern;
        }

        /// <summary>
        /// Get all bus routes from the PTV API.
        /// </summary>
        /// <returns>Bus Route collection.</returns>
        public async Task<DepartureModel> GetStopDepartureAsync(StopModel stop, RouteModel route, DirectionModel direction)
        {
            List<DepartureModel> departures = new List<DepartureModel>();
            DepartureModel nextDeparture = new DepartureModel();

            // Get all departures for a route stop.
            string getDeparturesRequest =  
                string.Format("/v3/departures/route_type/2/stop/{0}/route/{1}", stop.StopId, 
                route.RouteId);

            PtvApiDeparturesResponse departuresResponse =
                await GetPtvApiResponse<PtvApiDeparturesResponse>(getDeparturesRequest);


            // If the response is healthy try to convert the API response to a route collection.
            if (departuresResponse.Status.Health == 1)
            {
                // Filter required direction.
                departuresResponse.Departures = departuresResponse.Departures.Where(d => d.direction_id == direction.DirectionId).ToList();

                foreach (PtvApiDeparture apiDeparture in departuresResponse.Departures)
                {
                    try
                    {
                        departures.Add(new DepartureModel
                        {
                            Stop = stop,
                            Route = route,
                            RunId = apiDeparture.run_id,
                            Direction = direction,
                            Disruptions = apiDeparture.disruption_ids,
                            ScheduledDeparture = DateTime.Parse(apiDeparture.scheduled_departure_utc, null, DateTimeStyles.AssumeLocal),
                            AtPlatform = apiDeparture.at_platform,
                            PlatformNumber = apiDeparture.platform_number,
                            Flags = apiDeparture.flags
                        });
                    } 
                    catch (Exception e)
                    {
                        System.Diagnostics.Trace.TraceError("GetDepartureAsync Exception: {0}", e.Message);
                    }
                }
            }

            // Order by route name.
            departures = departures.OrderBy(d => d.ScheduledDeparture).ToList();

            // Filter out past departures.
            departures = departures.Where(d => d.ScheduledDeparture >= DateTime.Now).ToList();

            // Return the next scheduled departure.
            if (departures.Count != 0)
            {
                nextDeparture = departures.First();
            }

            return nextDeparture;
        }


        /// <summary>
        /// Generic PTV API Get Request function.
        /// </summary>
        /// <typeparam name="T">PTV API object type to be retrieved.</typeparam>
        /// <param name="request">API request string.</param>
        /// <returns>The API response.</returns>
        private async Task<T> GetPtvApiResponse<T>(string request)
        {
            T response = default(T);

            try
            {
                // Sign the API request with developer ID and key.
                string clientRequest = ApiSigner.SignApiUrl(PTV_API_BASE_URL, request);

                // Send a request to the PTV API.
                HttpResponseMessage httpResponse = await Client.GetAsync(clientRequest);

                if (httpResponse.IsSuccessStatusCode)
                {
                    // Deserialise the JSON API response into strongly typed objects.
                    response = await httpResponse.Content.ReadAsAsync<T>();
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError("GetPtvApiResponse Exception: {0}", e.Message);
            }

            return response;
        }
    }
}