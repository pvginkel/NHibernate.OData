using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NHibernate.Engine;
using NHibernate.Metadata;
using NHibernate.Persister.Entity;
using NHibernate.Type;

namespace NHibernate.OData
{
    /// <summary>
    /// Service for responding to OData queries.
    /// </summary>
    public class ODataService
    {
        internal static readonly XNamespace NsAtom = "http://www.w3.org/2005/Atom";
        internal static readonly XNamespace NsApp = "http://www.w3.org/2007/app";
        internal static readonly XNamespace NsEdm = "http://schemas.microsoft.com/ado/2007/05/edm";
        internal static readonly XNamespace NsEdmx = "http://schemas.microsoft.com/ado/2007/06/edmx";
        internal static readonly XNamespace NsMetadata = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
        internal static readonly XNamespace NsDataServices = "http://schemas.microsoft.com/ado/2007/08/dataservices";

        private readonly ISessionFactoryImplementor _sessionFactory;

        /// <summary>
        /// Gets the namespace the service is registered on.
        /// </summary>
        public XNamespace ServiceNamespace { get; private set; }

        /// <summary>
        /// Gets the schema namespace.
        /// </summary>
        public string SchemaNamespace { get; private set; }

        /// <summary>
        /// Gets the container namespace.
        /// </summary>
        public string ContainerName { get; private set; }

        internal string ServiceResponse { get; private set; }

