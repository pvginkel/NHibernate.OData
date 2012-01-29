using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NHibernate.Criterion;
using NHibernate.Persister.Entity;
using NHibernate.Tuple;
using NHibernate.Type;
using System.Collections;

namespace NHibernate.OData
{
    /// <summary>
    /// Represents an executed OData request.
    /// </summary>
    public class ODataRequest
    {
        private readonly ODataService _service;
        private readonly ISession _session;
        private readonly string _path;
        private readonly string _queryString;

        /// <summary>
        /// Gets the data service version of the response. This must be set
        /// to the HTTP header "DataServiceVersion" in the HTTP response.
        /// </summary>
        public string DataServiceVersion { get; private set; }

        /// <summary>
        /// Gets the response of the OData request.
        /// </summary>
        public string Response { get; private set; }

        /// <summary>
        /// Gets the content type of the OData request. This must be set to
        /// the HTTP header "Content-Type" in the HTTP response.
        /// </summary>
        public string ContentType
        {
            get { return "application/xml;charset=utf-8"; }
        }

        internal ODataRequest(ODataService service, ISession session, string path, string queryString)
        {
            Require.NotNull(service, "service");
            Require.NotNull(session, "session");
            Require.NotNull(path, "path");

            _service = service;
            _session = session;
            _path = path.Trim('/');
            _queryString = queryString;

            PrepareResponse();
        }

        private void PrepareResponse()
        {
            if (_path.Length == 0)
                PrepareServiceResponse();
            else if ("$metadata".Equals(_path, StringComparison.OrdinalIgnoreCase))
                PrepareMetadataResponse();
            else
                PrepareDataResponse();
        }

        private void PrepareServiceResponse()
        {
            DataServiceVersion = "1.0;";
            Response = _service.ServiceResponse;
        }

        private void PrepareMetadataResponse()
        {
            DataServiceVersion = "1.0;";
            Response = _service.MetadataResponse;
        }

