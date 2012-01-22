# NHibernate.OData

LGPL License.

## Introduction

NHibernate.OData is an OData parser for NHibernate.

The goal of NHibernate.OData is to parse OData query strings and turn them
into ICriteria. It currently implements the $filter, $orderby, $skip and
$top sections of the OData specification located at
[http://msdn.microsoft.com/en-us/library/dd541188.aspx](http://msdn.microsoft.com/en-us/library/dd541188.aspx).

The availability of stand-alone parsers like these is very limited. At the time
this project was started, the only good parser available was part
of the WCF Web API project [http://wcf.codeplex.com/](http://wcf.codeplex.com/).
This project contains a complete OData parser which applies the filters
of an OData query string to an IQueryable. This code can be found in the
WCFWebApi/src/Microsoft.ApplicationServer.Http/Microsoft/ApplicationServer/Query
directory at [http://wcf.codeplex.com/SourceControl/BrowseLatest](http://wcf.codeplex.com/SourceControl/BrowseLatest).
However, the license of this source code limits its usage in production
environments. This project was created as an open source alternative.

NHibernate.OData does **not** implement a full OData service. However, it can
be used to build such a service. In it's current form it's mainly useful to
provide an OData query interface for example as part of a public web service.

## Usage

The OData parser is exposed as three extension methods on the ISession class
named ODataQuery and is used as follows:

    ICriteria query = session.ODataQuery<Customer>("$filter=substringof(Name, 'Harry')");

The above method is the only public API of the library. There is no configuration
required to get up and running; only a reference to the ISession used to create
the ICriteria.

## Limitations

The current implementation does not access metadata. This results in the
following limitations:

* The cast operator is a no-op except for when casting to integer types. In that case it is converted to a round call;

* The isof operator is a no-op.

## Bugs

Bugs should be reported through github at
[http://github.com/pvginkel/NHibernate.OData/issues](http://github.com/pvginkel/NHibernate.OData/issues).

## License

NHibernate.OData is licensed under the LGPL 3.
