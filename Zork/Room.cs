using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace Zork
{
	public enum Directions
	{
		North,
		South,
		East,
		West
	}
	public class Room : IEquatable<Room>
	{
		[JsonProperty(Order = 1)]
		public string Name { get; }
        [JsonProperty(Order = 2)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "Neighbors", Order = 3)]
		private Dictionary<Directions, string> NeighborNames { get; set; }

		[JsonIgnore]
		public IReadOnlyDictionary<Directions, Room> Neighbors { get; private set; }

        public static bool operator ==(Room lhs, Room rhs) { 
			if(ReferenceEquals(lhs, rhs)) return true;

			if(lhs is null || rhs is null) return false;

			return lhs.Name == rhs.Name;
		}

		public static bool operator !=(Room lhs, Room rhs) => !(lhs == rhs);
		public override bool Equals(object obj) => obj is Room room ? this == room : false;
		public bool Equals(Room other) => this == other;
		public override string ToString() => Name;
		public override int GetHashCode() => Name.GetHashCode();
		public void UpdateNeighbors(World world) => Neighbors = (from entry in NeighborNames
																 let room = world.RoomsByName.GetValueOrDefault(entry.Value)
																 where room != null
																 select (Directions: entry.Key, Room: room))
																 .ToDictionary(pair => pair.Directions, pair => pair.Room);
    }
	public enum Fields
	{
		Name = 0,
		Description
	}

	public class World
	{
		public HashSet<Room> Rooms { get; set;}

		[JsonIgnore]
		public IReadOnlyDictionary<string, Room> RoomsByName => mRoomsByName;

		public Player SpawnPlayer() => new Player(this, StartingLocation);

		[OnDeserialized]
		private void OnDeserialized()
		{
			mRoomsByName = Rooms.ToDictionary(room => room.Name, room => room);

			foreach(Room room in Rooms)
			{
				room.UpdateNeighbors(this);
			}
		}

		[JsonProperty]
		private string StartingLocation { get; set; }

		private Dictionary<string, Room> mRoomsByName;
	}

	public class Player
	{
		public World World { get; }

		[JsonIgnore]
		public Room Location { get; private set; }
		[JsonIgnore]
		public string LocationName
		{
			get
			{
				return Location?.Name;
			}
			set
			{
				Location = World?.RoomsByName.GetValueOrDefault(value);
			}
		}

		[JsonIgnore]
		public int Moves = 0;

		public Player(World world, string startingLocation)
		{
			World = world;
			LocationName = startingLocation;
		}

		public bool Move(Directions direction)
		{
			bool isValidMove = Location.Neighbors.TryGetValue(direction, out Room destination);
			if (isValidMove)
			{
				Location = destination;
			}
			return isValidMove;
		}
	}
}
