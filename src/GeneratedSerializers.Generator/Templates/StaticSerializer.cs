%FILE_HEADER%

using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Uno.Logging;
using GeneratedSerializers;

namespace %NAMESPACE%
{
	%CLASS_ATTRIBUTES%
	internal class %CLASS% : ISerializer, IObjectSerializer, IStaticSerializerProvider
	{
#if DEBUG
		private const string _notRegisteredError = "The requested serializer ({0}) is not registered. Add it to SerializationConfig.xml file.";
#else
		private const string _notRegisteredError = "The requested serializer ({0}) is not registered.";
#endif

		internal static %CLASS% Instance { get; private set; }

		private readonly ISerializer _fallbackSerializer;
		private readonly IObjectSerializer _fallbackObjectSerializer;

		public %CLASS%(ISerializer fallbackSerializer = null, IObjectSerializer fallbackObjectSerializer = null)
		{
			_fallbackSerializer = fallbackSerializer;
			_fallbackObjectSerializer = fallbackObjectSerializer;

			if (Instance == null)
			{
				Instance = this;
			}
#if NETFX_CORE || XAMARIN
			else
			{
				this.Log().Warn("Only one instance of %CLASS% can be created per app. Be sure to register it in the proper Container (Core).");
			}
#endif
		}

		public IObjectSerializer GetObjectSerializer()
		{
			return this;
		}

#region Serializers factories
private delegate IStaticSerializer SerializerFactory();
		private static readonly IDictionary<string, SerializerFactory> _serializerFactories = %SERIALIZER_FACTORIES_MAP%

		%SERIALIZER_FACTORIES%
		#endregion

#if NETFX_CORE || XAMARIN || SILVERLIGHT
		private static readonly global::Uno.Collections.SynchronizedDictionary<Type, IStaticSerializer> _serializers = new global::Uno.Collections.SynchronizedDictionary<Type, IStaticSerializer>();
#else
private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, IStaticSerializer> _serializers = new System.Collections.Concurrent.ConcurrentDictionary<Type, IStaticSerializer>();
#endif
		private static IStaticSerializer FindSerializer(Type type) => _serializers.GetOrAdd(type, FindSerializerCore);

		private static IStaticSerializer FindSerializerCore(Type type)
		{
			var typeName = type.ToString();
			SerializerFactory factory;
			if (_serializerFactories.TryGetValue(typeName, out factory))
			{
				return factory();
			}
			else
			{
				typeof(%CLASS%).Log().Warn("** PERFORMANCE HIT ** Try to resolve a serializer for a type which was not registered: " + typeName);
				return null;
			}
		}

		IStaticSerializer IStaticSerializerProvider.Get<T>() => Holder<T>.Serializer;

		T ISerializer.Deserialize<T>(Stream stream)
		{
			var serializer = Holder<T>.Serializer;
			if (serializer != null)
			{
				using (var reader = new StreamJsonReader(stream))
				{
					char? _;
					return (T) serializer.Deserialize(reader, reader.ReadNonWhiteSpaceChar(), out _);
				}
			}
			else if (_fallbackSerializer != null)
			{
				return _fallbackSerializer.Deserialize<T>(stream);
			}
			else
			{
				throw new ArgumentOutOfRangeException(_notRegisteredError.InvariantCultureFormat(typeof(T).ToString()));
			}
		}

		Stream ISerializer.Serialize<T>(T instance)
		{
			var serializer = Holder<T>.Serializer;
			if (serializer != null)
			{
				var stream = new MemoryStream();
				using (var writer = new StreamJsonWriter(stream, canDisposeStream: false)) // Do not dispose it: we must not dispose the stream!
				{
					serializer.Serialize(writer, instance);
				}

				stream.Position = 0;

				return stream;
			}
			else if (_fallbackSerializer != null)
			{
				return _fallbackSerializer.Serialize(instance);
			}
			else
			{
				throw new ArgumentOutOfRangeException(_notRegisteredError.InvariantCultureFormat(typeof(T).ToString()));
			}
		}

		bool IObjectSerializer.IsSerializable(Type valueType)
		{
			return FindSerializer(valueType) != null
				|| (_fallbackObjectSerializer?.IsSerializable(valueType) ?? false);
		}