        private void PrepareDataResponse()
        {
            DataServiceVersion = "2.0;";

            string entityName;

            var path = new PathParser(_path).Parse();

            if (path.Members.Count > 2)
                throw new ODataException(ErrorMessages.PathParser_InvalidPath);

            entityName = Inflector.Singularize(path.Members[path.Members.Count - 1].Name);

            IEnumerable entities;
            object parentEntity = null;
            string parentEntityName = Inflector.Singularize(path.Members[0].Name);

            if (path.Members[0].IdExpression != null)
            {
                object parentId = path.Members[0].IdExpression.Value;
                parentEntity = _session.Load(parentEntityName, parentId);
            }

            if (parentEntity != null && path.Members.Count == 1)
            {
                entities = new[] { parentEntity };
            }
            else
            {
                var criteria =
                    String.IsNullOrEmpty(_queryString)
                    ? _session.CreateCriteria(entityName)
                    : _session.ODataQuery(entityName, _queryString);

                if (path.Members.Count == 2)
                {
                    if (parentEntity == null || path.Members[1].IdExpression != null)
                        throw new ODataException(ErrorMessages.PathParser_InvalidPath);

                    var parentPersister = _service.GetPersister(parentEntityName);
                    var property = GetProperty(parentPersister, path.Members[1].Name);
                    var collectionType = property.Type as CollectionType;
                    var manyToOneType = property.Type as ManyToOneType;

                    if (collectionType != null)
                    {
                        criteria.Add(Restrictions.Eq(parentEntityName, parentEntity));
                    }
                    else if (manyToOneType != null)
                    {
                        var childEntity = parentPersister.GetPropertyValue(parentEntity, property.Name, EntityMode.Poco);
                        var childPersister = _service.GetPersister(property.Type.ReturnedClass);

                        object idValue = childPersister.GetIdentifier(childEntity, EntityMode.Poco);

                        criteria.Add(Restrictions.Eq(childPersister.IdentifierPropertyName, idValue));
                    }
                    else
                    {
                        throw new ODataException(String.Format(ErrorMessages.ODataRequest_PropertyNotARelationship, path.Members[1].Name, parentPersister.EntityType.ReturnedClass.Name));
                    }
                }

                entities = criteria.List();
            }

            var feedElement = new XElement(
                ODataService.NsAtom + "feed",
                new XAttribute(XNamespace.Xml + "base", _service.ServiceNamespace),
                new XAttribute(XNamespace.Xmlns + "d", ODataService.NsDataServices),
                new XAttribute(XNamespace.Xmlns + "m", ODataService.NsMetadata),
                new XAttribute("xmlns", ODataService.NsAtom),
                new XElement(
                    ODataService.NsAtom + "title",
                    new XAttribute("type", "text"),
                    _path
                ),
                new XElement(
                    ODataService.NsAtom + "id",
                    _service.ServiceNamespace.ToString().TrimEnd('/') + "/" + _path
                ),
                new XElement(
                    ODataService.NsAtom + "updated",
                    DateTime.UtcNow.ToString("s") + "Z"
                ),
                new XElement(
                    ODataService.NsAtom + "link",
                    new XAttribute("rel", "self"),
                    new XAttribute("title", _path),
                    new XAttribute("href", _path)
                )
            );

            var persister = _service.GetPersister(entityName);

            foreach (var entity in entities)
            {
                var propertiesElement = new XElement(
                    ODataService.NsMetadata + "properties"
                );

                string id = Inflector.Pluralize(entityName) + "(" + LiteralUtil.EscapeValue(persister.GetIdentifier(entity, EntityMode.Poco)) + ")";

                var entryElement = new XElement(
                    ODataService.NsAtom + "entry",
                    new XElement(
                        ODataService.NsAtom + "id",
                        _service.ServiceNamespace.ToString().TrimEnd('/') + "/" + id
                    ),
                    new XElement(
                        ODataService.NsAtom + "author",
                        new XElement(ODataService.NsAtom + "name")
                    )
                );

                feedElement.Add(entryElement);

                propertiesElement.Add(AddProperty(persister.IdentifierPropertyName, persister.IdentifierType, persister.GetIdentifier(entity, EntityMode.Poco)));

                var values = persister.GetPropertyValues(entity, EntityMode.Poco);

                for (int i = 0; i < values.Length; i++)
                {
                    var propertyType = persister.PropertyTypes[i];
                    string propertyName = persister.PropertyNames[i];

                    var collectionType = propertyType as CollectionType;
                    var manyToOneType = propertyType as ManyToOneType;

                    if (collectionType != null || manyToOneType != null)
                    {
                        entryElement.Add(new XElement(
                            ODataService.NsAtom + "link",
                            new XAttribute("rel", ODataService.NsDataServices.ToString().TrimEnd('/') + "/related/" + propertyName),
                            new XAttribute("type", "application/atom+xml;type=entry"),
                            new XAttribute("title", propertyName),
                            new XAttribute("href", id + "/" + propertyName)
                        ));
                    }
                    else
                    {
                        propertiesElement.Add(AddProperty(propertyName, propertyType, values[i]));
                    }
                }

                entryElement.Add(
                    new XElement(
                        ODataService.NsAtom + "category",
                        new XAttribute("term", "SouthWind." + entityName),
                        new XAttribute("schema", ODataService.NsDataServices)
                    )
                );

                entryElement.Add(
                    new XElement(
                        ODataService.NsAtom + "content",
                        new XAttribute("type", "application/xml"),
                        propertiesElement
                    )
                );
            }

            Response = new XDocument(feedElement).ToString(SaveOptions.DisableFormatting);
        }

        private static XElement AddProperty(string propertyName, IType propertyType, object value)
        {
            var propertyElement = new XElement(
                ODataService.NsDataServices + propertyName
                );

            if (propertyType.ReturnedClass != typeof(string))
                propertyElement.Add(new XAttribute(ODataService.NsMetadata + "type", LiteralUtil.GetEdmType(propertyType.ReturnedClass)));

            string serialized = LiteralUtil.SerializeValue(value);

            if (serialized == null)
                propertyElement.Add(new XAttribute(ODataService.NsMetadata + "null", "true"));
            else
                propertyElement.Add(new XText(serialized));
            return propertyElement;
        }

        private StandardProperty GetProperty(SingleTableEntityPersister persister, string propertyName)
        {
            for (int i = 0; i < persister.PropertyNames.Length; i++)
            {
                if (persister.PropertyNames[i] == propertyName)
                    return persister.EntityMetamodel.Properties[i];
            }

            throw new ODataException(String.Format(ErrorMessages.ODataRequest_PropertyDoesNotExistOnParent, persister.EntityType.ReturnedClass.Name, propertyName));
        }
    }
}
