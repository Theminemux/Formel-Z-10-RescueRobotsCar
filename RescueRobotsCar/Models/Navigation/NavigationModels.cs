namespace RescueRobotsCar.Models.Navigation
{
    public enum SkyDirection
    {
        North   = 0,
        East    = 1,
        South   = 2,
        West    = 3
    }
    
    public enum RelativeDirection
    {
        Forward     = 0,
        Backward    = 1,
        Left        = 2,
        Right       = 3
    }

    public class TrackMap
    {
        private List<TrackPiece> _track = [];
        private readonly Dictionary<Position, TrackPiece> _grid = new();

        public required List<TrackPiece> Track 
        { 
            get => _track;
            set
            {
                _track = value;
                BuildGrid();
            }
        }

        private void BuildGrid()
        {
            _grid.Clear();
            foreach (var piece in _track)
            {
                _grid[piece.Position] = piece;
            }
        }

        public bool GetPieceAt(Position pos, out TrackPiece trackpiece)
        {
            return _grid.TryGetValue(pos, out trackpiece!);
        }
    }

    public class TrackPiece
    {
        public enum PieceType
        {
            Straight        = 0,
            Curve           = 1,
            Intersection    = 2,
            Start           = 3,
            End             = 4,
            Obstacle        = 5,
            Roadless        = 6
        }
        public required Position Position { get; set; } 
        public required PieceType Type { get; set; } 
        public required bool Drivable { get; set; }
        public required List<Exit> Exits { get; set; }
        public Metadata? Metadata { get; set; }
    }

    public struct Exit
    {
        public required SkyDirection Direction { get; set; }
        public required Position PiecePosition { get; set; }
    }

    public struct Position : IEquatable<Position>
    {
        public required int X { get; set; }
        public required int Y { get; set; }
        public required int Level { get; set; }

        public bool Equals(Position other) =>
            X == other.X && Y == other.Y && Level == other.Level;

        public override bool Equals(object? obj) =>
            obj is Position other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(X, Y, Level);
    }

    public struct Metadata
    {
        public string? Note { get; set; }
        public bool? SpecialUnderground { get; set; }
        public bool? Ramp { get; set; }
    }

    public class TrackRoute
    {
        public required TrackPiece StartPiece { get; set; } 
        public required TrackPiece EndPiece { get; set; } 
        public List<TrackPiece>? Route { get; set; }
    }
}