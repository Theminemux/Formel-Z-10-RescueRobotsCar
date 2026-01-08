using RescueRobotsCar.Models.Navigation;

namespace RescueRobotsCar.Services
{
    public class NavigatorService
    {
        private Logger _logger;

        private TrackMap Map = default!;

        public NavigatorService(Logger logger)
        {
            _logger = logger;
        }   

        public void LoadMap(TrackMap map)
        {
            Map = map;
            _logger.Log($"Map loaded with {Map.Track.Count} pieces", Logger.Severity.Info);
        }

        public bool GenerateRoute(TrackRoute route, int maxDepth, out TrackRoute FinalRoute)
        {
            TrackPiece startPiece = route.StartPiece;
            TrackPiece endPiece = route.EndPiece;
            TrackRoute Route = new TrackRoute()
            {
                StartPiece = startPiece,
                EndPiece = endPiece,
                Route = new List<TrackPiece>()
            };

            if (maxDepth <= 0 || maxDepth > 50)
                throw new ArgumentOutOfRangeException(nameof(maxDepth), "maxDepth must be greater than zero.");
            if (startPiece == endPiece)
                throw new ArgumentException("StartPiece and EndPiece cannot be the same.");

            List<List<TrackPiece>> route_tree = [];
            List<TrackPiece> current_path = [startPiece];
            route_tree.Add(current_path);
            int currentDepth = 0;
            bool found = false;

            while (currentDepth <= maxDepth && !found)
            {
                currentDepth++;
                foreach (var path in route_tree)
                {
                    foreach (var exit in path.Last().Exits)
                    {
                        if (!Map.GetPieceAt(exit.PiecePosition, out TrackPiece exitPiece))
                        {
                            _logger.Log($"Map is missing piece at {exit.PiecePosition}", Logger.Severity.Warning);
                            continue;
                        }
                        found = exitPiece == endPiece;
                        if (!found)
                        {
                            List<TrackPiece> new_path = new List<TrackPiece>(path) { exitPiece };
                            route_tree.Add(new_path);
                        } else
                        {
                            Route.Route = new List<TrackPiece>(path) { exitPiece };
                            FinalRoute = Route;
                            _logger.Log($"Route found with {Route.Route.Count} pieces", Logger.Severity.Info);
                            return true;
                        }
                    }
                }
            }
            FinalRoute = null!;
            _logger.Log($"No route found within depth {maxDepth}", Logger.Severity.Warning);
            return false;
        }
    }
}
