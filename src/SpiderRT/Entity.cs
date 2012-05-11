using System;

namespace SpiderRT
{
	public class Entity : IEquatable<Entity>
	{
		public Guid Id { get; set; }

		public Entity()
		{
			Id = Guid.NewGuid();
		}

		#region Equality
		public bool Equals(Entity other)
		{
			if(ReferenceEquals(null, other))
			{
				return false;
			}
			if(ReferenceEquals(this, other))
			{
				return true;
			}
			return other.Id.Equals(Id);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}
			if(ReferenceEquals(this, obj))
			{
				return true;
			}
			if(obj.GetType() != typeof(Entity))
			{
				return false;
			}
			return Equals((Entity) obj);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		public static bool operator ==(Entity left, Entity right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Entity left, Entity right)
		{
			return !Equals(left, right);
		}
		#endregion
	}
}