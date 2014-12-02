# NHibernate.OData

LGPL License.

[Download from NuGet](http://nuget.org/packages/NHibernate.OData).

## Introduction

NHibernate.OData is an OData parser for NHibernate.

The goal of NHibernate.OData is to parse OData query strings and turn them
into ICriteria. It currently implements the `$filter`, `$orderby`, `$skip` and
`$top` sections of the OData specification located at
[http://msdn.microsoft.com/en-us/library/dd541188.aspx](http://msdn.microsoft.com/en-us/library/dd541188.aspx).

The availability of stand-alone parsers like these is very limited. At the time
this project was started, the only good parser available was part
of the WCF Web API project [http://wcf.codeplex.com/](http://wcf.codeplex.com/).
This project contains a complete OData parser which applies the filters
of an OData query string to an `IQueryable`. This code can be found in the
WCFWebApi/src/Microsoft.ApplicationServer.Http/Microsoft/ApplicationServer/Query
directory at [http://wcf.codeplex.com/SourceControl/BrowseLatest](http://wcf.codeplex.com/SourceControl/BrowseLatest).
However, the license of this source code limits its usage in production
environments. This project was created as an open source alternative.

## Usage

There are two ways in which NHibernate.OData can be used. If all that is required
is a way to query the database, the `ODataQuery` extension method can be used.
This method parses an OData query string and creates an `ICriteria` for that
query string on a specific entity:

    ICriteria query = session.ODataQuery<Customer>("$filter=substringof('Harry', Name)");

NHibernate.OData requires some caching based on the session factory associated
with the current session. When using the extension methods on `ODataParser`,
a static shared instance is used to manage this cache. If it is necessary to
manage the lifetime of this cache, the `ODataContext` class can be used instead.
Internally the static extension methods on the `ODataParser` class route calls
to the shared `ODataContext` instance.

Alternatively, the `ODataService` class provides a service for implementing
a full OData service. To use the `ODataService` class, a new instance of this
class must be instantiated for an `ISessionFactory` and a few configuration
parameters (see the constructor documentation). Queries can then be
executed using the Query method on the `ODataService` class which takes a
session, OData path and OData query string.

The demo application which is included in the ZIP file shows how such a service
can be implemented with minimal effort. This demo application makes use of
[NHttp](http://github.com/pvginkel/NHttp) so it can run without a web
server available.

## Join type

The default join type of NHibernate.OData is the inner join type. This may not give
the expected results. If you want the join type to be the left outer join type, call the
overload that takes a configuration object and set the `OuterJoin` property to true.

## Limitations

The current implementation does not access metadata while querying. This
results in the following limitations:

* The cast operator is a no-op except for when casting to integer types. In that case it is converted to a round call;

* The isof operator is a no-op.

Also, currently NHibernate.OData is read-only and can only be used to query
the database.

## Bugs

Bugs should be reported through github at
[http://github.com/pvginkel/NHibernate.OData/issues](http://github.com/pvginkel/NHibernate.OData/issues).

## License

NHibernate.OData is licensed under the LGPL 3.