		string IObjectSerializer.ToString(object value, Type valueType)
		{
			var serializer = FindSerializer(valueType);
			if (serializer != null)
			{
				var builder = new StringBuilder();
				using (var writer = new StringJsonWriter(builder))
				{
					serializer.Serialize(writer, value);
				}

				return builder.ToString();
			}
			else if (_fallbackObjectSerializer != null)
			{
				return _fallbackObjectSerializer.ToString(value, valueType);
			}
			else
			{
				throw new ArgumentOutOfRangeException(_notRegisteredError.InvariantCultureFormat(valueType.ToString()));
			}
		}

		Stream IObjectSerializer.ToStream(object value, Type valueType)
		{
			var serializer = FindSerializer(valueType);
			if (serializer != null)
			{
				var stream = new MemoryStream();
				using (var writer = new StreamJsonWriter(stream, canDisposeStream: false)) // Do not dispose it: we must not dispose the stream!
				{ 
					serializer.Serialize(writer, value);
				}

				stream.Position = 0;

				return stream;
			}
			else if (_fallbackObjectSerializer != null)
			{
				return _fallbackObjectSerializer.ToStream(value, valueType);
			}
			else
			{
				throw new ArgumentOutOfRangeException(_notRegisteredError.InvariantCultureFormat(valueType.ToString()));
			}
		}

		void IObjectSerializer.WriteToString(object value, Type valueType, StringBuilder targetString)
		{
			var serializer = FindSerializer(valueType);
			if (serializer != null)
			{
				using (var writer = new StringJsonWriter(targetString))
				{
					serializer.Serialize(writer, value);
				}
			}
			else if (_fallbackObjectSerializer != null)
			{
				_fallbackObjectSerializer.WriteToString(value, valueType, targetString);
			}
			else
			{
				throw new ArgumentOutOfRangeException(_notRegisteredError.InvariantCultureFormat(valueType.ToString()));
			}
		}

		void IObjectSerializer.WriteToStream(object value, Type valueType, Stream targetStream, bool canDisposeTargetStream = true)
		{
			var serializer = FindSerializer(valueType);
			if (serializer != null)
			{
				using (var writer = new StreamJsonWriter(targetStream, canDisposeStream: canDisposeTargetStream))
				{
					serializer.Serialize(writer, value);
				}
			}
			else if (_fallbackObjectSerializer != null)
			{
				_fallbackObjectSerializer.WriteToStream(value, valueType, targetStream, canDisposeTargetStream);
			}
			else
			{
				throw new ArgumentOutOfRangeException(_notRegisteredError.InvariantCultureFormat(valueType.ToString()));
			}
		}

		object IObjectSerializer.FromString(string value, Type targetType)
		{
			var serializer = FindSerializer(targetType);
			if (serializer != null)
			{
				using (var reader = new StringJsonReader(value))
				{
					char? _;
					return serializer.Deserialize(reader, reader.ReadNonWhiteSpaceChar(), out _);
				}
			}
			else if (_fallbackObjectSerializer != null)
			{
				return _fallbackObjectSerializer.FromString(value, targetType);
			}
			else
			{
				throw new ArgumentOutOfRangeException(_notRegisteredError.InvariantCultureFormat(targetType.ToString()));
			}
		}

		object IObjectSerializer.FromStream(Stream stream, Type targetType)
		{
			var serializer = FindSerializer(targetType);
			if (serializer != null)
			{
				using (var reader = new StreamJsonReader(stream))
				{
					char? _;
					return serializer.Deserialize(reader, reader.ReadNonWhiteSpaceChar(), out _);
				}
			}
			else if (_fallbackObjectSerializer != null)
			{
				return _fallbackObjectSerializer.FromStream(stream, targetType);
			}
			else
			{
				throw new ArgumentOutOfRangeException(_notRegisteredError.InvariantCultureFormat(targetType.ToString()));
			}
		}

		private static class Holder<T>
		{
			static Holder()
			{
				Serializer = (IStaticSerializer<T>)FindSerializer(typeof(T));
			}

			public static IStaticSerializer<T> Serializer { get; }
		}

		%GENERIC_SERIALIZERS%

	}
}
