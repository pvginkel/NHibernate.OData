@echo off

pushd "%~dp0"

cd ..

cd NHibernate.OData.Test\bin\Debug

"C:\Program Files (x86)\PartCover\PartCover .NET 4.0\PartCover.exe" --target "C:\Program Files (x86)\NUnit\nunit-console-x86.exe" --target-args NHibernate.OData.Test.dll --output partcover.xml --include [*NHibernate.OData*]* --exclude [*NHibernate.OData.Test*]*

"C:\Program Files (x86)\PartCover\PartCover .NET 4.0\PartCover.Browser.exe" --report partcover.xml