        internal string MetadataResponse { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataService"/> class
        /// with the specified session factory, service namespace, schema namespace
        /// and container name.
        /// </summary>
        /// <param name="sessionFactory">Session factory for which the service
        /// is created.</param>
        /// <param name="serviceNamespace">Namespace the service is registered
        /// on. This is the web address through which the service can be reached
        /// publicly.</param>
        /// <param name="schemaNamespace">Namespace of the schema. This usually
        /// is the name of the domain and is used to identify the schema.</param>
        public ODataService(ISessionFactory sessionFactory, XNamespace serviceNamespace, string schemaNamespace)
            : this(sessionFactory, serviceNamespace, schemaNamespace, schemaNamespace)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataService"/> class
        /// with the specified session factory, service namespace, schema namespace
        /// and container name.
        /// </summary>
        /// <param name="sessionFactory">Session factory for which the service
        /// is created.</param>
        /// <param name="serviceNamespace">Namespace the service is registered
        /// on. This is the web address through which the service can be reached
        /// publicly.</param>
        /// <param name="schemaNamespace">Namespace of the schema. This usually
        /// is the name of the domain and is used to identify the schema.</param>
        /// <param name="containerName">Container name. This is used to identify
        /// the current container and can e.g. be the name of the database.
        /// Defaults to the schema namespace.</param>
        public ODataService(ISessionFactory sessionFactory, XNamespace serviceNamespace, string schemaNamespace, string containerName)
        {
            Require.NotNull(sessionFactory, "sessionFactory");
            Require.NotNull(serviceNamespace, "serviceNamespace");
            Require.NotNull(schemaNamespace, "schemaNamespace");
            Require.NotNull(containerName, "containerName");

            ServiceNamespace = serviceNamespace;
            SchemaNamespace = schemaNamespace;
            ContainerName = containerName;

            _sessionFactory = (ISessionFactoryImplementor)sessionFactory;

            LoadSchema();

            ServiceResponse = CreateServiceResponse();
            MetadataResponse = CreateMetadataResponse();
        }

        private void LoadSchema()
        {

        }

        private string CreateServiceResponse()
        {
            var workspaceElement = new XElement(
                NsApp + "workspace",
                new XElement(NsAtom + "title", "Default")
            );

            foreach (var type in _sessionFactory.GetAllClassMetadata().Values)
            {
                string name = Inflector.Pluralize(GetPersister(type).TableName);

                workspaceElement.Add(new XElement(
                    NsApp + "collection",
                    new XAttribute("href", name),
                    new XElement(
                        NsAtom + "title",
                        name
                    )
                ));
            }

            var document = new XDocument(
                new XElement(
                    NsApp + "service",
                    new XAttribute(XNamespace.Xmlns + "atom", NsAtom),
                    new XAttribute(XNamespace.Xmlns + "app", NsApp),
                    new XAttribute("xmlns", NsApp),
                    new XAttribute(XNamespace.Xml + "base", ServiceNamespace),
                    workspaceElement
                )
            );

            return document.ToString(SaveOptions.DisableFormatting);
        }

        private string CreateMetadataResponse()
        {
            var schemaElement = new XElement(
                NsEdm + "Schema",
                new XAttribute(XNamespace.Xmlns + "d", NsDataServices),
                new XAttribute(XNamespace.Xmlns + "m", NsMetadata),
                new XAttribute("xmlns", NsEdm),
                new XAttribute("Namespace", SchemaNamespace)
            );

            var entityContainerElement = new XElement(
                NsEdm + "EntityContainer",
                new XAttribute("Name", ContainerName),
                new XAttribute(NsMetadata + "IsDefaultEntityContainer", "true")
            );

            var associations = new Dictionary<string, XElement>();

            foreach (var type in _sessionFactory.GetAllClassMetadata().Values)
            {
                var persister = GetPersister(type);

                string entityName = persister.EntityType.ReturnedClass.Name;

                var entityElement = new XElement(
                    NsEdm + "EntityType",
                    new XAttribute("Name", entityName),
                    new XElement(
                        NsEdm + "Key",
                        new XElement(
                            NsEdm + "PropertyRef",
                            new XAttribute("Name", persister.IdentifierPropertyName)
                        )
                    )
                );

                schemaElement.Add(entityElement);

                entityElement.Add(new XElement(
                    NsEdm + "Property",
                    new XAttribute("Name", persister.IdentifierPropertyName),
                    new XAttribute("Type", LiteralUtil.GetEdmType(persister.IdentifierType.ReturnedClass)),
                    new XAttribute("Nullable", "false")
                ));

                foreach (var property in persister.EntityMetamodel.Properties)
                {
                    var collectionType = property.Type as CollectionType;
                    var manyToOneType = property.Type as ManyToOneType;

                    var propertyType = property.Type.ReturnedClass;

                    if (collectionType != null || manyToOneType != null)
                    {
                        string fromRole;
                        string toRole;
                        string relationship;
                        string multiplicity;

                        if (collectionType != null)
                        {
                            propertyType = propertyType.GetGenericArguments()[0];

                            fromRole = entityName + "_" + property.Name;
                            toRole = propertyType.Name + "_" + entityName;
                            relationship = toRole + "_" + fromRole;
                            multiplicity = "*";
                        }
                        else
                        {
                            fromRole = entityName + "_" + property.Name;
                            toRole = propertyType.Name + "_" + Inflector.Pluralize(entityName);
                            relationship = fromRole + "_" + toRole;
                            multiplicity = "0..1";
                        }

                        entityElement.Add(new XElement(
                            NsEdm + "NavigationProperty",
                            new XAttribute("Name", property.Name),
                            new XAttribute("Relationship", SchemaNamespace + "." + relationship),
                            new XAttribute("FromRole", fromRole),
                            new XAttribute("ToRole", toRole)
                        ));

                        XElement association;

                        if (!associations.TryGetValue(relationship, out association))
                        {
                            association = new XElement(
                                NsEdm + "Association",
                                new XAttribute("Name", relationship)
                            );

                            associations.Add(relationship, association);
                        }

                        association.Add(new XElement(
                            NsEdm + "End",
                            new XAttribute("Role", toRole),
                            new XAttribute("Type", SchemaNamespace + "." + propertyType.Name),
                            new XAttribute("Multiplicity", multiplicity)
                        ));
                    }
                    else
                    {
                        entityElement.Add(new XElement(
                            NsEdm + "Property",
                            new XAttribute("Name", property.Name),
                            new XAttribute("Type", LiteralUtil.GetEdmType(propertyType)),
                            new XAttribute("Nullable", property.IsNullable ? "true" : "false")
                        ));
                    }
                }

                entityContainerElement.Add(new XElement(
                    NsEdm + "EntitySet",
                    new XAttribute("Name", Inflector.Pluralize(entityName)),
                    new XAttribute("EntityType", SchemaNamespace + "." + entityName)
                ));
            }

            foreach (var association in associations.Values)
            {
                schemaElement.Add(association);
            }

            schemaElement.Add(entityContainerElement);

            var document = new XDocument(
                new XElement(
                    NsEdmx + "Edmx",
                    new XAttribute(XNamespace.Xmlns + "edmx", NsEdmx),
                    new XAttribute("Version", "1.0"),
                    new XElement(
                        NsEdmx + "DataServices",
                        new XAttribute(XNamespace.Xmlns + "m", NsMetadata),
                        new XAttribute(NsMetadata + "DataServiceVersion", "2.0"),
                        schemaElement
                    )
                )
            );

            return document.ToString(SaveOptions.DisableFormatting);
        }

        internal SingleTableEntityPersister GetPersister(string entityName)
        {
            var persister = ResolvePersister(entityName);

            return GetPersister(_sessionFactory.GetClassMetadata(persister.EntityName));
        }

        internal SingleTableEntityPersister GetPersister(System.Type entityType)
        {
            return GetPersister(_sessionFactory.GetClassMetadata(entityType));
        }

        internal SingleTableEntityPersister GetPersister(IClassMetadata type)
        {
            var persister = type as SingleTableEntityPersister;

            if (persister == null)
                throw new ODataException(String.Format(ErrorMessages.ODataService_UnsupportedPersister, type.GetType().FullName));
            if (!persister.HasIdentifierProperty)
                throw new ODataException(String.Format(ErrorMessages.ODataService_PersisterMustHaveIdentifierProperty, type.GetType().FullName));

            return persister;
        }

        private IEntityPersister ResolvePersister(string entityName)
        {
            // Check for an exact match.

            var persister = _sessionFactory.TryGetEntityPersister(entityName);

            if (persister != null)
                return persister;

            // Couldn't find persister through exact name, try finding a single implementing class.

            string[] implementors = _sessionFactory.GetImplementors(entityName);

            if (implementors.Length > 1)
                throw new ODataException(String.Format(ErrorMessages.ODataRequest_AmbiguousEntityName, entityName));
            else if (implementors.Length == 0)
                return null;
            else
                return _sessionFactory.GetEntityPersister(implementors[0]);
        }

        /// <summary>
        /// Executes an OData query with the specified session, path and
        /// query string and returns a request object.
        /// </summary>
        /// <param name="session">Session to execute the query on.</param>
        /// <param name="path">Path component of the OData request.</param>
        /// <returns>Completed OData request.</returns>
        public ODataRequest Query(ISession session, string path)
        {
            return Query(session, path, null);
        }

        /// <summary>
        /// Executes an OData query with the specified session, path and
        /// query string and returns a request object.
        /// </summary>
        /// <param name="session">Session to execute the query on.</param>
        /// <param name="path">Path component of the OData request.</param>
        /// <param name="queryString">Query string component of the OData
        /// request.</param>
        /// <returns>Completed OData request.</returns>
        public ODataRequest Query(ISession session, string path, string queryString)
        {
            return new ODataRequest(this, session, path, queryString);
        }
    }
}
