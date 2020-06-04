# Umbrella Static Serializers

1. Install the `Umbrella` Nuget package 
1. In the project: 
   * **Add an XML** file in project named `SerializationConfig.xml` with the following content:
     ```xml
         <?xml version="1.0" encoding="utf-8"?>
         <SerializerGenerationConfiguration xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
           <!-- The type of static serializer to generate -->
           <SerializationType>Json</SerializationType>

           <!-- The default namespace of entities section. 
           For example "MyProject.Client.Serialization.EntityQueryResponse<VenuesResponse>",
           VenuesResponse will be resolved in "MyProject.Entities.*.VenuesResponse" -->
           <EntitiesNameSpace>MyProject.Entities</EntitiesNameSpace>

           <!-- Optional : Namespace of serializers (Default is [EntitiesNameSpace].Serializers) -->
           <SerializersNameSpace>MyProject.Client.Serialization</SerializersNameSpace>
         </SerializerGenerationConfiguration>
     ```
   * **BUILD ACTION**: Ensure the _Build action_ is set to `None`. Press `F4` when the file is selected to see the _Build action_.
   * **COPY TO OUTPUT**: Ensure the _Copy to ouput directory_ is set to `Do not copy`.
1. Create an empty file named `SerializableTypes.cs`
   * Add the following lines :
      ``` csharp
      [assembly: JsonSerializableType(typeof(MyEntity))]
      [assembly: JsonSerializableType(typeof(IEnumerable<MyEntity>))]
      ```
  
   * You should add the types that used (on which you want a serializer generated) a root deserializable types.
     The `IEnumerable<T>` is defined here because `ISerializer<IEnumerable<MyEntity>>.Deserialize()` is used.

1. Add a **partial** `module.cs` class with a **partial** method _InitSerializer_
   ``` csharp
      using System;
      using Funq;
      using nVentive.Umbrella.Patterns.Serialization;

      namespace MyProject.Client.Serialization // This namespace should be the one defined in the `SerializationConfig.xml` file.
      {
        public partial class Module
        {
          // Sample bootstrapping, please adapt to your project.
          public static void Initialize(Container container)
          {
            InitSerializer(factory => container.Register<ISerializer>(c => factory(c.ResolveNamed<ISerializer>("CustomSerializer"))));
            // If you do not have custom serializers :
            // InitSerializer(factory => container.Register<ISerializer>(c => factory(null)));
          }

          // This methid will be implemented by generated code.
          // * You should pass an action who will callback the func parameter to
          //   get the instance of the serializer. You can optionnally give a fallback
          //   serializer if you can (or `null`).
          // * It has been designed this way to support most IoC systems.
          // * The action will be called immediatly. The func parameter can by called at
          //   any moment from any thread.
          static partial void InitSerializer(Action<Func<ISerializer, ISerializer>> serializerRegister);
        }
      }
   ```
